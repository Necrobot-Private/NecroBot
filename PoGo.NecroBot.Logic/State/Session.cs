#region using directives
using System.Linq;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Service;
using PokemonGo.RocketAPI;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Service.Elevation;
using System.Collections.Generic;
using POGOProtos.Map.Fort;
using System;
using PokemonGo.RocketAPI.Extensions;
using PoGo.NecroBot.Logic.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Caching;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public interface ISession
    {
        ISettings Settings { get; set; }
        Inventory Inventory { get; }
        Client Client { get; }
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
        void ResetSessionToWithNextBot(double lat = 0, double lng = 0, double att = 0);
        void AddForts(List<FortData> mapObjects);
        void AddVisibleForts(List<FortData> mapObjects);
        Task<bool> WaitUntilActionAccept(BotActions action, int timeout = 30000);
        List<BotActions> Actions { get; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        MemoryCache Cache { get; set; }
        Queue<AuthConfig> Accounts { get; }
    }


    public class Session : ISession
    {
        public Session(ISettings settings, ILogicSettings logicSettings, IElevationService elevationService) : this(settings, logicSettings, elevationService, Common.Translation.Load(logicSettings))
        {
        }
        private Queue<AuthConfig> accounts;
        public List<BotActions> Actions { get { return this.botActions; } }
        public Session(ISettings settings, ILogicSettings logicSettings, IElevationService elevationService, ITranslation translation)
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            this.Forts = new List<FortData>();
            this.VisibleForts = new List<FortData>();
            this.Cache = new MemoryCache("Necrobot2");
            this.accounts = new Queue<AuthConfig>();
            this.EventDispatcher = new EventDispatcher();
            this.LogicSettings = logicSettings;

            this.ElevationService = elevationService;
            
            this.Settings = settings;

            this.Translation = translation;
            this.Reset(settings, LogicSettings);
            this.Stats = new SessionStats();
            this.accounts = new Queue<AuthConfig>();
            foreach (var acc in logicSettings.Bots)
            {
                this.accounts.Enqueue(acc);
            }
            if (!this.accounts.Any(x => (x.AuthType == PokemonGo.RocketAPI.Enums.AuthType.Ptc && x.PtcUsername == settings.PtcUsername) ||
                                        (x.AuthType == PokemonGo.RocketAPI.Enums.AuthType.Google && x.GoogleUsername == settings.GoogleUsername)
                                        ))
            {
                this.accounts.Enqueue(new AuthConfig()
                {
                    AuthType = settings.AuthType,
                    GooglePassword = settings.GooglePassword,
                    GoogleUsername = settings.GoogleUsername,
                    PtcPassword = settings.PtcPassword,
                    PtcUsername = settings.PtcUsername
                });
            }
        }
        public List<FortData> Forts { get; set; }
        public List<FortData> VisibleForts { get; set; }
        public GlobalSettings GlobalSettings { get; set; }

        public ISettings Settings { get; set; }

        public Inventory Inventory { get; private set; }

        public Client Client { get; private set; }

        public GetPlayerResponse Profile { get; set; }
        public Navigation Navigation { get; private set; }

        public ILogicSettings LogicSettings { get; set; }

        public ITranslation Translation { get; }

        public IEventDispatcher EventDispatcher { get; }

        public TelegramService Telegram { get; set; }

        public SessionStats Stats { get; set; }

        public IElevationService ElevationService { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public MemoryCache Cache { get; set; }
	public Queue<AuthConfig> Accounts
        {
            get
            {
                return this.accounts;
            }
        }

        private List<BotActions> botActions = new List<BotActions>();
        public void Reset(ISettings settings, ILogicSettings logicSettings)
        {
            ApiFailureStrategy _apiStrategy = new ApiFailureStrategy(this);
            Client = new Client(Settings, _apiStrategy);
            // ferox wants us to set this manually
            Inventory = new Inventory(Client, logicSettings);
            Navigation = new Navigation(Client, logicSettings);
            Navigation.WalkStrategy.UpdatePositionEvent +=
                (lat, lng) => this.EventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng });
        }

        public void ResetSessionToWithNextBot(double lat=0, double lng=0, double att=0)
        {
            var nextBot = this.accounts.Dequeue();
            this.Settings.AuthType = nextBot.AuthType;
            this.Settings.GooglePassword = nextBot.GooglePassword;
            this.Settings.GoogleUsername = nextBot.GoogleUsername;
            this.Settings.PtcPassword = nextBot.PtcPassword;
            this.Settings.PtcUsername = nextBot.PtcUsername;
            this.Settings.DefaultAltitude = att == 0 ? this.Client.CurrentAltitude : att;
            this.Settings.DefaultLatitude = lat == 0 ? this.Client.CurrentLatitude : lat;
            this.Settings.DefaultLongitude = lng ==0 ? this.Client.CurrentLongitude : lng;
            this.Stats = new SessionStats();

            //ApiFailureStrategy _apiStrategy = new ApiFailureStrategy(this);
            //Client = new Client(Settings, _apiStrategy);
            //Inventory = new Inventory(Client, this.LogicSettings);
            this.Reset(this.Settings, this.LogicSettings);
            CancellationTokenSource.Cancel();
            this.CancellationTokenSource = new CancellationTokenSource();

            this.accounts.Enqueue(nextBot); //put it to the last then it will cycle loop.
            this.EventDispatcher.Send(new BotSwitchedEvent() { });
        }
        public void AddForts(List<FortData> data)
        {
            this.Forts.RemoveAll(p => data.Any(x => x.Id == p.Id && x.Type == FortType.Checkpoint));
            this.Forts.AddRange(data.Where(x => x.Type == FortType.Checkpoint));
            foreach (var item in data.Where(p => p.Type == FortType.Gym))
            {
                var exist = this.Forts.FirstOrDefault(x => x.Id == item.Id);
                if (exist != null && exist.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }
                else
                {
                    this.Forts.RemoveAll(x => x.Id == item.Id);
                    this.Forts.Add(item);
                }
            }
        }

        public void AddVisibleForts(List<FortData> mapObjects)
        {
            var notexist = mapObjects.Where(p => !this.VisibleForts.Any(x => x.Id == p.Id));
            this.VisibleForts.AddRange(notexist);
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
                await Task.Delay(1000);
            }
            return false; //timedout
        }
    }
}