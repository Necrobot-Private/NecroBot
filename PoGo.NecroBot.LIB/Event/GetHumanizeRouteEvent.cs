using System.Collections.Generic;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.LIB.Event
{
    public class GetHumanizeRouteEvent : IEvent
    {
        public GeoCoordinate Destination;
        public List<GeoCoordinate> Route;
    }
}