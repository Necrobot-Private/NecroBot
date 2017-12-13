using PoGo.NecroBot.Logic.Tasks;

namespace PoGo.NecroBot.Logic.Event.Snipe
{
    public class SnipePokemonUpdateEvent   : IEvent
    {
        public MSniperServiceTask.MSniperInfo2 Data
        {
            get; set;
        }

        public string EncounterId
        {
            get; set;
        }

        public bool IsRemoteEvent { get; set; }
        public SnipePokemonUpdateEvent(string encounterId, bool isRemoteEvent = false)
        {
            EncounterId = encounterId;
            IsRemoteEvent = IsRemoteEvent;
        }

        public SnipePokemonUpdateEvent(string encounterId, bool isRemoteEvent = false, MSniperServiceTask.MSniperInfo2 find = null) : this(encounterId, isRemoteEvent)
        {
            Data = find;
        }
    }
}
