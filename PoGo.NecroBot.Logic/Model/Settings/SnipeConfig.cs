using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Snipe Config", Description = "Set your snipe settings.", ItemRequired = Required.DisallowNull)]
    public class SnipeConfig : BaseConfig
    {
        [NecroBotConfig(Description = "Tell bot to use location service, detail at - https://github.com/5andr0/PogoLocationFeeder", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool UseSnipeLocationServer { get; set; }

        [NecroBotConfig(Description = "IP Address or server name of location server, usually this is localhost or 127.0.0.1", Position = 2)]
        [DefaultValue("localhost")]
        [MinLength(0)]
        [MaxLength(32)]
        //[RegularExpression(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")] //Ip Only
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string SnipeLocationServer { get; set; }

        [NecroBotConfig(Description = "Port number of location server. ", Position = 3)]
        [DefaultValue(16969)]
        [Range(1, 65535)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int SnipeLocationServerPort { get; set; }

        [NecroBotConfig(Description = "Number of balls in inventory to get sniper function to work. ", Position = 9)]
        [DefaultValue(20)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public int MinPokeballsToSnipe { get; set; }

        [NecroBotConfig(Description = "Min balls allowed to exist sniper", Position = 10)]
        [DefaultValue(0)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public int MinPokeballsWhileSnipe { get; set; }

        [NecroBotConfig(Description = "Delay time between 2 snipes.", Position = 11)]
        [DefaultValue(60000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public int MinDelayBetweenSnipes { get; set; }

        [NecroBotConfig(Description = "The area bot try to scan for target pokemon.", Position = 12)]
        [DefaultValue(0.005)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public double SnipingScanOffset { get; set; }

        [NecroBotConfig(Description = "That setting will make bot snipe when it reaches every pokestop.", Position = 13)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public bool SnipeAtPokestops { get; set; }

        [DefaultValue(false)]
        [NecroBotConfig(Description = "Turn this on to ignore pokemon with unknown IV from data source", Position = 14)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public bool SnipeIgnoreUnknownIv { get; set; }

        [NecroBotConfig(Description = "Bot will transfer pokemon for sniping if the sniping pokemon is of a higher IV than this.", Position = 15)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        public bool UseTransferIvForSnipe { get; set; }

        [NecroBotConfig(Description = "Turn this on it for bot only by priority to snipe pokemon not in pokedex.", Position = 16)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 16)]
        public bool SnipePokemonNotInPokedex { get; set; }

        /*SnipeLimit*/
        [NecroBotConfig(Description = "Turn this on to limit the speed for sniping.", Position = 17)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 17)]
        public bool UseSnipeLimit { get; set; }

        [NecroBotConfig(Description = "Delay time between 2 snipes", Position = 18)]
        [DefaultValue(10 * 60)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 18)]
        public int SnipeRestSeconds { get; set; }

        [NecroBotConfig(Description = "Limits number of snipe in hour.", Position = 19)]
        [DefaultValue(39)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 19)]
        public int SnipeCountLimit { get; set; }

        [NecroBotConfig(Description = "Allow MSniper feature with bot.", Position = 20)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 20)]
        public bool ActivateMSniper = true;

        [NecroBotConfig(Description = "Min IV that the bot will automatically snipe pokemon", Position = 21)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 21)]
        public int MinIVForAutoSnipe { get; set; }

        [NecroBotConfig(Description = "Min Level that the bot will automatically snipe pokemon", Position = 22)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 22)]
        public int MinLevelForAutoSnipe { get; set; }

        [NecroBotConfig(Description = "Only auto snipe pokemon that have been verified (overwriteable by invidual pokemon)", Position = 23)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 23)]
        public bool AutosnipeVerifiedOnly { get; set; }

        [NecroBotConfig(Description = "Set the amount of candy you want bot to auto snipe if it has less candy than this value.", Position = 24)]
        [DefaultValue(0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 24)]
        public int DefaultAutoSnipeCandy { get; set; }

        [NecroBotConfig(Description = "Total time in minutes bot will ignore auto snipe when out of pokeballs", Position = 25)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 25)]
        public int SnipePauseOnOutOfBallTime { get;  set; }

        [NecroBotConfig(Description = "Max distance in km that will allow bot to auto snipe.", Position = 26)]
        [DefaultValue(0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 26)]
        public double AutoSnipeMaxDistance { get;  set; }

        [NecroBotConfig(Description = "Number of auto snipe on a pokemon in a row that bot pickup for snipe", Position = 27)]
        [DefaultValue(10)]
        [Range(1,1000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 27)]
        public int AutoSnipeBatchSize { get; set; }
    }
}