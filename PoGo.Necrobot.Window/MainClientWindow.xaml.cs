using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MahApps.Metro;
using MahApps.Metro.Controls;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class MainClientWindow : MetroWindow
    {
        Timer timer = new Timer();
        private static Dictionary<LogLevel, string> ConsoleColors_Solarized = new Dictionary<LogLevel, string>()
        {
                { LogLevel.Error, "#DC322F" },
                { LogLevel.Caught, "#859900" },
                { LogLevel.Info, "#CB4B16" } ,
                { LogLevel.Warning, "#B58900" } ,
                { LogLevel.Pokestop, "#2AA198" }  ,
                { LogLevel.Farming, "#D33682" },
                { LogLevel.Sniper, "#657b83" },
                { LogLevel.Recycling, "#6C71C4" },
                { LogLevel.Flee, "#B58900" },
                { LogLevel.Transfer, "#268BD2" },
                { LogLevel.Evolve, "#268BD2" },
                { LogLevel.Berry, "##B58900" },
                { LogLevel.Egg, "#B58900" },
                { LogLevel.Debug, "#93A1A1" },
                { LogLevel.Update, "#657B83" },
                { LogLevel.New, "#859900" },
                { LogLevel.SoftBan, "#DC322F" },
                { LogLevel.LevelUp, "#D33682" },
                { LogLevel.Gym, "#D33682" },
                { LogLevel.Service , "#657b83" }
        };

        private static Dictionary<LogLevel, string> ConsoleColors_Default = new Dictionary<LogLevel, string>()
        {
                { LogLevel.Error, "Red" },
                { LogLevel.Caught, "Green" },
                { LogLevel.Info, "DarkCyan" },
                { LogLevel.Warning, "DarkYellow" },
                { LogLevel.Pokestop, "Cyan" },
                { LogLevel.Farming, "Magenta" },
                { LogLevel.Sniper, "White" },
                { LogLevel.Recycling, "DarkMagenta" },
                { LogLevel.Flee, "DarkYellow" },
                { LogLevel.Transfer, "DarkGreen" },
                { LogLevel.Evolve, "DarkGreen" },
                { LogLevel.Berry, "DarkYellow" },
                { LogLevel.Egg, "DarkYellow" },
                { LogLevel.Debug, "Gray" },
                { LogLevel.Update, "White" },
                { LogLevel.New, "Green" },
                { LogLevel.SoftBan, "Red" },
                { LogLevel.LevelUp, "Magenta" },
                { LogLevel.Gym, "Magenta" },
                { LogLevel.Service , "White" }
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

            Application.Current.MainWindow.Width = Settings.Default.Width;
            Application.Current.MainWindow.Height = Settings.Default.Height;

            BrowserSync();
            ConsoleSync();
            ResetSync();
        }

        private void InitBrowser()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string appDir = Path.GetDirectoryName(path);
            var uri = new Uri(Path.Combine(appDir, @"PokeEase\index.html"));

            webView.URL = uri.ToString();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Upgrade Settings, if Any
            Settings.Default.Upgrade();

            // Timer Timing
            timer.Start();
            timer.Interval = 1;
            timer.Elapsed += TimerTick;

            // Populate ComboBox's w/ Available Themes & Schemes
            Scheme.ItemsSource = new List<string> { "Light", "Dark" };
            Theme.ItemsSource = new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
            var LightTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
            var DarkTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
            var LightConsoleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDF6E3"));
            var DarkConsoleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002B36"));
            var ConsoleWhite = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#657B83"));

            //=============STARTUP CONFIGURATION=============\\
            if (Settings.Default.Theme == "" || Settings.Default.ResetLayout == true)
            {
                Settings.Default.Theme = "Red";
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            else if (Settings.Default.Scheme == "" || Settings.Default.ResetLayout == true)
            {
                Settings.Default.Scheme = "Dark";
                Settings.Default.SchemeValue = "BaseDark";
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            
            Theme.SelectedValue = Settings.Default.Theme;
            Scheme.SelectedValue = Settings.Default.Scheme;
            Settings.Default.Save();

            ChangeThemeTo(Settings.Default.Theme);
            ChangeSchemeTo(Settings.Default.Scheme);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadHelpArticleAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            var Theme = Settings.Default.Theme;
            var Scheme = Settings.Default.Scheme;
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (tabPokemons.IsMouseOver | tabPokemons.IsSelected | Settings.Default.Scheme == "Light")
                    tabPokemons.Foreground = Brushes.Black;
                else if (!tabPokemons.IsMouseOver | !tabPokemons.IsSelected | Settings.Default.Scheme == "Dark")
                    tabPokemons.Foreground = Brushes.White;
                if (tabItems.IsMouseOver | tabItems.IsSelected | Settings.Default.Scheme == "Light")
                    tabItems.Foreground = Brushes.Black;
                else if (!tabItems.IsMouseOver | !tabItems.IsSelected | Settings.Default.Scheme == "Dark")
                    tabItems.Foreground = Brushes.White;
                if (Theme != Settings.Default.Theme & Settings.Default.ResetLayout == true)
                    Settings.Default.ResetLayout = false;
                Settings.Default.Save();
            });
        }

        public void BrowserSync()
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();
            if (Settings.Default.BrowserToggled == false)
            {
                if (tabBrowser.IsSelected)
                    tabConsole.IsSelected = true;

                tabBrowser.IsEnabled = false;
                browserMenuText.Text = translator.EnableHub;
                webView.Browser.Dispose();
                webView.Dispose();
            }
            else if (Settings.Default.BrowserToggled == true)
            {
                tabBrowser.IsEnabled = true;
                browserMenuText.Text = translator.DisableHub;
                InitBrowser();
            }
            Settings.Default.Save();
        }

        public void ConsoleSync()
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();
            if (Settings.Default.ConsoleToggled == true)
            {
                consoleMenuText.Text = translator.HideConsole;
                ConsoleHelper.ShowConsoleWindow();
                Settings.Default.ConsoleText = "Hide Console";
            }
            if (Settings.Default.ConsoleToggled == false)
            {
                consoleMenuText.Text = translator.ShowConsole;
                ConsoleHelper.HideConsoleWindow();
                Settings.Default.ConsoleText = "Show Console";
            }
        }

        public void ResetSync()
        {
            if (Settings.Default.ResetLayout == true)
            {
                Theme.SelectedValue = "Red";
                Scheme.SelectedValue = "Dark";
                ChangeThemeTo("Red");
                ChangeSchemeTo("Dark");
                DefaultReset.IsEnabled = false;
            }
            else if (Settings.Default.ResetLayout == false & Settings.Default.Theme == "Red" & Settings.Default.Scheme == "Dark")
                DefaultReset.IsEnabled = false;
            else if (Settings.Default.ResetLayout == false)
                DefaultReset.IsEnabled = true;
        }

        private DateTime lastClearLog = DateTime.Now;
        public void LogToConsoleTab(string message, LogLevel level, string color)
        {
            if (Settings.Default.ResetLayout == false)
            {
                color = ConsoleColors_Solarized[level];
            }
            else
            {
                // TODO
                color = ConsoleColors_Default[level];
            }

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

        private void MenuConsole_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ConsoleToggled == true)
                Settings.Default.ConsoleToggled = false;
            else if (Settings.Default.ConsoleToggled == false)
                Settings.Default.ConsoleToggled = true;
            Settings.Default.Save();
            Settings.Default.Reload();
            ConsoleSync();
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
                grbPlayerInfo.Height = 120;
            }
            else
            {
                grbPlayerInfo.Height = 35;
                btnHideInfo.Content = translator.Show;
            }
        }

        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ChangeThemeTo(Convert.ToString(Theme.SelectedValue));
        }

        private void Scheme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ChangeSchemeTo(Convert.ToString(Scheme.SelectedValue));
        }

        private void ChangeThemeTo(string Theme)
        {
            if (Theme == "Red")
                Settings.Default.Theme = "Red";
            if (Theme == "Green")
                Settings.Default.Theme = "Green";
            if (Theme == "Blue")
                Settings.Default.Theme = "Blue";
            if (Theme == "Purple")
                Settings.Default.Theme = "Purple";
            if (Theme == "Orange")
                Settings.Default.Theme = "Orange";
            if (Theme == "Lime")
                Settings.Default.Theme = "Lime";
            if (Theme == "Emerald")
                Settings.Default.Theme = "Emerald";
            if (Theme == "Teal")
                Settings.Default.Theme = "Teal";
            if (Theme == "Cyan")
                Settings.Default.Theme = "Cyan";
            if (Theme == "Cobalt")
                Settings.Default.Theme = "Cobalt";
            if (Theme == "Indigo")
                Settings.Default.Theme = "Indigo";
            if (Theme == "Violet")
                Settings.Default.Theme = "Violet";
            if (Theme == "Pink")
                Settings.Default.Theme = "Pink";
            if (Theme == "Magenta")
                Settings.Default.Theme = "Magenta";
            if (Theme == "Crimson")
                Settings.Default.Theme = "Crimson";
            if (Theme == "Amber")
                Settings.Default.Theme = "Amber";
            if (Theme == "Yellow")
                Settings.Default.Theme = "Yellow";
            if (Theme == "Brown")
                Settings.Default.Theme = "Brown";
            if (Theme == "Olive")
                Settings.Default.Theme = "Olive";
            if (Theme == "Steel")
                Settings.Default.Theme = "Steel";
            if (Theme == "Mauve")
                Settings.Default.Theme = "Mauve";
            if (Theme == "Taupe")
                Settings.Default.Theme = "Taupe";
            if (Theme == "Sienna")
                Settings.Default.Theme = "Sienna";
            Settings.Default.Save();
            var AccountsBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Accounts/AccountsIMG_{Settings.Default.Theme}.png"));
            var ConsoleBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Console/ConsoleIMG_{Settings.Default.Theme}.png"));
            var EggsBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Eggs/EggsIMG_{Settings.Default.Theme}.png"));
            var HubBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Hub/HubIMG_{Settings.Default.Theme}.png"));
            var ItemsBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Items/ItemsIMG_{Settings.Default.Theme}.png"));
            var MapBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Map/MapIMG_{Settings.Default.Theme}.png"));
            var PokemonBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Pokemon/PokemonIMG_{Settings.Default.Theme}.png"));
            var SniperBitmap = new BitmapImage(new Uri($"pack://application:,,,/Resources/Sniper/SniperIMG_{Settings.Default.Theme}.png"));
            accountsIMG.Source = AccountsBitmap;
            consoleIMG.Source = ConsoleBitmap;
            eggsIMG.Source = EggsBitmap;
            browserIMG.Source = HubBitmap;
            itemsIMG.Source = ItemsBitmap;
            mapIMG.Source = MapBitmap;
            pokemonIMG.Source = PokemonBitmap;
            sniperIMG.Source = SniperBitmap;

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Settings.Default.Theme), ThemeManager.GetAppTheme(Settings.Default.SchemeValue));
            if (Settings.Default.ResetLayout == true)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Red"), ThemeManager.GetAppTheme("BaseDark"));
                Settings.Default.ResetLayout = false;
            }
            Settings.Default.Save();
            ResetSync();
        }

        private void ChangeSchemeTo(string Scheme)
        {
            var LightTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
            var DarkTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
            var LightConsoleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDF6E3"));
            var DarkConsoleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002B36"));
            var ConsoleWhite = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#657B83"));

            if (Scheme == "Light")
            {
                Settings.Default.Scheme = "Light";
                Settings.Default.SchemeValue = "BaseLight";

                tabAccounts.Background = LightTabColor;
                tabBrowser.Background = LightTabColor;
                tabMap.Background = LightTabColor;
                tabSniper.Background = LightTabColor;
                tabConsole.Background = LightTabColor;
                tabPokemons.Background = LightTabColor;
                tabItems.Background = LightTabColor;
                tabEggs.Background = LightTabColor;

                txtCmdInput.Background = LightConsoleBackground;
                txtCmdInput.Foreground = ConsoleWhite;
                consoleLog.Background = LightConsoleBackground;
            }
            else if (Scheme == "Dark")
            {
                Settings.Default.Scheme = "Dark";
                Settings.Default.SchemeValue = "BaseDark";

                tabAccounts.Background = DarkTabColor;
                tabBrowser.Background = DarkTabColor;
                tabMap.Background = DarkTabColor;
                tabSniper.Background = DarkTabColor;
                tabConsole.Background = DarkTabColor;
                tabPokemons.Background = DarkTabColor;
                tabItems.Background = DarkTabColor;
                tabEggs.Background = DarkTabColor;

                txtCmdInput.Background = DarkConsoleBackground;
                txtCmdInput.Foreground = ConsoleWhite;
                consoleLog.Background = DarkConsoleBackground;
            }

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Settings.Default.Theme), ThemeManager.GetAppTheme(Settings.Default.SchemeValue));
            Settings.Default.ResetLayout = false;
            Settings.Default.Save();
            ResetSync();
        }

        private void TxtCmdInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Logger.Write(txtCmdInput.Text, LogLevel.Info, ConsoleColor.White);
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
            var account = (BotAccount)btn.CommandParameter;
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
                    var responseContent = await client.GetAsync("https://github.com/Necrobot-Private/NecroBot/releases.atom");
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
            if (SystemParameters.PrimaryScreenWidth < 1366)
                WindowState = WindowState.Maximized;
        }

        private void BrowserToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.BrowserToggled == true)
                Settings.Default.BrowserToggled = false;
            else if (Settings.Default.BrowserToggled == false)
                Settings.Default.BrowserToggled = true;
            Settings.Default.Save();
            BrowserSync();
        }
        public void ReInitializeSession(ISession session, GlobalSettings globalSettings, BotAccount requestedAccount = null)
        {
            if (session.LogicSettings.MultipleBotConfig.StartFromDefaultLocation)
                session.ReInitSessionWithNextBot(requestedAccount, globalSettings.LocationConfig.DefaultLatitude, globalSettings.LocationConfig.DefaultLongitude, session.Client.CurrentAltitude);
            else
                session.ReInitSessionWithNextBot(); //current location
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.MainWindow.Height = Settings.Default.Height;
            Application.Current.MainWindow.Width = Settings.Default.Width;
            Settings.Default.Save();
            Process.GetCurrentProcess().Kill();
        }

        private void DefaultReset_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ResetLayout == true)
                Settings.Default.ResetLayout = false;
            else if (Settings.Default.ResetLayout == false)
                Settings.Default.ResetLayout = true;
            Settings.Default.Save();
            Settings.Default.Reload();
            ResetSync();
        }
    }
}