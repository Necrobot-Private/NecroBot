using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;
using TinyIoC;
using PoGo.NecroBot.Logic.Common;
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
            Exp = stats.FirstOrDefault(x => x.Experience > 0).Experience;
            LevelExp = stats.FirstOrDefault(x => x.NextLevelXp > 0).NextLevelXp;

            this.playerProfile = profile.Profile;
        }

        public void OnInventoryRefreshed(IEnumerable<InventoryItem> inventory)
        {
            if (this.playerProfile == null || this.playerProfile.PlayerData.BuddyPokemon == null || this.playerProfile.PlayerData.BuddyPokemon.Id == 0) return;

            var buddyData = this.playerProfile.PlayerData.BuddyPokemon;

            if (buddyData == null) return;
             
            var buddy = inventory
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.Id == this.playerProfile.PlayerData.BuddyPokemon.Id)
                .FirstOrDefault();

            if (buddy == null) return;

            this.BuddyPokemonId = buddy.PokemonId;
            this.BuddyCurrentKM = buddyData.LastKmAwarded;
            this.BuddyTotalKM = buddy.BuddyTotalKmWalked;

            this.RaisePropertyChanged("BuddyPokemonId");
            this.RaisePropertyChanged("BuddyCurrentKM");
            this.RaisePropertyChanged("BuddyTotalKM");
        }

        internal void DirtyEventHandle(Statistics stat)
        {
            this.Runtime = stat.FormatRuntime();
            this.EXPPerHour = (int)(stat.TotalExperience / stat.GetRuntime());
            this.PKMPerHour = (int)(stat.TotalPokemons / stat.GetRuntime());
            this.TimeToLevelUp = $"{stat.StatsExport.HoursUntilLvl:00}h :{stat.StatsExport.MinutesUntilLevel:00}m";
            this.Level = stat.StatsExport.Level;
            this.Stardust = stat.TotalStardust;
            this.Exp = stat.StatsExport.CurrentXp;
            this.LevelExp = stat.StatsExport.LevelupXp;
            this.PokemonTransfered = stat.TotalPokemonTransferred;
            this.RaisePropertyChanged("TotalPokemonTransferred;");
            this.RaisePropertyChanged("Runtime");
            this.RaisePropertyChanged("EXPPerHour");
            this.RaisePropertyChanged("PKMPerHour");
            this.RaisePropertyChanged("TimeToLevelUp");
            this.RaisePropertyChanged("Level");
            this.RaisePropertyChanged("Stardust");
            this.RaisePropertyChanged("Exp");
            this.RaisePropertyChanged("LevelExp");

        }

        internal void UpdatePokestopLimit(PokestopLimitUpdate ev)
        {
            this.PokestopLimit = $"{ev.Value}/{ev.Limit}";
            this.RaisePropertyChanged("PokestopLimit");
        }

        internal void UpdateCatchLimit(CatchLimitUpdate ev)
        {
            this.CatchLimit = $"{ev.Value}/{ev.Limit}";
            this.RaisePropertyChanged("CatchLimit");
        }

        public void UpdateSpeed(double speed)
        {
            this.WalkSpeed = speed;
            this.RaisePropertyChanged("WalkSpeed");

        }
    }
}
