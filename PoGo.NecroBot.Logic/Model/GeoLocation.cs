using Geocoding.Google;
using Google.Common.Geometry;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model
{
    public class GeoLocation
    {
        private static int GEOCODING_MAX_RETRIES = 5;
        private static int GEOLOCATION_PRECISION = 3;
        private static long BlacklistTimestamp = DateTime.UtcNow.ToUnixTime();
        
        public long Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public string Locality { get; set; }    // City/Town
        public string AdminLevel1 { get; set; } // State
        public string AdminLevel2 { get; set; } // County
        public string AdminLevel3 { get; set; } // City/Town

        public GeoLocation()
        {
        }

        public GeoLocation(ulong capturedCellId)
        {
            var cellId = new S2CellId(capturedCellId);
            var latlng = cellId.ToLatLng();
            Init(latlng.LatDegrees, latlng.LngDegrees);
        }

        public GeoLocation(double latitude, double longitude)
        {
            Init(latitude, longitude);
        }

        private void Init(double latitude, double longitude)
        {
            Latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
            Longitude = Math.Round(longitude, GEOLOCATION_PRECISION);
        }

        public async Task ReverseGeocode()
        {
            GoogleGeocoder geocoder = new GoogleGeocoder();
            var addresses = await geocoder.ReverseGeocodeAsync(Latitude, Longitude).ConfigureAwait(false);
            GoogleAddress addr = addresses.Where(a => !a.IsPartialMatch).FirstOrDefault();

            if (addr != null)
            {
                if (addr[GoogleAddressType.Country] != null)
                    Country = addr[GoogleAddressType.Country].LongName;
                if (addr[GoogleAddressType.Locality] != null)
                    Locality = addr[GoogleAddressType.Locality].LongName;
                if (addr[GoogleAddressType.AdministrativeAreaLevel1] != null)
                    AdminLevel1 = addr[GoogleAddressType.AdministrativeAreaLevel1].LongName;
                if (addr[GoogleAddressType.AdministrativeAreaLevel2] != null)
                    AdminLevel2 = addr[GoogleAddressType.AdministrativeAreaLevel2].LongName;
                if (addr[GoogleAddressType.AdministrativeAreaLevel3] != null)
                    AdminLevel3 = addr[GoogleAddressType.AdministrativeAreaLevel3].LongName;
            }
        }

        public static string GetIdTokenFromLatLng(double latitude, double longitude)
        {
            latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
            longitude = Math.Round(longitude, GEOLOCATION_PRECISION);

            //return S2CellId.FromLatLng(S2LatLng.FromDegrees(latitude, longitude)).ParentForLevel(15).ToToken();
            return $"{latitude},{longitude}";
        }

        public static async Task<GeoLocation> FindOrUpdateInDatabase(ulong capturedCellId)
        {
            var cellId = new S2CellId(capturedCellId);
            var latlng = cellId.ToLatLng();
            return await FindOrUpdateInDatabase(latlng.LatDegrees, latlng.LngDegrees).ConfigureAwait(false);
        }

        public static async Task<GeoLocation> FindOrUpdateInDatabase(double latitude, double longitude)
        {
            if (BlacklistTimestamp > DateTime.UtcNow.ToUnixTime())
                return null;

            using (var db = new GeoLocationConfigContext())
            {
                latitude = Math.Round(latitude, GEOLOCATION_PRECISION);
                longitude = Math.Round(longitude, GEOLOCATION_PRECISION);

                var geoLocation = db.GeoLocation.Where(x => x.Latitude == latitude && x.Longitude == longitude).FirstOrDefault();

                if (geoLocation != null)
                    return geoLocation;

                geoLocation = new GeoLocation(latitude, longitude);
                
                for (var i = 0; i < GEOCODING_MAX_RETRIES; i++)
                {
                    try
                    {
                        await geoLocation.ReverseGeocode().ConfigureAwait(false);
                        break;
                    }
                    catch (GoogleGeocodingException e)
                    {
                        if (e.Status == GoogleStatus.OverQueryLimit)
                        {
                            BlackList();
                            return null;
                        }

                        // Just ignore exception and retry after delay
                        await Task.Delay(i * 100).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (i == GEOCODING_MAX_RETRIES - 1)
                        {
                            BlackList();
                            return null;
                        }

                        // Just ignore exception and retry after delay
                        await Task.Delay(i * 100).ConfigureAwait(false);
                    }
                }

                // Before we store it to the database, ensure it must at least have country field set.
                if (string.IsNullOrEmpty(geoLocation.Country))
                    return null;

                db.GeoLocation.Add(geoLocation);
                await db.SaveChangesAsync().ConfigureAwait(false);
                return geoLocation;
            }
        }

        private static void BlackList()
        {
            var now = DateTime.UtcNow.ToUnixTime();
            if (BlacklistTimestamp < now)
                BlacklistTimestamp = now + 60 * 1000 * 60; // 1 hour blacklist
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Country))
            {
                if (!string.IsNullOrEmpty(Locality))
                    return $"{Locality}, {Country}";
                if (!string.IsNullOrEmpty(AdminLevel3))
                    return $"{AdminLevel3}, {Country}";
                if (!string.IsNullOrEmpty(AdminLevel2))
                    return $"{AdminLevel2}, {Country}";
                if (!string.IsNullOrEmpty(AdminLevel1))
                    return $"{AdminLevel1}, {Country}";
                return Country;
            }
            return $"{Latitude}, {Longitude}";
        }
    }
}
