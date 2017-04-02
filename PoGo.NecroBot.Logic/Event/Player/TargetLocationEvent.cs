namespace PoGo.NecroBot.Logic.Event.Player
{
    public class TargetLocationEvent : IEvent
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public TargetLocationEvent(double lat, double lng)
        {

            Latitude = lat;

            Longitude = lng;

        }
    }
}
