using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class HumanlikeDelays : BaseConfig
    {
        public HumanlikeDelays() : base()
        {
        }

        [NecroBotConfig(Description = "Enables the usage of Human-Like Delays", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UseHumanlikeDelays { get; set; }

        [NecroBotConfig(Description = "Sets delay time when a catch is successfull", Position = 2)]
        [DefaultValue(13000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int CatchSuccessDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time when a catch is error", Position = 3)]
        [DefaultValue(1000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int CatchErrorDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time when a catch escapes", Position = 4)]
        [DefaultValue(3500)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int CatchEscapeDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time when a catch flees", Position = 5)]
        [DefaultValue(2000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int CatchFleeDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time when a catch is missed", Position = 6)]
        [DefaultValue(1000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int CatchMissedDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time when throwing the ball before catch", Position = 7)]
        [DefaultValue(1500)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int BeforeCatchDelay { get; set; }
    }
}
