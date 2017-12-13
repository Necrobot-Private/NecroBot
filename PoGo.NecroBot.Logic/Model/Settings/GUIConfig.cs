using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "GUI Config", Description = "All settings related to GUI", ItemRequired = Required.DisallowNull)]

    public class GUIConfig : BaseConfig
    {
        public GUIConfig() : base()
        {
        }

        [NecroBotConfig(Description = "The number of seconds that the bot will display auto snipe data", Position = 1)]
        [DefaultValue(150)]
        [Range(0, 900)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeItemListDisplayTime { get; set; }

        [NecroBotConfig(Description = "Interval to refresh auto snipe data ", Position = 1)]
        [DefaultValue(10)]
        [Range(0, 30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeListRefreshInterval { get; set; }


    }

}
