using POGOProtos.Data;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Model
{
    public interface IGeoLocation
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
        double Altitude { get; set; }
        string Name { get; set; }
        GeoCoordinate ToGeoCoordinate();
    }

    public class FortLocation : MapLocation
    {
        public FortData FortData { get; set; }
        public FortDetailsResponse FortInfo { get; set; }

        public FortLocation(double lat, double lng, double alt, FortData fortData,
            FortDetailsResponse fortInfo) : base(lat, lng, alt)
        {
            FortData = fortData;

            if (fortInfo != null)
            {
                FortInfo = fortInfo;
                Name = fortInfo.Name;
            }
        }
    }

    public class GPXPointLocation : MapLocation
    {
        public GPXPointLocation(double lat, double lng, double alt) : base(lat, lng, alt)
        {
        }
    }

    public class SnipeLocation : MapLocation
    {
        public SnipeLocation(double lat, double lng, double alt) : base(lat, lng, alt)
        {
        }

        public PokemonData Pokemon { get; set; }
    }

    public class MapLocation : IGeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Altitude { get; set; }
        public string Name { get; set; }

        public MapLocation(double lat, double lng, double alt)
        {
            Latitude = lat;
            Longitude = lng;
            Altitude = alt;
        }

        public GeoCoordinate ToGeoCoordinate()
        {
            return new GeoCoordinate(Latitude, Longitude, Altitude);
        }
    }
}