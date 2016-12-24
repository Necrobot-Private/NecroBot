using System.Collections.Generic;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.LIB.Event
{
    public class InventoryListEvent : IEvent
    {
        public List<ItemData> Items;
    }
}