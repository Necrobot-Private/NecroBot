﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
using DotNetBrowser;
using DotNetBrowser.WPF;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Model.Settings;
using static PoGo.NecroBot.Logic.MultiAccountManager;
using System.Windows.Media.Imaging;
using PoGo.NecroBot.Logic.Event;
using System.Reflection;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class MainClientWindow : MetroWindow
    {
        private static Dictionary<LogLevel, string> ConsoleColors = new Dictionary<LogLevel, string>()
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

        BrowserView webView;

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
                tabBrowser.IsEnabled = false;
            }
        }

        private void InitBrowser()
        {
            webView = new WPFBrowserView(BrowserFactory.Create());
            browserLayout.Children.Add((UIElement)webView.GetComponent());


            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string appDir = Path.GetDirectoryName(path);
            var uri = new Uri(Path.Combine(appDir, @"PokeEase\index.html"));

            webView.Browser.LoadURL(uri.ToString());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadHelpArticleAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Width = Settings.Default.Width;
            Height = Settings.Default.Height;

            if (datacontext.PlayerInfo.Level == currentSession.LogicSettings.LevelLimit) // Warn Player on Reaching this Level
            {
                currentSession.EventDispatcher.Send(new ErrorEvent
                {
                    Message = currentSession.Translation.GetTranslation(TranslationString.LevelLimitReached)
                });
            }
            ChangeThemeTo(Settings.Default.Theme);
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

        private void ChangeThemeTo(string color)
        {
            ResourceDictionary dict = new ResourceDictionary()
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{color}.xaml", UriKind.Absolute)
            };
            var theme = Application.Current.Resources.MergedDictionaries.LastOrDefault();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Remove(theme);

            // THE CODE BELOW IS BUGGED - NEEDS CRUCIAL FIX!

            accountsIMG.Source = new BitmapImage(new Uri(@"pack://application:,,,/Tabs/AccountsIMG_Blue.png", UriKind.Absolute));
            //browserIMG.Source = new BitmapImage(new Uri($"Resources/HubIMG_{color}.png"));
            //mapIMG.Source = new BitmapImage(new Uri($"Resources/MapIMG_{color}.png"));
            //sniperIMG.Source = new BitmapImage(new Uri($"Resources/SniperIMG_{color}.png"));
            //consoleIMG.Source = new BitmapImage(new Uri($"Resources/ConsoleIMG_{color}.png"));
            //pokemonIMG.Source = new BitmapImage(new Uri($"Resources/PokemonIMG_{color}.png"));
            //itemsIMG.Source = new BitmapImage(new Uri($"Resources/ItemsIMG_{color}.png"));
            //eggsIMG.Source = new BitmapImage(new Uri($"Resources/EggsIMG_{color}.png"));

            /*if (color == "Cobalt" || color == "Cyan") // Use Blue Icons
            {
                accountsIMG.Source = new BitmapImage(new Uri("Resources/AccountsIMG_Blue.png"));
                browserIMG.Source = new BitmapImage(new Uri("Resources/HubIMG_Blue.png"));
                mapIMG.Source = new BitmapImage(new Uri("Resources/MapIMG_Blue.png"));
                sniperIMG.Source = new BitmapImage(new Uri("Resources/SniperIMG_Blue.png"));
                consoleIMG.Source = new BitmapImage(new Uri("Resources/ConsoleIMG_Blue.png"));
                pokemonIMG.Source = new BitmapImage(new Uri("Resources/PokemonIMG_Blue.png"));
                itemsIMG.Source = new BitmapImage(new Uri("Resources/ItemsIMG_Blue.png"));
                eggsIMG.Source = new BitmapImage(new Uri("Resources/EggsIMG_Blue.png"));
            }
            else if (color == "Crimson") // Use Red Icons
            {
                accountsIMG.Source = new BitmapImage(new Uri("Resources/AccountsIMG_Red.png"));
                browserIMG.Source = new BitmapImage(new Uri("Resources/HubIMG_Red.png"));
                mapIMG.Source = new BitmapImage(new Uri("Resources/MapIMG_Red.png"));
                sniperIMG.Source = new BitmapImage(new Uri("Resources/SniperIMG_Red.png"));
                consoleIMG.Source = new BitmapImage(new Uri("Resources/ConsoleIMG_Red.png"));
                pokemonIMG.Source = new BitmapImage(new Uri("Resources/PokemonIMG_Red.png"));
                itemsIMG.Source = new BitmapImage(new Uri("Resources/ItemsIMG_Red.png"));
                eggsIMG.Source = new BitmapImage(new Uri("Resources/EggsIMG_Red.png"));
            }
            else if (color == "Emerald" || color == "Lime") // Use Green Icons
            {
                accountsIMG.Source = new BitmapImage(new Uri("Resources/AccountsIMG_Green.png"));
                browserIMG.Source = new BitmapImage(new Uri("Resources/HubIMG_Green.png"));
                mapIMG.Source = new BitmapImage(new Uri("Resources/MapIMG_Green.png"));
                sniperIMG.Source = new BitmapImage(new Uri("Resources/SniperIMG_Green.png"));
                consoleIMG.Source = new BitmapImage(new Uri("Resources/ConsoleIMG_Green.png"));
                itemsIMG.Source = new BitmapImage(new Uri("Resources/ItemsIMG_Green.png"));
                eggsIMG.Source = new BitmapImage(new Uri("Resources/EggsIMG_Green.png"));
            }
            else if (color == "Purple") // Use Indigo Icons
            {
                accountsIMG.Source = new BitmapImage(new Uri("Resources/AccountsIMG_Indigo.png"));
                browserIMG.Source = new BitmapImage(new Uri("Resources/HubIMG_Indigo.png"));
                mapIMG.Source = new BitmapImage(new Uri("Resources/MapIMG_Indigo.png"));
                sniperIMG.Source = new BitmapImage(new Uri("Resources/SniperIMG_Indigo.png"));
                consoleIMG.Source = new BitmapImage(new Uri("Resources/ConsoleIMG_Indigo.png"));
                pokemonIMG.Source = new BitmapImage(new Uri("Resources/PokemonIMG_Indigo.png"));
                itemsIMG.Source = new BitmapImage(new Uri("Resources/ItemsIMG_Indigo.png"));
                eggsIMG.Source = new BitmapImage(new Uri("Resources/EggsIMG_Indigo.png"));
            }
            else if (color =="Pink") // Use Violet Icons
            {
                accountsIMG.Source = new BitmapImage(new Uri("Resources/AccountsIMG_Violet.png"));
                browserIMG.Source = new BitmapImage(new Uri("Resources/HubIMG_Violet.png"));
                mapIMG.Source = new BitmapImage(new Uri("Resources/MapIMG_Violet.png"));
                sniperIMG.Source = new BitmapImage(new Uri("Resources/SniperIMG_Violet.png"));
                consoleIMG.Source = new BitmapImage(new Uri("Resources/ConsoleIMG_Violet.png"));
                pokemonIMG.Source = new BitmapImage(new Uri("Resources/PokemonIMG_Violet.png"));
                itemsIMG.Source = new BitmapImage(new Uri("Resources/ItemsIMG_Violet.png"));
                eggsIMG.Source = new BitmapImage(new Uri("Resources/EggsIMG_Violet.png"));
            }*/

        }

        private void Theme_Selected(object sender, RoutedEventArgs e)
        {
            Popup1.IsOpen = !Popup1.IsOpen;
        }

        private void OnTheme_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            ChangeThemeTo(rad.Content as string);
            Settings.Default.Theme = rad.Content as string;
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

                webView.Browser.Dispose();
                webView.Dispose();
            }
            else if (!Settings.Default.BrowserToggled)
            {
                tabBrowser.IsEnabled = true;
                InitBrowser();
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
            Settings.Default.Width = Width;
            Settings.Default.Height = Height;
            Settings.Default.Save();
            Process.GetCurrentProcess().Kill();
        }
    }
}
