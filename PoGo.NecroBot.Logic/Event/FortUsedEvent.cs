using POGOProtos.Data;
using POGOProtos.Map.Fort;

namespace PoGo.NecroBot.Logic.Event
{
    public class FortUsedEvent : IEvent
    {
        public int Exp;
        public string Gems;
        public string Id;
        public bool InventoryFull;
        public string Items;
        public string Badges;
        public string BonusLoot;
        public string RaidTickets;
        public string TeamBonusLoot;
        public PokemonData PokemonDataEgg;
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public string Name;
        public FortData Fort;
    }
}