using Newtonsoft.Json;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Settings
{

    public class BotSwitch
    {
        [ExcelConfig(Key ="AV")]
        public int IV { get; set; }
        [ExcelConfig(Key = "AW")]
        public int LV { get; set; }
        [ExcelConfig(Key = "AY")]
        public string Operator { get; set; }

        [ExcelConfig(Key = "AX")]
        public List<List<PokemonMove>> Moves { get; set; }
        [ExcelConfig(Key = "AZ")]
        public double RemainTimes { get; set; }

        public BotSwitch() {
            this.Moves = new List<List<PokemonMove>>();
        }
        public BotSwitch(int iv, int lv, double remain)
        {
            this.Moves = new List<List<PokemonMove>>();
            this.IV = iv;
            this.LV = lv;
            this.RemainTimes = remain;
        }
        public static Dictionary<PokemonId, BotSwitch> Default()
        {
            return new Dictionary<PokemonId, BotSwitch>()
            {
                { PokemonId.Lickitung, new BotSwitch(30, 0, 60) },
                { PokemonId.Dragonite, new BotSwitch(10, 0, 60) },
                { PokemonId.Lapras, new BotSwitch(10, 0, 60) },
                { PokemonId.Exeggutor, new BotSwitch(10, 0, 60) },
                { PokemonId.Magmar, new BotSwitch(70, 0, 60) },
                { PokemonId.Arcanine, new BotSwitch(10, 0, 60) },
                { PokemonId.Beedrill, new BotSwitch(10, 0, 60) },
                { PokemonId.Blastoise, new BotSwitch(10, 0, 60) },
                { PokemonId.Charizard, new BotSwitch(10, 0, 60) },
                { PokemonId.Venusaur, new BotSwitch(10, 200, 60) },
                { PokemonId.Vileplume, new BotSwitch(10, 0, 60) },
                { PokemonId.Vaporeon, new BotSwitch(10, 0, 60) },
                { PokemonId.Dragonair, new BotSwitch(70, 0, 60) },
                { PokemonId.Dratini, new BotSwitch(90, 200, 60) },
                { PokemonId.Snorlax, new BotSwitch(30, 0, 60) },
                { PokemonId.Kangaskhan, new BotSwitch(80, 0, 60) },
                { PokemonId.Ninetales, new BotSwitch(10, 0, 60) },
                { PokemonId.Electabuzz, new BotSwitch(10, 0, 60) },
                { PokemonId.Magikarp, new BotSwitch(95, 0, 60) },
            };
        }
    }

    [JsonObject(Title = "Multiple Bot Config", Description = "Use this to setup the condition when we switch to next bot", ItemRequired = Required.DisallowNull)]
    public class MultipleBotConfig
    {
        [ExcelConfig (Description = "Bot will switch to new account after x minutes ", Position = 1)]
        [DefaultValue(55)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int RuntimeSwitch = 55;

        [ExcelConfig(Description = "Allow bot switch account when encountered with a rare pokemon that you definied in the list", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public bool OnRarePokemon = true;

        [ExcelConfig(Description = "Allow bot switch account when encountered with pokemon IV higher than this value", Position = 3)]
        [DefaultValue(90.0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double MinIVToSwitch = 95.0;

        [ExcelConfig(Description = "Bot will switch to new account after collect this EXP in one login session ", Position = 4)]
        [DefaultValue(25000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public int EXPSwitch = 25000;

        [ExcelConfig(Description = "Bot will switch to new account after x  pokestop farm", Position = 5)]
        [DefaultValue(500)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int PokestopSwitch = 500;

        [ExcelConfig(Description = "Bot will switch to new account after x  pokemon catch ", Position = 6)]
        [DefaultValue(200)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int PokemonSwitch = 200;

        [ExcelConfig(Description = "Bot will switch to new account after x pokemon catch in 1 hours - not being used atm ", Position = 7)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int PokemonPerHourSwitch = 100; //only apply if runtime > 1h. 
        [ExcelConfig(Description = "Tell bot to start at default location", Position = 8)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]

        public bool StartFromDefaultLocation = true; //only apply if runtime > 1h. 

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public Dictionary<PokemonId, BotSwitch> PokemonSwitches = BotSwitch.Default();

        public static MultipleBotConfig Default()
        {
            return new MultipleBotConfig();
        }
    }
}
