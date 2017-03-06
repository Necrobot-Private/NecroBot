using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;

namespace PoGo.Necrobot.Window.Model
{
    public class MapPokemonViewModel : PokemonViewModelBase
    {
        private NearbyPokemon item;
        private FortData fort;

        public MapPokemonViewModel(NearbyPokemon item, FortData fort)
        {
            this.item = item;
            
            this.PokemonId = item.PokemonId;

            if (fort != null)
            {
                this.fort = fort;
                this.Latitude = fort.Latitude;
                this.Longitude = fort.Longitude;
            }

            this.EncounterId = item.EncounterId;
            this.Distance = item.DistanceInMeters;
            this.FortId = item.FortId;
        }
        public double Latitude{ get; set; }
        public double Longitude { get; set; }
        public DateTime Time { get; set; }
        public ulong EncounterId { get; set; }
        public double Distance { get; set; }
        public string FortId { get; set; }
    }
}
