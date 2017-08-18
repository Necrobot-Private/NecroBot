using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "MapzenWalk Config", Description = "Set your MapzenWalk settings.", ItemRequired = Required.DisallowNull)]
    public class MapzenWalkConfig    : BaseConfig
    {
        public MapzenWalkConfig() : base()
        {
        }

        internal enum MapzenWalkTravelModes
        {
            auto,
            bicycle,
            pedestrian
        }

        [NecroBotConfig(Description = "Allows bot to use Mapzen API to resolve path", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UseMapzenWalk { get; set; }

        [NecroBotConfig(Description = "API Key used to connect to Mapzen Services", Position = 2)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string MapzenTurnByTurnApiKey { get; set; }

        [NecroBotConfig(Description = "Set the heuristic to find routes", Position = 3)]
        [DefaultValue("bicycle")]
        [EnumDataType(typeof(MapzenWalkTravelModes))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public string MapzenWalkHeuristic { get; set; }

        [NecroBotConfig(Description = "API Key used to connect to Mapzen API Elevation Services", Position = 4)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string MapzenElevationApiKey { get; set; }
    }
}