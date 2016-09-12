#region using directives

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class UpgradeFilter
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

        public UpgradeFilter()
        {

        }

        public UpgradeFilter(string levelUpByCPorIv, double upgradePokemonCpMinimum, double upgradePokemonIvMinimum,
            string upgradePokemonMinimumStatsOperator, bool onlyUpgradeFavorites)
        {
            LevelUpByCPorIv = levelUpByCPorIv;
            UpgradePokemonCpMinimum = upgradePokemonCpMinimum;
            UpgradePokemonIvMinimum = upgradePokemonIvMinimum;
            UpgradePokemonMinimumStatsOperator = upgradePokemonMinimumStatsOperator;
            OnlyUpgradeFavorites = onlyUpgradeFavorites;
        }

        [DefaultValue("iv")]
        [EnumDataType(typeof(CPorIv))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string LevelUpByCPorIv { get; set; } = "iv";

        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public double UpgradePokemonCpMinimum { get; set; } = 1250;

        [DefaultValue(90)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public double UpgradePokemonIvMinimum { get; set; } = 90;

        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string UpgradePokemonMinimumStatsOperator { get; set; } = "or";

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public bool OnlyUpgradeFavorites { get; set; }

        internal static Dictionary<PokemonId, UpgradeFilter> Default()
        {
            return new Dictionary<PokemonId, UpgradeFilter>
            {
                {PokemonId.Dratini, new UpgradeFilter("iv", 600, 99, "or", false)}
            };
        }
    }
}