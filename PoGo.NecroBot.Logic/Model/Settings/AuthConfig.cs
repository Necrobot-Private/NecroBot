using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PokemonGo.RocketAPI.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Authentication Config",
         Description = "Set your authentication type (Google or Ptc) and your login informartion.",
         ItemRequired = Required.DisallowNull)]
    public class AuthConfig  :BaseConfig
    {
        public AuthConfig() :base() { }
        [DefaultValue(AuthType.Google)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public AuthType AuthType { get; set; }

        [DefaultValue(null)]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?")] // Valid email
        [MinLength(0)]
        [MaxLength(64)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string GoogleUsername { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(50)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public string GooglePassword { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string PtcUsername { get; set; }

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public string PtcPassword { get; set; }

        [JsonIgnore]
        public double RuntimeTotal { get; set; }
        [JsonIgnore]
        public DateTime ReleaseBlockTime { get; set; }
    }
}