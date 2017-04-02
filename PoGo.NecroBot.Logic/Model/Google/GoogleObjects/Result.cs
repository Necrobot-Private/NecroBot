namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class Result
    {
        public Address_Components[] Address_components { get; set; }
        public string Formatted_address { get; set; }
        public Geometry Geometry { get; set; }
        public string Place_id { get; set; }
        public string[] Types { get; set; }
    }
}