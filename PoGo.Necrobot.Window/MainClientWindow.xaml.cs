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
using PoGo.NecroBot.Logic.Logging;
using System.Diagnostics;
using TinyIoC;

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
                PlayerInfo = new PlayerInfoModel() { Exp = 0 }
            };

            this.DataContext = datacontext;

        }
           
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private DateTime lastClearLog = DateTime.Now;
        public void LogToConsoleTab(string message, LogLevel level, string color)
        {


            Dictionary<LogLevel, string> colors = new Dictionary<LogLevel, string>()
            {
                { LogLevel.Error, "Red" },
                { LogLevel.Caught, "Green" },
                { LogLevel.Info, "DarkCyan" } ,
                { LogLevel.Warning, "DarkYellow" } ,
                { LogLevel.Pokestop,"Cyan" }  ,
                { LogLevel.Farming,"Magenta" },
                { LogLevel.Sniper,"White" },
                { LogLevel.Recycling,"DarkMagenta" },
                { LogLevel.Flee,"DarkYellow" },
                { LogLevel.Transfer,"DarkGreen" },
                { LogLevel.Evolve,"DarkGreen" },
                { LogLevel.Berry,"DarkYellow" },
                { LogLevel.Egg,"DarkYellow" },
                { LogLevel.Debug,"Gray" },
                { LogLevel.Update,"White" },
                { LogLevel.New,"Green" },
                { LogLevel.SoftBan,"Red" },
                { LogLevel.LevelUp,"Magenta" },
                { LogLevel.Gym,"Magenta" },
                { LogLevel.Service ,"White" }
            };

            if (string.IsNullOrEmpty(color) || color == "Black") color = colors[level];

            this.Invoke(() =>
            {
                if (lastClearLog.AddMinutes(15) < DateTime.Now)
                {
                    consoleLog.Document.Blocks.Clear();
                    lastClearLog = DateTime.Now;
                }
                if (string.IsNullOrEmpty(color) || color == "Black") color = "white";

                consoleLog.AppendText(message + "\r", color);

                consoleLog.ScrollToEnd();
            });
        }

        public void OnBotStartedEventHandler(ISession session, StatisticsAggregator stat)
        {
            this.currentSession = session;

            session.EventDispatcher.EventReceived += HandleBotEvent;
            stat.GetCurrent().DirtyEvent += OnPlayerStatisticChanged;
            this.currentSession = session;
            this.botMap.Session = session;
            this.playerStats = stat;
            this.ctrPokemonInventory.Session = session;
            this.ctrlItemControl.Session = session;
            this.ctrlSniper.Session = session;
            this.ctrlEggsControl.Session = session;
            this.datacontext.PokemonList.Session = session;
            botMap.SetDefaultPosition(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude);
            var accountManager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            gridAccounts.ItemsSource = accountManager.Accounts;
        }

        private void OnPlayerStatisticChanged()
        {
            var stat = this.playerStats.GetCurrent();


            this.datacontext.PlayerInfo.DirtyEventHandle(stat);
        }

        private void PokemonInventory_OnPokemonItemSelected(PokemonDataViewModel selected)
        {
            var numberSelected = datacontext.PokemonList.Pokemons.Count(x => x.IsSelected);
            lblCount.Text = $"Select : {numberSelected}";
        }
        bool isConsoleShowing = false;
        private void menuConsole_Click(object sender, RoutedEventArgs e)
        {
            if (isConsoleShowing)
            {
                consoleMenuText.Text = "Show Console";
                ConsoleHelper.HideConsoleWindow();
            }
            else
            {

                consoleMenuText.Text = "Close Console";
                ConsoleHelper.ShowConsoleWindow();

            }

            isConsoleShowing = !isConsoleShowing;
        }

        private void menuSetting_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new AppConfigWindow(this, System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config\\config.json"));
            configWindow.ShowDialog();
        }

        private void btnHideInfo_Click(object sender, RoutedEventArgs e)
        {
            if (grbPlayerInfo.Height == 35)
            {
                btnHideInfo.Content = "HIDE";
                grbPlayerInfo.Height = 135;
            }
            else
            {
                grbPlayerInfo.Height = 35;
                btnHideInfo.Content = "SHOW";
            }
        }

        private void ChangeThemeTo(string color)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{color}.xaml", UriKind.Absolute);


            var theme = Application.Current.Resources.MergedDictionaries.LastOrDefault();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Remove(theme);

        }

        private void Theme_Selected(object sender, RoutedEventArgs e)
        {
            Popup1.IsOpen = !Popup1.IsOpen;
        }

        private void OnTheme_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            ChangeThemeTo(rad.Content as string);
        }

        private void txtCmdInput_KeyDown(object sender, KeyEventArgs e)
        {

            if(e.Key == Key.Enter)
            {
                Logger.Write(txtCmdInput.Text, LogLevel.Info, ConsoleColor.White);
                txtCmdInput.Text = "";
            }
        }

        private void txtCmdInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtCmdInput.Text = "";
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var addedItem = e.AddedItems[0];
                if (addedItem != null)
                {
                    if (addedItem.GetType() == typeof(TabItem) && ((TabItem)addedItem).Content?.GetType() == typeof(Controls.ItemsInventory))
                    {
                        DataContext dataContext = (DataContext)((TabItem)addedItem).DataContext;
                        dataContext?.ItemsList?.SyncSelectedValues();
                    }
                }
            }
        }

        private void menuAuth_Click(object sender, RoutedEventArgs e)
        {
            popAccounts.IsOpen = true;
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://snipe.necrobot2.com?donate");
        }

        private void btnSwitchAcount_Click(object sender, RoutedEventArgs e)
        {
            var btn = ((Button)sender);
            var account = (MultiAccountManager.BotAccount)btn.CommandParameter ;

            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();

            manager.SwitchAccountTo(account);
        }
    }
}
