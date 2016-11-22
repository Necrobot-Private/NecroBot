#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.LIB.Event
{
    public class NoPokeballEvent : IEvent
    {
        public int Cp;
        public PokemonId Id;
    }
}