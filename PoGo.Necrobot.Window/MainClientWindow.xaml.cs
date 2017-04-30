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
        private static Dictionary<LogLevel, string> ConsoleColors_Dark = new Dictionary<LogLevel, string>()
        {
                { LogLevel.Error, "#dc322f" },
                { LogLevel.Caught, "#859900" },
                { LogLevel.Info, "#268bd2" },
                { LogLevel.Warning, "#b58900" },
                { LogLevel.Pokestop, "#2aa198" },
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

        private static Dictionary<LogLevel, string> ConsoleColors_Light = new Dictionary<LogLevel, string>()
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
            Sync();

            datacontext = new DataContext()
            {
                PlayerInfo = new PlayerInfoModel() { Exp = 0 }
            };

            DataContext = datacontext;
            txtCmdInput.Text = TinyIoCContainer.Current.Resolve<UITranslation>().InputCommand;

            Width = Settings.Default.Width;
            Height = Settings.Default.Height;
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
            var LightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
            var DarkColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
            var LightConsole = Brushes.Black;
            var DarkConsole = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002B36"));

            //=============THEME & SCHEME STARTUP CONFIGURATION=============\\
            if (Settings.Default.Theme == "")
            {
                Settings.Default.Theme = "Blue";
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            else if (Settings.Default.Scheme == "")
            {
                Settings.Default.Scheme = "Light";
                Settings.Default.SchemeValue = "BaseLight";
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            if (Settings.Default.Scheme == "Light")
            {
                tabAccounts.Background = LightColor;
                tabBrowser.Background = LightColor;
                tabMap.Background = LightColor;
                tabSniper.Background = LightColor;
                tabConsole.Background = LightColor;
                tabPokemons.Background = LightColor;
                tabItems.Background = LightColor;
                tabEggs.Background = LightColor;
                tabPokemons.Foreground = Brushes.Black;
                tabItems.Foreground = Brushes.Black;

                txtCmdInput.Background = LightConsole;
                consoleLog.Background = LightConsole;
            }
            else if (Settings.Default.Scheme == "Dark")
            {
                tabAccounts.Background = DarkColor;
                tabBrowser.Background = DarkColor;
                tabMap.Background = DarkColor;
                tabSniper.Background = DarkColor;
                tabConsole.Background = DarkColor;
                tabPokemons.Background = DarkColor;
                tabItems.Background = DarkColor;
                tabEggs.Background = DarkColor;
                tabPokemons.Foreground = Brushes.White;
                tabItems.Foreground = Brushes.White;

                txtCmdInput.Background = DarkConsole;
                consoleLog.Background = DarkConsole;
            }
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
            var SchemeValue = "Base" + Settings.Default.Scheme;
            Theme.SelectedValue = Settings.Default.Theme;
            Scheme.SelectedValue = Settings.Default.Scheme;
            Settings.Default.Save();

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Settings.Default.Theme), ThemeManager.GetAppTheme(Settings.Default.SchemeValue));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadHelpArticleAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //=============Console Saving=============\\
            ConsoleWindow();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
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
            });
        }

        public void Sync()
        {
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();
            //=============DotNetBrowser Saving=============\\
            if (Settings.Default.BrowserToggled)
            {
                InitBrowser();
                browserMenuText.Text = translator.DisableHub;
            }
            else if (!Settings.Default.BrowserToggled)
            {
                browserMenuText.Text = translator.EnableHub;
                tabBrowser.IsEnabled = false;
            }
        }

        public void ConsoleWindow()
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
            Settings.Default.ConsoleToggled = !Settings.Default.ConsoleToggled;
            Settings.Default.Save();
        }

        private DateTime lastClearLog = DateTime.Now;
        public void LogToConsoleTab(string message, LogLevel level, string color)
        {
            if (string.IsNullOrEmpty(color) || color == "Black" & Settings.Default.Scheme == "Light")
                color = ConsoleColors_Light[level];
            else if (string.IsNullOrEmpty(color) || color == "Black" & Settings.Default.Scheme == "Dark")
                color = ConsoleColors_Dark[level];

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
            ConsoleWindow();
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
            string Selection = Convert.ToString(Theme.SelectedValue);
            if (Selection == "Red")
                Settings.Default.Theme = "Red";
            if (Selection == "Green")
                Settings.Default.Theme = "Green";
            if (Selection == "Blue")
                Settings.Default.Theme = "Blue";
            if (Selection == "Purple")
                Settings.Default.Theme = "Purple";
            if (Selection == "Orange")
                Settings.Default.Theme = "Orange";
            if (Selection == "Lime")
                Settings.Default.Theme = "Lime";
            if (Selection == "Emerald")
                Settings.Default.Theme = "Emerald";
            if (Selection == "Teal")
                Settings.Default.Theme = "Teal";
            if (Selection == "Cyan")
                Settings.Default.Theme = "Cyan";
            if (Selection == "Cobalt")
                Settings.Default.Theme = "Cobalt";
            if (Selection == "Indigo")
                Settings.Default.Theme = "Indigo";
            if (Selection == "Violet")
                Settings.Default.Theme = "Violet";
            if (Selection == "Pink")
                Settings.Default.Theme = "Pink";
            if (Selection == "Magenta")
                Settings.Default.Theme = "Magenta";
            if (Selection == "Crimson")
                Settings.Default.Theme = "Crimson";
            if (Selection == "Amber")
                Settings.Default.Theme = "Amber";
            if (Selection == "Yellow")
                Settings.Default.Theme = "Yellow";
            if (Selection == "Brown")
                Settings.Default.Theme = "Brown";
            if (Selection == "Olive")
                Settings.Default.Theme = "Olive";
            if (Selection == "Steel")
                Settings.Default.Theme = "Steel";
            if (Selection == "Mauve")
                Settings.Default.Theme = "Mauve";
            if (Selection == "Taupe")
                Settings.Default.Theme = "Taupe";
            if (Selection == "Sienna")
                Settings.Default.Theme = "Sienna";
            Settings.Default.Save();
            tabPokemons.Foreground = Brushes.Black;
            tabItems.Foreground = Brushes.Black;
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
        }

        private void Scheme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string Selection = Convert.ToString(Scheme.SelectedValue);
            var LightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
            var DarkColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
            var LightConsole = Brushes.Black;
            var DarkConsole = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002B36"));

            if (Selection == "Light")
            {
                Settings.Default.Scheme = "Light";
                Settings.Default.SchemeValue = "BaseLight";

                tabAccounts.Background = LightColor;
                tabBrowser.Background = LightColor;
                tabMap.Background = LightColor;
                tabSniper.Background = LightColor;
                tabConsole.Background = LightColor;
                tabPokemons.Background = LightColor;
                tabItems.Background = LightColor;
                tabEggs.Background = LightColor;

                txtCmdInput.Background = LightConsole;
                consoleLog.Background = LightConsole;
            }
            else if (Selection == "Dark")
            {
                Settings.Default.Scheme = "Dark";
                Settings.Default.SchemeValue = "BaseDark";

                tabAccounts.Background = DarkColor;
                tabBrowser.Background = DarkColor;
                tabMap.Background = DarkColor;
                tabSniper.Background = DarkColor;
                tabConsole.Background = DarkColor;
                tabPokemons.Background = DarkColor;
                tabItems.Background = DarkColor;
                tabEggs.Background = DarkColor;

                txtCmdInput.Background = DarkConsole;
                consoleLog.Background = DarkConsole;
            }
            Settings.Default.Save();
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Settings.Default.Theme), ThemeManager.GetAppTheme(Settings.Default.SchemeValue));
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
            var translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            if (Settings.Default.BrowserToggled)
            {
                if (tabBrowser.IsSelected)
                    tabConsole.IsSelected = true;

                tabBrowser.IsEnabled = false;
                browserMenuText.Text = translator.EnableHub;
                webView.Browser.Dispose();
                webView.Dispose();
                Settings.Default.BrowserToggled = false;
            }
            else if (!Settings.Default.BrowserToggled)
            {
                tabBrowser.IsEnabled = true;
                browserMenuText.Text = translator.DisableHub;
                InitBrowser();
                Settings.Default.BrowserToggled = true;
            }
            Settings.Default.Save();
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
            Process.GetCurrentProcess().Kill();
        }
    }
}