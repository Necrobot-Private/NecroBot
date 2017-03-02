using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "GUI Config", Description = "Setting up all config related to GUI", ItemRequired = Required.DisallowNull)]

    public class GUIConfig : BaseConfig
    {
        public GUIConfig() : base()
        {
        }

        [NecrobotConfig(Description = "The number of seconds that bot display auto snipe data", Position = 1)]
        [DefaultValue(150)]
        [Range(0, 900)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeItemListDisplayTime { get; set; }

        [NecrobotConfig(Description = "Interval to refresh auto snipe data ", Position = 1)]
        [DefaultValue(10)]
        [Range(0, 30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeListRefreshInterval { get; set; }


    }

}
