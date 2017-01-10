using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
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
