using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(
         Title = "Console Config",
         Description = "Set your console settings.",
         ItemRequired = Required.DisallowNull
     )]
    public class ConsoleConfig : BaseConfig
    {
        public ConsoleConfig() : base()
        {
        }

        [DefaultValue("en")]
        [RegularExpression(@"^[a-zA-Z]{2}(-[a-zA-Z]{2})*$")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecrobotConfig(SheetName = "ConsoleConfig", Position = 1, Description = "Language Transation code (ex: en = english)")]
        public string TranslationLanguageCode { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecrobotConfig(Position = 2, Description = "If enabled, bot will display a welcome message on startup")]
        public bool StartupWelcomeDelay { get; set; }

        [DefaultValue(2)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecrobotConfig(Position = 3, Description = "Amount Of Pokemon To Display On Start")]
        public int AmountOfPokemonToDisplayOnStart { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 4, Description = "If Enabled, Bot will display a Message on hitting the level limit")]
        public bool EnableLevelLimit { get; set; }

        [DefaultValue(35)]
        [Range(0, 40)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [NecrobotConfig(Position = 5, Description = "Have the Bot warn you if Reached this Level")]
        public int LevelLimit { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        [NecrobotConfig(Position = 6, Description = "Detailed Inventory Count to Display Before Recycling")]
        public bool DetailedCountsBeforeRecycling { get; set; }
    }
}