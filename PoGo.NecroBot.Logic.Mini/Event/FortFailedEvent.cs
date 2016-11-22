namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class FortFailedEvent : IEvent
    {
        public bool Looted;
        public int Max;
        public string Name;
        public int Try;
    }
}