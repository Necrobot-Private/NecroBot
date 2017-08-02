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

        [NecrobotConfig(Description = "Allows bot to display lists of items to be recycled", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool VerboseRecycling { get; set; }

        [NecrobotConfig(Description = "Specify percentage of inventory full to start recycle", Position = 2)]
        [DefaultValue(90)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double RecycleInventoryAtUsagePercentage { get; set; }

        [NecrobotConfig(Description = "Turn on randomizing recycled items", Position = 3)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public bool RandomizeRecycle;

        [NecrobotConfig(Description = "Number of randomized items to be recycled", Position = 4)]
        [DefaultValue(3)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int RandomRecycleValue { get; set; }

        /*Amounts*/
        [NecrobotConfig(Description = "How many pokeballs (normal, great, ultra) to be kept ", Position = 5)]
        [DefaultValue(120)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int TotalAmountOfPokeballsToKeep { get; set; }

        [NecrobotConfig(Description = "How many potions (normal, hyper, ultra, max) to be kept ", Position = 6)]
        [DefaultValue(80)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int TotalAmountOfPotionsToKeep { get; set; }

        [NecrobotConfig(Description = "How many revives (normal, max) to be kept ", Position = 7)]
        [DefaultValue(60)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int TotalAmountOfRevivesToKeep { get; set; }

        [NecrobotConfig(Description = "How many berries to be kept ", Position = 8)]
        [DefaultValue(50)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public int TotalAmountOfBerriesToKeep { get; set; }

        [NecrobotConfig(Description = "How many Evolution to be kept ", Position = 8)]
        [DefaultValue(50)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public int TotalAmountOfEvolutionToKeep { get; set; }

        /* Percents */
        [NecrobotConfig(Description = "Use recycle percents instead of totals (for example PercentOfInventoryPokeballsToKeep instead of TotalAmountOfPokeballsToKeep) ", Position = 10)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public bool UseRecyclePercentsInsteadOfTotals { get; set; }

        [NecrobotConfig(Description = "How many pokeballs (normal, great, ultra) to be kept as a percent of inventory ", Position = 11)]
        [DefaultValue(35)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public int PercentOfInventoryPokeballsToKeep { get; set; }

        [NecrobotConfig(Description = "How many potions (normal, hyper, ultra, max) to be kept as a percent of inventory ", Position = 12)]
        [DefaultValue(35)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public int PercentOfInventoryPotionsToKeep { get; set; }

        [NecrobotConfig(Description = "How many revives (normal, max) to be kept as a percent of inventory ", Position = 13)]
        [DefaultValue(20)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public int PercentOfInventoryRevivesToKeep { get; set; }

        [NecrobotConfig(Description = "How many berries to be kept as a percent of inventory ", Position = 14)]
        [DefaultValue(10)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public int PercentOfInventoryBerriesToKeep { get; set; }

        [NecrobotConfig(Description = "How many evolution to be kept as a percent of inventory ", Position = 14)]
        [DefaultValue(10)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public int PercentOfInventoryEvolutionToKeep { get; set; }
    }
}