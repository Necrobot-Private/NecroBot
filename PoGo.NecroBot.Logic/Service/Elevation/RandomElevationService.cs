using Caching;
using GeoCoordinatePortable;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.State;
using System;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class RandomElevationService : BaseElevationService
    {
        private double minElevation = 5;
        private double maxElevation = 50;
        private Random rand = new Random();

        public RandomElevationService(ISession session, LRUCache<string, double> cache) : base(session, cache)
        {
        }

        public override string GetServiceId()
        {
            return "Random Elevation Service (Necrobot Default)";
        }

        public override double GetElevationFromWebService(double lat, double lng)
        {
            return rand.NextDouble() * (maxElevation - minElevation) + minElevation;
        }
    }
}
