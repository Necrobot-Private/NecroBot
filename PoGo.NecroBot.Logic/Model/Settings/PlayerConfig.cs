using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Player Config", Description = "Set your player settings.", ItemRequired = Required.DisallowNull)]
    public class PlayerConfig  :BaseConfig
    {
        public PlayerConfig() : base()
        {
        }

        [DefaultValue(4000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int DelayBetweenPlayerActions { get; set; }

        [NecrobotConfig(Description = "Set delay time for evolve action", Position = 2)]
        [DefaultValue(20000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int EvolveActionDelay { get; set; }

        [NecrobotConfig(Description = "Set delay time for transfer action", Position = 3)]
        [DefaultValue(5000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int TransferActionDelay { get; set; }

        [NecrobotConfig(Description = "Set delay time for recycle item", Position = 4)]
        [DefaultValue(1000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int RecycleActionDelay { get; set; }

        [NecrobotConfig(Description = "Set delay time for rename pokemon action", Position = 5)]
        [DefaultValue(2000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int RenamePokemonActionDelay { get; set; }

        [NecrobotConfig(Description = "Set delay time for random action", Position = 6)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public bool UseNearActionRandom { get; set; }

        [NecrobotConfig(Description = "Randomize numeric settings by percent.", Position = 7)]
        [DefaultValue(5)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int RandomizeSettingsByPercent { get; set; }

        [NecrobotConfig(Description = "Auto finish first time experience tutorial, bot will use your ptc username or firstpart of email as username", Position = 8)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public bool AutoFinishTutorial { get;  set; }
    }
}