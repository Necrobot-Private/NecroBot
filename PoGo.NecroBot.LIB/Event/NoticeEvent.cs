namespace PoGo.NecroBot.LIB.Event
{
    public class NoticeEvent : IEvent
    {
        public string Message = "";

        public override string ToString()
        {
            return Message;
        }
    }
}