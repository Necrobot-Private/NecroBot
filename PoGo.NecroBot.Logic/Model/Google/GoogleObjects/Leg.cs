namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class Leg
    {
        public ValueText Distance { get; set; }
        public ValueText Duration { get; set; }
        public string End_address { get; set; }
        public Geo End_location { get; set; }
        public string Start_address { get; set; }
        public Geo Start_location { get; set; }
        public Step[] Steps { get; set; }
        public object[] Via_waypoint { get; set; }
    }
}