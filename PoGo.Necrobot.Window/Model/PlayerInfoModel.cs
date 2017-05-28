using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyIoC;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Utils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;

namespace PoGo.Necrobot.Window.Model
{
    public class PlayerInfoModel : ViewModelBase
    {
        public DateTime Insence_expires = new DateTime(0);
        public DateTime Luckyeggs_expires = new DateTime(0);
        public PokemonId BuddyPokemonId { get; set; }
        public string Name { get; set; }

        public double KmRemaining; // Not Working Quite Right
        public double KmToWalk
        {
            get { return KmRemaining; }
            set
            {
                KmRemaining = value;
                RaisePropertyChanged("KmToWalk");
                RaisePropertyChanged("EggPerc");
            }
        }

        public int EggPerc
        {
            get
            {
                if (KmToWalk > 0)
                    return (int)Math.Floor(KmRemaining / KmToWalk * 100);
                return 0;
            }
        }

        private double exp;
        public double Exp
        {
            get { return exp; }
            set
            {
                exp = value;
                RaisePropertyChanged("Exp");
                RaisePropertyChanged("PercentComplete");
            }
        }

        private double levelExp;
        public double LevelExp
        {
            get { return levelExp; }
            set
            {
                levelExp = value;
                RaisePropertyChanged("LevelExp");
                RaisePropertyChanged("PercentComplete");
            }
        }

        public int PercentComplete
        {
            get
            {
                if (LevelExp > 0)
                    return (int)Math.Floor(Exp / LevelExp * 100);
                return 0;
            }
        }

        private int expH;
        public int EXPPerHour
        {
            get { return expH; }
            set
            {
                expH = value;
                RaisePropertyChanged("EXPPerHour");
            }
        }

        private int pkmH;
        public int PKMPerHour
        {
            get { return pkmH; }
            set
            {
                pkmH = value;
                RaisePropertyChanged("PKMPerHour");

            }
        }

        private string runtime;
        public string Runtime
        {
            get { return runtime; }
            set
            {
                runtime = value;
                RaisePropertyChanged("Runtime");

            }
        }

        private string levelupTime;
        public string TimeToLevelUp
        {
            get { return levelupTime; }
            set
            {
                levelupTime = value;
                RaisePropertyChanged("TimeToLevelUp");

            }
        }
        private int stardust;
        public int Stardust
        {
            get { return stardust; }
            set
            {
                stardust = value;
                RaisePropertyChanged("Stardust");

            }
        }
        private int level;
        private GetPlayerResponse playerProfile;

        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                RaisePropertyChanged("Level");
            }
        }

        public double BuddyTotalKM { get; set; }
        public double BuddyCurrentKM { get; set; }
        public string PokestopLimit { get; set; }
        public string CatchLimit { get; set; }
        public double WalkSpeed { get; set; }
        public string CollectPokeCoin { get; set; }

        //Still Needs Some Work(TheWizard1328)
        public int pokemontransfered;
        public int PokemonTransfered //{ get; set; }
        {
            get { return pokemontransfered; }
            set
            {
                pokemontransfered = value;
                RaisePropertyChanged("PokemonTransfered");
            }
        }

        internal async Task OnProfileUpdateAsync(ProfileEvent profile)
        {
            var stats = profile.Stats;
            var playerStats = stats.FirstOrDefault(x => x.Experience > 0);
            if (playerStats != null)
            {
                Exp = playerStats.Experience;
                LevelExp = playerStats.NextLevelXp;
            }

            await GetPokeCoin();
            playerProfile = profile.Profile;
        }

        public void OnInventoryRefreshed(IEnumerable<InventoryItem> inventory)
        {
            if (playerProfile == null || playerProfile.PlayerData.BuddyPokemon == null || playerProfile.PlayerData.BuddyPokemon.Id == 0) return;

            var buddyData = playerProfile.PlayerData.BuddyPokemon;

            if (buddyData == null) return;

            var buddy = inventory
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.Id == playerProfile.PlayerData.BuddyPokemon.Id)
                .FirstOrDefault();

            var appliedItems =
                Session.Inventory.GetAppliedItems().Result
                .SelectMany(aItems => aItems.Item)
                .ToDictionary(item => item.ItemId, item => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(item.ExpireMs));

            var items =
                     Session.Inventory.GetItems().Result
                    .Where(i => i != null)
                    .OrderBy(i => i.ItemId);

            foreach (var item in items)
            {
                if (appliedItems.ContainsKey(item.ItemId))
                {   
                    switch (item.ItemId)
                    {
                        case ItemId.ItemLuckyEgg:
                            Luckyeggs_expires = appliedItems[item.ItemId];
                            break;
                        case ItemId.ItemIncenseOrdinary:
                            Insence_expires = appliedItems[item.ItemId];
                            break;
                        default:
                            Insence_expires = appliedItems[item.ItemId];
                            break;
                    }
                }
            }

            if (buddy == null) return;

            BuddyPokemonId = buddy.PokemonId;
            BuddyCurrentKM = buddyData.LastKmAwarded;
            BuddyTotalKM = buddy.BuddyTotalKmWalked;

            RaisePropertyChanged("BuddyPokemonId");
            RaisePropertyChanged("BuddyCurrentKM");
            RaisePropertyChanged("BuddyTotalKM");
        }

        internal void DirtyEventHandle(Statistics stat)
        {
            Runtime = stat.FormatRuntime();
            EXPPerHour = (int)(stat.TotalExperience / stat.GetRuntime());
            PKMPerHour = (int)(stat.TotalPokemons / stat.GetRuntime());
            TimeToLevelUp = $"{stat.StatsExport.HoursUntilLvl:00}h :{stat.StatsExport.MinutesUntilLevel:00}m";
            Level = stat.StatsExport.Level;
            Stardust = stat.TotalStardust;
            Exp = stat.StatsExport.CurrentXp - stat.StatsExport.PreviousXp;
            LevelExp = stat.StatsExport.LevelupXp - stat.StatsExport.PreviousXp;
            PokemonTransfered = stat.TotalPokemonTransferred;
            //Still needs some work(TheWizard1328)
            //KmRemaining = incubator.TargetKmWalked - kmWalked;
            //KmToWalk = incubator.TargetKmWalked - incubator.StartKmWalked;

            RaisePropertyChanged("TotalPokemonTransferred;");
            RaisePropertyChanged("Runtime");
            RaisePropertyChanged("EXPPerHour");
            RaisePropertyChanged("PKMPerHour");
            RaisePropertyChanged("TimeToLevelUp");
            RaisePropertyChanged("Level");
            RaisePropertyChanged("Stardust");
            RaisePropertyChanged("Exp");
            RaisePropertyChanged("LevelExp");

            RaisePropertyChanged("KmToWalk");
            RaisePropertyChanged("KmRemaining");
            RaisePropertyChanged("EggPerc");
        }

        internal void UpdatePokestopLimit(PokestopLimitUpdate ev)
        {
            PokestopLimit = $"{ev.Value}/{ev.Limit}";
            RaisePropertyChanged("PokestopLimit");
            UpdateEggs(KmRemaining);
        }

        internal void UpdateCatchLimit(CatchLimitUpdate ev)
        {
            CatchLimit = $"{ev.Value}/{ev.Limit}";
            RaisePropertyChanged("CatchLimit");
            UpdateEggs(KmRemaining);
        }

        public void UpdateSpeed(double speed)
        {
            WalkSpeed = speed;
            RaisePropertyChanged("WalkSpeed");
            UpdateEggs(KmRemaining);
        }

        public void UpdateEggs(double kmremaining)
        {
            KmRemaining = kmremaining;
            RaisePropertyChanged("KmRemaining");
        }

        public async Task GetPokeCoin()
        {
            Session = TinyIoCContainer.Current.Resolve<ISession>();
            var deployed = await Session.Inventory.GetDeployedPokemons();
            var count = (deployed.Count() * 10).ToString();
            CollectPokeCoin = $"Collect PokeCoin ({count})";
            RaisePropertyChanged("CollectPokeCoin");
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            var time = Insence_expires - DateTime.UtcNow;
            if (Insence_expires.Ticks == 0 || time.TotalSeconds < 0)
            {
                //not implanted
            }
            else
            {
                // my value here  00:00:00
            }

            var timel = Luckyeggs_expires - DateTime.UtcNow;
            if (Luckyeggs_expires.Ticks == 0 || timel.TotalSeconds < 0)
            {
                //not implanted
            }
            else
            {
                // my value here  00:00:00
            }
        }
    }
}