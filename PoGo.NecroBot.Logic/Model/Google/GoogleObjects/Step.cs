namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class Step
    {
        public ValueText Distance { get; set; }
        public ValueText Duration { get; set; }
        public Geo End_location { get; set; }
        public string Html_instructions { get; set; }
        public Polyline Polyline { get; set; }
        public Geo Start_location { get; set; }
        public string Travel_mode { get; set; }
        public string Maneuver { get; set; }
    }
}