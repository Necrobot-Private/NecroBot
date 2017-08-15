using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Item Recycle Filter", Description = "", ItemRequired = Required.DisallowNull)]
    public class ItemRecycleFilter :BaseConfig
    {
        public ItemRecycleFilter() :base()
        {
        }

        public ItemRecycleFilter(ItemId key, int value)
        {
            Key = key;
            Value = value;
        }

        [NecroBotConfig(Description ="Item Name")]
        [DefaultValue(ItemId.ItemUnknown)]
        [JsonProperty(Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public ItemId Key { get; set; }

        [NecroBotConfig(Description = "Item Amount to keep")]
        [DefaultValue(0)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int Value { get; set; }

        internal static List<ItemRecycleFilter> ItemRecycleFilterDefault()
        {
            return new List<ItemRecycleFilter>
            {
                new ItemRecycleFilter(ItemId.ItemUnknown, 0),
                new ItemRecycleFilter(ItemId.ItemLuckyEgg, 200),
                new ItemRecycleFilter(ItemId.ItemIncenseOrdinary, 100),
                new ItemRecycleFilter(ItemId.ItemIncenseSpicy, 100),
                new ItemRecycleFilter(ItemId.ItemIncenseCool, 100),
                new ItemRecycleFilter(ItemId.ItemIncenseFloral, 100),
                new ItemRecycleFilter(ItemId.ItemTroyDisk, 100),
                new ItemRecycleFilter(ItemId.ItemXAttack, 100),
                new ItemRecycleFilter(ItemId.ItemXDefense, 100),
                new ItemRecycleFilter(ItemId.ItemXMiracle, 100),
                new ItemRecycleFilter(ItemId.ItemSpecialCamera, 100),
                new ItemRecycleFilter(ItemId.ItemIncubatorBasicUnlimited, 100),
                new ItemRecycleFilter(ItemId.ItemIncubatorBasic, 100),
                new ItemRecycleFilter(ItemId.ItemPokemonStorageUpgrade, 100),
                new ItemRecycleFilter(ItemId.ItemItemStorageUpgrade, 100),
                new ItemRecycleFilter(ItemId.ItemPokeBall, 50),
                new ItemRecycleFilter(ItemId.ItemRevive, 10),
                new ItemRecycleFilter(ItemId.ItemPotion, 10),
                new ItemRecycleFilter(ItemId.ItemHyperPotion, 10),
                new ItemRecycleFilter(ItemId.ItemGreatBall, 100),
                new ItemRecycleFilter(ItemId.ItemBlukBerry, 30),
                new ItemRecycleFilter(ItemId.ItemNanabBerry, 30),
                new ItemRecycleFilter(ItemId.ItemWeparBerry, 30),
                new ItemRecycleFilter(ItemId.ItemPinapBerry, 30),
                new ItemRecycleFilter(ItemId.ItemGoldenNanabBerry, 30),
                new ItemRecycleFilter(ItemId.ItemGoldenPinapBerry, 30),
                new ItemRecycleFilter(ItemId.ItemGoldenRazzBerry, 30),
                new ItemRecycleFilter(ItemId.ItemDragonScale, 10),
                new ItemRecycleFilter(ItemId.ItemKingsRock, 10),
                new ItemRecycleFilter(ItemId.ItemSunStone, 10),
                new ItemRecycleFilter(ItemId.ItemMetalCoat, 10),
                new ItemRecycleFilter(ItemId.ItemUpGrade, 10)
            };
        }
    }
}