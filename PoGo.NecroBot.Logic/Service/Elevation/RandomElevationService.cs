using System;
using PoGo.NecroBot.Logic.Model.Settings;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class RandomElevationService : BaseElevationService
    {
        private double minElevation = 5;
        private double maxElevation = 50;
        private Random rand = new Random();

        public RandomElevationService(GlobalSettings settings) : base(settings)
        {
        }

        public override string GetServiceId()
        {
            return "Random Elevation Service (NecroBot Default)";
        }

// jjskuld - Ignore CS1998 warning for now.
#pragma warning disable 1998
        public override async Task<double> GetElevationFromWebService(double lat, double lng)
        {
            return rand.NextDouble() * (maxElevation - minElevation) + minElevation;
        }
#pragma warning restore 1998
    }
}