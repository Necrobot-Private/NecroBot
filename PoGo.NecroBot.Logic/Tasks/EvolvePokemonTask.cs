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

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class EvolvePokemonTask
    {
        private static DateTime _lastLuckyEggTime;

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //await session.Inventory.RefreshCachedInventory();

            var pokemonToEvolveTask = session.Inventory
                .GetPokemonToEvolve(session.LogicSettings.PokemonsToEvolve);
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
                    var totalPokemon = session.Inventory.GetPokemons();
                    var totalEggs = session.Inventory.GetEggs();

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
                        if (shouldUseLuckyEgg(session, pokemonToEvolve))
                        {
                            await UseLuckyEgg(session);
                        }
                        await Evolve(session, pokemonToEvolve);
                    }
                }
                else if (session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                         session.LogicSettings.EvolveAllPokemonAboveIv)
                {
                    if (shouldUseLuckyEgg(session, pokemonToEvolve))
                    {
                        await UseLuckyEgg(session);
                    }
                    await Evolve(session, pokemonToEvolve);
                }
            }
        }

        public static async Task UseLuckyEgg(ISession session)
        {
            var inventoryContent = session.Inventory.GetItems();

            var luckyEggs = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg);
            var luckyEgg = luckyEggs.FirstOrDefault();

            if (_lastLuckyEggTime.AddMinutes(30).Ticks > DateTime.Now.Ticks)
                return;

            _lastLuckyEggTime = DateTime.Now;
            var responseLuckyEgg = await session.Client.Inventory.UseItemXpBoost();
            if (responseLuckyEgg.Result == UseItemXpBoostResponse.Types.Result.Success)
            {
                if (luckyEgg != null) session.EventDispatcher.Send(new UseLuckyEggEvent { Count = luckyEgg.Count - 1 });
                TinyIoCContainer.Current.Resolve<MultiAccountManager>().DisableSwitchAccountUntil(DateTime.Now.AddMinutes(30));
            }
            DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
        }

        private static async Task Evolve(ISession session, List<PokemonData> pokemonToEvolve)
        {
            int sequence = 1;
            foreach (var pokemon in pokemonToEvolve)
            {
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                if (await session.Inventory.CanEvolvePokemon(pokemon))
                {
                    try
                    {
                        // no cancellationToken.ThrowIfCancellationRequested here, otherwise the lucky egg would be wasted.
                        var evolveResponse = await session.Client.Inventory.EvolvePokemon(pokemon.Id);
                        if (evolveResponse.Result == EvolvePokemonResponse.Types.Result.Success)
                        {
                            session.EventDispatcher.Send(new PokemonEvolveEvent
                            {
                                Id = pokemon.PokemonId,
                                Exp = evolveResponse.ExperienceAwarded,
                                UniqueId = pokemon.Id,
                                Result = evolveResponse.Result,
                                Sequence = pokemonToEvolve.Count() == 1 ? 0 : sequence++,
                                EvolvedPokemon = evolveResponse.EvolvedPokemonData
                            });
                        }

                        if (!pokemonToEvolve.Last().Equals(pokemon))
                            DelayingUtils.Delay(session.LogicSettings.EvolveActionDelay, 0);
                    }
                    catch
                    {
                        Logger.Write("ERROR - Evolve failed", color: ConsoleColor.Red);
                    }
                }
            }
        }

        private static Boolean shouldUseLuckyEgg(ISession session, List<PokemonData> pokemonToEvolve)
        {
            var inventoryContent = session.Inventory.GetItems();

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
                    var evolvablePokemon = session.Inventory.GetPokemons();

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