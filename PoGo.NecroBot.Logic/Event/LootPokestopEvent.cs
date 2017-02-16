using PoGo.NecroBot.Logic.Event;
using POGOProtos.Map.Fort;

namespace PoGo.NecroBot.Logic.Event
{
    public class LootPokestopEvent : IEvent
    {
        public FortData Pokestop;
    }
}