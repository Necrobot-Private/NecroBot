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
        public int IV { get; set; }
        public int CP { get; set; }
        public double RemainTimes { get; set; }

        public BotSwitch(int iv, int cp, double remain)
        {
            this.IV = iv;
            this.CP = cp;
            this.RemainTimes = remain;
        }
        public static Dictionary<PokemonId, BotSwitch> Default()
        {
            return new Dictionary<PokemonId, BotSwitch>()
            {
                {PokemonId.Kangaskhan, new BotSwitch(50, 500, 60) }
            };
        }
    }

    [JsonObject(Title = "Multiple Bot Config", Description = "Use this to setup the condition when we switch to next bot", ItemRequired = Required.DisallowNull)]
    public class MultipleBotConfig
    {
        [DefaultValue(55)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int AfterRuntime = 55;

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public bool OnRarePokemon = true;

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public Dictionary<PokemonId, BotSwitch> PokemonSwitches = BotSwitch.Default();

        public static MultipleBotConfig Default()
        {
            return new MultipleBotConfig();
        }
    }
}
