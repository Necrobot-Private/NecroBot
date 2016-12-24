#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.LIB.Event
{
    public class SnipeScanEvent : IEvent
    {
        public Location Bounds { get; set; }
        public PokemonId PokemonId { get; set; }
        public double Iv { get; set; }
        public string Source { get; set; }
    }
}