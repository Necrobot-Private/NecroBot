using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "MapzenWalk Config", Description = "Set your mapzenwalk settings.", ItemRequired = Required.DisallowNull)]
    public class MapzenWalkConfig
    {
        internal enum MapzenWalkTravelModes
        {
            auto,
            bicycle,
            pedestrian
        }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UseMapzenWalk = false;

        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string MapzenTurnByTurnApiKey;

        [DefaultValue("bicycle")]
        [EnumDataType(typeof(MapzenWalkTravelModes))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public string MapzenWalkHeuristic = "bicycle";

        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string MapzenElevationApiKey;
    }
}