#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Event
{
    public class EggHatchedEvent : IEvent
    {
        public int Cp;
        public ulong Id;
        public double Level;
        public int MaxCp;
        public double Perfection;
        public PokemonId PokemonId;
        public double Dist;
        public long HXP;
        public int HSD;
        public int HCandy;
    }
}
