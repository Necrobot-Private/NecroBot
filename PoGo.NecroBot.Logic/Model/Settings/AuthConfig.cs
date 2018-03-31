using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PokemonGo.RocketAPI.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Authentication Config",
         Description = "Set your Authentication type (Google or Ptc) and your login information.",
         ItemRequired = Required.DisallowNull)]
    public class AuthConfig  :BaseConfig
    {
        public AuthConfig() : base() { }
        [DefaultValue(AuthType.Google)]
        //[JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public AuthType AuthType { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(64)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string Username { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(50)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public string Password { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public bool AutoExitBotIfAccountFlagged { get; set; }

        [DefaultValue(40.781441)]
        [Range(-90, 90)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public double AccountLatitude { get; set; }

        [DefaultValue(-73.966586)]
        [Range(-180, 180)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public double AccountLongitude { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public bool AccountActive { get; set; }

        //TimeZone Player locale settings
        [DefaultValue("US")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public string Country { get; set; }

        [DefaultValue("en")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public string Language { get; set; }

        [DefaultValue("America/New_York")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public string TimeZone { get; set; }

        [DefaultValue("en-us")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public string POSIX { get; set; }

        [DefaultValue(0)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public double RunStart { get; set; }

        [DefaultValue(86399)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public double RunEnd { get; set; }

        // Total runtime since client started
        [JsonIgnore]
        public double RuntimeTotal { get; set; }
        
        [JsonIgnore]
        public DateTime LastRuntimeUpdatedAt { get; set; }

        [JsonIgnore]
        public DateTime ReleaseBlockTime { get; set; }
    }
}
