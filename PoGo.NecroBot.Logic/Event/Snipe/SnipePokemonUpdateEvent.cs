using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.EncounterId = encounterId;
            this.IsRemoteEvent = IsRemoteEvent;
        }

        public SnipePokemonUpdateEvent(string encounterId, bool isRemoteEvent = false, MSniperServiceTask.MSniperInfo2 find = null) : this(encounterId, isRemoteEvent)
        {
            this.Data = find;
        }
    }
}
