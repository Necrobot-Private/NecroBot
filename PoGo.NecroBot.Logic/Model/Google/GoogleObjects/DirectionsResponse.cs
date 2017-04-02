namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class DirectionsResponse
    {
        public GeocodedWaypoints[] Geocoded_waypoints { get; set; }
        public Route[] Routes { get; set; }
        public string Status { get; set; }
    }
}