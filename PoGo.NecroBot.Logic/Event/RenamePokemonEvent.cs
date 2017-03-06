#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Event
{
    public class RenamePokemonEvent : IEvent
    {
        public ulong Id;
        public PokemonId PokemonId;
        public string OldNickname;
        public string NewNickname;
    }
}