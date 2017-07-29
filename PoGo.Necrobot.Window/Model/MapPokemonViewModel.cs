using System;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;

namespace PoGo.NecroBot.Window.Model
{
    public class MapPokemonViewModel : PokemonViewModelBase
    {
        private NearbyPokemon item;
        private FortData fort;

        public MapPokemonViewModel(NearbyPokemon item, FortData fort)
        {
            this.item = item;

            PokemonId = item.PokemonId;

            if (fort != null)
            {
                this.fort = fort;
                Latitude = fort.Latitude;
                Longitude = fort.Longitude;
            }

            EncounterId = item.EncounterId;
            Distance = item.DistanceInMeters;
            FortId = item.FortId;
        }
        public double Latitude{ get; set; }
        public double Longitude { get; set; }
        public DateTime Time { get; set; }
        public ulong EncounterId { get; set; }
        public double Distance { get; set; }
        public string FortId { get; set; }
    }
}
