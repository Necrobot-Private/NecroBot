using PoGo.NecroBot.Window.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PoGo.NecroBot.Window.Controls
{
    public delegate void OnSnipePokemon(SnipePokemonViewModel selected, bool allBotInstanceSnipe);

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
            var button = (Button)sender;

            var b = button.CommandParameter;
            DoSnipe(b, false);
        }

        private void DoSnipe(object b, bool all = false)
        {
            var model = DataContext as ObservableCollection<SnipePokemonViewModel>;

            var select = model.FirstOrDefault(x => x.Ref == b);

            if (select != null)
            {
                OnSnipePokemon?.Invoke(select, false);
                select.AllowSnipe = false;
                select.RaisePropertyChanged("AllowSnipe");
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();

        }

        private void SnipeAll_Click(object sender, RoutedEventArgs e)
        {

            var button = (Button)sender;

            var b = button.CommandParameter;
            DoSnipe(b, true);
        }
    }
}
