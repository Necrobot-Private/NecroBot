using System.Collections.Generic;
using Newtonsoft.Json;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Model.Yours
{
    public class RoutingResponse
    {
        public string Type { get; set; }
        public Crs Crs { get; set; }
        public List<List<double>> Coordinates { get; set; }
        public Properties2 Properties { get; set; }
    }

    public class Properties
    {
        public string Name { get; set; }
    }

    public class Crs
    {
        public string Type { get; set; }
        public Properties Properties { get; set; }
    }

    public class Properties2
    {
        public string Distance { get; set; }
        public string Description { get; set; }
        public string Traveltime { get; set; }
    }

    public class YoursWalk
    {
        public List<GeoCoordinate> Waypoints { get; set; }
        public double Distance { get; set; }

        public YoursWalk(string yoursResponse)
        {
            RoutingResponse yoursResponseParsed = JsonConvert.DeserializeObject<RoutingResponse>(yoursResponse);

            Distance = double.Parse(yoursResponseParsed.Properties.Distance) * 1000;

            Waypoints = new List<GeoCoordinate>();
            foreach (List<double> coordinate in yoursResponseParsed.Coordinates)
            {
                Waypoints.Add(new GeoCoordinate(coordinate.ToArray()[1], coordinate.ToArray()[0]));
            }
        }

        public static YoursWalk Get(string yoursResponse)
        {
            return new YoursWalk(yoursResponse);
        }
    }
}