using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Event.Snipe
{
    public class AllBotSnipeEvent : IEvent
    {
        public string EncounterId { get; set; }
        public AllBotSnipeEvent(string encounterId)
        {
            this.EncounterId = encounterId;
        }
    }
}
