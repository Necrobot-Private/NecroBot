using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.PoGoUtils
{
    //mabe there a game master setting for this already 

    public class PokemonEvolutionHelper
    {
        private static Dictionary<ItemId, List<PokemonId>> itemRequirements = new Dictionary<ItemId, List<PokemonId>>()
        {
            {ItemId.ItemDragonScale, new List<PokemonId>() {PokemonId.Seadra } },
            { ItemId.ItemKingsRock, new List<PokemonId>() {PokemonId.Poliwhirl, PokemonId.Slowpoke } },
            { ItemId.ItemMetalCoat, new List<PokemonId>() {PokemonId.Scyther, PokemonId.Onix} },
            { ItemId.ItemUpGrade, new List<PokemonId>() {PokemonId.Porygon2} },
            { ItemId.ItemSunStone, new List<PokemonId>() {PokemonId.Gloom, PokemonId.Sunkern } }
        };
        public ItemId GetEvolutionItemRequirement(PokemonId pokemon)
        {
            foreach (var item in itemRequirements)
            {
                if (item.Value.Contains(pokemon)) return item.Key;
            }
            return ItemId.ItemUnknown;
        }
    }
}
