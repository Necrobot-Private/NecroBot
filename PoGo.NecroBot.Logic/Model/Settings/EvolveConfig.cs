using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Evolve Config", Description = "Set your evolve settings.", ItemRequired = Required.DisallowNull)]
    public class EvolveConfig : BaseConfig
    {
        internal static List<PokemonId> PokemonsToEvolveDefault()
        {
            return new List<PokemonId>
            {
                /*NOTE: keep all the end-of-line commas exept for the last one or an exception will be thrown!
               criteria: 12 candies*/
                PokemonId.Caterpie,
                PokemonId.Weedle,
                PokemonId.Pidgey,
                /*criteria: 25 candies*/
                //PokemonId.Bulbasaur,
                //PokemonId.Charmander,
                //PokemonId.Squirtle,
                PokemonId.Rattata
                //PokemonId.NidoranFemale,
                //PokemonId.NidoranMale,
                //PokemonId.Oddish,
                //PokemonId.Poliwag,
                //PokemonId.Abra,
                //PokemonId.Machop,
                //PokemonId.Bellsprout,
                //PokemonId.Geodude,
                //PokemonId.Gastly,
                //PokemonId.Eevee,
                //PokemonId.Dratini,
                /*criteria: 50 candies commons*/
                //PokemonId.Spearow,
                //PokemonId.Ekans,
                //PokemonId.Zubat,
                //PokemonId.Paras,
                //PokemonId.Venonat,
                //PokemonId.Psyduck,
                //PokemonId.Slowpoke,
                //PokemonId.Doduo,
                //PokemonId.Drowzee,
                //PokemonId.Krabby,
                //PokemonId.Horsea,
                //PokemonId.Goldeen,
                //PokemonId.Staryu
            };
        }

        public EvolveConfig() : base()
        {
        }

        #region Filter
        [NecroBotConfig(Description = "Lets the bot evolve all pokemons with enough candy and listed in PokemonEvolveFilter", Position = 10)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public bool EvolvePokemonsThatMatchFilter { get; set; }

        [NecroBotConfig(Description = "Lets the bot evolve any pokemon (also if not in PokemonEvolveFilter) with enough candy and at least an IV as in EvolveAnyPokemonAboveIvValue", Position = 20)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 20)]
        public bool EvolveAnyPokemonAboveIv { get; set; }

        [NecroBotConfig(Description = "If EvolveAnyPokemonAboveIv is true, this is the IV threshold for evolving a pokemon", Position = 30)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 30)]
        public float EvolveAnyPokemonAboveIvValue { get; set; }
        #endregion

        #region Trigger
        [NecroBotConfig(Description = "A pokemon will get evolved right away if enough candy and listed in PokemonEvolveFilter. This will lead to single evolutions", Position = 40)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 40)]
        public bool TriggerAsSoonAsFilterIsMatched { get; set; }

        [NecroBotConfig(Description = "Lets the bot bulk evolve all possible evolutions if at least as many evolutions exist as specified in TriggerOnEvolutionCountValue", Position = 50)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 50)]
        public bool TriggerOnEvolutionCount { get; set; }

        [NecroBotConfig(Description = "If TriggerOnEvolutionCount is true, this is the number of evolutions needed to trigger bulk evolving", Position = 60)]
        [DefaultValue(30)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 60)]
        public int TriggerOnEvolutionCountValue { get; set; }

        [NecroBotConfig(Description = "Lets the bot bulk evolve all possible evolutions if pokemon storage usage is at least as specified in TriggerOnStorageUsagePercentageValue", Position = 70)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 70)]
        public bool TriggerOnStorageUsagePercentage { get; set; }

        [NecroBotConfig(Description = "If TriggerOnStorageUsagePercentage is true, this is the percentage threshold of storage usage to trigger bulk evolving", Position = 80)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 80)]
        public double TriggerOnStorageUsagePercentageValue { get; set; }

        [NecroBotConfig(Description = "Lets the bot bulk evolve all possible evolutions if pokemons in storage are at least as specified in TriggerOnStorageUsageAbsoluteValue", Position = 90)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 90)]
        public bool TriggerOnStorageUsageAbsolute { get; set; }

        [NecroBotConfig(Description = "If TriggerOnStorageUsageAbsolute is true, this is the absolute threshold of storage usage to trigger bulk evolving", Position = 100)]
        [DefaultValue(240)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 100)]
        public int TriggerOnStorageUsageAbsoluteValue { get; set; }

        [NecroBotConfig(Description = "A pokemon will get evolved right away if enough candy, listed in PokemonEvolveFilter and a lucky egg is currently active", Position = 110)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 110)]
        public bool TriggerIfLuckyEggIsActive { get; set; }
        #endregion

        #region When Evolving
        [NecroBotConfig(Description = "When enabled, bot will not spend candies defined in PokemonEvolveFilter/MinCandiesBeforeEvolve, only using candies above that value", Position = 120)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 120)]
        public bool PreserveMinCandiesFromFilter { get; set; }

        [NecroBotConfig(Description = "Apply a lucky egg (if available) when bot is about to evolve in bulk as many pokemons as specified in ApplyLuckyEggOnEvolutionCountValue", Position = 130)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 130)]
        public bool ApplyLuckyEggOnEvolutionCount { get; set; }

        [NecroBotConfig(Description = "If ApplyLuckyEggOnEvolutionCount is true, this is the min number of evolutions needed to apply a lucky egg", Position = 140)]
        [DefaultValue(30)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 140)]
        public int ApplyLuckyEggOnEvolutionCountValue { get; set; }
        #endregion
    }
}