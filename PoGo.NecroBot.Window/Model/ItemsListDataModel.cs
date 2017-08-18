using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Window.Model
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
                    int count = 0;
                    string title = "DROP";
                    bool drop = true;
                    if (item.ItemId == ItemId.ItemLuckyEgg
                        || item.ItemId == ItemId.ItemIncenseOrdinary
                        || item.ItemId == ItemId.ItemIncenseCool
                        || item.ItemId == ItemId.ItemIncenseFloral
                        || item.ItemId == ItemId.ItemIncenseSpicy
                        || item.ItemId == ItemId.ItemMoveRerollFastAttack
                        || item.ItemId == ItemId.ItemMoveRerollSpecialAttack
                        || item.ItemId == ItemId.ItemRareCandy)
                    {
                        //count = 1;
                        title = "USE";
                        //drop = false;
                    }
                    else
                    {
                        count = item.Count;
                    }

                    Items.Add(new ItemsViewModel()
                    {
                        Name = item.ItemId.ToString().Replace("Item", "").Replace("Basic", "").Replace("Unlimited", "(∞)").Replace("Ordinary", "").Replace("TroyDisk", "Lure"),
                        ItemId = item.ItemId,
                        ItemCount = item.Count,
                        SelectedValue = count,
                        AllowDrop = drop,
                        DropText = title
                    });

                    foreach (var x in Items)
                    {
                        x.RaisePropertyChanged("DropText");
                        x.RaisePropertyChanged("SelectedValue");
                        x.RaisePropertyChanged("AllowDrop");
                    }
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

                    existing.DropText = existing.DropText;
                    existing.RaisePropertyChanged("DropText");

                    existing.AllowDrop = existing.AllowDrop;
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
