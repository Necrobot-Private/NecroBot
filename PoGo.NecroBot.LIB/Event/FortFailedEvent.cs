namespace PoGo.NecroBot.LIB.Event
{
    public class FortFailedEvent : IEvent
    {
        public bool Looted;
        public int Max;
        public string Name;
        public int Try;
    }
}