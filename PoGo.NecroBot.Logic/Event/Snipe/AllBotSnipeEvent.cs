namespace PoGo.NecroBot.Logic.Event.Snipe
{
    public class AllBotSnipeEvent : IEvent
    {
        public string EncounterId { get; set; }
        public AllBotSnipeEvent(string encounterId)
        {
            EncounterId = encounterId;
        }
    }
}
