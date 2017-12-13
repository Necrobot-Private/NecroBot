using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Human Walk Snipe Config", Description = "This feature allow bot pull data from pokemap site, if pokemon match with your config. bot will walk to pokemon's location to catch him.", ItemRequired = Required.DisallowNull)]
    public class HumanWalkSnipeConfig :BaseConfig
    {
        public HumanWalkSnipeConfig() : base()
        {
        }

        [NecroBotConfig(Position = 1, Description = "Allow bot using human walk sniper feature")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Enable { get; set; }

        [NecroBotConfig(Position = 2, Description = "Display list pokemon snipeable in console window")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DisplayPokemonList { get; set; }

        [NecroBotConfig(Position = 3, Description = "Max distance that you want bot travel for snipe")]
        [DefaultValue(1500.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxDistance { get; set; }

        [NecroBotConfig(Position = 4, Description = "Max walking time you want bot travel to snipe")]
        [DefaultValue(900.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxEstimateTime { get; set; }

        [NecroBotConfig(Position = 5, Description = "Minimun ball available in bag for catch em all mode. this mean continuously snipping if pokemon available.")]
        [DefaultValue(50)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int CatchEmAllMinBalls { get; set; }

        [NecroBotConfig(Position = 6, Description = "Try to catch em all - confused")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TryCatchEmAll { get; set; }

        [NecroBotConfig(Position = 7, Description = "Allow catch pokemon when walking to target - overwrite by pokemon filter")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool CatchPokemonWhileWalking { get; set; }

        [NecroBotConfig(Position = 8, Description = "Allow farm pokestop when walking to target - overwrite by pokemon filter")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SpinWhileWalking { get; set; }

        [NecroBotConfig(Position = 9, Description = "Set to make bot return to farm zone define in MaxTravelDistance in location config")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AlwaysWalkback { get; set; }

        [NecroBotConfig(Position = 10, Description = "The area the bot looking for pokemon from data service.")]
        [DefaultValue(0.025)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double SnipingScanOffset { get; set; }

        [NecroBotConfig(Position = 11, Description = "The max distance bot will always walk back regardless AlwaysWalkback")]
        [DefaultValue(300.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double WalkbackDistanceLimit { get; set; }

        [NecroBotConfig(Position = 12, Description = "Turn it on will always looking for pokemon at default location no matter what how far from current location")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IncludeDefaultLocation { get; set; }

        [NecroBotConfig(Position = 13, Description = "Use list pokemon pokemon to snipe")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseSnipePokemonList { get; set; }

        [NecroBotConfig(Position = 14, Description = "The maximun speed up that bot travel when snipe - overwrite by pokemon setting")]
        [DefaultValue(60.0)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxSpeedUpSpeed { get; set; }

        [NecroBotConfig(Position = 15, Description = "Allow bot speed up for snipe with the max speed defined above - overwrite by pokemon setting")]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AllowSpeedUp { get; set; }

        [NecroBotConfig(Position = 16, Description = "Milisecond delay time at destination before looking for pokemon - overwrite by pokemon setting")]
        [DefaultValue(10000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int DelayTimeAtDestination { get; set; } //  10 sec

        [NecroBotConfig(Position = 17, Description = "Datasource from pokeradar.info")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokeRadar { get; set; }

        [NecroBotConfig(Position = 18, Description = "Datasource from UseSkiplagged.info")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseSkiplagged { get; set; }

        [NecroBotConfig(Position = 19, Description = "Datasource from pokekcrew")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokecrew { get; set; }

        [NecroBotConfig(Position = 20, Description = "Datasource from pokesnipers")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokesnipers { get; set; }

        [NecroBotConfig(Position = 21, Description = "Datasource from pokezz.info")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokeZZ { get; set; }

        [NecroBotConfig(Position = 22, Description = "Datasource from pokewatcher")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokeWatcher { get; set; }

        [NecroBotConfig(Position = 23, Description = "Datasource from FPM")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseFastPokemap { get; set; }

        [NecroBotConfig(Position = 24, Description = "Datasource from location feeder")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePogoLocationFeeder { get; set; }

        [NecroBotConfig(Position = 25, Description = "Allow bot transfer while working to target  - overriteable")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AllowTransferWhileWalking { get; set; }
    }
}