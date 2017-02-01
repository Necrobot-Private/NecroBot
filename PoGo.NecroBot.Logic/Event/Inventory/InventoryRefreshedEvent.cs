using System.Collections.Generic;
using POGOProtos.Data.Player;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
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
            this.Candies = candy;
            this.PokemonSettings = settings;
        }

        public InventoryRefreshedEvent(IEnumerable<PlayerStats> playerStats,
            List<PokemonSettings> pokemonSettings, List<Candy> candy)
        {
            this.PlayerStats = playerStats;
            this.PokemonSettings = pokemonSettings;
            this.Candies = candy;
        }
    }
}