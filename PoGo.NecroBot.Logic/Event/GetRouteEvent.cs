using System.Collections.Generic;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Event
{
    public class GetRouteEvent : IEvent
    {
        public List<GeoCoordinate> Points;
    }
}