#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Service;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using TinyIoC;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public interface ISession
    {
        ISettings Settings { get; set; }
        Inventory Inventory { get; }
        Client Client { get; set; }
        GetPlayerResponse Profile { get; set; }
        Navigation Navigation { get; }
        ILogicSettings LogicSettings { get; set; }
        ITranslation Translation { get; }
        IEventDispatcher EventDispatcher { get; }
        TelegramService Telegram { get; set; }
        SessionStats Stats { get; }
        IElevationService ElevationService { get; set; }
        List<FortData> Forts { get; set; }
        List<FortData> VisibleForts { get; set; }
        bool ReInitSessionWithNextBot(Account authConfig = null, double lat = 0, double lng = 0, double att = 0);
        void AddForts(List<FortData> mapObjects);
        void AddVisibleForts(List<FortData> mapObjects);
        Task<bool> WaitUntilActionAccept(BotActions action, int timeout = 30000);
        List<BotActions> Actions { get; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        MemoryCache Cache { get; set; }
        DateTime LoggedTime { get; set; }
        DateTime CatchBlockTime { get; set; }
        Statistics RuntimeStatistics { get; }
        GymTeamState GymState { get; set; }
        double KnownLatitudeBeforeSnipe { get; set; }
        double KnownLongitudeBeforeSnipe { get; set; }
        bool SaveBallForByPassCatchFlee { set; get; }
    }

    public class Session : ISession
    {
        public Session(GlobalSettings globalSettings,ISettings settings, ILogicSettings logicSettings, IElevationService elevationService) : this(
           globalSettings, settings, logicSettings, elevationService, Common.Translation.Load(logicSettings))
        {
            LoggedTime = DateTime.Now;
        }

        public bool SaveBallForByPassCatchFlee { get; set; }
        public DateTime LoggedTime { get; set; }
        private List<AuthConfig> accounts;

        public List<BotActions> Actions
        {
            get { return botActions; }
        }

        public Session(GlobalSettings globalSettings, ISettings settings, ILogicSettings logicSettings,
            IElevationService elevationService, ITranslation translation)
        {
            GlobalSettings = globalSettings;
            CancellationTokenSource = new CancellationTokenSource();
            Forts = new List<FortData>();
            VisibleForts = new List<FortData>();
            Cache = new MemoryCache("NecroBot2");
            accounts = new List<AuthConfig>();
            EventDispatcher = new EventDispatcher();
            LogicSettings = logicSettings;
            RuntimeStatistics = new Statistics();

            ElevationService = elevationService;

            Settings = settings;

            Translation = translation;
            Reset(settings, LogicSettings);
            Stats = new SessionStats(this);

            AnalyticsService = new AnalyticsService();
            
            accounts.AddRange(logicSettings.Bots);
            if (!accounts.Any(x => x.AuthType == settings.AuthType && x.Username == settings.Username))
            {
                accounts.Add(new AuthConfig()
                {
                    AuthType = settings.AuthType,
                    Password = settings.Password,
                    Username = settings.Username,
                    AutoExitBotIfAccountFlagged = settings.AutoExitBotIfAccountFlagged,
                    AccountLatitude = settings.AccountLatitude,
                    AccountLongitude = settings.AccountLongitude,
                    AccountActive = settings.AccountActive
                });
            }
            if (File.Exists("runtime.log"))
            {
                var lines = File.ReadAllLines("runtime.log");
                foreach (var item in lines)
                {
                    var arr = item.Split(';');
                    var acc = accounts.FirstOrDefault(p => p.Username == arr[0]);
                    if (acc != null)
                    {
                        acc.RuntimeTotal = Convert.ToDouble(arr[1]);
                    }
                }
            }

            GymState = new GymTeamState();
        }

        public List<FortData> Forts { get; set; }
        public List<FortData> VisibleForts { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public ISettings Settings { get; set; }
        public Inventory Inventory { get; private set; }
        public Client Client { get; set; }
        public GetPlayerResponse Profile { get; set; }
        public Navigation Navigation { get; private set; }
        public ILogicSettings LogicSettings { get; set; }
        public ITranslation Translation { get; }
        public IEventDispatcher EventDispatcher { get; }
        public TelegramService Telegram { get; set; }
        public SessionStats Stats { get; set; }
        public IElevationService ElevationService { get; set; }
        public AnalyticsService AnalyticsService { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public MemoryCache Cache { get; set; }

        public List<AuthConfig> Accounts
        {
            get { return accounts; }
        }

        public DateTime CatchBlockTime { get; set; }
        public Statistics RuntimeStatistics { get; }
        private List<BotActions> botActions = new List<BotActions>();

        public void Reset(ISettings settings, ILogicSettings logicSettings)
        {
            KnownLatitudeBeforeSnipe = 0;
            KnownLongitudeBeforeSnipe = 0;
            if(GlobalSettings.Auth.DeviceConfig.UseRandomDeviceId)
            {
                settings.DeviceId = DeviceConfig.GetDeviceId(settings.Username);
                Logger.Debug($"Username : {Settings.Username} , Device ID :{Settings.DeviceId}");
            }
            Client = new Client(settings);
            // ferox wants us to set this manually
            Inventory = new Inventory(this, Client, logicSettings, async () =>
            {
                var candy = (await Inventory.GetPokemonFamilies().ConfigureAwait(false)).ToList();
                var pokemonSettings = (await Inventory.GetPokemonSettings().ConfigureAwait(false)).ToList();
                EventDispatcher.Send(new InventoryRefreshedEvent(null, pokemonSettings, candy));
            });
            Navigation = new Navigation(Client, logicSettings);
            Navigation.WalkStrategy.UpdatePositionEvent +=
                (session, lat, lng,s) => EventDispatcher.Send(new UpdatePositionEvent {Latitude = lat, Longitude = lng, Speed = s});

            Navigation.WalkStrategy.UpdatePositionEvent += LoadSaveState.SaveLocationToDisk;
        }

        //TODO : Need add BotManager to manage all feature related to multibot, 
        public bool ReInitSessionWithNextBot(Account bot = null, double lat = 0, double lng = 0, double att = 0)
        {
            CatchBlockTime = DateTime.Now; //remove any block
            MSniperServiceTask.BlockSnipe();
            VisibleForts.Clear();
            Forts.Clear();

            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var nextBot = manager.GetSwitchableAccount(bot);
            var Account = !string.IsNullOrEmpty(nextBot.Nickname) ? nextBot.Nickname : nextBot.Username;
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var TotXP = 0;

            for (int i = 0; i < nextBot.Level + 1; i++)
            {
                TotXP = TotXP + Statistics.GetXpDiff(i);
            }

            long? XP = nextBot.CurrentXp;
            if (XP == null) { XP = 0; }
            long? SD = nextBot.Stardust;
            if (SD == null) { SD = 0; }
            long? Lvl = nextBot.Level;
            if (Lvl == null) { Lvl = 0; }
            var NLevelXP = nextBot.NextLevelXp;
            if (nextBot.NextLevelXp == null) { NLevelXP = 0; }

            Logger.Write($"Account changed to {Account}", LogLevel.BotStats);

            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification)
                PushNotificationClient.SendNotification(session, $"Account changed to", $"{Account}\n" +
                                                                 $"Lvl: {Lvl}\n" +
                                                                 $"XP : {XP:#,##0}({(double)XP / ((double)NLevelXP) * 100:#0.00}%)\n" +
                                                                 $"SD : {SD:#,##0}", true).ConfigureAwait(false);

            if (nextBot != null)
                manager.SwitchAccounts(nextBot);

            Settings.DefaultAltitude = att == 0 ? Client.CurrentAltitude : att;
            Settings.DefaultLatitude = lat == 0 ? Client.CurrentLatitude : lat;
            Settings.DefaultLongitude = lng == 0 ? Client.CurrentLongitude : lng;
            Stats = new SessionStats(this);
            Reset(Settings, LogicSettings);
            //CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();

            EventDispatcher.Send(new BotSwitchedEvent(nextBot)
            {
            });

            if (LogicSettings.MultipleBotConfig.DisplayList)
            {
                manager.DumpAccountList();
            }
            return true;
        }

        public void AddForts(List<FortData> data)
        {
            data.RemoveAll(x => LocationUtils.CalculateDistanceInMeters(x.Latitude, x.Longitude, Settings.DefaultLatitude, Settings.DefaultLongitude) > 10000);

            Forts.RemoveAll(p => data.Any(x => x.Id == p.Id && x.Type == FortType.Checkpoint));
            Forts.AddRange(data.Where(x => x.Type == FortType.Checkpoint));
            foreach (var item in data.Where(p => p.Type == FortType.Gym))
            {
                var exist = Forts.FirstOrDefault(x => x.Id == item.Id);
                if (exist != null && exist.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }
                else
                {
                    Forts.RemoveAll(x => x.Id == item.Id);
                    Forts.Add(item);
                }
            }
        }

        public void AddVisibleForts(List<FortData> mapObjects)
        {
            var notexist = mapObjects.Where(p => !VisibleForts.Any(x => x.Id == p.Id));
            VisibleForts.AddRange(notexist);
        }

        public async Task<bool> WaitUntilActionAccept(BotActions action, int timeout = 30000)
        {
            if (botActions.Count == 0) return true;
            var waitTimes = 0;
            while (true && waitTimes < timeout)
            {
                if (botActions.Count == 0) return true;
                ///implement logic of action dependent
                waitTimes += 1000;
                await Task.Delay(1000).ConfigureAwait(false);
            }
            return false; //timedout
        }
        public GymTeamState GymState { get; set; }

        public double KnownLatitudeBeforeSnipe { get; set; }
        public double KnownLongitudeBeforeSnipe { get; set; }
    }
}
