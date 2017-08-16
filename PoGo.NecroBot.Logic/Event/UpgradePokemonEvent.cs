#region using directives

using POGOProtos.Data;
using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Event
{
    public class UpgradePokemonEvent : IEvent
    {
        public int BestCp;
        public double BestPerfection;
        public int Cp;
        public int Candy;
        public PokemonId PokemonId;
        public ulong Id;
        public double Perfection;
        public int USD;
        public double Lvl;

        public PokemonData Pokemon { get; set; }
    }
}
