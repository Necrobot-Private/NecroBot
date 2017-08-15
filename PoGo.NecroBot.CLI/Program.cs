#region using directives

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using PoGo.NecroBot.CLI.CommandLineUtility;
using PoGo.NecroBot.CLI.Forms;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Service;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using ProgressBar = PoGo.NecroBot.CLI.Resources.ProgressBar;
using CommandLine;
using CommandLine.Text;
using PokemonGo.RocketAPI;
using System.Net.Http;
using POGOProtos.Networking.Responses;

#endregion using directives

namespace PoGo.NecroBot.CLI
{
    class Options
    {
        [Option('i', "init", Required = false, HelpText = "Init account")]
        public bool Init { get; set; }

        [Option('t', "template", DefaultValue = "", Required = false, HelpText = "Prints all messages to standard output.")]
        public string Template { get; set; }

        [Option('p', "password", DefaultValue = "", Required = false, HelpText = "Password")]
        public string Password { get; set; }

        [Option('g', "google", DefaultValue = false, Required = false, HelpText = "is google account")]
        public bool IsGoogle { get; set; }

        [Option('s', "start", DefaultValue = 1, HelpText = "Start account", Required = false)]
        public int Start { get; set; }

        [Option('e', "end", DefaultValue = 10, HelpText = "End account", Required = false)]
        public int End { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(HelpText.AutoBuild(this,
               (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current)));
            Console.ReadKey();
            Console.ResetColor();
            Environment.Exit(0);
            return null;
        }
    }

    public class Program
    {
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        private static string _subPath = "";

        private static bool _enableJsonValidation = true;
        private static bool _ignoreKillSwitch;

        private static readonly Uri StrKillSwitchUri =
            new Uri("https://raw.githubusercontent.com/NecroBot-Private/NecroBot/master/KillSwitch.txt");

        private static readonly Uri StrMasterKillSwitchUri =
            new Uri("https://raw.githubusercontent.com/NecroBot-Private/NecroBot/master/PoGo.NecroBot.Logic/MKS.txt");

        private static Session _session;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            RunBotWithParameters(null, args);
        }

        public static void RunBotWithParameters(Action<ISession, StatisticsAggregator> onBotStarted, string[] args)
        {
            var ioc = TinyIoC.TinyIoCContainer.Current;
            //Setup Logger for API
            APIConfiguration.Logger = new APILogListener();

            //Application.EnableVisualStyles();
            var strCulture = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

            var culture = CultureInfo.CreateSpecificCulture("en");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;

            Console.Title = @"NecroBot2 Loading";
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                QuitEvent.Set();
                eArgs.Cancel = true;
            };

            // Command line parsing
            var commandLine = new Arguments(args);
            // Look for specific arguments values
            if (commandLine["subpath"] != null && commandLine["subpath"].Length > 0)
            {
                _subPath = commandLine["subpath"];
            }
            if (commandLine["jsonvalid"] != null && commandLine["jsonvalid"].Length > 0)
            {
                switch (commandLine["jsonvalid"])
                {
                    case "true":
                        _enableJsonValidation = true;
                        break;

                    case "false":
                        _enableJsonValidation = false;
                        break;
                }
            }
            if (commandLine["killswitch"] != null && commandLine["killswitch"].Length > 0)
            {
                switch (commandLine["killswitch"])
                {
                    case "true":
                        _ignoreKillSwitch = false;
                        break;

                    case "false":
                        _ignoreKillSwitch = true;
                        break;
                }
            }

            bool excelConfigAllow = false;
            if (commandLine["provider"] != null && commandLine["provider"] == "excel")
            {
                excelConfigAllow = true;
            }

            //
            Logger.AddLogger(new ConsoleLogger(LogLevel.Service), _subPath);
            Logger.AddLogger(new FileLogger(LogLevel.Service), _subPath);
            Logger.AddLogger(new WebSocketLogger(LogLevel.Service), _subPath);

            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), _subPath);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");
            var excelConfigFile = Path.Combine(profileConfigPath, "config.xlsm");

            GlobalSettings settings;
            var boolNeedsSetup = false;

            if (File.Exists(configFile))
            {
                // Load the settings from the config file
                settings = GlobalSettings.Load(_subPath, _enableJsonValidation);
                if (excelConfigAllow)
                {
                    if (!File.Exists(excelConfigFile))
                    {
                        Logger.Write(
                            "Migrating existing json confix to excel config, please check the config.xlsm in your config folder"
                        );

                        ExcelConfigHelper.MigrateFromObject(settings, excelConfigFile);
                    }
                    else
                        settings = ExcelConfigHelper.ReadExcel(settings, excelConfigFile);

                    Logger.Write("Bot will run with your excel config, loading excel config");
                }
            }
            else
            {
                settings = new GlobalSettings
                {
                    ProfilePath = profilePath,
                    ProfileConfigPath = profileConfigPath,
                    GeneralConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config"),
                    ConsoleConfig = { TranslationLanguageCode = strCulture }
                };

                boolNeedsSetup = true;
            }
            if (commandLine["latlng"] != null && commandLine["latlng"].Length > 0)
            {
                var crds = commandLine["latlng"].Split(',');
                try
                {
                    var lat = double.Parse(crds[0]);
                    var lng = double.Parse(crds[1]);
                    settings.LocationConfig.DefaultLatitude = lat;
                    settings.LocationConfig.DefaultLongitude = lng;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                if (options.Init)
                {
                    settings.GenerateAccount(options.IsGoogle, options.Template, options.Start, options.End, options.Password);
                }
            }
            var lastPosFile = Path.Combine(profileConfigPath, "LastPos.ini");
            if (File.Exists(lastPosFile) && settings.LocationConfig.StartFromLastPosition)
            {
                var text = File.ReadAllText(lastPosFile);
                var crds = text.Split(':');
                try
                {
                    var lat = double.Parse(crds[0]);
                    var lng = double.Parse(crds[1]);
                    //If lastcoord is snipe coord, bot start from default location

                    if (LocationUtils.CalculateDistanceInMeters(lat, lng, settings.LocationConfig.DefaultLatitude, settings.LocationConfig.DefaultLongitude) < 2000)
                    {
                        settings.LocationConfig.DefaultLatitude = lat;
                        settings.LocationConfig.DefaultLongitude = lng;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (!_ignoreKillSwitch)
            {
                if (CheckMKillSwitch() || CheckKillSwitch())
                    return;
            }

            var logicSettings = new LogicSettings(settings);
            var translation = Translation.Load(logicSettings);
            TinyIoC.TinyIoCContainer.Current.Register<ITranslation>(translation);

            if (settings.GPXConfig.UseGpxPathing)
            {
                var xmlString = File.ReadAllText(settings.GPXConfig.GpxFile);
                var readgpx = new GpxReader(xmlString, translation);
                var nearestPt = readgpx.Tracks.SelectMany(
                        (trk, trkindex) =>
                            trk.Segments.SelectMany(
                                (seg, segindex) =>
                                    seg.TrackPoints.Select(
                                        (pt, ptindex) =>
                                            new
                                            {
                                                TrackPoint = pt,
                                                TrackIndex = trkindex,
                                                SegIndex = segindex,
                                                PtIndex = ptindex,
                                                Latitude = Convert.ToDouble(pt.Lat, CultureInfo.InvariantCulture),
                                                Longitude = Convert.ToDouble(pt.Lon, CultureInfo.InvariantCulture),
                                                Distance = LocationUtils.CalculateDistanceInMeters(
                                                    settings.LocationConfig.DefaultLatitude,
                                                    settings.LocationConfig.DefaultLongitude,
                                                    Convert.ToDouble(pt.Lat, CultureInfo.InvariantCulture),
                                                    Convert.ToDouble(pt.Lon, CultureInfo.InvariantCulture)
                                                )
                                            }
                                    )
                            )
                    )
                    .OrderBy(pt => pt.Distance)
                    .FirstOrDefault(pt => pt.Distance <= 5000);

                if (nearestPt != null)
                {
                    settings.LocationConfig.DefaultLatitude = nearestPt.Latitude;
                    settings.LocationConfig.DefaultLongitude = nearestPt.Longitude;
                    settings.LocationConfig.ResumeTrack = nearestPt.TrackIndex;
                    settings.LocationConfig.ResumeTrackSeg = nearestPt.SegIndex;
                    settings.LocationConfig.ResumeTrackPt = nearestPt.PtIndex;
                }
            }

            IElevationService elevationService = new ElevationService(settings);

            if (boolNeedsSetup)
            {
                //validation auth.config
                AuthAPIForm form = new AuthAPIForm(true);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    settings.Auth.APIConfig = form.Config;
                }

                _session = new Session(settings, new ClientSettings(settings, elevationService), logicSettings, elevationService, translation);

                StarterConfigForm configForm = new StarterConfigForm(_session, settings, elevationService, configFile);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    var fileName = Assembly.GetEntryAssembly().Location;
                    Process.Start(fileName);
                    Environment.Exit(0);
                }

                //if (GlobalSettings.PromptForSetup(_session.Translation))
                //{
                //    _session = GlobalSettings.SetupSettings(_session, settings, elevationService, configFile);

                //    var fileName = Assembly.GetExecutingAssembly().Location;
                //    Process.Start(fileName);
                //    Environment.Exit(0);
                //}
            }
            else
            {
                _session = new Session(settings, new ClientSettings(settings, elevationService), logicSettings, elevationService, translation);

                var apiCfg = settings.Auth.APIConfig;

                if (apiCfg.UsePogoDevAPI)
                {
                    if (string.IsNullOrEmpty(apiCfg.AuthAPIKey))
                    {
                        Logger.Write(
                            "You have selected PogoDev API but you have not provided an API Key, please press any key to exit and correct you auth.json, \r\n The Pogodev API key can be purchased at - https://talk.pogodev.org/d/51-api-hashing-service-by-pokefarmer",
                            LogLevel.Error
                        );
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    try
                    {
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("X-AuthToken", apiCfg.AuthAPIKey);
                        var maskedKey = apiCfg.AuthAPIKey.Substring(0, 4) + "".PadLeft(apiCfg.AuthAPIKey.Length - 8, 'X') + apiCfg.AuthAPIKey.Substring(apiCfg.AuthAPIKey.Length - 4, 4);
                        HttpResponseMessage response = client.PostAsync($"https://pokehash.buddyauth.com/{_session.Client.ApiEndPoint}", null).Result;
                        string AuthKey = response.Headers.GetValues("X-AuthToken").FirstOrDefault();
                        string MaxRequestCount = response.Headers.GetValues("X-MaxRequestCount").FirstOrDefault();
                        DateTime AuthTokenExpiration = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(Convert.ToDouble(response.Headers.GetValues("X-AuthTokenExpiration").FirstOrDefault())).ToLocalTime();
                        TimeSpan Expiration = AuthTokenExpiration - DateTime.Now;
                        string Result = string.Format("Key: {0} RPM: {1} Expires in: {2} days ({3})", maskedKey, MaxRequestCount, Expiration.Days - 1, AuthTokenExpiration);
                        Logger.Write(Result, LogLevel.Info, ConsoleColor.Green);
                    }
                    catch
                    {
                        Logger.Write("The HashKey is invalid or has expired, please press any key to exit and correct you auth.json, \r\nThe Pogodev API key can be purchased at - https://talk.pogodev.org/d/51-api-hashing-service-by-pokefarmer", LogLevel.Error);
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
                else if (apiCfg.UseLegacyAPI)
                {
                    Logger.Write(
                   "You bot will start after 15 seconds, You are running bot with Legacy API (0.45), but it will increase your risk of being banned and triggering captchas. Config Captchas in config.json to auto-resolve them",
                   LogLevel.Warning);

#if RELEASE
                    Thread.Sleep(15000);
#endif
                }
                else
                {
                    Logger.Write(
                         "At least 1 authentication method must be selected, please correct your auth.json.",
                         LogLevel.Error
                     );
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                //GlobalSettings.Load(_subPath, _enableJsonValidation);

                //Logger.Write("Press a Key to continue...",
                //    LogLevel.Warning);
                //Console.ReadKey();
                //return;

                if (excelConfigAllow)
                {
                    ExcelConfigHelper.MigrateFromObject(settings, excelConfigFile);
                }
            }

            ioc.Register<ISession>(_session);

            Logger.SetLoggerContext(_session);

            MultiAccountManager accountManager = new MultiAccountManager(settings, logicSettings.Bots);
            ioc.Register(accountManager);

            if (boolNeedsSetup)
            {
                StarterConfigForm configForm = new StarterConfigForm(_session, settings, elevationService, configFile);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    var fileName = Assembly.GetEntryAssembly().Location;

                    Process.Start(fileName);
                    Environment.Exit(0);

                }

                //if (GlobalSettings.PromptForSetup(_session.Translation))
                //{
                //    _session = GlobalSettings.SetupSettings(_session, settings, elevationService, configFile);

                //    var fileName = Assembly.GetExecutingAssembly().Location;
                //    Process.Start(fileName);
                //    Environment.Exit(0);
                //}
                else
                {
                    GlobalSettings.Load(_subPath, _enableJsonValidation);

                    Logger.Write("Press a Key to continue...",
                        LogLevel.Warning);
                    Console.ReadKey();
                    return;
                }

                if (excelConfigAllow)
                {
                    ExcelConfigHelper.MigrateFromObject(settings, excelConfigFile);
                }
            }

            ProgressBar.Start("NecroBot2 is starting up", 10);

            ProgressBar.Fill(20);

            var machine = new StateMachine();
            var stats = _session.RuntimeStatistics;

            ProgressBar.Fill(30);
            var strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
            stats.DirtyEvent +=
                () =>
                {
                    GetPlayerResponse x = _session.Client.Player.GetPlayer().Result;
                    string warn = x.Warn ? "*(Flagged)*-" : null;

                    Console.Title = $"[NecroBot2 v{strVersion}] Team: {x.PlayerData.Team} - {warn}" +
                                    stats.GetTemplatedStats(
                                        _session.Translation.GetTranslation(TranslationString.StatsTemplateString),
                                        _session.Translation.GetTranslation(TranslationString.StatsXpTemplateString));
                };
            ProgressBar.Fill(40);

            var aggregator = new StatisticsAggregator(stats);
            onBotStarted?.Invoke(_session, aggregator);

            ProgressBar.Fill(50);
            var listener = new ConsoleEventListener();
            ProgressBar.Fill(60);
            var snipeEventListener = new SniperEventListener();

            _session.EventDispatcher.EventReceived += evt => listener.Listen(evt, _session);
            _session.EventDispatcher.EventReceived += evt => aggregator.Listen(evt, _session);
            _session.EventDispatcher.EventReceived += evt => snipeEventListener.Listen(evt, _session);

            ProgressBar.Fill(70);

            machine.SetFailureState(new LoginState());
            ProgressBar.Fill(80);

            ProgressBar.Fill(90);

            _session.Navigation.WalkStrategy.UpdatePositionEvent +=
                (session, lat, lng, speed) => _session.EventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng, Speed = speed });
            _session.Navigation.WalkStrategy.UpdatePositionEvent += LoadSaveState.SaveLocationToDisk;

            ProgressBar.Fill(100);

            if (settings.WebsocketsConfig.UseWebsocket)
            {
                var websocket = new WebSocketInterface(settings.WebsocketsConfig.WebSocketPort, _session);
                _session.EventDispatcher.EventReceived += evt => websocket.Listen(evt, _session);
            }

            var bot = accountManager.GetStartUpAccount();

            _session.ReInitSessionWithNextBot(bot);

            machine.AsyncStart(new VersionCheckState(), _session, _subPath, excelConfigAllow);

            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
            } 
			
            if (settings.TelegramConfig.UseTelegramAPI)
                _session.Telegram = new TelegramService(settings.TelegramConfig.TelegramAPIKey, _session);

            if (_session.LogicSettings.EnableHumanWalkingSnipe &&
                _session.LogicSettings.HumanWalkingSnipeUseFastPokemap)
            {
                HumanWalkSnipeTask.StartFastPokemapAsync(_session,
                    _session.CancellationTokenSource.Token).ConfigureAwait(false); // that need to keep data live
            }

            if (_session.LogicSettings.UseSnipeLocationServer ||
              _session.LogicSettings.HumanWalkingSnipeUsePogoLocationFeeder)
                SnipePokemonTask.AsyncStart(_session);


            if (_session.LogicSettings.DataSharingConfig.EnableSyncData)
            {
                BotDataSocketClient.StartAsync(_session, Properties.Resources.EncryptKey);
                _session.EventDispatcher.EventReceived += evt => BotDataSocketClient.Listen(evt, _session);
            }
            settings.CheckProxy(_session.Translation);

            if (_session.LogicSettings.ActivateMSniper)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
                //temporary disable MSniper connection because site under attacking.
                //MSniperServiceTask.ConnectToService();
                //_session.EventDispatcher.EventReceived += evt => MSniperServiceTask.AddToList(evt);
            }

            // jjskuld - Don't await the analytics service since it starts a worker thread that never returns.
#pragma warning disable 4014
            _session.AnalyticsService.StartAsync(_session, _session.CancellationTokenSource.Token);
#pragma warning restore 4014
            _session.EventDispatcher.EventReceived += evt => AnalyticsService.Listen(evt, _session);

            var trackFile = Path.GetTempPath() + "\\NecroBot2.io";

            if (!File.Exists(trackFile) || File.GetLastWriteTime(trackFile) < DateTime.Now.AddDays(-1))
            {
                Thread.Sleep(10000);
                Thread mThread = new Thread(delegate ()
                {
                    var infoForm = new InfoForm();
                    infoForm.ShowDialog();
                });
                File.WriteAllText(trackFile, DateTime.Now.Ticks.ToString());
                mThread.SetApartmentState(ApartmentState.STA);

                mThread.Start();
            }

            QuitEvent.WaitOne();
        }

        private static void EventDispatcher_EventReceived(IEvent evt)
        {
            throw new NotImplementedException();
        }

        private static bool CheckMKillSwitch()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var responseContent = client.GetAsync(StrMasterKillSwitchUri).Result;
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                        return true;

                    var strResponse1 = responseContent.Content.ReadAsStringAsync().Result;
                    
                    if (string.IsNullOrEmpty(strResponse1))
                        return true;

                    var strSplit1 = strResponse1.Split(';');

                    if (strSplit1.Length > 1)
                    {
                        var strStatus1 = strSplit1[0];
                        var strReason1 = strSplit1[1];
                        var strExitMsg = strSplit1[2];

                        if (strStatus1.ToLower().Contains("disable"))
                        {
                            Logger.Write(strReason1 + $"\n", LogLevel.Warning);

                            Logger.Write(strExitMsg + $"\n" + "Please press enter to continue", LogLevel.Error);
                            Console.ReadLine();
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                        return false;


                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message, LogLevel.Error);
                }
            }

            return false;
        }

        private static bool CheckKillSwitch()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var responseContent = client.GetAsync(StrKillSwitchUri).Result;
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                        return true;

                    var strResponse = responseContent.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(strResponse))
                        return true;

                    var strSplit = strResponse.Split(';');

                    if (strSplit.Length > 1)
                    {
                        var strStatus = strSplit[0];
                        var strReason = strSplit[1];

                        if (strStatus.ToLower().Contains("disable"))
                        {
                            Logger.Write(strReason + $"\n", LogLevel.Warning);

                            if (PromptForKillSwitchOverride(strReason))
                            {
                                // Override
                                Logger.Write("Overriding Killswitch... you have been warned!", LogLevel.Warning);
                                return false;
                            }

                            Logger.Write("The bot will now close, please press enter to continue", LogLevel.Error);
                            Console.ReadLine();
                            Environment.Exit(0);
                            return true;
                        }
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message, LogLevel.Error);
                }
            }

            return false;
        }

        private static void UnhandledExceptionEventHandler(object obj, UnhandledExceptionEventArgs args)
        {
            Logger.Write("Exception caught, writing LogBuffer.", force: true);
            //throw new Exception();
        }

        public static bool PromptForKillSwitchOverride(string strReason)
        {
            Logger.Write("Do you want to override killswitch to bot at your own risk? Y/N", LogLevel.Warning);

            /*while (true)
              {
                  var strInput = Console.ReadLine().ToLower();

                  switch (strInput)
                  {
                      case "y":
                          // Override killswitch
                          return true;

                      case "n":
                          return false;

                      default:
                          Logger.Write("Enter y or n", LogLevel.Error);
                          continue;
                  }
              }*/
            DialogResult result = MessageBox.Show($"{strReason} \n\r Do you want to override killswitch to bot at your own risk? Y/N", $"{Application.ProductName} - Old API detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Yes: return true;
                case DialogResult.No: return false;
            }
            return false;
        }
    }
}
