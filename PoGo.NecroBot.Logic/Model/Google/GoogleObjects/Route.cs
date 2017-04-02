namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class Route
    {
        public Bounds Bounds { get; set; }
        public string Copyrights { get; set; }
        public Leg[] Legs { get; set; }
        public Polyline Overview_polyline { get; set; }
        public string Summary { get; set; }
        public object[] Warnings { get; set; }
        public object[] Waypoint_order { get; set; }
    }
}