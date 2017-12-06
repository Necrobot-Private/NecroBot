using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Strategies.Walk
{
    class FlyStrategy : BaseWalkStrategy
    {
        public FlyStrategy(Client client) : base(client)
        {
        }

        public override string RouteName => "NecroBot Flying";


        public override async Task Walk(IGeoLocation targetLocation,
            Func<Task> functionExecutedWhileWalking, ISession session, CancellationToken cancellationToken,
            double walkSpeed = 0.0)
        {
            var curLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            var destinaionCoordinate = new GeoCoordinate(targetLocation.Latitude, targetLocation.Longitude);

            var dist = LocationUtils.CalculateDistanceInMeters(curLocation, destinaionCoordinate);
            if (dist >= 100)
            {
                var nextWaypointDistance = dist * 70 / 100;
                var nextWaypointBearing = LocationUtils.DegreeBearing(curLocation, destinaionCoordinate);

                var waypoint = await LocationUtils.CreateWaypoint(curLocation, nextWaypointDistance, nextWaypointBearing).ConfigureAwait(false);
                var sentTime = DateTime.Now;

                // We are setting speed to 0, so it will be randomly generated speed.
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, waypoint, 0).ConfigureAwait(false);
                base.DoUpdatePositionEvent(session, waypoint.Latitude, waypoint.Longitude, walkSpeed,0);

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                    var millisecondsUntilGetUpdatePlayerLocationResponse =
                        (DateTime.Now - sentTime).TotalMilliseconds;

                    curLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                    var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(curLocation, destinaionCoordinate);

                    dist = LocationUtils.CalculateDistanceInMeters(curLocation, destinaionCoordinate);

                    if (dist >= 100)
                        nextWaypointDistance = dist * 70 / 100;
                    else
                        nextWaypointDistance = dist;

                    nextWaypointBearing = LocationUtils.DegreeBearing(curLocation, destinaionCoordinate);
                    waypoint = await LocationUtils.CreateWaypoint(curLocation, nextWaypointDistance, nextWaypointBearing).ConfigureAwait(false);
                    sentTime = DateTime.Now;
                    // We are setting speed to 0, so it will be randomly generated speed.
                    await LocationUtils.UpdatePlayerLocationWithAltitude(session, waypoint, 0).ConfigureAwait(false);
                    base.DoUpdatePositionEvent(session, waypoint.Latitude, waypoint.Longitude, walkSpeed);


                    if (functionExecutedWhileWalking != null)
                        await functionExecutedWhileWalking().ConfigureAwait(false); // look for pokemon
                } while (LocationUtils.CalculateDistanceInMeters(curLocation, destinaionCoordinate) >= 10);
            }
            else
            {
                // We are setting speed to 0, so it will be randomly generated speed.
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, targetLocation.ToGeoCoordinate(), 0).ConfigureAwait(false);
                base.DoUpdatePositionEvent(session, targetLocation.Latitude, targetLocation.Longitude,walkSpeed);
                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking().ConfigureAwait(false); // look for pokemon
            }
        }

        public override double CalculateDistance(double sourceLat, double sourceLng, double destinationLat,
            double destinationLng, ISession session = null)
        {
            return LocationUtils.CalculateDistanceInMeters(sourceLat, sourceLng, destinationLat, destinationLng);
        }
    }
}