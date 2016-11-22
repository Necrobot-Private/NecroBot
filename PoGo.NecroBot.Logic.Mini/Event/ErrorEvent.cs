namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class ErrorEvent : IEvent
    {
        public string Message = "";

        public override string ToString()
        {
            return Message;
        }
    }
}