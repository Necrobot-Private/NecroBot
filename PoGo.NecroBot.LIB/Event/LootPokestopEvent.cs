using POGOProtos.Map.Fort;

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class LootPokestopEvent : IEvent
    {
        public FortData Pokestop;
    }
}