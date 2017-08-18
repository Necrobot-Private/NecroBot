using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Map.Fort;
using PokemonGo.RocketAPI.Extensions;
using System;
using TinyIoC;

namespace PoGo.NecroBot.Window.Model
{
    public class FortViewModel : ViewModelBase
    {
        private FortData fort;

        public string FortId => fort.Id;

        public double Latitude => fort.Latitude;
        public double Longitude => fort.Longitude;
        public double Distance { get; set; }

        public string FortName
        {
            get
            {
                // TODO - Need to store the fort name. For now just avoid binding errors.
                return "";
            }
        }

        public string FortStatusColor
        {
            get
            {
                // TODO - Need to return the correct status color.
                return "Red";
            }
        }

        public string FortIcon
        {
            get
            {
                string fortIcon = "";
                if (fort.LureInfo != null)
                {
                    if (fort.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/Lured.png";
                    else
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/VisitedLure.png";
                }
                else
                {
                    if (fort.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/Normal.png";
                    else
                        fortIcon = "https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/NecroEase/markers/Visited.png";
                }
                return fortIcon;
            }
        }

        public FortViewModel(FortData data)
        {
            Session = TinyIoCContainer.Current.Resolve<ISession>();
            fort = data;
            
            UpdateDistance(Session.Client.CurrentLatitude, Session.Client.CurrentLongitude);
        }

        public void UpdateFortData(FortData newFort)
        {
            var originalFort = fort;
            fort = newFort;

            RaisePropertyChanged("FortIcon");
        }

        protected bool IsVisited(FortData data)
        {
            return fort.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime();
        }

        internal void UpdateDistance(double lat, double lng)
        {
            Distance = LocationUtils.CalculateDistanceInMeters(lat, lng, Latitude, Longitude);
            RaisePropertyChanged("Distance");

        }
    }
}