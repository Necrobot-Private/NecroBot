using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Poke Stop Config", Description = "Set your poke stop settings.", ItemRequired = Required.DisallowNull)]
    public class PokeStopConfig : BaseConfig
    {
        public PokeStopConfig() : base()
        {
        }

        [NecroBotConfig(Description = "Allows bot to check for pokestop daily limit - PokeStopLimit per PokeStopLimitMinutes", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UsePokeStopLimit { get; set; }

        [NecroBotConfig(Description = "Max number of pokestops bot is allowed to farm a day", Position = 2)]
        [DefaultValue(700)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int PokeStopLimit {get; set; }

        [NecroBotConfig(Description = "Time duration apply for the limit above in minutes", Position = 3)]
        [DefaultValue(60 * 22 + 30)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int PokeStopLimitMinutes { get; set; }
    }
}