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
        public static bool IsActivated(ISession session)
        {
            if (session.LogicSettings.TriggerEvolveAsSoonAsFilterIsMatched
                || session.LogicSettings.TriggerEvolveIfLuckyEggIsActive
                || session.LogicSettings.TriggerEvolveOnEvolutionCount
                || session.LogicSettings.TriggerEvolveOnStorageUsageAbsolute
                || session.LogicSettings.TriggerEvolveOnStorageUsagePercentage)
            {
                return true;
            }
            return false;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemonToEvolveTask = await session.Inventory
                .GetPokemonToEvolve(session.LogicSettings.PokemonEvolveFilters).ConfigureAwait(false);
            var pokemonsToEvolve = pokemonToEvolveTask.Where(p => p != null).ToList();
            int evolutionCount = pokemonsToEvolve.Count();

            session.EventDispatcher.Send(new EvolveCountEvent
            {
                Evolves = evolutionCount
            });

            if (evolutionCount > 0 && (
                session.LogicSettings.TriggerEvolveAsSoonAsFilterIsMatched
                || IsTriggerByEvolutionCount(session, evolutionCount)
                || await IsTriggerByLuckyEggActive(session).ConfigureAwait(false)
                || await IsTriggerByStorageUsagePercentage(session, evolutionCount).ConfigureAwait(false)
                || await IsTriggerByStorageUsageAbsolute(session, evolutionCount).ConfigureAwait(false)))
            {
                if (await ShouldUseLuckyEgg(session, pokemonsToEvolve).ConfigureAwait(false))
                {
                    await UseLuckyEgg(session).ConfigureAwait(false);
                }
                await Evolve(session, cancellationToken, pokemonsToEvolve).ConfigureAwait(false);
            }
        }

        private static async Task<bool> IsTriggerByLuckyEggActive(ISession session)
        {
            if(session.LogicSettings.TriggerEvolveIfLuckyEggIsActive)
            {
                TimeSpan luckyEggRemainingTime = await session.Inventory.GetLuckyEggRemainingTime().ConfigureAwait(false);

                if (luckyEggRemainingTime.TotalSeconds > 0)
                {
                    Logger.Write(session.Translation.GetTranslation(TranslationString.UseLuckyEggActive, luckyEggRemainingTime.Minutes, luckyEggRemainingTime.Seconds), LogLevel.Info, ConsoleColor.DarkGreen);
                    return true;
                }
            }
            return false;
        }

        private static bool IsTriggerByEvolutionCount(ISession session, int pokemonsToEvolveCount)
        {
            if (!session.LogicSettings.TriggerEvolveOnEvolutionCount)
            {
                return false;
            }

            int luckyEggMin = session.LogicSettings.TriggerEvolveOnEvolutionCountValue;
            int missingPossibleEvolutions = luckyEggMin - pokemonsToEvolveCount;

            if (missingPossibleEvolutions > 0)
            {
                session.EventDispatcher.Send(new UpdateEvent()
                {
                    Message = session.Translation.GetTranslation(
                        TranslationString.WaitingForMoreEvolutionsToEvolve,
                        missingPossibleEvolutions,
                        pokemonsToEvolveCount,
                        luckyEggMin)
                });
                return false;
            }
            return true;
        }

        private static async Task<bool> IsTriggerByStorageUsagePercentage(ISession session, int pokemonsToEvolveCount)
        {
            if (!session.LogicSettings.TriggerEvolveOnStorageUsagePercentage)
            {
                return false;
            }

            var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
            int thresholdFromRelConfig = Convert.ToInt32(maxStorage * session.LogicSettings.TriggerEvolveOnStorageUsagePercentageValue / 100.0f);            
            return await IsTriggerByStorageUsage(session, pokemonsToEvolveCount, thresholdFromRelConfig).ConfigureAwait(false);
        }

        private static async Task<bool> IsTriggerByStorageUsageAbsolute(ISession session, int pokemonsToEvolveCount)
        {
            if (!session.LogicSettings.TriggerEvolveOnStorageUsageAbsolute)
            {
                return false;
            }

            int thresholdFromAbsConfig = session.LogicSettings.TriggerEvolveOnStorageUsageAbsoluteValue;
            return await IsTriggerByStorageUsage(session, pokemonsToEvolveCount, thresholdFromAbsConfig).ConfigureAwait(false);
        }

        private static async Task<bool> IsTriggerByStorageUsage(ISession session, int pokemonsToEvolveCount, int storageThresholdAbs)
        {
            var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
            var neededPokemonsToStartEvolve = Math.Max(0, Math.Min(storageThresholdAbs, maxStorage));

            // Calculate missing pokemons until storage full enough
            var totalPokemon = await session.Inventory.GetPokemons().ConfigureAwait(false);
            int missingPokemonsInStorage = neededPokemonsToStartEvolve - totalPokemon.Count();

            if (missingPokemonsInStorage > 0)
            {
                session.EventDispatcher.Send(new UpdateEvent()
                {
                    Message = session.Translation.GetTranslation(
                    TranslationString.WaitingForMorePokemonToEvolve,
                    pokemonsToEvolveCount,
                    missingPokemonsInStorage,
                    totalPokemon.Count(),
                    neededPokemonsToStartEvolve,
                    0.0 // Deprecated
                )
                });
                return false;
            }

            return true;
        }

        public static async Task UseLuckyEgg(ISession session)
        {
            var inventoryContent = await session.Inventory.GetItems().ConfigureAwait(false);
            var luckyEgg = inventoryContent.FirstOrDefault(p => p.ItemId == ItemId.ItemLuckyEgg);

            if (luckyEgg.Count == 0) // We tried to use egg but we don't have any more. Just return.
                return;

            TimeSpan luckyEggRemainingTime = await session.Inventory.GetLuckyEggRemainingTime().ConfigureAwait(false);
            if (luckyEggRemainingTime.TotalSeconds > 0)
                return; // There is still an egg active

            var responseLuckyEgg = await session.Client.Inventory.UseItemXpBoost().ConfigureAwait(false);
            if (responseLuckyEgg.Result == UseItemXpBoostResponse.Types.Result.Success)
            {
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

        private static async Task Evolve(ISession session, CancellationToken cancellationToken, List<PokemonData> pokemonToEvolve)
        {
            int sequence = 1;
            foreach (var pokemon in pokemonToEvolve)
            {
                var filter = session.LogicSettings.PokemonEvolveFilters.GetFilter<EvolveFilter>(pokemon.PokemonId);
                if (await session.Inventory.CanEvolvePokemon(pokemon, filter).ConfigureAwait(false))
                {
                    try
                    {
                        TimeSpan luckyEggRemainingTime = await session.Inventory.GetLuckyEggRemainingTime().ConfigureAwait(false);
                        if (luckyEggRemainingTime.TotalSeconds <= 0)
                            // do not waste lucky egg
                            cancellationToken.ThrowIfCancellationRequested();

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
                        {
                            await DelayingUtils.DelayAsync(session.LogicSettings.EvolveActionDelay, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
                        }
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
            var luckyEggItemData = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg).FirstOrDefault();

            if (session.LogicSettings.EvolveApplyLuckyEggOnEvolutionCount && luckyEggItemData?.Count > 0)
            {
                int applyLuckyEggThreshold = session.LogicSettings.EvolveApplyLuckyEggOnEvolutionCountValue;
                if (pokemonToEvolve.Count >= applyLuckyEggThreshold)
                {
                    return true;
                }

                var evolvablePokemon = await session.Inventory.GetPokemons().ConfigureAwait(false);
                var deltaPokemonToUseLuckyEgg = applyLuckyEggThreshold - pokemonToEvolve.Count;
                var availableSpace = session.Profile.PlayerData.MaxPokemonStorage - evolvablePokemon.Count();

                if (deltaPokemonToUseLuckyEgg > availableSpace)
                {
                    var possibleLimitInThisIteration = pokemonToEvolve.Count + availableSpace;

                    session.EventDispatcher.Send(new NoticeEvent()
                    {
                        Message = session.Translation.GetTranslation(
                            TranslationString.UseLuckyEggsMinPokemonAmountTooHigh,
                            applyLuckyEggThreshold,
                            possibleLimitInThisIteration
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
            return false;
        }
    }
}
