using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using PoGo.NecroBot.Logic.State;
using PoGo.Necrobot.Window.Win32;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Model.Settings;
using System.IO;
using PoGo.NecroBot.Logic.Common;
using TinyIoC;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.ErrorHandler);
        }

        
        private void ErrorHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject.ToString());
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);
        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            
            base.OnLoadCompleted(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string languageCode = "en";

            if (File.Exists("config\\config.json"))
            {
                var config = GlobalSettings.Load("config",false);
                languageCode = config.ConsoleConfig.TranslationLanguageCode;
            }

            if (!System.IO.Directory.Exists(@"config\Translations"))
            {
                System.IO.Directory.CreateDirectory(@"config\Translations");
            }

            var uiTranslation = new UITranslation(languageCode);
            TinyIoCContainer.Current.Register<UITranslation>(uiTranslation);

            MainWindow = new MainClientWindow();


            ConsoleHelper.AllocConsole();
            UILogger logger = new UILogger();
            logger.LogToUI = ((MainClientWindow) MainWindow).LogToConsoleTab;

            Logger.AddLogger(logger, string.Empty);

            Task.Run(() =>
            {
                NecroBot.CLI.Program.RunBotWithParameters(OnBotStartedEventHandler, new string[] { });
            });
            ConsoleHelper.HideConsoleWindow();
            MainWindow.Show();
        }
        public void OnBotStartedEventHandler(ISession session, StatisticsAggregator stat)
        {
            this.Dispatcher.Invoke(() =>
            {
                var main = MainWindow as MainClientWindow;
                main.OnBotStartedEventHandler(session, stat);
            });
        }
    }
}
