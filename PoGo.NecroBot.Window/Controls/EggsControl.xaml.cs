using PoGo.NecroBot.Window.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PoGo.NecroBot.Window.Controls
{
    /// <summary>
    /// Interaction logic for EggsControl.xaml
    /// </summary>
    public partial class EggsControl : UserControl
    {
        public ISession Session{ get; set; }
        public EggsControl()
        {
            InitializeComponent();
        }

        private async void HatchEgg_Click(object sender, RoutedEventArgs e)
        {
            if (lsIncubators.SelectedItem == null)
            {
                MessageBox.Show("Please select an incubator to hatch eggs", "Hatch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var eggId = (ulong)((Button)sender).CommandParameter;
            var incubator = lsIncubators.SelectedItem as IncubatorViewModel;
            await UseIncubatorsTask.Execute(Session, Session.CancellationTokenSource.Token, eggId, incubator.Id);
        }
    }
}