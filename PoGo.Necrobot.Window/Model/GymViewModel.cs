using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
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
                switch (fort.OwnedByTeam)
                {
                    case TeamColor.Neutral:
                        fortIcon = "images/gym-unoccupied.png";
                        break;
                    case TeamColor.Blue:
                        fortIcon = "images/gym-mystic.png";
                        break;
                    case TeamColor.Red:
                        fortIcon = "images/gym-valor.png";
                        break;
                    case TeamColor.Yellow:
                        fortIcon = "images/gym-instinct.png";
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
                        fortIcon = "images/team-unoccupied.png";
                        break;
                    case TeamColor.Blue:
                        fortIcon = "images/team-mystic.png";
                        break;
                    case TeamColor.Red:
                        fortIcon = "images/team-valor.png";
                        break;
                    case TeamColor.Yellow:
                        fortIcon = "images/team-instinct.png";
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
