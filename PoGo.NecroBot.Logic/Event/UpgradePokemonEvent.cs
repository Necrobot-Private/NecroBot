#region using directives

using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;

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

        public PokemonData Pokemon { get; set; }
    }
}