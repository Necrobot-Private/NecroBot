using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Custom Catch Config", Description = "Set your custom catch settings.", ItemRequired = Required.DisallowNull)]
    public class CustomCatchConfig :BaseConfig
    {
        public CustomCatchConfig() : base()
        {
        }

        [NecroBotConfig(Description = "Allows bot to simulate throws as humanlike as possible", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableHumanizedThrows { get; set; }

        [NecroBotConfig(Description = "Allow bot throws to miss pokemon", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public bool EnableMissedThrows { get; set; }

        [NecroBotConfig(Description = "Set percentage for how many pokemon bot can miss", Position = 3)]
        [DefaultValue(25)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int ThrowMissPercentage { get; set; }

        [NecroBotConfig(Description = "Set percentage for how many bot can throw balls with nice hits", Position = 4)]
        [DefaultValue(40)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int NiceThrowChance { get; set; }

        [NecroBotConfig(Description = "Set percentage for how many bot can throw balls with great hits", Position = 5)]
        [DefaultValue(30)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int GreatThrowChance { get; set; }

        [NecroBotConfig(Description = "Set percentage for how many bot can throw balls with excellent hits", Position = 6)]
        [DefaultValue(10)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int ExcellentThrowChance { get; set; }

        [NecroBotConfig(Description = "Set percentage for how many bot can throw balls with curve hits", Position = 7)]
        [DefaultValue(90)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int CurveThrowChance { get; set; }

        [NecroBotConfig(Description = "Forces bot to get a great throw if IV is higher than this value", Position = 8)]
        [DefaultValue(90.00)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public double ForceGreatThrowOverIv { get; set; }

        [NecroBotConfig(Description = "Forces bot to get a excellent throw if IV is higher than this value", Position = 9)]
        [DefaultValue(95.00)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public double ForceExcellentThrowOverIv { get; set; }

        [NecroBotConfig(Description = "Forces bot to get a great throw if CP higher than this value", Position = 10)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public int ForceGreatThrowOverCp { get; set; }

        [NecroBotConfig(Description = "Forces bot to get an excellent throw if CP higher than this value", Position = 11)]
        [DefaultValue(1500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public int ForceExcellentThrowOverCp { get; set; }

        [NecroBotConfig(Description = "Allow bot use transfer filter to catch pokemon - ", Position = 12)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public bool UseTransferFilterToCatch { get; set; }
    }
}