using System.Collections.Generic;
using System.Device.Location;

namespace PoGo.NecroBot.Logic.Event
{
    public class GetHumanizeRouteEvent : IEvent
    {
        public List<GeoCoordinate> Points;
    }
}