using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data.Raid;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
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
                bool asBoss = false;
                RaidInfo raidInfo = null;

                try
                {
                    if (fort.RaidInfo != raidInfo)
                    {
                        raidInfo = new RaidInfo(fort.RaidInfo);

                        PokemonId boss = raidInfo.RaidPokemon.PokemonId;

                        if (boss > 0 && raidInfo.RaidEndMs > 0)
                            asBoss = true;

                        if (raidInfo.RaidBattleMs > 0)
                            isRaid = true;
                    }
                }
                catch
                {
                    //
                }

                string gymStat = isRaid ? "-raid" : null;
                string gymBoss = asBoss ? $"https://cdn.rawgit.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{(int)fort.RaidInfo.RaidPokemon.PokemonId}.png" : null;

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