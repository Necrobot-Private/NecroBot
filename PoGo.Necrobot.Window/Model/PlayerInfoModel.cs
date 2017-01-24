using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Data.Player;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Event.Inventory;

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
        private int startdust;
        public int Startdust
        {
            get { return startdust; }
            set
            {
                startdust = value;
                RaisePropertyChanged("Startdust");

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

        internal void OnProfileUpdate(ProfileEvent profile)
        {
            var stats = profile.Stats;
            Exp = stats.FirstOrDefault(x => x.Experience > 0).Experience;
            LevelExp = stats.FirstOrDefault(x => x.NextLevelXp > 0).NextLevelXp;

            this.playerProfile = profile.Profile;
        }

        public void OnInventoryRefreshed(InventoryRefreshedEvent inventory)
        {
            if (this.playerProfile == null || this.playerProfile.PlayerData.BuddyPokemon == null || this.playerProfile.PlayerData.BuddyPokemon.Id == 0) return;

            var budyData = this.playerProfile.PlayerData.BuddyPokemon;

            var buddy = inventory.Inventory
                .InventoryDelta
                .InventoryItems
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.Id == this.playerProfile.PlayerData.BuddyPokemon.Id)
                .FirstOrDefault();

            this.BuddyPokemonId = buddy.PokemonId;
            this.BuddyCurrentKM = budyData.LastKmAwarded;
            this.BuddyTotalKM = buddy.BuddyTotalKmWalked;

            this.RaisePropertyChanged("BuddyPokemonId");
            this.RaisePropertyChanged("BuddyCurrentKM");
            this.RaisePropertyChanged("BuddyTotalKM");
        }
    }
}
