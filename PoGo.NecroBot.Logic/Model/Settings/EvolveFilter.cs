using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;
using System;
using TinyIoC;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)] //Dont set Title
    public class EvolveFilter   : BaseConfig, IPokemonFilter
    {
        public EvolveFilter()  :base()
        {
            this.AffectToPokemons = new List<PokemonId>();
            Moves = new List<List<PokemonMove>>();
            EnableEvolve = true;
            Operator = "or";
        }


        public EvolveFilter(double evolveIV, double evolveLV, double minCP, bool favorited = false,string evoOperator = "and", string evolveTo = "", List<List<PokemonMove>> moves = null, int minCandiesBeforeEvolve = 0)
        {
            this.Moves = new List<List<PokemonMove>>();
            if (moves != null) this.Moves = moves;
            EnableEvolve = true;
            this.MinIV = evolveIV;
            this.MinLV = evolveLV;
            this.EvolveTo = evolveTo;
            this.MinCP = minCP;
            this.Operator = evoOperator;
            this.FavoritedOnly = favorited;
            this.MinCandiesBeforeEvolve = minCandiesBeforeEvolve;
        }

        [NecrobotConfig(IsPrimaryKey = true, Key = "Enable Envolve", Description = "Allow bot auto evolve this pokemon", Position = 1)]
        [DefaultValue(false)]
        [JsonIgnore]
        public bool EnableEvolve { get; set; }

        [NecrobotConfig(Key = "Evolve Min IV", Description = "Min IV for auto evolve", Position = 2)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public double MinIV { get; set; }

        [NecrobotConfig(Key = "Evolve Min LV", Description = "Min LV for auto evolve", Position = 3)]
        [DefaultValue(95)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double MinLV { get; set; }

        [NecrobotConfig(Key = "Evolve Min CP", Description = "Min CP for auto evolve", Position = 4)]
        [DefaultValue(10)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double MinCP { get; set; }

        [NecrobotConfig(Key = "Moves", Description = "Define list of desire move for evolve", Position = 5)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public List<List<PokemonMove>> Moves { get; set; }

        [NecrobotConfig(Key = "Operator", Position = 6, Description = "The operator logic use to check for evolve")]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public string Operator { get; set; }

        [NecrobotConfig(Key = "Evolve To", Position = 7, Description = "Select branch to envolve to for multiple branch pokemon like Poliwirl")]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public string EvolveTo { get; set; }

        [NecrobotConfig(Key = "Affect To Pokemons", Position = 8, Description = "Set the list of pokemon you want to use the same config")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public List<PokemonId> AffectToPokemons { get; set; }

        [JsonIgnore]
        public PokemonId EvolveToPokemonId
        {
            get
            {
                PokemonId id = PokemonId.Missingno;

                if (Enum.TryParse<PokemonId>(this.EvolveTo, out id))
                {
                    return id;
                }

                return id;
            }
        }

        [NecrobotConfig(Key = "Evolve Favorite Only", Position = 9, Description = "If true, bot only evolve pokemon that are favorited only")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public bool FavoritedOnly { get; set; }

        [NecrobotConfig(Key = "Min Candies Before Evolve", Position = 10, Description = "If greater than 0, bot will not evolve right away, but instead keep transferring the pokemon to save up at least min candies before evolving.")]
        [DefaultValue(0)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public int MinCandiesBeforeEvolve { get; set; }

        internal static Dictionary<PokemonId, EvolveFilter> Default()
        {
            return new Dictionary<PokemonId, EvolveFilter>
            {
                {PokemonId.Rattata, new EvolveFilter(0, 0, 0, false,"or") {
                                                                         AffectToPokemons = new List<PokemonId>()
                                                                         {
                                                                                PokemonId.Zubat, 
                                                                                PokemonId.Pidgey,
                                                                                PokemonId.Caterpie,
                                                                                PokemonId.Weedle,
                                                                         }
                }},

                {PokemonId.Porygon, new EvolveFilter(100, 28, 500, false,"and",PokemonId.Porygon2.ToString())},
                {PokemonId.Gloom , new EvolveFilter(100, 28, 500, false,"and",PokemonId.Bellossom.ToString())} ,
                {PokemonId.Sunkern , new EvolveFilter(100, 28, 500, false,"and",PokemonId.Sunflora.ToString())}  ,
                {PokemonId.Slowpoke, new EvolveFilter(100, 28, 500, false,"and",PokemonId.Slowking.ToString())},
                {PokemonId.Poliwhirl , new EvolveFilter(100, 28, 500, false,"and",PokemonId.Politoed.ToString())},
                {PokemonId.Seadra , new EvolveFilter(100, 28, 500, false,"and",PokemonId.Kingdra.ToString())},
                {PokemonId.Dratini, new EvolveFilter(100, 30, 800, false,"and")}

            };
        }

        public IPokemonFilter GetGlobalFilter()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var _logicSettings = session.LogicSettings;

            return new EvolveFilter(_logicSettings.EvolveMinIV, _logicSettings.EvolveMinIV, _logicSettings.EvolveMinCP, _logicSettings.EvolveFavoritedOnly,_logicSettings.EvolveOperator);
        }
    }
}