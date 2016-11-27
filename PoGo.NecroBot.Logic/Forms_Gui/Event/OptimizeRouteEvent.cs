using System.Collections.Generic;
using POGOProtos.Map.Fort;

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class OptimizeRouteEvent : IEvent
    {
        public List<FortData> OptimizedRoute { get; set; }
    }
}