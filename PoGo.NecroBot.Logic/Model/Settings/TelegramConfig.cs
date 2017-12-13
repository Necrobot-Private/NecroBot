using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Telegram Messaging Client", Description = "Configure to use with Telegram Messaging.", ItemRequired = Required.DisallowNull)]
    public class TelegramConfig : BaseConfig
    {
        public TelegramConfig() : base()
        {
        }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecroBotConfig (Description = "Allows control of bot from Telegram commands", Position = 1)]
        public bool UseTelegramAPI { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(64)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecroBotConfig(Description = "Telegram API Key that's required for communication", Position = 2)]
        public string TelegramAPIKey { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecroBotConfig(Description = "Telegram password to connect", Position = 3)]
        public string TelegramPassword { get; set; }
    }
}