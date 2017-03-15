using System;
using PoGo.NecroBot.Logic.Model.Settings;
using System.Device.Location;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public abstract class BaseElevationService : IElevationService
    {
        protected GlobalSettings _settings;
        protected string _apiKey;

        public abstract string GetServiceId();
        public abstract double GetElevationFromWebService(double lat, double lng);

        public BaseElevationService(GlobalSettings settings)
        {
            _settings = settings;
        }
        
        public double GetElevation(double lat, double lng)
        {
            return GetElevationFromWebService(lat, lng);
        }

        public void UpdateElevation(ref GeoCoordinate position)
        {
            double elevation = GetElevation(position.Latitude, position.Longitude);
            // Only update the position elevation if we got a non-zero elevation.
            if (elevation != 0)
            {
                position.Altitude = elevation;
            }
        }

        public static double GetRandomElevation(double elevation)
        {
            // Adds a random elevation to the retrieved one. This was
            // previously set to 5 meters but since it's happening with
            // just a few seconds in between it is deemed unrealistic. 
            // Telling from real world examples ~1.2 meter fits better.
            return elevation + (new Random().NextDouble() * 1.2);
        }
    }
}