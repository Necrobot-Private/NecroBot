namespace PoGo.NecroBot.Logic.Event.UI
{
    public class StatusBarEvent : IEvent
    {
        public StatusBarEvent(string s)
        {
            Message = s;
        }

        public string Message { get; set; }
    }
}