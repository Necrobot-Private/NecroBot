using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class UseBerryEvent : IEvent
    {
        public ItemId BerryType;
        public int Count;
    }
}