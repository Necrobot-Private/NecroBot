using System.Collections.Generic;
using GeoCoordinatePortable;

namespace PoGo.NecroBot.Logic.Event
{
    public class GetHumanizeRouteEvent : IEvent
    {
        public List<GeoCoordinate> Points;
    }
}