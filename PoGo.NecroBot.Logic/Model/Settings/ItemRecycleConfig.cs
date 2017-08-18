using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Recycle Config", Description = "Set your recycle settings.", ItemRequired = Required.DisallowNull)]
    public class ItemRecycleConfig  : BaseConfig
    {
        public ItemRecycleConfig() : base()
        {
        }

        [NecroBotConfig(Description = "Allows bot to display lists of items to be recycled", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool VerboseRecycling { get; set; }

        [NecroBotConfig(Description = "Specify percentage of inventory full to start recycle", Position = 2)]
        [DefaultValue(80)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double RecycleInventoryAtUsagePercentage { get; set; }

        [NecroBotConfig(Description = "Turn on randomizing recycled items", Position = 3)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public bool RandomizeRecycle;

        [NecroBotConfig(Description = "Number of randomized items to be recycled", Position = 4)]
        [DefaultValue(3)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int RandomRecycleValue { get; set; }

        /*Amounts*/
        [NecroBotConfig(Description = "How many pokeballs (normal, great, ultra) to be kept ", Position = 5)]
        [DefaultValue(100)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int TotalAmountOfPokeballsToKeep { get; set; }

        [NecroBotConfig(Description = "How many potions (normal, hyper, ultra, max) to be kept ", Position = 6)]
        [DefaultValue(75)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int TotalAmountOfPotionsToKeep { get; set; }

        [NecroBotConfig(Description = "How many revives (normal, max) to be kept ", Position = 7)]
        [DefaultValue(75)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int TotalAmountOfRevivesToKeep { get; set; }

        [NecroBotConfig(Description = "How many berries to be kept ", Position = 8)]
        [DefaultValue(45)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public int TotalAmountOfBerriesToKeep { get; set; }

        [NecroBotConfig(Description = "How many Evolution to be kept ", Position = 9)]
        [DefaultValue(25)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public int TotalAmountOfEvolutionToKeep { get; set; }

        /* Percents */
        [NecroBotConfig(Description = "Use recycle percents instead of totals (for example PercentOfInventoryPokeballsToKeep instead of TotalAmountOfPokeballsToKeep) ", Position = 10)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public bool UseRecyclePercentsInsteadOfTotals { get; set; }

        [NecroBotConfig(Description = "How many pokeballs (normal, great, ultra) to be kept as a percent of inventory ", Position = 11)]
        [DefaultValue(35)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public int PercentOfInventoryPokeballsToKeep { get; set; }

        [NecroBotConfig(Description = "How many potions (normal, hyper, ultra, max) to be kept as a percent of inventory ", Position = 12)]
        [DefaultValue(35)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public int PercentOfInventoryPotionsToKeep { get; set; }

        [NecroBotConfig(Description = "How many revives (normal, max) to be kept as a percent of inventory ", Position = 13)]
        [DefaultValue(20)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public int PercentOfInventoryRevivesToKeep { get; set; }

        [NecroBotConfig(Description = "How many berries to be kept as a percent of inventory ", Position = 14)]
        [DefaultValue(10)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public int PercentOfInventoryBerriesToKeep { get; set; }

        [NecroBotConfig(Description = "How many evolution to be kept as a percent of inventory ", Position = 15)]
        [DefaultValue(10)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        public int PercentOfInventoryEvolutionToKeep { get; set; }
    }
}