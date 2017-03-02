namespace PoGo.NecroBot.Logic.Event
{
    public class ErrorEvent : IEvent
    {
        public string Message = "";
        public bool RequireExit { get; set; }
        public override string ToString()
        {
            return Message;
        }
    }
}