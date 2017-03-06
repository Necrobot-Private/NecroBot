using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Snipe Config", Description = "Set your snipe settings.", ItemRequired = Required.DisallowNull)]
    public class SnipeConfig : BaseConfig
    {
        [NecrobotConfig(Description = "Tell bot to use location service, detail at  - https://github.com/5andr0/PogoLocationFeeder", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UseSnipeLocationServer { get; set; }

        [NecrobotConfig(Description = "IP Address or server name of location server, ussually that is localhost", Position = 2)]
        [DefaultValue("localhost")]
        [MinLength(0)]
        [MaxLength(32)]
        //[RegularExpression(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")] //Ip Only
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string SnipeLocationServer { get; set; }

        [NecrobotConfig(Description = "Port number of location server. ", Position = 3)]
        [DefaultValue(16969)]
        [Range(1, 65535)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int SnipeLocationServerPort { get; set; }

        [NecrobotConfig(Description = "Number of ball in inventory to get sniper function work. ", Position = 9)]
        [DefaultValue(20)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public int MinPokeballsToSnipe { get; set; }

        [NecrobotConfig(Description = "Min ball allow to exist sniper", Position = 10)]
        [DefaultValue(0)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public int MinPokeballsWhileSnipe { get; set; }

        [NecrobotConfig(Description = "Delay time between 2 snipe. ", Position = 11)]
        [DefaultValue(60000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public int MinDelayBetweenSnipes { get; set; }

        [NecrobotConfig(Description = "The area bot try to scan for target pokemon. ", Position = 12)]
        [DefaultValue(0.005)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public double SnipingScanOffset { get; set; }

        [NecrobotConfig(Description = "That setting will make bot snipe when he reach every pokestop.", Position = 13)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public bool SnipeAtPokestops { get; set; }

        [DefaultValue(false)]
        [NecrobotConfig(Description = "Turn it on to ignore that pokemon with unknow IV from data source", Position = 14)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public bool SnipeIgnoreUnknownIv { get; set; }

        [NecrobotConfig(Description = "Bot will transfer pokemon for snipe if the snipping pokemon higher IV . ", Position = 15)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        public bool UseTransferIvForSnipe { get; set; }

        [NecrobotConfig(Description = "Turn this on it mean bot only priority to snipe pokemon not in pokedex.", Position = 16)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 16)]
        public bool SnipePokemonNotInPokedex { get; set; }

        /*SnipeLimit*/
        [NecrobotConfig(Description = "Turn on to limit the speed for sniping. ", Position = 17)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 17)]
        public bool UseSnipeLimit { get; set; }

        [NecrobotConfig(Description = "Delay time between 2 snipes ", Position = 18)]
        [DefaultValue(10 * 60)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 18)]
        public int SnipeRestSeconds { get; set; }

        [NecrobotConfig(Description = "Limit number of snipe in hour. ", Position = 19)]
        [DefaultValue(39)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 19)]
        public int SnipeCountLimit { get; set; }

        [NecrobotConfig(Description = "Allow MSniper feature with bot. ", Position = 20)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 20)]
        public bool ActivateMSniper = true;

        [NecrobotConfig(Description = "Min IV that bot will automatically snipe pokemon", Position = 21)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 21)]
        public int MinIVForAutoSnipe { get; set; }

        [NecrobotConfig(Description = "Min Level that bot will automatically snipe pokemon", Position = 22)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 22)]
        public int MinLevelForAutoSnipe { get; set; }

        [NecrobotConfig(Description = "Only auto snipe pokemon has been verified (overwriteable by invidual pokemon)", Position = 23)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 23)]
        public bool AutosnipeVerifiedOnly { get; set; }

        [NecrobotConfig(Description = "Set the amount of candy want bot auto snipe if we has less candy than this value.", Position = 24)]
        [DefaultValue(0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 24)]
        public int DefaultAutoSnipeCandy { get; set; }

        [NecrobotConfig(Description = "Total time in minutes bot will ignore autosnipe when out of ball", Position = 25)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 25)]
        public int SnipePauseOnOutOfBallTime { get;  set; }

        [NecrobotConfig(Description = "Max distance in km that allow bot autosnipe. set to Z mean not applied", Position = 26)]
        [DefaultValue(0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 26)]
        public double AutoSnipeMaxDistance { get;  set; }

        [NecrobotConfig(Description = "Number of autosnipe pokemon in a row that bot pickup for snipe", Position = 27)]
        [DefaultValue(10)]
        [Range(1,1000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 27)]
        public int AutoSnipeBatchSize { get; set; }
    }
}