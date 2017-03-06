using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    case POGOProtos.Enums.TeamColor.Neutral:
                        fortIcon = "images/gym-unoccupied.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Blue:
                        fortIcon = "images/gym-mystic.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Red:
                        fortIcon = "images/gym-valor.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Yellow:
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
                    case POGOProtos.Enums.TeamColor.Neutral:
                        fortIcon = "images/team-unoccupied.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Blue:
                        fortIcon = "images/team-mystic.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Red:
                        fortIcon = "images/team-valor.png";
                        break;
                    case POGOProtos.Enums.TeamColor.Yellow:
                        fortIcon = "images/team-instinct.png";
                        break;
                }
                return fortIcon;
            }
        }

        public GymViewModel(FortData data)
        {
            this.Session = TinyIoCContainer.Current.Resolve<ISession>();
            this.fort = data;
            
            UpdateDistance(this.Session.Client.CurrentLatitude, this.Session.Client.CurrentLongitude);
        }

        internal void UpdateDistance(double lat, double lng)
        {
            this.Distance = LocationUtils.CalculateDistanceInMeters(lat, lng, Latitude, Longitude);
            RaisePropertyChanged("Distance");

        }
    }
}
