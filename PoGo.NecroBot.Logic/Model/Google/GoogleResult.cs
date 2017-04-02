using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Model.Google.GoogleObjects;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Model.Google
{
    public class GoogleResult
    {
        public DirectionsResponse Directions { get; set; }
        public DateTime RequestDate { get; set; }
        public GeoCoordinate Origin { get; set; }
        public GeoCoordinate Destiny { get; set; }
        public List<GeoCoordinate> Waypoints { get; set; }
        public bool FromCache { get; set; }

        /// <summary>
        /// Google time to reach destiny. If car, consider traffic data.
        /// </summary>
        /// <returns></returns>
        public float TravelTime()
        {
            float tempo = 0;

            foreach (var legs in Directions.Routes.SelectMany(route => route.Legs))
            {
                tempo += legs.Duration.Value;
            }
            return tempo;
        }


        public double GetDistance()
        {
            float distance = 0;

            foreach (var legs in Directions.Routes.SelectMany(route => route.Legs))
            {
                distance += legs.Distance.Value;
            }
            return distance;
        }
    }
}