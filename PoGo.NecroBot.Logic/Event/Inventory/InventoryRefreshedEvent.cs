using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Event.Inventory
{
    public class InventoryRefreshedEvent :IEvent
    {
        public GetInventoryResponse Inventory { get; set; }

        public InventoryRefreshedEvent(GetInventoryResponse e)
        {
            this.Inventory = e;
        }
    }
}
