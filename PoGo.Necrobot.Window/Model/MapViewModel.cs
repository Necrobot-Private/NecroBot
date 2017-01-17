using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class MapViewModel  : ViewModelBase
    {
        private double lat;
        private double lng;
        public double CurrentLatitude { get { return lat; }
            set
            {
                lat = value;
                RaisePropertyChanged("CurrentLatitude");
            }
        }
        public double CurrentLongitude
        {
            get { return lng; }
            set
            {
                lng = value;
                RaisePropertyChanged("CurrentLongitude");
            }
        }
    }
}
