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
        public static async Task<List<FortData>> GetPokeStops(ISession session)
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

        private static bool checkForMoveBackToDefault = true;
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var distanceFromStart = LocationUtils.CalculateDistanceInMeters(
                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                session.Client.CurrentLatitude, session.Client.CurrentLongitude);

            var response = await session.Client.Player.UpdatePlayerLocation(session.Client.CurrentLatitude, session.Client.CurrentLongitude, session.Client.CurrentAltitude, 0);
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
        }
    }
}