using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class BotSwitchPokemonFilter    : BaseConfig, IPokemonFilter
    {
        [JsonIgnore]
        [NecroBotConfig(IsPrimaryKey = true, Key = "Allow Switch", Description = "Allows bot to use invidual filters for switching accounts", Position = 1)]
        public bool AllowBotSwitch { get; set; }

        [NecroBotConfig(Key = "Min IV", Description = "When this pokemon has a IV > this value, the bot will switch accounts", Position = 2)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int IV { get; set; }

        [NecroBotConfig(Key = "Min LV", Description = "When this pokemon has a LV > this value, the bot will switch accounts", Position = 3)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int LV { get; set; }

        [NecroBotConfig(Key = "Move", Description = "When a wild pokemon has the move match, the bot will switch accounts to catch", Position = 4)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public List<List<PokemonMove>> Moves { get; set; }


        [NecroBotConfig(Key = "Remain times", Description = "Number of seconds since pokemon disappeared", Position = 5)]
        [Range(0, 900)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]

        public int RemainTimes { get; set; }

        [NecroBotConfig(Key = "Operator", Description = "The operator to check", Position = 6)]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string Operator { get; set; }


        [NecroBotConfig(Key = "Affect to Pokemons", Description = "List of same pokemon to apply to this filter", Position = 6)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]

        public List<PokemonId> AffectToPokemons { get; set; }

        public BotSwitchPokemonFilter()
        {
            Moves = new List<List<PokemonMove>>();
            AffectToPokemons = new List<PokemonId>();
        }

        public BotSwitchPokemonFilter(int iv, int lv, int remain)
        {
            AffectToPokemons = new List<PokemonId>();

            Operator = "or";
            Moves = new List<List<PokemonMove>>();
            IV = iv;
            LV = lv;
            RemainTimes = remain;
        }

        public static Dictionary<PokemonId, BotSwitchPokemonFilter> Default()
        {
            return new Dictionary<PokemonId, BotSwitchPokemonFilter>()
            {
                {PokemonId.Lickitung, new BotSwitchPokemonFilter(30, 0, 60)},
                {PokemonId.Dragonite, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Lapras, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Exeggutor, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Magmar, new BotSwitchPokemonFilter(70, 0, 60)},
                {PokemonId.Arcanine, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Beedrill, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Blastoise, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Charizard, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Venusaur, new BotSwitchPokemonFilter(10, 100, 60)},
                {PokemonId.Vileplume, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Vaporeon, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Dragonair, new BotSwitchPokemonFilter(70, 0, 60)},
                {PokemonId.Dratini, new BotSwitchPokemonFilter(90, 100, 60)},
                {PokemonId.Snorlax, new BotSwitchPokemonFilter(30, 0, 60)},
                {PokemonId.Kangaskhan, new BotSwitchPokemonFilter(80, 0, 60)},
                {PokemonId.Ninetales, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Electabuzz, new BotSwitchPokemonFilter(10, 0, 60)},
                {PokemonId.Magikarp, new BotSwitchPokemonFilter(95, 0, 60)},
            };
        }

        public IPokemonFilter GetGlobalFilter()
        {
            //var session = TinyIoCContainer.Current.Resolve<ISession>();

            return null;
        }
    }

    [JsonObject(Title = "Multiple Bot Config", Description = "Use this to setup the conditions when we switch to next bot account", ItemRequired = Required.DisallowNull)]
    public class MultipleBotConfig   : BaseConfig
    {
        public MultipleBotConfig() : base()
        {
        }

        [NecroBotConfig (Description = "Bot will switch to new account after this many minutes ", Position = 1)]
        [DefaultValue(55)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int RuntimeSwitch { get; set; }

        [NecroBotConfig(Description = "Add +-this or anything between to the RuntimeSwitch", Position = 1)]
        [DefaultValue(10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int RuntimeSwitchRandomTime { get; set; }

        [NecroBotConfig(Description = "This many minutes to block this bot when reaching the daily limit ", Position = 1)]
        [DefaultValue(15)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]

        public int OnLimitPauseTimes { get; set; }

        [NecroBotConfig(Description = "Allows bot to switch account when encountering a rare pokemon that you've definied in the list", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public bool OnRarePokemon { get; set; }

        [NecroBotConfig(Description = "Allows bot to switch account when encountering a pokemon IV higher than this value", Position = 3)]
        [DefaultValue(90.0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double MinIVToSwitch { get; set; }

        [NecroBotConfig(Description = "Bot will switch to a new account after collecting this much EXP in a session ", Position = 4)]
        [DefaultValue(25000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int EXPSwitch { get; set; }

        [NecroBotConfig(Description = "Bot will switch to a new account after this many pokestops are farmed", Position = 5)]
        [DefaultValue(500)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int PokestopSwitch { get; set; }

        [NecroBotConfig(Description = "Bot will switch to a new account after this many pokemon are caught ", Position = 6)]
        [DefaultValue(200)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int PokemonSwitch { get; set; }

        [NecroBotConfig(Description = "Bot will switch to a new account after this many pokemon are caught in 1 hours - not being used atm ", Position = 7)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int PokemonPerHourSwitch { get; set; } //only apply if runtime > 1h. 

        [NecroBotConfig(Description = "Tell bot to start at default location", Position = 8)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public bool StartFromDefaultLocation { get; set; } //only apply if runtime > 1h. 

        [NecroBotConfig(Description = "How many times pokestop softban can triger bot switch, 0 means it doesn't switch", Position = 9)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        [Range(0, 100)]
        public int PokestopSoftbanCount { get; set; } //only apply if runtime > 1h. 


        [NecroBotConfig(Description = "Displays bot list (include ran time) on switch", Position = 10)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool DisplayList { get; set; }

        [NecroBotConfig(Description = "Bot will display a list of accounts that you have setup in auth.json then ask you to select which account you want to start with.", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool SelectAccountOnStartUp { get; set; }

        [NecroBotConfig(Description = "Number of continuously catch flees before switching account", Position = 12)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public int CatchFleeCount{ get; set; }

        [NecroBotConfig(Description = "Switch account on meeting catch limit", Position = 13)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public bool SwitchOnCatchLimit { get; set; }


        [NecroBotConfig(Description = "Switch account on meeting pokestop limit", Position = 14)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public bool SwitchOnPokestopLimit { get; set; }


        public static MultipleBotConfig Default()
        {
            return new MultipleBotConfig();
        }

        public static bool IsMultiBotActive(ILogicSettings logicSettings, MultiAccountManager manager)
        {
            return manager.AllowMultipleBot() && logicSettings.Bots != null && logicSettings.Bots.Count >= 1;
        }
    }
}