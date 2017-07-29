using MahApps.Metro.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Window.Model;

namespace PoGo.NecroBot.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PokedexWindow : MetroWindow
    {
        private ISession session;
        PokedexItemsViewModel dataViewModel;

        public PokedexWindow()
        {
            InitializeComponent();
        }

        public PokedexWindow(ISession session)
        {
            InitializeComponent();
            this.session = session;
            dataViewModel = new PokedexItemsViewModel();
            DataContext = dataViewModel;
        }

        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            //Task.Run(async () =>
            //{
            //    dataViewModel.UpdateWith(await this.session.Inventory.GetPokeDexItems());
            //});
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                var x = await session.Inventory.GetPokeDexItems();
                Dispatcher.Invoke(() =>
               {
                   dataViewModel.UpdateWith(x);
               });
            });
        }
    }
}
