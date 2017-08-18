#region using directives

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using TinyIoC;
using System.Collections.Generic;
using GeoCoordinatePortable;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchNearbyPokemonsTask
    {
        //add delegate
        public delegate void PokemonsEncounterDelegate(List<MapPokemon> pokemons);

        public static async Task Execute(ISession session, CancellationToken cancellationToken,
            PokemonId priority = PokemonId.Missingno, bool sessionAllowTransfer = true)
        {
            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            manager.ThrowIfSwitchAccountRequested();
            cancellationToken.ThrowIfCancellationRequested();

            if (!session.LogicSettings.CatchPokemon) return;

            var totalBalls = (await session.Inventory.GetItems().ConfigureAwait(false)).Where(x => x.ItemId == ItemId.ItemPokeBall || x.ItemId == ItemId.ItemGreatBall || x.ItemId == ItemId.ItemUltraBall).Sum(x => x.Count);

            if (session.SaveBallForByPassCatchFlee && totalBalls < 130)
            {
                return ;
            }

            if (session.Stats.CatchThresholdExceeds(session))
            {
                if (manager.AllowMultipleBot() &&
                    session.LogicSettings.MultipleBotConfig.SwitchOnCatchLimit &&
                    manager.AllowSwitch()
                    )
                {
                    throw new ActiveSwitchByRuleException()
                    {
                        MatchedRule = SwitchRules.CatchLimitReached,
                        ReachedValue = session.LogicSettings.CatchPokemonLimit
                    };
                }
                return;
            }

            Logger.Write(session.Translation.GetTranslation(TranslationString.LookingForPokemon), LogLevel.Debug);

            var nearbyPokemons = await GetNearbyPokemons(session).ConfigureAwait(false);

            Logger.Write($"There are {nearbyPokemons.Count()} pokemon nearby.", LogLevel.Debug);

            if (nearbyPokemons == null) return;

            var priorityPokemon = nearbyPokemons.Where(p => p.PokemonId == priority).FirstOrDefault();
            var pokemons = nearbyPokemons.Where(p => p.PokemonId != priority).ToList();

            //add pokemons to map
            OnPokemonEncounterEvent(pokemons.ToList());

            EncounterResponse encounter = null;
            //if that is snipe pokemon and inventories if full, execute transfer to get more room for pokemon
            if (priorityPokemon != null)
            {
                pokemons.Insert(0, priorityPokemon);
                await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                        new GeoCoordinate(priorityPokemon.Latitude, priorityPokemon.Longitude, session.Client.CurrentAltitude), 0).ConfigureAwait(false); // Set speed to 0 for random speed.
                encounter = await session.Client.Encounter
                    .EncounterPokemon(priorityPokemon.EncounterId, priorityPokemon.SpawnPointId).ConfigureAwait(false);

                if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (session.LogicSettings.TransferDuplicatePokemon)
                        await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);

                    if (session.LogicSettings.TransferWeakPokemon)
                        await TransferWeakPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);

                    if (session.LogicSettings.EvolveAllPokemonAboveIv ||
                        session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                        session.LogicSettings.UseLuckyEggsWhileEvolving ||
                        session.LogicSettings.KeepPokemonsThatCanEvolve)
                    {
                        await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            var allitems = await session.Inventory.GetItems().ConfigureAwait(false);
            var pokeBallsCount = allitems.FirstOrDefault(i => i.ItemId == ItemId.ItemPokeBall)?.Count;
            var greatBallsCount = allitems.FirstOrDefault(i => i.ItemId == ItemId.ItemGreatBall)?.Count;
            var ultraBallsCount = allitems.FirstOrDefault(i => i.ItemId == ItemId.ItemUltraBall)?.Count;
            var masterBallsCount = allitems.FirstOrDefault(i => i.ItemId == ItemId.ItemMasterBall)?.Count;
            masterBallsCount = masterBallsCount ?? 0; //return null ATM. need this code to logic check work
            var PokeBalls = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;

            if (0 < pokemons.Count && PokeBalls >= session.LogicSettings.PokeballsToKeepForSnipe) // Don't display if not enough Pokeballs - TheWizrad1328
                Logger.Write($"Catching {pokemons.Count} Pokemon Nearby...", LogLevel.Info);

            foreach (var pokemon in pokemons)
            {
                await MSniperServiceTask.Execute(session, cancellationToken).ConfigureAwait(false);
                
                /*
                if (LocationUtils.CalculateDistanceInMeters(pokemon.Latitude, pokemon.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) > session.Client.GlobalSettings.MapSettings.EncounterRangeMeters)
                {
                    Logger.Debug($"THIS POKEMON IS TOO FAR, {pokemon.Latitude}, {pokemon.Longitude}");
                    continue;
                }
                */

                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                
                if (session.Cache.GetCacheItem(CatchPokemonTask.GetEncounterCacheKey(pokemon.EncounterId)) != null)
                {
                    continue; //this pokemon has been skipped because not meet with catch criteria before.
                }

                if (PokeBalls < session.LogicSettings.PokeballsToKeepForSnipe && session.CatchBlockTime < DateTime.Now)
                {
                    session.CatchBlockTime = DateTime.Now.AddMinutes(session.LogicSettings.OutOfBallCatchBlockTime);
                    Logger.Write(session.Translation.GetTranslation(TranslationString.CatchPokemonDisable,
                        session.LogicSettings.OutOfBallCatchBlockTime, session.LogicSettings.PokeballsToKeepForSnipe));
                    return;
                }

                if (session.CatchBlockTime > DateTime.Now) return;

                if ((session.LogicSettings.UsePokemonToCatchLocallyListOnly &&
                     !session.LogicSettings.PokemonToCatchLocally.Pokemon.Contains(pokemon.PokemonId)) ||
                    (session.LogicSettings.UsePokemonToNotCatchFilter &&
                     session.LogicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId)))
                {
                    Logger.Write(session.Translation.GetTranslation(TranslationString.PokemonSkipped,
                        session.Translation.GetPokemonTranslation(pokemon.PokemonId)));
                    continue;
                }

                /*
                var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 500 : 100, cancellationToken).ConfigureAwait(false);
                */

                //to avoid duplicated encounter when snipe priority pokemon

                if (encounter == null || encounter.Status != EncounterResponse.Types.Status.EncounterSuccess)
                {
                    //await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                    //    new GeoCoordinate(pokemon.Latitude, pokemon.Longitude, session.Client.CurrentAltitude), 0).ConfigureAwait(false); // Set speed to 0 for random speed.
                    encounter =
                        await session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId).ConfigureAwait(false);
                }

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess &&
                    session.LogicSettings.CatchPokemon)
                {
                    // Catch the Pokemon
                    await CatchPokemonTask.Execute(session, cancellationToken, encounter, pokemon,
                        currentFortData: null, sessionAllowTransfer: sessionAllowTransfer).ConfigureAwait(false);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (session.LogicSettings.TransferDuplicatePokemon || session.LogicSettings.TransferWeakPokemon)
                    {
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferring)
                        });
                        if (session.LogicSettings.TransferDuplicatePokemon)
                            await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                        if (session.LogicSettings.TransferWeakPokemon)
                            await TransferWeakPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                        if (session.LogicSettings.EvolveAllPokemonAboveIv ||
                            session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                            session.LogicSettings.UseLuckyEggsWhileEvolving ||
                            session.LogicSettings.KeepPokemonsThatCanEvolve)
                        {
                            await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                        });
                }
                else
                {
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            session.Translation.GetTranslation(TranslationString.EncounterProblem, encounter.Status)
                    });
                }
                encounter = null;
                // If pokemon is not last pokemon in list, create delay between catches, else keep moving.
                if (!Equals(pokemons.ElementAtOrDefault(pokemons.Count() - 1), pokemon))
                {
                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public static async Task<IOrderedEnumerable<MapPokemon>> GetNearbyPokemons(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects().ConfigureAwait(false);
            if (mapObjects == null || mapObjects.MapCells == null) return null;

            var forts = mapObjects.MapCells.SelectMany(p => p.Forts).ToList();
            var nearbyPokemons = mapObjects.MapCells.SelectMany(x => x.NearbyPokemons).ToList();
            
            session.EventDispatcher.Send(new PokeStopListEvent(forts, nearbyPokemons));

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons)
                .Where(pokemon=>LocationUtils.CalculateDistanceInMeters(pokemon.Latitude, pokemon.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) <= session.Client.GlobalSettings.MapSettings.EncounterRangeMeters * 3)
                .OrderBy(
                    i =>
                        LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                            session.Client.CurrentLongitude,
                            i.Latitude, i.Longitude));

            return pokemons;
        }
        //add delegate event
        public static event PokemonsEncounterDelegate PokemonEncounterEvent;

        private static void OnPokemonEncounterEvent(List<MapPokemon> pokemons)
        {
            PokemonEncounterEvent?.Invoke(pokemons);
        }
    }
}
