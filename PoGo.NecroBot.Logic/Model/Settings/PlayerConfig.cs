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

        [NecroBotConfig(Description = "Sets delay time for evolve actions", Position = 2)]
        [DefaultValue(20000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int EvolveActionDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time for transfer actions", Position = 3)]
        [DefaultValue(5000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int TransferActionDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time for recycling items", Position = 4)]
        [DefaultValue(1000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int RecycleActionDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time for renaming pokemon actions", Position = 5)]
        [DefaultValue(2000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int RenamePokemonActionDelay { get; set; }

        [NecroBotConfig(Description = "Sets delay time for random actions", Position = 6)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public bool UseNearActionRandom { get; set; }

        [NecroBotConfig(Description = "Randomize numeric settings by percent.", Position = 7)]
        [DefaultValue(5)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int RandomizeSettingsByPercent { get; set; }

        [NecroBotConfig(Description = "Auto Complete first time experience tutorial (Bot will try to use your ptc username or firstpart of email as username)", Position = 8)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public bool AutoFinishTutorial { get;  set; }

        [NecroBotConfig(Description = "Skip first time experience tutorial", Position = 9)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool SkipFirstTimeTutorial { get; set; }

        [NecroBotConfig(Description = "Skip collecting level up rewards", Position = 10)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public bool SkipCollectingLevelUpRewards { get; set; }

        [NecroBotConfig(Description = "Sets the AutoWalkAI", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public bool AutoWalkAI { get; set; }

        [NecroBotConfig(Description = "Sets the AutoWalkAI Distance(Default = 250m)", Position = 12)]
        [DefaultValue(250)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public int AutoWalkDist { get; set; }

        [NecroBotConfig(Description = "Sets the LoadPokeStopsTimer interval(10 - 60 sec)", Position = 13)]
        [DefaultValue(30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public int PokeStopsTimer { get; set; }
    }
}
