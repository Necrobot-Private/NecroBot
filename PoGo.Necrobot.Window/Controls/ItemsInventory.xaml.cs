using System.Windows;
using System.Windows.Controls;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.Tasks;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for ItemsInventory.xaml
    /// </summary>
    public partial class ItemsInventory : UserControl
    {
        public ItemsInventory()
        {
            InitializeComponent();
        }

        public ISession Session { get; internal set; }

        private async void BtnDrop_Click(object sender, RoutedEventArgs e)
        {
            var itemId = (ItemId)((Button)sender).CommandParameter ;

            var data = DataContext as ItemsListViewModel;

            ItemsViewModel Item = data.Get(itemId);

            switch (Item.ItemId)
            {
                case ItemId.ItemLuckyEgg:
                    {
                        if (MessageBox.Show($"Do you want to use {Item.ItemId}", "Use item", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            data.Drop(Item);
                            await UseLuckyEggTask.Execute(Session);
                        }
                    }
                    break;
                case ItemId.ItemIncenseOrdinary:
                    {
                        if (MessageBox.Show($"Do you want to use {Item.ItemId}", "Use item", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            data.Drop(Item);
                            await UseIncenseTask.Execute(Session);
                        }
                    }
                    break;
                default:
                    {
                        if (MessageBox.Show($"Do you want to drop {Item.ItemCount - Item.SelectedValue} {Item.ItemId}", "Drop item", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            data.Drop(Item);
                            await RecycleItemsTask.DropItem(Session, Item.ItemId, Item.ItemCount - Item.SelectedValue);
                        }
                    }
                    break;
            }
        }
    }
}
