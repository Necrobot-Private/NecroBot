using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using System;
using TinyIoC;

namespace PoGo.Necrobot.Window.Model
{
    public class GymViewModel : ViewModelBase
    {
        private FortData fort;

        public string FortId => fort.Id;

        public double Latitude => fort.Latitude;
        public double Longitude => fort.Longitude;
        public double Distance { get; set; }

        public string TeamName => fort.OwnedByTeam.ToString();

        public int DefenderCP => fort.GuardPokemonCp;

        public long GymPoints => fort.GymPoints;

        public PokemonId DefenderId => fort.GuardPokemonId;

        public string GymIcon
        {
            get
            {
                string fortIcon = "";
                bool isRaid = false;
                bool isSpawn = false;
                bool asBoss = false;
                long asBossTime = 0;
                long isRaidTime = 0;
                long isRaidSpawnTime = 0;

                try
                {
                    isRaidTime = fort.RaidInfo.RaidBattleMs;

                    if (fort.RaidInfo != null)
                    {
                        asBossTime = fort.RaidInfo.RaidEndMs;
                        isRaidSpawnTime = fort.RaidInfo.RaidSpawnMs - asBossTime;

                        if (fort.RaidInfo.RaidPokemon.PokemonId > 0 && asBossTime > 0 || isRaidSpawnTime > 0)
                            asBoss = true;
                    }
                }
                catch
                {
                    //
                }

                if (isRaidTime > 0)
                    isRaid = true;

                if (isRaidSpawnTime > 0)
                    isSpawn = true;

                string gymBoss = null;

                if (asBoss)
                {
                    //TODO: Review this
                    TimeSpan time = new TimeSpan();

                    if (isSpawn)
                    {
                        time = new TimeSpan(isRaidSpawnTime);
                        DateTime tm = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(isRaidSpawnTime);
                        time = tm - DateTime.UtcNow;
                        gymBoss = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{(int)fort.RaidInfo.RaidPokemon.PokemonId}.png";
                    }
                    else
                    {
                        time = new TimeSpan(asBossTime);
                        DateTime tm = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(asBossTime);
                        time = tm - DateTime.UtcNow;
                        gymBoss = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{(int)fort.RaidInfo.RaidPokemon.PokemonId}.png";
                    }
                }

                string gymStat = isRaid ? "-raid" : null;
 
                // Solution to overlay 2 images in string mode ????
                // forticon + gymBoss or create asset 251 gyms red + 251 gyms blue + 251 neutral +251 yellow !!!!
                // WFP is compatible to var image ??? 

                switch (fort.OwnedByTeam)
                {
                    case TeamColor.Neutral:
                     /*/if (asBoss)
                            fortIcon = gymimage + gymBoss;
                        else //*/
                            fortIcon = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/markers/unoccupied{gymStat}.png";
                        break;
                    case TeamColor.Blue:
                            fortIcon = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/markers/mystic{gymStat}.png";
                        break;
                    case TeamColor.Red:
                            fortIcon = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/markers/valor{gymStat}.png";
                        break;
                    case TeamColor.Yellow:
                            fortIcon = $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/markers/instinct{gymStat}.png";
                        break;
                }
                return fortIcon;
            }
        }

        public string TeamIcon
        {
            get
            {
                string fortIcon = "";
                switch (fort.OwnedByTeam)
                {
                    case TeamColor.Neutral:
                        fortIcon = "https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/gui/unoccupied.png";
                        break;
                    case TeamColor.Blue:
                        fortIcon = "https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/gui/mystic.png";
                        break;
                    case TeamColor.Red:
                        fortIcon = "https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/gui/valor.png";
                        break;
                    case TeamColor.Yellow:
                        fortIcon = "https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/NecroEase/gui/instinct.png";
                        break;
                }
                return fortIcon;
            }
        }

        public GymViewModel(FortData data)
        {
            Session = TinyIoCContainer.Current.Resolve<ISession>();
            fort = data;
            
            UpdateDistance(Session.Client.CurrentLatitude, Session.Client.CurrentLongitude);
        }

        internal void UpdateDistance(double lat, double lng)
        {
            Distance = LocationUtils.CalculateDistanceInMeters(lat, lng, Latitude, Longitude);
            RaisePropertyChanged("Distance");

        }
    }
}