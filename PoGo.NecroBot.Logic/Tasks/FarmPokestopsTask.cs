#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Model;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class FarmPokestopsTask
    {
        public delegate void LootPokestopDelegate(FortData pokestop);

        private static bool checkForMoveBackToDefault = true;
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var distanceFromStart = LocationUtils.CalculateDistanceInMeters(
                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                session.Client.CurrentLatitude, session.Client.CurrentLongitude);

            var response = await session.Client.Player.UpdatePlayerLocation(session.Client.CurrentLatitude, session.Client.CurrentLongitude, session.Client.CurrentAltitude,0);
            // Edge case for when the client somehow ends up outside the defined radius
            if (session.LogicSettings.MaxTravelDistanceInMeters != 0 && checkForMoveBackToDefault &&
                distanceFromStart > session.LogicSettings.MaxTravelDistanceInMeters)
            {
                checkForMoveBackToDefault = false;
                Logger.Write(
                    session.Translation.GetTranslation(TranslationString.FarmPokestopsOutsideRadius, distanceFromStart),
                    LogLevel.Warning);

                var eggWalker = new EggWalker(1000, session);

                var defaultLocation = new MapLocation(session.Settings.DefaultLatitude,
                    session.Settings.DefaultLongitude,
                    LocationUtils.getElevation(session.ElevationService, session.Settings.DefaultLatitude, session.Settings.DefaultLongitude)
                );

                await session.Navigation.Move(defaultLocation,
                    async () =>
                    {
                        await MSniperServiceTask.Execute(session, cancellationToken);
                    },
                    session,
                    cancellationToken);

                // we have moved this distance, so apply it immediately to the egg walker.
                await eggWalker.ApplyDistance(distanceFromStart, cancellationToken);
            }
            checkForMoveBackToDefault = false;

            await CatchNearbyPokemonsTask.Execute(session, cancellationToken);

            // initialize the variables in UseNearbyPokestopsTask here, as this is a fresh start.
            UseNearbyPokestopsTask.Initialize();
            await UseNearbyPokestopsTask.Execute(session, cancellationToken);

            //add pokeStops to Map
            var pokestopList = await GetPokeStops(session);
            //get optimized route
            var pokeStops = RouteOptimizeUtil.Optimize(pokestopList.ToArray(), session.Client.CurrentLatitude,
                session.Client.CurrentLongitude);

            foreach (var pokeStop in pokeStops)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                FortSearchResponse fortSearch;
                var timesZeroXPawarded = 0;
                var fortTry = 0; //Current check
                const int retryNumber = 50; //How many times it needs to check to clear softban
                const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    fortSearch = await session.Client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                    if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                    if (fortSearch.ExperienceAwarded == 0)
                    {
                        timesZeroXPawarded++;

                        if (timesZeroXPawarded > zeroCheck)
                        {
                            if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                            {
                                break;
                                // Check if successfully looted, if so program can continue as this was "false alarm".
                            }

                            fortTry += 1;

                            session.EventDispatcher.Send(new FortFailedEvent
                            {
                                Name = fortInfo.Name,
                                Try = fortTry,
                                Max = retryNumber - zeroCheck,
                                Looted = false
                            });

                            if (!session.LogicSettings.FastSoftBanBypass)
                            {
                                DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
                            }
                        }
                    }
                    else
                    {
                        if (fortTry != 0)
                        {
                            session.EventDispatcher.Send(new FortFailedEvent
                            {
                                Name = fortInfo.Name,
                                Try = fortTry + 1,
                                Max = retryNumber - zeroCheck,
                                Looted = true
                            });
                        }

                        session.EventDispatcher.Send(new FortUsedEvent
                        {
                            Id = pokeStop.Id,
                            Name = fortInfo.Name,
                            Exp = fortSearch.ExperienceAwarded,
                            Gems = fortSearch.GemsAwarded,
                            Items = StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded),
                            Latitude = pokeStop.Latitude,
                            Longitude = pokeStop.Longitude,
                            InventoryFull = fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull
                        });
                        OnLootPokestopEvent(pokeStop);
                        break; //Continue with program as loot was succesfull.
                    }
                }
                while (fortTry < retryNumber - zeroCheck);
            }
            //end code to add pokestops to map
        }

        private static async Task<List<FortData>> GetPokeStops(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects();

            // Wasn't sure how to make this pretty. Edit as needed.
            var pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        i.Type == FortType.Checkpoint &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        ( // Make sure PokeStop is within max travel distance, unless it's set to 0.
                            LocationUtils.CalculateDistanceInMeters(
                                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                                i.Latitude, i.Longitude) < session.LogicSettings.MaxTravelDistanceInMeters ||
                            session.LogicSettings.MaxTravelDistanceInMeters == 0)
                );
            return pokeStops.ToList();
        }


        private static void OnLootPokestopEvent(FortData pokestop)
    {
        LootPokestopEvent?.Invoke(pokestop);
    }

    public static event LootPokestopDelegate LootPokestopEvent;
    }
}
