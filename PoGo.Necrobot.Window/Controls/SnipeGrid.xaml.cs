using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PoGo.Necrobot.Window.Controls
{
    public delegate void OnSnipePokemon(SnipePokemonViewModel selected);

    /// <summary>
    /// Interaction logic for SnipeGrid.xaml
    /// </summary>
    public partial class SnipeGrid : UserControl
    {
        public event OnSnipePokemon OnSnipePokemon;

        public SnipeGrid()
        {
            InitializeComponent();
        }
        private void Snipe_Click(object sender, RoutedEventArgs e)
        {
           var model = this.DataContext as ObservableCollection<SnipePokemonViewModel>;
            var button = (Button) sender;

            var b = button.CommandParameter;

            var select = model.FirstOrDefault(x => x.Ref == b);

            if(select!= null)
            {
                OnSnipePokemon?.Invoke(select);
                select.AllowSnipe = false;
                select.RaisePropertyChanged("AllowSnipe");
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();

        }
    }
}
