using Google.Common.Geometry;
using LiteDB;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model
{
    public class ElevationLocation
    {
        private const string CACHE_DIR = "Cache";
        private const string DB_NAME = CACHE_DIR + "\\elevations.db";
        private static AsyncLock DB_LOCK = new AsyncLock();
        private static int MAX_RETRIES = 5;
        private static int GEOLOCATION_PRECISION = 3;

        [BsonIndex]
        public string Id { get; set; }
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

            Id = GetIdTokenFromLatLng(latitude, longitude);
        }
        
        public static string GetIdTokenFromLatLng(double latitude, double longitude)
        {
            latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
            longitude = Math.Round(longitude, GEOLOCATION_PRECISION);

            //return S2CellId.FromLatLng(S2LatLng.FromDegrees(latitude, longitude)).ParentForLevel(15).ToToken();
            return $"{latitude},{longitude}";
        }

        public static async Task<ElevationLocation> FindOrUpdateInDatabase(ulong c, IElevationService service)
        {
            var cellId = new S2CellId(c);
            var latlng = cellId.ToLatLng();
            return await FindOrUpdateInDatabase(latlng.LatDegrees, latlng.LngDegrees, service).ConfigureAwait(false);
        }

        public static async Task<ElevationLocation> FindOrUpdateInDatabase(double latitude, double longitude, IElevationService service)
        {
            using (await DB_LOCK.LockAsync().ConfigureAwait(false))
            {
                if (!Directory.Exists(CACHE_DIR))
                {
                    Directory.CreateDirectory(CACHE_DIR);
                }

                using (var db = new LiteDatabase(DB_NAME))
                {
                    db.GetCollection<ElevationLocation>("locations").EnsureIndex(s => s.Id);

                    var locationsCollection = db.GetCollection<ElevationLocation>("locations");
                    var id = GetIdTokenFromLatLng(latitude, longitude);
                    var elevationLocation = locationsCollection.FindOne(x => x.Id == id);

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

                            elevationLocation = new ElevationLocation(latitude, longitude)
                            {
                                Altitude = altitude
                            };
                            locationsCollection.Insert(elevationLocation);

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
    }
}
