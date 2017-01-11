using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Inventory.Item;

namespace PoGo.Necrobot.Window.Model
{
    public class ItemsListViewModel : ViewModelBase
    {
        public ObservableCollection<ItemsViewModel> Items { get; set; }
        public int TotalItem { get { return Items.Count; } }
        public ItemsListViewModel()
        {
            this.Items = new ObservableCollection<ItemsViewModel>();
        }
        public void Update(List<ItemData> items)
        {
            foreach (var item in items)
            {
                var existing = this.Items.FirstOrDefault(x => x.ItemId == item.ItemId);
                if (existing == null)
                {
                    this.Items.Add(new ItemsViewModel()
                    {
                        Name = item.ItemId.ToString(),
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
                    existing.DropText = "Drop";
                    existing.RaisePropertyChanged("DropText");

                    existing.AllowDrop = true;
                    existing.RaisePropertyChanged("AllowDrop");

                }
            }
            this.RaisePropertyChanged("TotalItem");
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
            return this.Items.FirstOrDefault(x => x.ItemId == itemId);
        }
    }
}
