using MahApps.Metro.Controls;
using Microsoft.Win32;
using PoGo.NecroBot.Logic.Model.Settings;
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
using System.Windows.Shapes;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using PoGo.Necrobot.Window.Win32;
using PoGo.NecroBot.Logic.Event.Player;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class MainClientWindow : MetroWindow
    {
        
        public MainClientWindow()
        {
            InitializeComponent();

            datacontext = new Model.DataContext()
            {
                PlayerInfo = new PlayerInfoModel() { Exp = 0}
            };

            this.DataContext = datacontext;

        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new OpenFileDialog()
            {
                AddExtension = false,
                DefaultExt = "*.json",
            };
            if (openDlg.ShowDialog() == true)
            {
                string filename = openDlg.FileName;

                MainWindow settingWindow = new MainWindow(this, filename);
                this.Hide();
                settingWindow.Show();
            }
        }
        
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ConsoleHelper.AllocConsole();
            Task.Run(() =>
            {
                NecroBot.CLI.Program.RunBotWithParameters(this.OnBotStartedEventHandler, new string[] { });
            });
        }

        private void OnBotStartedEventHandler(ISession session, StatisticsAggregator stat)
        {
            session.EventDispatcher.EventReceived += HandleBotEvent;
            stat.GetCurrent().DirtyEvent += OnPlayerStatisticChanged;
            this.currentSession = session;
            this.playerStats = stat;
            this.ctrPokemonInventory.Session = session;
            botMap.SetDefaultPosition(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude);
        }

        private void OnPlayerStatisticChanged()
        {
            var stat = this.playerStats.GetCurrent();

            this.datacontext.PlayerInfo.Runtime = this.playerStats.GetCurrent().FormatRuntime();
            this.datacontext.PlayerInfo.EXPPerHour = (int) (stat.TotalExperience / stat.GetRuntime());
            this.datacontext.PlayerInfo.PKMPerHour = (int)(stat.TotalPokemons / stat.GetRuntime());
            this.datacontext.PlayerInfo.TimeToLevelUp = $"{this.playerStats.GetCurrent().StatsExport.HoursUntilLvl:00}h :{this.playerStats.GetCurrent().StatsExport.HoursUntilLvl:00}m";
            this.datacontext.PlayerInfo.Level = this.playerStats.GetCurrent().StatsExport.Level;
            this.datacontext.PlayerInfo.Startdust = this.playerStats.GetCurrent().TotalStardust;
        }

        private void PokemonInventory_OnPokemonItemSelected(PokemonDataViewModel selected)
        {
            var numberSelected = datacontext.PokemonList.Pokemons.Count(x => x.IsSelected);
            lblCount.Text = $"Select : {numberSelected}";
        }
    }
}
