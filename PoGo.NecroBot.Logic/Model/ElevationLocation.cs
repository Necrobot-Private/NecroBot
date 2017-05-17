using Google.Common.Geometry;
using PoGo.NecroBot.Logic.Service.Elevation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model
{
    public class ElevationLocation
    {
        private static int MAX_RETRIES = 5;
        private static int GEOLOCATION_PRECISION = 3;
        
        public long Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public ElevationLocation()
        {
        }

        public ElevationLocation(ulong c)
        {
            var cellId = new S2CellId(c);
            var latlng = cellId.ToLatLng();
            Init(latlng.LatDegrees, latlng.LngDegrees);
        }

        public ElevationLocation(double latitude, double longitude)
        {
            Init(latitude, longitude);
        }

        private void Init(double latitude, double longitude)
        {
            Latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
            Longitude = Math.Round(longitude, GEOLOCATION_PRECISION);
        }
        
        public static async Task<ElevationLocation> FindOrUpdateInDatabase(ElevationConfigContext db, ulong c, IElevationService service)
        {
            var cellId = new S2CellId(c);
            var latlng = cellId.ToLatLng();
            return await FindOrUpdateInDatabase(db, latlng.LatDegrees, latlng.LngDegrees, service).ConfigureAwait(false);
        }

        public static async Task<ElevationLocation> FindOrUpdateInDatabase(ElevationConfigContext db, double latitude, double longitude, IElevationService service)
        {
            latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
            longitude = Math.Round(longitude, GEOLOCATION_PRECISION);

            var elevationLocation = db.ElevationLocation.FirstOrDefault(x => x.Latitude == latitude && x.Longitude == longitude);
                
            if (elevationLocation != null)
                return elevationLocation;
                    
            for (var i = 0; i < MAX_RETRIES; i++)
            {
                try
                {
                    var altitude = await service.GetElevation(latitude, longitude).ConfigureAwait(false);
                    if (altitude == 0 || altitude < -100)
                    {
                        // Invalid altitude
                        return null;
                    }
                    
                    db.ElevationLocation.Add(new ElevationLocation(latitude, longitude)
                    {
                        Altitude = altitude
                    });

                    await db.SaveChangesAsync().ConfigureAwait(false);

                    return elevationLocation;
                }
                catch (Exception)
                {
                    // Just ignore exception and retry after delay
                    await Task.Delay(i * 100).ConfigureAwait(false);
                }
            }

            return null;
        }
    }
}
