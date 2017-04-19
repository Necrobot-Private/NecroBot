using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using PoGo.Necrobot.Window.Properties;
using PoGo.NecroBot.Logic.State;
using PoGo.Necrobot.Window.Win32;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.Logging;
using System.Diagnostics;
using TinyIoC;
using PoGo.NecroBot.Logic.Common;
using System.ServiceModel.Syndication;
using System.Net;
using System.Xml;
using System.IO;
using System.Net.Http;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Model.Settings;
using static PoGo.NecroBot.Logic.MultiAccountManager;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class MainClientWindow : MetroWindow
    {
        private static Dictionary<LogLevel, string> ConsoleColors = new Dictionary<LogLevel, string>()
            {
                { LogLevel.Error, "#dc322f" },
                { LogLevel.Caught, "#859900" },
                { LogLevel.Info, "#268bd2" } ,
                { LogLevel.Warning, "#b58900" } ,
                { LogLevel.Pokestop, "#2aa198" }  ,
                { LogLevel.Farming, "#d33682" },
                { LogLevel.Sniper, "#93a1a1" },
                { LogLevel.Recycling, "#cb4b16" },
                { LogLevel.Flee, "#b58900" },
                { LogLevel.Transfer, "#586e75" },
                { LogLevel.Evolve, "#586e75" },
                { LogLevel.Berry, "#b58900" },
                { LogLevel.Egg, "#b58900" },
                { LogLevel.Debug, "#2aa198" },
                { LogLevel.Update, "#fdf6e3" },
                { LogLevel.New, "#859900" },
                { LogLevel.SoftBan, "#dc322f" },
                { LogLevel.LevelUp, "#d33682" },
                { LogLevel.Gym, "#d33682" },
                { LogLevel.Service , "#fdf6e3" }
            };

        public MainClientWindow()
        {
            InitializeComponent();

            datacontext = new DataContext()
            {
                PlayerInfo = new PlayerInfoModel() { Exp = 0 }
            };

            DataContext = datacontext;
            txtCmdInput.Text = TinyIoCContainer.Current.Resolve<UITranslation>().InputCommand;
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            if (Settings.Default.BrowserToggled)
            {
                InitBrowser();
                browserMenuText.Text = translator.DisableHub;
            }
            else if (!Settings.Default.BrowserToggled)
            {
                browserMenuText.Text = translator.EnableHub;
            }
        }

        private void InitBrowser()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string appDir = Path.GetDirectoryName(path);
            var uri = new Uri(Path.Combine(appDir, @"PokeEase\index.html"));

            webView.URL = uri.ToString();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadHelpArticleAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if (datacontext.PlayerInfo.Level == 35) // Warn Player on Reaching this Level -- NEEDS CONFIG SETTING
            {
                Logger.Write($"You have reached Level {datacontext.PlayerInfo.Level} and it is recommended to Switch Accounts",LogLevel.Warning);
            }
            ChangeThemeTo(Settings.Default.Theme);
            ChangeSchemeTo(Settings.Default.Scheme);
        }
        private DateTime lastClearLog = DateTime.Now;
        public void LogToConsoleTab(string message, LogLevel level, string color)
        { 
            if (string.IsNullOrEmpty(color) || color == "Black")
                color = ConsoleColors[level];

            consoleLog.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (lastClearLog.AddMinutes(15) < DateTime.Now)
                {
                    consoleLog.Document.Blocks.Clear();
                    lastClearLog = DateTime.Now;
                }
                if (string.IsNullOrEmpty(color) || color == "Black") color = "white";

                consoleLog.AppendText(message + "\r", color);

                consoleLog.ScrollToEnd();
            }));
        }

        public void OnBotStartedEventHandler(ISession session, StatisticsAggregator stat)
        {
            currentSession = session;

            session.EventDispatcher.EventReceived += HandleBotEvent;
            stat.GetCurrent().DirtyEvent += OnPlayerStatisticChanged;
            currentSession = session;
            botMap.Session = session;
            playerStats = stat;
            ctrPokemonInventory.Session = session;
            ctrlItemControl.Session = session;
            ctrlSniper.Session = session;
            ctrlEggsControl.Session = session;
            datacontext.PokemonList.Session = session;
            botMap.SetDefaultPosition(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude);
            var accountManager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            gridAccounts.ItemsSource = accountManager.Accounts;
        }

        private void OnPlayerStatisticChanged()
        {
            var stat = playerStats.GetCurrent();
            datacontext.PlayerInfo.DirtyEventHandle(stat);
        }
        private void PokemonInventory_OnPokemonItemSelected(PokemonDataViewModel selected)
        {
            var numberSelected = datacontext.PokemonList.Pokemons.Count(x => x.IsSelected);
            lblCount.Text = $"Select : {numberSelected}";
        }
        bool isConsoleShowing = false;
        private void MenuConsole_Click(object sender, RoutedEventArgs e)
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            if (isConsoleShowing)
            {
                consoleMenuText.Text = translator.ShowConsole; 
                ConsoleHelper.HideConsoleWindow();
            }
            else
            {
                consoleMenuText.Text = translator.HideConsole;
                ConsoleHelper.ShowConsoleWindow();
            }

            isConsoleShowing = !isConsoleShowing;
        }

        private void MenuSetting_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new SettingsWindow(this, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\config.json"));
            configWindow.ShowDialog();         
        }

        private void BtnHideInfo_Click(object sender, RoutedEventArgs e)
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            if (grbPlayerInfo.Height == 35)
            {
                btnHideInfo.Content = translator.Hide;
                grbPlayerInfo.Height = 135;
            }
            else
            {
                grbPlayerInfo.Height = 35;
                btnHideInfo.Content = translator.Show;
            }
        }

        private void ChangeThemeTo(string Theme)
        {
            ResourceDictionary dict = new ResourceDictionary()
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{Theme}.xaml", UriKind.Absolute)
            };
            var theme = Application.Current.Resources.MergedDictionaries.LastOrDefault();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Remove(theme);

            if (Settings.Default.Scheme != "BaseLight") // If Not Equivalent to Default
            {
                ChangeSchemeTo_KeepTheme(Settings.Default.Scheme);
            }
        }

        private void ChangeThemeTo_KeepScheme(string Theme)
        {
            ResourceDictionary dict = new ResourceDictionary()
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{Theme}.xaml", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void ChangeSchemeTo(string Scheme)
        {
            ResourceDictionary dict = new ResourceDictionary()
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{Scheme}.xaml", UriKind.Absolute)
            };
            var scheme = Application.Current.Resources.MergedDictionaries.LastOrDefault();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Remove(scheme);

            if (Settings.Default.Theme != "Blue") // If not Equivalent to Default
            {
                ChangeThemeTo_KeepScheme(Settings.Default.Theme);
            }
        }

        private void ChangeSchemeTo_KeepTheme(string Scheme)
        {
            ResourceDictionary dict = new ResourceDictionary()
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{Scheme}.xaml", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void Theme_Selected(object sender, RoutedEventArgs e)
        {
            Popup1.IsOpen = !Popup1.IsOpen;
        }

        private void Scheme_Selected(object sender, RoutedEventArgs e)
        {
            Popup2.IsOpen = !Popup2.IsOpen;
        }

        private void OnTheme_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            ChangeThemeTo(rad.Content as string);
            Settings.Default.Theme = rad.Content as string;
            Settings.Default.Save();
        }

        private void OnScheme_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            var Scheme = rad.Content as string;
            if (Scheme == "Light")
            {
                ChangeSchemeTo("BaseLight");
                Settings.Default.Scheme = "BaseLight";
            }
            if (Scheme == "Dark")
            {
                ChangeSchemeTo("BaseDark");
                Settings.Default.Scheme = "BaseDark";
            }
            Settings.Default.Save();
        }

        private void TxtCmdInput_KeyDown(object sender, KeyEventArgs e)
        {

            if(e.Key == Key.Enter)
            {
                NecroBot.Logic.Logging.Logger.Write(txtCmdInput.Text, LogLevel.Info, ConsoleColor.White);
                txtCmdInput.Text = "";
            }
        }

        private void TxtCmdInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtCmdInput.Text = "";
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        
        private void BtnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://snipe.necrobot2.com?donate");
        }

        private void BtnSwitchAcount_Click(object sender, RoutedEventArgs e)
        {
            var btn = ((Button)sender);
            var account = (MultiAccountManager.BotAccount)btn.CommandParameter;

            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();

            manager.SwitchAccountTo(account);
        }

        DateTime lastTimeLoadHelp = DateTime.MinValue;
        private async Task LoadHelpArticleAsync()
        {
            if (lastTimeLoadHelp < DateTime.Now.AddMinutes(-30))
            {
                using (HttpClient client = new HttpClient())
                {
                    var responseContent = await client.GetAsync("http://necrobot2.com/feed.xml");
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                        return;

                    var xml = await responseContent.Content.ReadAsStringAsync();
                    
                    var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(xml)));
                    lastTimeLoadHelp = DateTime.Now;

                    Dispatcher.Invoke(() =>
                    {
                        lsvHelps.ItemsSource = feed.Items.OrderByDescending(x => x.PublishDate);
                    });
                }
            }
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            popHelpArticles.IsOpen = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadHelpArticleAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hlink = sender as Hyperlink;
            
            Process.Start(hlink.NavigateUri.ToString());
            popHelpArticles.IsOpen = false;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            if(SystemParameters.PrimaryScreenWidth<1366)
                WindowState = WindowState.Maximized;
        }

        private void BrowserToggle_Click(object sender, RoutedEventArgs e)
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            if (Settings.Default.BrowserToggled)
            {
                if (tabBrowser.IsSelected)
                    tabConsole.IsSelected = true;

                tabBrowser.IsEnabled = false;
                browserMenuText.Text = translator.EnableHub;
                Settings.Default.BrowserToggled = false;
                Settings.Default.Save();

                MessageBoxResult msgbox = MessageBox.Show("Would you Like to Restart to kill unneccesary browser tasks and free up extra cpu?","Free Up CPU",MessageBoxButton.YesNo,MessageBoxImage.Question);
                if (msgbox == MessageBoxResult.Yes)
                {
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                else
                { }
            }
            else if (!Settings.Default.BrowserToggled)
            {
                tabBrowser.IsEnabled = true;
                browserMenuText.Text = translator.DisableHub;
                Settings.Default.BrowserToggled = true;
                Settings.Default.Save();
            }
        }
        public void ReInitializeSession(ISession session, GlobalSettings globalSettings, BotAccount requestedAccount = null)
        {
            if (session.LogicSettings.MultipleBotConfig.StartFromDefaultLocation)
            {
                session.ReInitSessionWithNextBot(requestedAccount, globalSettings.LocationConfig.DefaultLatitude, globalSettings.LocationConfig.DefaultLongitude, session.Client.CurrentAltitude);
            }
            else
            {
                session.ReInitSessionWithNextBot(); //current location
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
