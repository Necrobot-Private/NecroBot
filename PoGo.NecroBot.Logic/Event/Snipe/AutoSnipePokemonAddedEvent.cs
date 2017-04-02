namespace PoGo.NecroBot.Logic.Event.Snipe
{
    public class AutoSnipePokemonAddedEvent : IEvent
    {
        public AutoSnipePokemonAddedEvent(EncounteredEvent data)
        {
            EncounteredEvent = data;
        }

        public EncounteredEvent EncounteredEvent { get; set; }
    }
}