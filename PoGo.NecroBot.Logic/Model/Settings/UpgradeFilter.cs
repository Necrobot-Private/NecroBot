#region using directives

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;
using TinyIoC;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class UpgradeFilter    : BaseConfig, IPokemonFilter
    {
        internal enum Operator
        {
            or,
            and
        }

        internal enum CPorIv
        {
            cp,
            iv
        }

        public UpgradeFilter(): base()
        {
            Moves = new List<List<PokemonMove>>();
            AffectToPokemons = new List<PokemonId>();
        }

        public UpgradeFilter(double minLevel, double upgradePokemonCpMinimum, double upgradePokemonIvMinimum,
            string upgradePokemonMinimumStatsOperator, bool onlyUpgradeFavorites)
        {
            AffectToPokemons = new List<PokemonId>();
            Moves = new List<List<PokemonMove>>();
            UpgradePokemonLvlMinimum = minLevel;
            UpgradePokemonCpMinimum = upgradePokemonCpMinimum;
            UpgradePokemonIvMinimum = upgradePokemonIvMinimum;
            UpgradePokemonMinimumStatsOperator = upgradePokemonMinimumStatsOperator;
            OnlyUpgradeFavorites = onlyUpgradeFavorites;
            AllowTransfer = true;
        }

        [JsonIgnore]
        [NecroBotConfig(IsPrimaryKey = true, Key = "Allow Upgrade", Position = 1, Description = "If enabled, will allow custom filter for level up")]
        public bool AllowTransfer { get; set; }

        [NecroBotConfig(Key = "Min Level To Upgrade", Position = 2, Description ="Min Level to upgrade")]
        [Range(0,100)]
        [DefaultValue(30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double UpgradePokemonLvlMinimum { get; set; }

        [NecroBotConfig(Key = "Min Upgrade CP", Position = 3, Description = "Upgrade by IV or CP")]
        [DefaultValue(2250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double UpgradePokemonCpMinimum { get; set; } 

        [NecroBotConfig(Key = "Min Upgrade IV", Position = 4, Description = "Define Min IV to upgrade")]
        [DefaultValue(100)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double UpgradePokemonIvMinimum { get; set; } 

        [NecroBotConfig(Key = "Operator", Position = 5, Description = "Operator logic to check pokemon for upgrade")]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string UpgradePokemonMinimumStatsOperator { get; set; }

        [NecroBotConfig(Key = "Only Farovite", Position = 6, Description = "If Enabled, Bot will only upgrade pokemon that are favorited only, and it will ignore all those condition check.")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public bool OnlyUpgradeFavorites { get; set; }

        [NecroBotConfig(Key = "Move Set", Position = 6, Description = "Only update if move set match with pair in the list")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public List<List<PokemonMove>> Moves { get; set; } = new List<List<PokemonMove>>();

        [NecroBotConfig(Key = "Affect To Pokemons", Position = 7, Description = "Define list of pokemon that this upgrade filter will also be applied to")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]

        public List<PokemonId> AffectToPokemons { get; set; }

        internal static Dictionary<PokemonId, UpgradeFilter> Default()
        {
            return new Dictionary<PokemonId, UpgradeFilter>
            {
                {PokemonId.Dratini, new UpgradeFilter(100, 600, 99, "or", false)}
            };
        }

        public IPokemonFilter GetGlobalFilter()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var _logicSettings = session.LogicSettings;
            return new UpgradeFilter(_logicSettings.UpgradePokemonLvlMinimum, _logicSettings.UpgradePokemonCpMinimum,
                _logicSettings.UpgradePokemonIvMinimum, _logicSettings.UpgradePokemonMinimumStatsOperator,
                _logicSettings.OnlyUpgradeFavorites);



        }
    }
}