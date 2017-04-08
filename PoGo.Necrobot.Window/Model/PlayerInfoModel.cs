using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;
using PoGo.NecroBot.Logic.Utils;

namespace PoGo.Necrobot.Window.Model
{
    public class PlayerInfoModel : ViewModelBase
    {
        public PokemonId BuddyPokemonId { get; set; }
        public string Name { get; set; }
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
        public int PokemonTransfered { get; set; }

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
            RaisePropertyChanged("TotalPokemonTransferred;");
            RaisePropertyChanged("Runtime");
            RaisePropertyChanged("EXPPerHour");
            RaisePropertyChanged("PKMPerHour");
            RaisePropertyChanged("TimeToLevelUp");
            RaisePropertyChanged("Level");
            RaisePropertyChanged("Stardust");
            RaisePropertyChanged("Exp");
            RaisePropertyChanged("LevelExp");

        }

        internal void UpdatePokestopLimit(PokestopLimitUpdate ev)
        {
            PokestopLimit = $"{ev.Value}/{ev.Limit}";
            RaisePropertyChanged("PokestopLimit");
        }

        internal void UpdateCatchLimit(CatchLimitUpdate ev)
        {
            CatchLimit = $"{ev.Value}/{ev.Limit}";
            RaisePropertyChanged("CatchLimit");
        }

        public void UpdateSpeed(double speed)
        {
            WalkSpeed = speed;
            RaisePropertyChanged("WalkSpeed");

        }
    }
}
