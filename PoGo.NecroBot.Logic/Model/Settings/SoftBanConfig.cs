using System.ComponentModel;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Soft Ban Config", Description = "Set your soft ban settings.", ItemRequired = Required.DisallowNull)]
    public class SoftBanConfig     : BaseConfig
    {
        public SoftBanConfig() : base()
        {
        }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecroBotConfig(Description = "Allows bot to resolve softbans automatically", Position = 1)]
        public bool FastSoftBanBypass { get; set; }

        [DefaultValue(1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecroBotConfig(Description = "Bypass pokestop spin count.", Position = 2)]
        public int ByPassSpinCount { get; set; }
    }
}