using System.Collections.Generic;
using POGOProtos.Data.Player;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;

namespace PoGo.NecroBot.Logic.Event.Inventory
{
    public class InventoryRefreshedEvent : IEvent
    {
        public IEnumerable<PlayerStats> PlayerStats { get; set; }
        
        public List<PokemonSettings> PokemonSettings { get; set; }

        public List<Candy> Candies { get; set; }

        public InventoryRefreshedEvent(List<PokemonSettings> settings, List<Candy> candy)
        {
            Candies = candy;
            PokemonSettings = settings;
        }

        public InventoryRefreshedEvent(IEnumerable<PlayerStats> playerStats,
            List<PokemonSettings> pokemonSettings, List<Candy> candy)
        {
            PlayerStats = playerStats;
            PokemonSettings = pokemonSettings;
            Candies = candy;
        }
    }
}