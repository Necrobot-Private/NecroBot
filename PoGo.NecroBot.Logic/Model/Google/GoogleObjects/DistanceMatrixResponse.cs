namespace PoGo.NecroBot.Logic.Model.Google.GoogleObjects
{
    public class DistanceMatrixResponse
    {
        public string[] Destination_addresses { get; set; }
        public string[] Origin_addresses { get; set; }
        public Row[] Rows { get; set; }
        public string Status { get; set; }
    }
}