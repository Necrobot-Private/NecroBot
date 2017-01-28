using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic;
using PoGo.Necrobot.Window;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.Necrobot.Window.Converters;
using PoGo.NecroBot.Logic.State;
using PoGo.Necrobot.Window.Model;

namespace PoGo.Necrobot.Window
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
            this.DataContext = dataViewModel;
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
                var x = this.session.Inventory.GetPokeDexItems();
                this.Dispatcher.Invoke(() =>
               {
                   dataViewModel.UpdateWith(x);
               });
            });
        }
    }
}
