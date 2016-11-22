using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.LIB.Event
{
    public class UseBerryEvent : IEvent
    {
        public ItemId BerryType;
        public int Count;
    }
}