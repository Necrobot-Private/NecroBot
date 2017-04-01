using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using POGOProtos.Inventory.Item;

namespace PoGo.Necrobot.Window.Model
{
    public class ItemsListViewModel : ViewModelBase
    {
        public ObservableCollection<ItemsViewModel> Items { get; set; }
        public int TotalItem { get { return Items.Count; } }
        public ItemsListViewModel()
        {
            Items = new ObservableCollection<ItemsViewModel>();
        }

        public void SyncSelectedValues()
        {
            foreach (var item in Items)
            {
                item.SelectedValue = item.ItemCount;
                item.RaisePropertyChanged("SelectedValue");
            }
        }

        public void Update(List<ItemData> items)
        {
            foreach (var item in items)
            {
                var existing = Items.FirstOrDefault(x => x.ItemId == item.ItemId);
                if (existing == null)
                {
                    Items.Add(new ItemsViewModel()
                    {
                        Name = item.ItemId.ToString().Replace("Item", "").Replace("Basic", "").Replace("Unlimited", "(∞)").Replace("Ordinary", "").Replace("TroyDisk", "Lure"),
                        ItemId = item.ItemId,
                        ItemCount = item.Count,
                        SelectedValue = item.Count,
                        AllowDrop = true,
                        DropText = "Drop"
                    });
                }
                else
                {
                    existing.ItemCount = item.Count;
                    existing.RaisePropertyChanged("ItemCount");

                    if (existing.SelectedValue > existing.ItemCount)
                    {
                        existing.SelectedValue = existing.ItemCount;
                        existing.RaisePropertyChanged("SelectedValue");
                    }
                    
                    existing.DropText = "Drop";
                    existing.RaisePropertyChanged("DropText");

                    existing.AllowDrop = true;
                    existing.RaisePropertyChanged("AllowDrop");
                }
            }
            RaisePropertyChanged("TotalItem");
        }

        internal void Drop(ItemsViewModel item)
        {
            item.AllowDrop = false;
            item.DropText = "Recycleing...";
            item.RaisePropertyChanged("AllowDrop");
            item.RaisePropertyChanged("DropText");
        }

        internal ItemsViewModel Get(ItemId itemId)
        {
            return Items.FirstOrDefault(x => x.ItemId == itemId);
        }
    }
}
