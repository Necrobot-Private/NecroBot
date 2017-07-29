using System;
using System.Collections.Generic;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.Window.Model
{
    public class MapViewModel  : ViewModelBase
    {
        private double lat;
        private double lng;

        public MapViewModel()
        {
            NearbyPokemons = new ObservableCollectionExt<MapPokemonViewModel>();
        }
        public ObservableCollectionExt<MapPokemonViewModel> NearbyPokemons { get; set; }

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

        internal void OnEncounterEvent(EncounteredEvent encounteredEvent)
        {
            throw new NotImplementedException();
        }
    }
}
