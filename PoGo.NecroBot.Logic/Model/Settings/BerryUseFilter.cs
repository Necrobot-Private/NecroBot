using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PoGo.NecroBot.Logic.Common;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)] //Dont set Title
    public class ItemUseFilter : BaseConfig
    {
        public ItemUseFilter()
        {
            this.Pokemons = new List<PokemonId>();
        }

        public ItemUseFilter(int minIV, int minLV, int minCP, List<PokemonId> pokemons, string op = "or", double catchChange=0.3, int maxUse=10)
        {
            this.UseItemMinIV = minIV;
            this.CatchProbability = catchChange;
            this.UseItemMinLevel = minLV;
            this.UseItemMinCP = minCP;
            this.Pokemons = pokemons;
            this.Operator = op;
            this.MaxItemsUsePerPokemon = maxUse;
            this.UseIfExceedBagRecycleFilter = true;
        }
                         
        [NecrobotConfig(Key = "Min IV", Description = "Min IV to use this item", Position = 2)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int UseItemMinIV {get; set;}

        [NecrobotConfig(Key = "Min Level", Description = "Min LV  to use this item", Position = 3)]
        [DefaultValue(20)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int UseItemMinLevel { get; set; }

        [NecrobotConfig(Key = "Min CP", Description = "Min CP to use this item", Position = 4)]
        [DefaultValue(500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int UseItemMinCP { get; set; }

        [NecrobotConfig(Key = "Operator", Position = 6, Description = "The operator logic use to check for using the item")]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public string Operator { get; set; }

        [NecrobotConfig(Key = "Catch Probability ", Position = 6, Description = "Define catch change to use this item")]
        [DefaultValue(0.5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public double CatchProbability { get;  set; }

        [NecrobotConfig(Key = "Pokemons", Position = 6, Description = "Define list pokemon will apply this berries, empty is allow all")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public List<PokemonId> Pokemons { get; set; }

        [NecrobotConfig(Key = "MaxItemsUse", Position = 7, Description = "Define how many item will be use for same pokemon")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]

        public int MaxItemsUsePerPokemon { get; set; }


        [NecrobotConfig(Key = "UseIfExceedFilter", Position = 8, Description = "If your items exceed recycle filter, ot will always use this when possible")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        [DefaultValue(true)]
        public bool UseIfExceedBagRecycleFilter { get; set; }

        internal static Dictionary<ItemId, ItemUseFilter> Default()
        {
            return new Dictionary<ItemId, ItemUseFilter>
            {
                //use for hight catch flee
                {ItemId.ItemNanabBerry, new ItemUseFilter(50, 20, 500,  new List<PokemonId>() { PokemonId.Abra, PokemonId.Dragonite, PokemonId.Venusaur, PokemonId.Blastoise, PokemonId.Charizard }  , "and", 0.3 , 20) },
                //use for hight CP, low probability catch
                { ItemId.ItemRazzBerry, new ItemUseFilter(90, 0, 1500,  new List<PokemonId>() { }  , "and", 0.3 , 20) },
                //use for candy pokemon
                { ItemId.ItemPinapBerry, new ItemUseFilter(0, 0, 0,  new List<PokemonId>() { PokemonId.Lapras, PokemonId.Snorlax, PokemonId.Chansey, PokemonId.Dratini, PokemonId.Porygon, PokemonId.Porygon2 }  , "or", 1 ,10) },
            };
        }
    }
}