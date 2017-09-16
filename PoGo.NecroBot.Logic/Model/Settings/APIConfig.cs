using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "API Config", Description = "Set Preferred API Type to use", ItemRequired = Required.DisallowNull)]
    public class APIConfig : BaseConfig
    {
        public APIConfig() : base()
        {
        }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UsePogoDevAPI { get; set; }

        [DefaultValue("")]
        [MinLength(0)]
        [MaxLength(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string AuthAPIKey { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public bool UseLegacyAPI { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public bool DiplayHashServerLog { get; set; }

        [DefaultValue("https://pokehash.buddyauth.com/")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public string UrlHashServices { get; set; }

        [DefaultValue(Constants.ApiEndPoint)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public string EndPoint { get; set; }
    }
}