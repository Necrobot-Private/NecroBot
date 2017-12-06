using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Pokemon Config", Description = "Set your pokemon settings.", ItemRequired = Required.DisallowNull)]
    public class PokemonConfig  :BaseConfig
    {
        public PokemonConfig() : base()
        {
        }

        public enum Operator
        {
            or,
            and
        }

        internal enum CpIv
        {
            cp,
            iv
        }

        [NecroBotConfig(Description = "Allows bot to Catch Pokemon", Position = 100)]
        /*Catch*/
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 100)]
        public bool CatchPokemon { get; set; }

        [NecroBotConfig(Description = "Delay time between time catching pokemon", Position = 200)]
        [DefaultValue(2000)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 200)]
        public int DelayBetweenPokemonCatch { get; set; }

        /*CatchLimit*/
        [NecroBotConfig(Description = "Check for daily limit catch rate - CatchPokemonLimit per CatchPokemonLimitMinutes", Position = 30)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 30)]
        public bool UseCatchLimit { get; set; }

        [NecroBotConfig(Description = "Number of pokemon allowed for catch duration", Position = 40)]
        [DefaultValue(500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 40)]
        public int CatchPokemonLimit { get; set; }

        [NecroBotConfig(Description = "Catch duration applied for catch limit & number", Position = 50)]
        [DefaultValue(60 * 22 + 30)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 50)]
        public int CatchPokemonLimitMinutes { get; set; }

        /*Incense*/
        [NecroBotConfig(Description = "Allows bot to use Incense", Position = 60)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 60)]
        public bool UseIncenseConstantly;

        /*Egg*/
        [NecroBotConfig(Description = "Allows bot to put an egg in an Incubator for hatching", Position = 70)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 70)]
        public bool UseEggIncubators { get; set; }

        [NecroBotConfig(Description = "When Enabled, bot will only put 10km egg into a non-infinity incubator", Position = 80)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 80)]
        public bool UseLimitedEggIncubators { get; set; }

        [NecroBotConfig(Description = "When Enabled, bot will always use a lucky egg when they are available in bag", Position = 90)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 90)]
        public bool UseLuckyEggConstantly;

        /*Berries*/
        //[NecroBotConfig(Description = "Specify min CP will be use berries when catch", Position = 120)]
        //[DefaultValue(1000)]
        //[Range(0, 9999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 120)]
        //public int UseBerriesMinCp { get; set; }

        //[NecroBotConfig(Description = "Specify min IV will be use berries when catch", Position = 130)]
        //[DefaultValue(90)]
        //[Range(0, 100)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 130)]
        //public float UseBerriesMinIv { get; set; }

        //[NecroBotConfig(Description = "Specify max catch chance  will be use berries when catch", Position = 140)]
        //[DefaultValue(0.20)]
        //[Range(0, 1)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 140)]
        //public double UseBerriesBelowCatchProbability { get; set; }

        //[NecroBotConfig(Description = "The operator logic for berry use", Position = 150)]
        //[DefaultValue("or")]
        //[EnumDataType(typeof(Operator))]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 150)]
        //public string UseBerriesOperator { get; set; }

        //[NecroBotConfig(Description = "Number of berries can be used for 1 pokemon", Position = 160)]
        //[DefaultValue(30)]
        //[Range(0, 999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 160)]
        //public int MaxBerriesToUsePerPokemon { get; set; }

        /*Transfer*/
        [NecroBotConfig(Description = "Allows bot to transfer weak/low cp pokemon", Position = 170)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 170)]
        public bool TransferWeakPokemon;

        [NecroBotConfig(Description = "Allows bot to transfer all duplicate pokemon", Position = 180)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 180)]
        public bool TransferDuplicatePokemon { get; set; }

        [NecroBotConfig(Description = "Allows bot to transfer duplicated pokemon right after catch", Position = 190)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 190)]
        public bool TransferDuplicatePokemonOnCapture { get; set; }

        /*Rename*/
        [NecroBotConfig(Description = "Allows bot to rename pokemon after catch", Position = 200)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 200)]
        public bool RenamePokemon;

        [NecroBotConfig(Description = "Set Min IV for rename, bot will only rename pokemon that has IV higher then this value", Position = 210)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 210)]
        public bool RenameOnlyAboveIv;

        [NecroBotConfig(Description = "The template for pokemon rename", Position = 220)]
        [DefaultValue("{Name}_{IV}_Lv{Level}")]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 220)]
        public string RenameTemplate { get; set; }

        /*Favorite*/
        [NecroBotConfig(Description = "Set min IV for auto favoriting pokemon", Position = 230)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 230)]
        public float FavoriteMinIvPercentage { get; set; }


        [NecroBotConfig(Description = "Allows bot to auto favorite pokemon after catch", Position = 240)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 240)]
        public bool AutoFavoritePokemon;
        
        [NecroBotConfig(Description = "Allows bot to auto favorite any shiny pokemon on catch", Position = 250)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 250)]
        public bool AutoFavoriteShinyOnCatch;

        /*PokeBalls*/
        [NecroBotConfig(Description = "Number of balls that will be used to catch a pokemon", Position = 260)]
        [DefaultValue(6)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 260)]
        public int MaxPokeballsPerPokemon { get; set; }

        [NecroBotConfig(Description = "Min CP for using Great Ball instead of PokeBall", Position = 270)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 270)]
        public int UseGreatBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min CP for using Ultra Ball instead of Great Ball", Position = 280)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 280)]
        public int UseUltraBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min CP for using Master Ball instead of Ultra Ball", Position = 290)]
        [DefaultValue(1500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 290)]
        public int UseMasterBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min IV for using Great Ball instead of PokeBall", Position = 300)]
        [DefaultValue(85.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 300)]
        public double UseGreatBallAboveIv { get; set; }

        [NecroBotConfig(Description = "Min CP for using Ultra Ball instead of Great Ball", Position = 310)]
        [DefaultValue(95.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 310)]
        public double UseUltraBallAboveIv { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Ultra Ball instead of PokeBall", Position = 320)]
        [DefaultValue(0.2)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 320)]
        public double UseGreatBallBelowCatchProbability { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Ultra Ball instead of Great Ball", Position = 330)]
        [DefaultValue(0.1)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 330)]
        public double UseUltraBallBelowCatchProbability { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Master Ball instead of Ultra Ball", Position = 340)]
        [DefaultValue(0.05)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 340)]
        public double UseMasterBallBelowCatchProbability { get; set; }

        /*PoweUp*/
        [NecroBotConfig(Description = "Allows bot to power up Pokemon ", Position = 350)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 350)]
        public bool AutomaticallyLevelUpPokemon;

        [NecroBotConfig(Description = "Only allow the bot to upgrade favorited pokemon", Position = 360)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 360)]
        public bool OnlyUpgradeFavorites { get; set; }

        [NecroBotConfig(Description = "Use level up on this list of pokemon", Position = 370)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 370)]
        public bool UseLevelUpList { get; set; }

        [NecroBotConfig(Description = "Number of times to upgrade a Pokemon", Position = 380)]
        [DefaultValue(5)]
        [Range(0, 99)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 380)]
        public int AmountOfTimesToUpgradeLoop { get; set; }

        [NecroBotConfig(Description = "Min stardust to keep for auto power up", Position = 390)]
        [DefaultValue(5000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 390)]
        public int GetMinStarDustForLevelUp { get; set; }

        [NecroBotConfig(Description = "Select pokemon to powerup by IV or CP", Position = 400)]
        [DefaultValue("iv")]
        [EnumDataType(typeof(CpIv))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 400)]
        public string LevelUpByCPorIv { get; set; }

        [NecroBotConfig(Description = "Min CP for pokemon upgrade", Position = 410)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 410)]
        public float UpgradePokemonCpMinimum { get; set; }

        [NecroBotConfig(Description = "Min IV for pokemon upgrade", Position = 420)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 420)]
        public float UpgradePokemonIvMinimum { get; set; }

        [NecroBotConfig(Description = "Logic operator for selecting pokemon for upgrade", Position = 430)]
        [DefaultValue("and")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 430)]
        public string UpgradePokemonMinimumStatsOperator { get; set; }

        /*Keep*/
        [NecroBotConfig(Description = "Allows bot to keep pokemon for evolving if configured in EvolveConfig and appropriate candy is available", Position = 490)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 490)]
        public bool KeepPokemonsToBeEvolved { get; set; }

        [NecroBotConfig(Description = "Specify min CP to not transfer pokemon", Position = 500)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 500)]
        public int KeepMinCp { get; set; }

        [NecroBotConfig(Description = "Specify min IV to not transfer pokemon", Position = 510)]
        [DefaultValue(90)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 510)]
        public float KeepMinIvPercentage { get; set; }

        [NecroBotConfig(Description = "Specify min LV to not transfer pokemon", Position = 520)]
        [DefaultValue(6)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 520)]
        public int KeepMinLvl { get; set; }

        [NecroBotConfig(Description = "Logic operator for keep pokemon check", Position = 530)]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 530)]
        public string KeepMinOperator { get; set; }

        [NecroBotConfig(Description = "Tell bot to check level before transfer", Position = 540)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 540)]
        public bool UseKeepMinLvl;

        [NecroBotConfig(Description = "Keep pokemon has higher IV then CP to not transfer pokemon", Position = 550)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 550)]
        public bool PrioritizeIvOverCp { get; set; }

        [NecroBotConfig(Description = "Min number of duplicated pokemon to keep", Position = 560)]
        [DefaultValue(1)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 560)]
        public int KeepMinDuplicatePokemon { get; set; }

        [NecroBotConfig(Description = "Max number of duplicated pokemon to keep", Position = 570)]
        [DefaultValue(1000)]
        [Range(0, 100000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 570)]
        public int KeepMaxDuplicatePokemon { get; set; }

        /*NotCatch*/
        [NecroBotConfig(Description = "Use the list pokemon not catch filter", Position = 580)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 580)]
        public bool UsePokemonToNotCatchFilter { get; set; }

        [NecroBotConfig(Description = "Use the Pokemon To Catch Local List", Position = 590)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 590)]
        public bool UsePokemonToCatchLocallyListOnly { get; set; }

        /*Dump Stats*/
        [NecroBotConfig(Description = "Allows bot to dump list pokemon to csv file", Position = 600)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 600)]
        public bool DumpPokemonStats;

        [DefaultValue(10000)]
        [NecroBotConfig(Description = "Delay time between pokemon upgrades", Position = 610)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 610)]
        public int DelayBetweenPokemonUpgrade { get; set; }

        [DefaultValue(5)]
        [NecroBotConfig(Description = "Temporarily disable catching pokemon for certain minutes if bot runs out of balls", Position = 620)]
        [Range(0, 120)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 620)]
        public int OutOfBallCatchBlockTime { get; set; }

        [DefaultValue(50)]
        [NecroBotConfig(Description = "Number of balls you want to save for snipe or manual play - it means if total balls is less than this value, catch pokemon will be deactivated", Position = 630)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 630)]
        public int PokeballToKeepForSnipe { get; set; }

        [DefaultValue(true)]
        [NecroBotConfig(Description = "Transfer multiple pokemon at once - this will increase bot speed and reduce api call", Position = 640)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 640)]
        public bool UseBulkTransferPokemon { get; set; }

        [DefaultValue(10)]
        [NecroBotConfig(Description = "Bot will transfer pokemons only when MaxStogare < pokemon + buffer", Position = 650)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 650)]
        public int BulkTransferStogareBuffer { get; set; }

        [DefaultValue(100)]
        [NecroBotConfig(Description = "Maximun number of pokemon in a transfer", Position = 660)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 660)]
        [Range(1,100)]
        public int BulkTransferSize { get; set; }

        [DefaultValue(Operator.or)]
        [NecroBotConfig(Description = "Use ball operator between IV and CP ", Position = 670)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 670)]
        public Operator UseBallOperator  { get; set; }


        /*Favorite CP*/
        [NecroBotConfig(Description = "Set min CP for auto favoriting pokemon", Position = 680)]
        [DefaultValue(0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 680)]
        public float FavoriteMinCp { get; set; }

        [NecroBotConfig(Description = "Set Buddy pokemon", Position = 690)]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 690)]
        public string DefaultBuddyPokemon { get; set; }

        [NecroBotConfig(Description = "Min level to use favoriting", Position = 700)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 700)]
        public int FavoriteMinLevel { get; set; }

        [NecroBotConfig(Description = "The logic operator to check compbo IV, CP, Level to favorite pokemon", Position = 710)]
        [DefaultValue(Operator.and)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 710)]
        public Operator FavoriteOperator { get; set; }

        [NecroBotConfig(Description = "If Enabled, bot will only rename pokemon not meeting transfer settings, otherwise, the bot will rename all pokemon in bag", Position = 720)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 720)]
        public  bool RenamePokemonRespectTransferRule { get;  set; }

        [NecroBotConfig(Description = "Minimum pokemon level to upgrade", Position = 730)]
        [DefaultValue(30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 730)]
        public double UpgradePokemonLvlMinimum { get;  set; }

        [NecroBotConfig(Description = "Allows bot to bypass catchflee - not recommended to use this feature", Position = 790)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 790)]
        public bool ByPassCatchFlee{ get; set; }
        
        [NecroBotConfig(SheetName = "EvolveConfig", Description = "Setting up for pokemon evolving", Position = 800)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore, Order = 800)]
        public EvolveConfig EvolveConfig = new EvolveConfig();
    }
}
