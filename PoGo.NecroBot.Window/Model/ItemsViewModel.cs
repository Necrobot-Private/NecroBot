using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Window.Model
{
    public class ItemsViewModel  :ViewModelBase
    {
        public ItemId ItemId { get; set; }

        public string Name { get; set; }
        public int SelectedValue { get; set; }
        public int ItemCount { get; set; }

        public bool AllowDrop { get; set; }

        public string DropText { get; set; }

    }
}
