namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class Geometry
    {
        public Geo Location { get; set; }
        public string Location_type { get; set; }
        public Bounds Viewport { get; set; }
    }
}