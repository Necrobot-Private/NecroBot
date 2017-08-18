using System;
using System.Collections.Generic;
using System.Linq;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Window.Model
{
    public class PlayerInfoModel : ViewModelBase
    {
        private static DateTime expires = new DateTime(0);
        private string lucky_expires;
        public string Lucky_expires
        {
            get { return lucky_expires; }
            set
            {
                lucky_expires = value;
            }
        }
        private string insence_expires;
        public string Insence_expires
        {
            get { return insence_expires; }
            set
            {
                insence_expires = value;
            }
        }
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

        internal void OnProfileUpdate(ProfileEvent profile)
        {
            var stats = profile.Stats;
            var playerStats = stats.FirstOrDefault(x => x.Experience > 0);
            if (playerStats != null)
            {
                Exp = playerStats.Experience;
                LevelExp = playerStats.NextLevelXp;
            }

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

            //Code added by furtif
            var inventory = Session.Inventory.GetCachedInventory().Result;
            var eggsListViewModel = new EggsListViewModel();
            eggsListViewModel.OnInventoryRefreshed(inventory);

            foreach (var x in eggsListViewModel.Incubators)
            {
                if (x.IsUnlimited && x.InUse)
                {
                    KmRemaining = x.TotalKM - x.KM;
                    KmToWalk = x.TotalKM;
                }
            }
            //

            RaisePropertyChanged("KmToWalk");
            RaisePropertyChanged("KmRemaining");
            RaisePropertyChanged("EggPerc");

            RaisePropertyChanged("TotalPokemonTransferred;");
            RaisePropertyChanged("Runtime");
            RaisePropertyChanged("EXPPerHour");
            RaisePropertyChanged("PKMPerHour");
            RaisePropertyChanged("TimeToLevelUp");
            RaisePropertyChanged("Level");
            RaisePropertyChanged("Stardust");
            RaisePropertyChanged("Exp");
            RaisePropertyChanged("LevelExp");

            // get applied items
            var appliedItems =
                Session.Inventory.GetAppliedItems().Result
                .SelectMany(aItems => aItems.Item)
                .ToDictionary(item => item.ItemId, item => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(item.ExpireMs));

            var items =
                     Session.Inventory.GetItems().Result
                    .Where(i => i != null)
                    .OrderBy(i => i.ItemId);

            bool isLucky = false;
            Lucky_expires = $"00m 00s";
            Insence_expires = $"00m 00s";

            foreach (var item in items)
            {
                if (appliedItems.ContainsKey(item.ItemId))
                {
                    expires = appliedItems[item.ItemId];
                    if (item.ItemId == ItemId.ItemLuckyEgg) isLucky = true;
                }
            }

            var time = expires - DateTime.UtcNow;
            if (expires.Ticks == 0 || time.TotalSeconds < 0)
            {
                // my value here  00m 00s
            }
            else
            {
                // my value here  00m 00s
                if (isLucky)
                    Lucky_expires = $"{time.Minutes}m {Math.Abs(time.Seconds)}s";
                else
                    Insence_expires = $"{time.Minutes}m {Math.Abs(time.Seconds)}s";
            }

            RaisePropertyChanged("Lucky_expires");
            RaisePropertyChanged("Insence_expires");
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
    }
}