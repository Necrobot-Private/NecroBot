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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using PoGo.NecroBot.Logic.Model;
using DotNetBrowser;
using DotNetBrowser.WPF;
using PokemonGo.RocketAPI.Extensions;
using PoGo.Necrobot.Window.Win32;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class MainClientWindow : MetroWindow
    {
        private AccountConfigContext _context = new AccountConfigContext();
        BrowserView webView;

        Timer timer = new Timer();
        private static Dictionary<LogLevel, Tuple<string, string, string>> ConsoleColors = new Dictionary<LogLevel, Tuple<string, string, string>>()
        {
            // Console Colors <Default, Solarized_Dark, Solarized_Light>
                { LogLevel.Error, new Tuple<string, string, string>("Pink", "#FF1E8E", "") },
                { LogLevel.Caught, new Tuple<string, string, string>("Green", "#83FF08", "") },
                { LogLevel.Info, new Tuple<string, string, string>("Green", "#83FF08", "") },
                { LogLevel.Warning, new Tuple<string, string, string>("Orange", "#FF8308", "") },
                { LogLevel.Pokestop, new Tuple<string, string, string>("Cyan", "#B4E1FD", "") },
                { LogLevel.Farming, new Tuple<string, string, string>("Green", "#83FF08", "") },
                { LogLevel.Sniper, new Tuple<string, string, string>("Grey", "#B6B6B6", "") },
                { LogLevel.Recycling, new Tuple<string, string, string>("Grey", "#B6B6B6", "") },
                { LogLevel.Flee, new Tuple<string, string, string>("Purple", "#8308FF", "") },
                { LogLevel.Transfer, new Tuple<string, string, string>("Grey", "#B6B6B6", "") },
                { LogLevel.Evolve, new Tuple<string, string, string>("Cyan", "#B4E1FD", "") },
                { LogLevel.Berry, new Tuple<string, string, string>("Orange", "#FF8308", "") },
                { LogLevel.Egg, new Tuple<string, string, string>("Cyan", "#B4E1FD", "") },
                { LogLevel.Debug, new Tuple<string, string, string>("Grey", "#B6B6B6", "") },
                { LogLevel.Update, new Tuple<string, string, string>("Blue", "#0883FF", "") },
                { LogLevel.New, new Tuple<string, string, string>("Blue", "#0883FF", "") },
                { LogLevel.SoftBan, new Tuple<string, string, string>("Pink", "#FF1E8E", "") },
                { LogLevel.LevelUp, new Tuple<string, string, string>("Blue", "#0883FF", "") },
                { LogLevel.Gym, new Tuple<string, string, string>("Purple", "#8308FF", "") },
                { LogLevel.Service , new Tuple<string, string, string>("Grey", "#B6B6B6", "") }
        };

        private static SolidColorBrush DarkSolarizedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002B36"));
        private static SolidColorBrush LightSolarizedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDF6E3"));
        private static SolidColorBrush SolarizedConsoleWhite = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#657b83"));
        private static SolidColorBrush LightTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
        private static SolidColorBrush DarkTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));

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
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appDir = Path.GetDirectoryName(path);
#if DEBUG
                LoggerProvider.Instance.LoggingEnabled = true;
                LoggerProvider.Instance.FileLoggingEnabled = true;
                string logsDir = Path.Combine(appDir, "Logs");
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);
                LoggerProvider.Instance.OutputFile = Path.Combine(logsDir, $"DotNetBrowser-{DateTime.UtcNow.ToUnixTime()}.log");
#endif
                var uri = new Uri(Path.Combine(appDir, @"PokeEase\index.html"));
                var browser = BrowserFactory.Create(BrowserType.LIGHTWEIGHT);

                webView = new WPFBrowserView(browser);
                browserLayout.Children.Add((UIElement)webView.GetComponent());
                webView.Browser.LoadURL(uri.ToString());
            }
            catch
            {
                NecroBot.Logic.Logging.Logger.Write("DotNetBrowser has encountered an issue, and has been shut down to prevent a crash", LogLevel.Warning);
                Settings.Default.BrowserToggled = false;
                Settings.Default.Save();
                BrowserSync();
            }
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
            MapMode.ItemsSource = new List<string> { "Normal", "Satellite" };
            ConsoleThemer.ItemsSource = new List<string> { "Default", "Low Contrast (Light)", "Low Contrast (Dark)" };
            var LightTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
            var DarkTabColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));

            //=============STARTUP CONFIGURATION=============\\
            if (Settings.Default.ConsoleTheme == "" || Settings.Default.Theme == "" || Settings.Default.Scheme == "" || Settings.Default.SchemeValue == "")
                Settings.Default.Reset();
            
            Theme.SelectedValue = Settings.Default.Theme;
            Scheme.SelectedValue = Settings.Default.Scheme;
            MapMode.SelectedValue = Settings.Default.MapMode;
            ConsoleThemer.SelectedValue = Settings.Default.ConsoleTheme;
            Settings.Default.Save();

            ChangeThemeTo(Settings.Default.Theme);
            ChangeSchemeTo(Settings.Default.Scheme);
            ChangeMapModeTo(Settings.Default.MapMode);
            ChangeConsoleThemeTo(Settings.Default.ConsoleTheme);

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
                if (Theme != Settings.Default.Theme | Scheme != Settings.Default.Scheme & Settings.Default.ResetLayout == true)
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
                if (webView != null)
                {
                    if (webView.Browser != null)
                        webView.Browser.Dispose();
                    webView.Dispose();
                }
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
            Settings.Default.Save();
        }

        public void ResetSync()
        {
            if (Settings.Default.ResetLayout == true)
            {
                Theme.SelectedValue = "Red";
                Scheme.SelectedValue = "Dark";
                MapMode.SelectedValue = "Normal";
                ConsoleThemer.SelectedValue = "Default";
                ChangeThemeTo("Red");
                ChangeSchemeTo("Dark");
                ChangeMapModeTo("Normal");
                ChangeConsoleThemeTo("Default");
                DefaultReset.IsEnabled = false;
            }
            else if (Settings.Default.ResetLayout == false & Settings.Default.Theme == "Red" & Settings.Default.Scheme == "Dark" & Settings.Default.MapMode == "Normal" & Settings.Default.ConsoleTheme == "Default")
                DefaultReset.IsEnabled = false;
            else if (Settings.Default.ResetLayout == false)
                DefaultReset.IsEnabled = true;
        }

        private DateTime lastClearLog = DateTime.Now;
        public void LogToConsoleTab(string message, LogLevel level, string color)
        {
            consoleLog.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Settings.Default.ConsoleTheme == "Low Contrast (Light)")
                    color = ConsoleColors[level].Item3;
                if (Settings.Default.ConsoleTheme == "Low Contrast (Dark)")
                    color = ConsoleColors[level].Item2;
                else if (Settings.Default.ConsoleTheme == "Default" || Settings.Default.ResetLayout == true)
                    color = ConsoleColors[level].Item1;

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
            botMap.GetStyle();
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
            ConsoleSync();
        }

        private void MenuSetting_Click(object sender, RoutedEventArgs e)
        {
            var ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\config.json");
            try
            {
                var configWindow = new SettingsWindow(this, ConfigPath);
                configWindow.ShowDialog();
            }
            catch
            {
                MessageBox.Show($"{ConfigPath} couldn't be found or is Invalid", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            ChangeThemeTo((string)Theme.SelectedValue);
        }

        private void Scheme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ChangeSchemeTo((string)Scheme.SelectedValue);
        }

        private void MapMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeMapModeTo((string)MapMode.SelectedValue);
        }

        private void ConsoleThemer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeConsoleThemeTo((string)ConsoleThemer.SelectedValue);
        }

        private void ChangeMapModeTo(string MapMode)
        {
            if (MapMode == "Normal")
                Settings.Default.MapMode = "Normal";
            if (MapMode == "Satellite")
                Settings.Default.MapMode = "Satellite";
            Settings.Default.Save();
            botMap.GetStyle();
            ResetSync();
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

        private void ChangeConsoleThemeTo(string ConsoleScheme)
        {
            if (ConsoleScheme == "Default")
            {
                Settings.Default.ConsoleTheme = "Default";
                txtCmdInput.Background = Brushes.Black;
                txtCmdInput.Foreground = Brushes.White;
                consoleLog.Background = Brushes.Black;
                ConsoleThemer.Background = Brushes.Black;
                ConsoleThemer.Foreground = Brushes.White;
            }
            if (ConsoleScheme == "Low Contrast (Light)")
            {
                Settings.Default.ConsoleTheme = "Low Contrast (Light)";
                txtCmdInput.Background = LightSolarizedBackground;
                txtCmdInput.Foreground = SolarizedConsoleWhite;
                consoleLog.Background = LightSolarizedBackground;
                ConsoleThemer.Background = LightSolarizedBackground;
                ConsoleThemer.Foreground = SolarizedConsoleWhite;
            }
            if (ConsoleScheme == "Low Contrast (Dark)")
            {
                Settings.Default.ConsoleTheme = "Low Contrast (Dark)";
                txtCmdInput.Background = DarkSolarizedBackground;
                txtCmdInput.Foreground = SolarizedConsoleWhite;
                consoleLog.Background = DarkSolarizedBackground;
                ConsoleThemer.Background = DarkSolarizedBackground;
                ConsoleThemer.Foreground = SolarizedConsoleWhite;
            }
            Settings.Default.Save();
            ResetSync();
        }

        private void ChangeSchemeTo(string Scheme)
        {
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
            }

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Settings.Default.Theme), ThemeManager.GetAppTheme(Settings.Default.SchemeValue));
            Settings.Default.Save();
            ResetSync();
        }

        private void TxtCmdInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NecroBot.Logic.Logging.Logger.Write(txtCmdInput.Text, LogLevel.Info, ConsoleColor.White);
                txtCmdInput.Text = "Enter Your Command";
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
            var account = (Account)btn.CommandParameter;
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
        public void ReInitializeSession(ISession session, GlobalSettings globalSettings, Account requestedAccount = null)
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