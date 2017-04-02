using System;
using System.Collections.Generic;
using GeoCoordinatePortable;
using System.Globalization;
using System.Linq;

namespace PoGo.NecroBot.Logic.Model.Google
{
    public class GoogleWalk
    {
        public List<GeoCoordinate> Waypoints { get; set; }
        public double Distance { get; set; }

        public GoogleWalk(GoogleResult googleResult)
        {
            if (googleResult.Directions.Routes == null)
                throw new ArgumentException("Invalid google route.");

            var route = googleResult.Directions.Routes.First();

            Distance = googleResult.GetDistance();

            Waypoints = new List<GeoCoordinate>
            {
                // In some cases, player are inside build
                googleResult.Origin,

                new GeoCoordinate(route.Legs.First().Start_location.Lat, route.Legs.First().Start_location.Lng)
            };
            Waypoints.AddRange(route.Overview_polyline.DecodePoly());
            Waypoints.Add(new GeoCoordinate(route.Legs.Last().End_location.Lat, route.Legs.Last().End_location.Lng));

            // In some cases, player need to get inside a  build
            Waypoints.Add(googleResult.Destiny);
        }

        /// <summary>
        /// Used for test purpose
        /// </summary>
        /// <returns></returns>
        public string GetTextFlyPath() => "[" + string.Join(",", Waypoints.Select(geoCoordinate => $"{{lat: {geoCoordinate.Latitude.ToString(new CultureInfo("en-US"))}, lng: {geoCoordinate.Longitude.ToString(new CultureInfo("en-US"))}}}").ToList()) + "]";

        private GeoCoordinate _lastNextStep;

        public GeoCoordinate NextStep(GeoCoordinate actualLocation)
        {
            if (!Waypoints.Any())
            {
                return _lastNextStep ?? (_lastNextStep = actualLocation);
            }

            do
            {
                _lastNextStep = Waypoints.FirstOrDefault();
                Waypoints.Remove(_lastNextStep);
            } while (actualLocation.GetDistanceTo(_lastNextStep) < 20 || Waypoints.Any());

            return _lastNextStep;
        }

        public static GoogleWalk Get(GoogleResult googleResult)
        {
            return new GoogleWalk(googleResult);
        }
    }
}