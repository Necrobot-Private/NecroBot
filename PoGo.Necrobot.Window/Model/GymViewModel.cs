using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using System;
using TinyIoC;

namespace PoGo.NecroBot.Window.Model
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
                DateTime expires = new DateTime(0);
                TimeSpan time = new TimeSpan(0);
                string finalText = null;
                string boss = null;

                try
                {
                    if (fort.RaidInfo != null)
                    {
                        if (fort.RaidInfo.RaidBattleMs > 0)
                        {
                            expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(fort.RaidInfo.RaidBattleMs);
                            time = expires - DateTime.UtcNow;
                            if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                            {
                                finalText = $"Next RAID starts in: {time.Hours}h {time.Minutes}m";
                                isRaid = true;
                            }
                        }

                        if (fort.RaidInfo.RaidPokemon.PokemonId > 0)
                        {
                            expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(fort.RaidInfo.RaidEndMs);
                            time = expires - DateTime.UtcNow;
                            if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                            {
                                asBoss = true;
                                boss = $"Boss: {fort.RaidInfo.RaidPokemon.PokemonId} CP: {fort.RaidInfo.RaidPokemon.Cp}";
                                finalText = $"Local RAID ends in: {time.Hours}h {time.Minutes}m";
                            }
                        }

                        if (fort.RaidInfo.RaidSpawnMs > 0)
                        {
                            expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(fort.RaidInfo.RaidSpawnMs);
                            time = expires - DateTime.UtcNow;
                            if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                            {
                                isSpawn = true;
                                finalText = !asBoss ? $"Local SPAWN ends in: {time.Hours}h {time.Minutes}m" : $"Local SPAWN ends in: {time.Hours}h {time.Minutes}m\n\r{boss}";
                            }
                        }
                    }
                }
                catch
                {

                }

                if (isSpawn) { } //
                if (asBoss) { } //

                string gymStat = isRaid ? "-raid" : null;
 
                // Solution to overlay 2 images in string mode ????
                // forticon + gymBoss or create asset 251 gyms red + 251 gyms blue + 251 neutral +251 yellow !!!!
                // WPF is compatible to var image ??? 

                switch (fort.OwnedByTeam)
                {
                    case TeamColor.Neutral:
                     /*/if (asBoss)
                            fortIcon = gymimage + gymBoss;
                        else //*/
                            fortIcon = $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/unoccupied{gymStat}.png";
                        break;
                    case TeamColor.Blue:
                            fortIcon = $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/mystic{gymStat}.png";
                        break;
                    case TeamColor.Red:
                            fortIcon = $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/valor{gymStat}.png";
                        break;
                    case TeamColor.Yellow:
                            fortIcon = $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/instinct{gymStat}.png";
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
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/gui/unoccupied.png";
                        break;
                    case TeamColor.Blue:
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/gui/mystic.png";
                        break;
                    case TeamColor.Red:
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/gui/valor.png";
                        break;
                    case TeamColor.Yellow:
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/gui/instinct.png";
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