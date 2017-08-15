using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Location Config", Description = "Set your location settings.", ItemRequired = Required.DisallowNull)]
    public class LocationConfig : BaseConfig
    {
        public LocationConfig() : base()
        {
        }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecroBotConfig(Description = "When enabled, bot will teleport instead of walking. this is not recommended.", Position = 1)]
        public bool DisableHumanWalking { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecroBotConfig(Description = "When enabled, bot will start from the last known location instead of default location", Position = 2)]
        public bool StartFromLastPosition { get; set; }

        [DefaultValue(40.785092)]
        [Range(-90, 90)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecroBotConfig(Description = "Default Latitude that bot will start playing at.", Position = 3)]
        public double DefaultLatitude { get; set; }

        [DefaultValue(-73.968286)]
        [Range(-180, 180)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecroBotConfig(Description = "Default Longitude that bot will start playing at.", Position = 4)]
        public double DefaultLongitude { get; set; }

        [DefaultValue(24.98)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [NecroBotConfig(Description = "The walking speed that applies to bot for moving between pokestops.", Position = 5)]
        public double WalkingSpeedInKilometerPerHour { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        [NecroBotConfig(Description = "Turn this option on to add random speed variation into Walking Speed.", Position = 6)]
        public bool UseWalkingSpeedVariant { get; set; }

        [DefaultValue(1.2)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        [NecroBotConfig(Description = "The random speed add/minus into walking speed when UseWalkingSpeedVariant is set to true", Position = 7)]
        public double WalkingSpeedVariant { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        [NecroBotConfig(Description = "Display variant speed change in console window ", Position = 8)]
        public bool ShowVariantWalking { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        [NecroBotConfig(Description = "When Enabled, bot will stop at a pokestop randomly, making him more humanlike", Position = 9)]
        public bool RandomlyPauseAtStops { get; set; }

        [DefaultValue(10)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        [NecroBotConfig(Description = "Set random offset change when bot starts from default location", Position = 10)]
        public int MaxSpawnLocationOffset { get; set; }

        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        [NecroBotConfig(Description = "The radius distance for bot to travel from default location. Notice that this may be changed depending on other configs, as bot may walk out of that radius", Position = 11)]
        public int MaxTravelDistanceInMeters { get; set; }

        [JsonIgnore]
        public int ResumeTrack = 0;
        [JsonIgnore]
        public int ResumeTrackSeg = 0;
        [JsonIgnore]
        public int ResumeTrackPt = 0;
    }
}