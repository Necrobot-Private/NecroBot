using System.Collections.Generic;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class InventoryListEvent : IEvent
    {
        public List<ItemData> Items;
    }
}