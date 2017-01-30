using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
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
    public class FortViewModel : ViewModelBase
    {
        private FortData fort;

        public string FortId => fort.Id;

        public double Latitude => fort.Latitude;
        public double Longitude => fort.Longitude;
        public double Distance { get; set; }

        public string FortIcon
        {
            get
            {
                string fortIcon = "";
                if (fort.LureInfo != null)
                {
                    if (fort.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                        fortIcon = "images/Lured.png";
                    else
                        fortIcon = "images/VisitedLure.png";
                }
                else
                {
                    if (fort.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                        fortIcon = "images/Normal.png";
                    else
                        fortIcon = "images/Visited.png";
                }
                return fortIcon;
            }
        }
        public FortViewModel(FortData data)
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
