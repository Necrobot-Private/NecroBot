using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)] //Dont set Title
    public class HumanWalkSnipeFilter
    {
        public HumanWalkSnipeFilter()
        {
        }

        [JsonIgnore]
        [NecroBotConfig(IsPrimaryKey = true, Key = "Allow Snipe", Position = 1, Description = "Allows bot to snipe this pokemon")]
        public bool AllowSnipe { get; set; }

        [DefaultValue(300.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecroBotConfig(Key = "MaxDistance" , Description = "Max distance that bot is allowed to walk to snipe this pokemon", Position = 2)]
        public double MaxDistance { get; set; }

        [NecroBotConfig(Key = "Priority", Description = "Priority for this pokemon compared to others", Position = 3)]
        [DefaultValue(1)]
        [Range(0, 151)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int Priority { get; set; }

        [NecroBotConfig(Key = "Max Walk Times", Description = "Max walk time for bot to catch this pokemon compared to others", Position = 4)]
        [DefaultValue(200.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double MaxWalkTimes { get; set; }

        [NecroBotConfig(Key = "Allow Catch", Description = "Allows bot to catch pokemon while it walking to snipe target", Position = 5)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public bool CatchPokemonWhileWalking { get; set; }

        [NecroBotConfig(Key = "Allow Spin", Description = "Allows bot spin pokestops while it's walking to snipe target", Position = 6)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public bool SpinPokestopWhileWalking { get; set; }

        [NecroBotConfig(Key = "Max Speed", Description = "The max speed up to apply while walking to this pokemon", Position = 7)]
        [DefaultValue(60.0)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public double MaxSpeedUpSpeed { get; set; }

        [NecroBotConfig(Key = "Allow SpeedUP", Description = "Allows bot to speed up to catch this pokemon", Position = 8)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public bool AllowSpeedUp { get; set; }

        [NecroBotConfig(Key = "Delay at Dest", Description = "Sets delay time at destination", Position = 9)]
        [DefaultValue(10000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public int DelayTimeAtDestination { get; set; }

        [NecroBotConfig(Key = "Allow Transfer", Description = "Allows bot to transfer pokemon while it's walking to target", Position = 10)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool AllowTransferWhileWalking { get; set; }

        public HumanWalkSnipeFilter(double maxDistance, double maxWalkTimes, int priority, bool catchPokemon,
            bool spinPokestop, bool allowSpeedUp, double speedUpSpeed, int delay = 10, bool allowtransfer = false)
        {
            AllowSnipe = true;
            MaxDistance = maxDistance;
            MaxWalkTimes = maxWalkTimes;
            Priority = priority;
            CatchPokemonWhileWalking = catchPokemon;
            SpinPokestopWhileWalking = spinPokestop;
            AllowSpeedUp = allowSpeedUp;
            MaxSpeedUpSpeed = speedUpSpeed;
            DelayTimeAtDestination = delay;
            AllowTransferWhileWalking = allowtransfer;
        }

        internal static Dictionary<PokemonId, HumanWalkSnipeFilter> Default()
        {
            return new Dictionary<PokemonId, HumanWalkSnipeFilter>
            {
                //http://fraghero.com/heres-the-full-pokemon-go-pokemon-list-most-common-to-the-rarest/

                {PokemonId.Magikarp, new HumanWalkSnipeFilter(300, 200, 10, true, true, false, 60.0)},
                {PokemonId.Eevee, new HumanWalkSnipeFilter(500, 200, 10, true, true, false, 60.0)},
                {PokemonId.Electabuzz, new HumanWalkSnipeFilter(1500, 700, 2, true, true, false, 60.0)},
                {PokemonId.Dragonite, new HumanWalkSnipeFilter(3000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Dragonair, new HumanWalkSnipeFilter(3000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Dratini, new HumanWalkSnipeFilter(2000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Charizard, new HumanWalkSnipeFilter(3000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Snorlax, new HumanWalkSnipeFilter(3000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Lapras, new HumanWalkSnipeFilter(3000, 900, 1, false, false, false, 60.0)},
                {PokemonId.Exeggutor, new HumanWalkSnipeFilter(1500, 600, 1, false, true, false, 60.0)},
                {PokemonId.Vaporeon, new HumanWalkSnipeFilter(1800, 800, 2, false, false, false, 60.0)},
                {PokemonId.Lickitung, new HumanWalkSnipeFilter(1800, 800, 2, false, false, false, 60.0)},
                {PokemonId.Flareon, new HumanWalkSnipeFilter(1800, 800, 2, false, false, false, 60.0)},
                {PokemonId.Scyther, new HumanWalkSnipeFilter(1000, 800, 3, false, false, false, 60.0)},
                {PokemonId.Beedrill, new HumanWalkSnipeFilter(1000, 800, 3, false, false, false, 60.0)},
                {PokemonId.Chansey, new HumanWalkSnipeFilter(1500, 800, 2, false, false, false, 60.0)},
                {PokemonId.Hitmonlee, new HumanWalkSnipeFilter(1500, 800, 2, false, false, false, 60.0)},
                {PokemonId.Machamp, new HumanWalkSnipeFilter(1500, 800, 2, false, false, false, 60.0)},
                {PokemonId.Ninetales, new HumanWalkSnipeFilter(1500, 800, 2, false, false, false, 60.0)},
                {PokemonId.Jolteon, new HumanWalkSnipeFilter(1200, 800, 2, false, false, false, 60.0)},
                {PokemonId.Poliwhirl, new HumanWalkSnipeFilter(1200, 800, 2, false, false, false, 60.0)},
                {PokemonId.Rapidash, new HumanWalkSnipeFilter(1500, 800, 2, false, false, false, 60.0)},
                {PokemonId.Cloyster, new HumanWalkSnipeFilter(1200, 800, 2, false, false, false, 60.0)},
                {PokemonId.Dodrio, new HumanWalkSnipeFilter(1200, 800, 2, false, false, false, 60.0)},
                {PokemonId.Clefable, new HumanWalkSnipeFilter(1000, 800, 3, false, false, false, 60.0)},
                {PokemonId.Golbat, new HumanWalkSnipeFilter(200, 800, 6, true, true, false, 60.0)},
                {PokemonId.Jynx, new HumanWalkSnipeFilter(1200, 800, 4, true, true, false, 60.0)},
                {PokemonId.Rhydon, new HumanWalkSnipeFilter(1200, 800, 4, true, true, false, 60.0)},
                {PokemonId.Kangaskhan, new HumanWalkSnipeFilter(800, 800, 4, true, true, false, 60.0)},
                {PokemonId.Wigglytuff, new HumanWalkSnipeFilter(1250, 800, 4, true, true, false, 60.0)},
                {PokemonId.Gyarados, new HumanWalkSnipeFilter(1800, 800, 2, false, false, false, 60.0)},
                {PokemonId.Dewgong, new HumanWalkSnipeFilter(1800, 800, 2, false, false, false, 60.0)},
                {PokemonId.Blastoise, new HumanWalkSnipeFilter(3000, 900, 1, false, false, true, 60.0)},
                {PokemonId.Venusaur, new HumanWalkSnipeFilter(3000, 900, 1, false, false, true, 60.0)},
                {PokemonId.Bulbasaur, new HumanWalkSnipeFilter(500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Charmander, new HumanWalkSnipeFilter(500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Squirtle, new HumanWalkSnipeFilter(500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Omanyte, new HumanWalkSnipeFilter(1500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Marowak, new HumanWalkSnipeFilter(1500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Venomoth, new HumanWalkSnipeFilter(1500, 300, 3, true, true, false, 60.0)},
                {PokemonId.Vileplume, new HumanWalkSnipeFilter(1500, 300, 3, true, true, false, 60.0)}
            };
        }
    }
}