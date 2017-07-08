#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Event
{
    public class TransferPokemonEvent : IEvent
    {
        public int BestCp;
        public double BestPerfection;
        public int Cp;
        public PokemonId PokemonId;
        public double Perfection;
        public ulong Id;
        public int Candy { get; internal set; }
        public PokemonFamilyId FamilyId { get; internal set; }
        public double Level;
        public bool Slashed;
    }
}