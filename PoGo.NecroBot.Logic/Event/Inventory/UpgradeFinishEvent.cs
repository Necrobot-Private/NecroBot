using POGOProtos.Data;

namespace PoGo.NecroBot.Logic.Event.Inventory
{
    public class FinishUpgradeEvent : IEvent
    {
        public ulong PokemonId { get; set; }
        public PokemonData Pokemon { get; set; }
    }
}