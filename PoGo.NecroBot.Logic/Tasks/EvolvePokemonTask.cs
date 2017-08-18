#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using TinyIoC;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class EvolvePokemonTask
    {
        private static DateTime _lastLuckyEggTime;

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemonToEvolveTask = await session.Inventory
                .GetPokemonToEvolve(session.LogicSettings.PokemonEvolveFilters).ConfigureAwait(false);
            var pokemonToEvolve = pokemonToEvolveTask.Where(p => p != null).ToList();

            session.EventDispatcher.Send(new EvolveCountEvent
            {
                Evolves = pokemonToEvolve.Count()
            });

            if (pokemonToEvolve.Any())
            {
                if (session.LogicSettings.KeepPokemonsThatCanEvolve)
                {
                    var luckyEggMin = session.LogicSettings.UseLuckyEggsMinPokemonAmount;
                    var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
                    var totalPokemon = await session.Inventory.GetPokemons().ConfigureAwait(false);
                    var totalEggs = await session.Inventory.GetEggs().ConfigureAwait(false);

                    var pokemonNeededInInventory = (maxStorage - totalEggs.Count()) *
                                                   session.LogicSettings.EvolveKeptPokemonsAtStorageUsagePercentage /
                                                   100.0f;
                    var needPokemonToStartEvolve = Math.Round(
                        Math.Max(0, Math.Min(session.LogicSettings.EvolveKeptPokemonIfBagHasOverThisManyPokemon,
                            Math.Min(pokemonNeededInInventory, session.Profile.PlayerData.MaxPokemonStorage))));

                    var deltaCount = needPokemonToStartEvolve - totalPokemon.Count();
                    if (session.LogicSettings.UseLuckyEggsWhileEvolving)
                    {
                        if (luckyEggMin > maxStorage)
                        {
                            session.EventDispatcher.Send(new WarnEvent
                            {
                                Message = session.Translation.GetTranslation(
                                    TranslationString.UseLuckyEggsMinPokemonAmountTooHigh,
                                    luckyEggMin, maxStorage)
                            });
                            return;
                        }
                    }

                    if (deltaCount > 0)
                    {
                        session.EventDispatcher.Send(new UpdateEvent()
                        {
                            Message = session.Translation.GetTranslation(
                                TranslationString.WaitingForMorePokemonToEvolve,
                                pokemonToEvolve.Count,
                                deltaCount,
                                totalPokemon.Count(),
                                needPokemonToStartEvolve,
                                session.LogicSettings.EvolveKeptPokemonsAtStorageUsagePercentage
                            )
                        });
                        return;
                    }
                    else
                    {
                        if (await ShouldUseLuckyEgg(session, pokemonToEvolve).ConfigureAwait(false))
                        {
                            await UseLuckyEgg(session).ConfigureAwait(false);
                        }
                        await Evolve(session, pokemonToEvolve).ConfigureAwait(false);
                    }
                }
                else if (session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                         session.LogicSettings.EvolveAllPokemonAboveIv)
                {
                    if (await ShouldUseLuckyEgg(session, pokemonToEvolve).ConfigureAwait(false))
                    {
                        await UseLuckyEgg(session).ConfigureAwait(false);
                    }
                    await Evolve(session, pokemonToEvolve).ConfigureAwait(false);
                }
            }
        }

        public static async Task UseLuckyEgg(ISession session)
        {
            var inventoryContent = await session.Inventory.GetItems().ConfigureAwait(false);
            var luckyEgg = inventoryContent.FirstOrDefault(p => p.ItemId == ItemId.ItemLuckyEgg);

            if (luckyEgg.Count == 0) // We tried to use egg but we don't have any more. Just return.
                return;

            if (_lastLuckyEggTime.AddMinutes(30).Ticks > DateTime.Now.Ticks)
                return;

            var responseLuckyEgg = await session.Client.Inventory.UseItemXpBoost().ConfigureAwait(false);
            if (responseLuckyEgg.Result == UseItemXpBoostResponse.Types.Result.Success)
            {
                _lastLuckyEggTime = DateTime.Now;

                // Get refreshed lucky egg so we have an accurate count.
                luckyEgg = inventoryContent.FirstOrDefault(p => p.ItemId == ItemId.ItemLuckyEgg);

                if (luckyEgg != null) session.EventDispatcher.Send(new UseLuckyEggEvent { Count = luckyEgg.Count });
                TinyIoCContainer.Current.Resolve<MultiAccountManager>().DisableSwitchAccountUntil(DateTime.Now.AddMinutes(30));
            }
            await DelayingUtils.DelayAsync(session.LogicSettings.DelayBetweenPlayerActions, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
        }

        public static async Task<ItemId> GetRequireEvolveItem(ISession session, PokemonId from, PokemonId to)
        {
            var settings = (await session.Inventory.GetPokemonSettings().ConfigureAwait(false)).FirstOrDefault(x => x.PokemonId == from);
            if (settings == null) return ItemId.ItemUnknown;

            var branch = settings.EvolutionBranch.FirstOrDefault(x => x.Evolution == to);
            if (branch == null) return ItemId.ItemUnknown;
            return branch.EvolutionItemRequirement;
        }
        private static async Task Evolve(ISession session, List<PokemonData> pokemonToEvolve)
        {
            int sequence = 1;
            foreach (var pokemon in pokemonToEvolve)
            {
                var filter = session.LogicSettings.PokemonEvolveFilters.GetFilter<EvolveFilter>(pokemon.PokemonId);
                if (await session.Inventory.CanEvolvePokemon(pokemon, filter).ConfigureAwait(false))
                {
                    try
                    {
                        // no cancellationToken.ThrowIfCancellationRequested here, otherwise the lucky egg would be wasted.
                        var evolveResponse = await session.Client.Inventory.EvolvePokemon(pokemon.Id, filter == null ? ItemId.ItemUnknown : await GetRequireEvolveItem(session, pokemon.PokemonId, filter.EvolveToPokemonId)).ConfigureAwait(false);
                        var CandyUsed = session.Inventory.GetCandyCount(pokemon.PokemonId);

                        if (evolveResponse.Result == EvolvePokemonResponse.Types.Result.Success)
                        {
                            session.EventDispatcher.Send(new PokemonEvolveEvent
                            {
                                Id = pokemon.PokemonId,
                                Exp = evolveResponse.ExperienceAwarded,
                                UniqueId = pokemon.Id,
                                Result = evolveResponse.Result,
                                Sequence = pokemonToEvolve.Count() == 1 ? 0 : sequence++,
                                EvolvedPokemon = evolveResponse.EvolvedPokemonData,
                                Candy = await CandyUsed
                            });
                        }

                        if (!pokemonToEvolve.Last().Equals(pokemon))
                            await DelayingUtils.DelayAsync(session.LogicSettings.EvolveActionDelay, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Logger.Write("ERROR - Evolve failed", color: ConsoleColor.Red);
                    }
                }
            }
        }

        private static async Task<bool> ShouldUseLuckyEgg(ISession session, List<PokemonData> pokemonToEvolve)
        {
            var inventoryContent = await session.Inventory.GetItems().ConfigureAwait(false);
            var luckyEggs = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg);
            var luckyEgg = luckyEggs.FirstOrDefault();

            if (session.LogicSettings.UseLuckyEggsWhileEvolving && luckyEgg != null && luckyEgg.Count > 0)
            {
                if (pokemonToEvolve.Count >= session.LogicSettings.UseLuckyEggsMinPokemonAmount)
                {
                    return true;
                }
                else
                {
                    var evolvablePokemon = await session.Inventory.GetPokemons().ConfigureAwait(false);
                    var deltaPokemonToUseLuckyEgg = session.LogicSettings.UseLuckyEggsMinPokemonAmount -
                                                    pokemonToEvolve.Count;
                    var availableSpace = session.Profile.PlayerData.MaxPokemonStorage - evolvablePokemon.Count();

                    if (deltaPokemonToUseLuckyEgg > availableSpace)
                    {
                        var possibleLimitInThisIteration = pokemonToEvolve.Count + availableSpace;

                        session.EventDispatcher.Send(new NoticeEvent()
                        {
                            Message = session.Translation.GetTranslation(
                                TranslationString.UseLuckyEggsMinPokemonAmountTooHigh,
                                session.LogicSettings.UseLuckyEggsMinPokemonAmount, possibleLimitInThisIteration
                            )
                        });
                    }
                    else
                    {
                        session.EventDispatcher.Send(new NoticeEvent()
                        {
                            Message = session.Translation.GetTranslation(
                                TranslationString.CatchMorePokemonToUseLuckyEgg,
                                deltaPokemonToUseLuckyEgg
                            )
                        });
                    }
                }
            }
            return false;
        }
    }
}
