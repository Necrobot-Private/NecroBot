using System;
using POGOProtos.Enums;
using Pogo;

namespace PoGo.NecroBot.Logic.Event
{
    public class EncounteredEvent : IEvent
    {
        public PokemonId PokemonId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double IV { get; set; }
        public int Level { get; set; }
        public DateTime Expires { get; set; }
        public double ExpireTimestamp { get; set; }
        public string SpawnPointId { get; set; }
        public string EncounterId { get; set; }
        public string Move1 { get; set; }
        public string Move2 { get; set; }
        public bool IsRecievedFromSocket { get; set; }
        public string RecieverId { get; set; }

        public Pokemon ToPokemon()
        {
            return new Pokemon
            {
                Latitude = Latitude,
                Longitude = Longitude,
                EncounterId = EncounterId,
                SpawnPointId = SpawnPointId,
                PokemonId = (int)PokemonId,
                Level = Level,
                Iv = IV,
                Move1 = Move1,
                Move2 = Move2,
                ExpiredTime = ExpireTimestamp
            };
        }
    }
}