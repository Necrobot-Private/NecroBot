using Pogo;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Event
{
    public class SnipeEvent : IEvent
    {
        public string Message = "";
        public override string ToString()
        {
            return Message;
        }
    }
    public class SnipeFailedEvent : IEvent
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId PokemonId { get; set; }
        public ulong EncounterId { get; set; }

        public Pokemon ToPokemon()
        {
            return new Pokemon
            {
                EncounterId = EncounterId.ToString()
            };
        }
    }
}