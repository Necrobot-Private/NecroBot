using System.ComponentModel;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Update Config", Description = "Set your update settings.", ItemRequired = Required.DisallowNull)]
    public class UpdateConfig : BaseConfig
    {
        public UpdateConfig() : base()
        {
        }

        public const int CURRENT_SCHEMA_VERSION = 29;

        [DefaultValue(CURRENT_SCHEMA_VERSION)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecroBotConfig(Description = "Allows bot to automatically check for latest version, and it will display message on console if an update is available.", Position = 1)]
        public int SchemaVersion { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecroBotConfig(Description = "Allows bot to automatically update to latest version", Position = 2)]
        public bool CheckForUpdates { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecroBotConfig(Description = "Transfer existing config when bot updates", Position = 3)]
        public bool AutoUpdate { get; set; }
    }
}