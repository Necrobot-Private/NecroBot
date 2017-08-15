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

        [NecroBotConfig(Description = "Allows bot to Catch Pokemon", Position = 1)]
        /*Catch*/
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool CatchPokemon { get; set; }

        [NecroBotConfig(Description = "Delay time between time catching pokemon", Position = 2)]
        [DefaultValue(2000)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int DelayBetweenPokemonCatch { get; set; }

        /*CatchLimit*/
        [NecroBotConfig(Description = "Check for daily limit catch rate - CatchPokemonLimit per CatchPokemonLimitMinutes", Position = 3)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public bool UseCatchLimit { get; set; }

        [NecroBotConfig(Description = "Number of pokemon allowed for catch duration", Position = 4)]
        [DefaultValue(500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int CatchPokemonLimit { get; set; }

        [NecroBotConfig(Description = "Catch duration applied for catch limit & number", Position = 5)]
        [DefaultValue(60 * 22 + 30)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int CatchPokemonLimitMinutes { get; set; }

        /*Incense*/
        [NecroBotConfig(Description = "Allows bot to use Incense", Position = 6)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public bool UseIncenseConstantly;

        /*Egg*/
        [NecroBotConfig(Description = "Allows bot to put an egg in an Incubator for hatching", Position = 7)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public bool UseEggIncubators { get; set; }

        [NecroBotConfig(Description = "When Enabled, bot will only put 10km egg into a non-infinity incubator", Position = 8)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public bool UseLimitedEggIncubators { get; set; }

        [NecroBotConfig(Description = "When Enabled, bot will always use a lucky egg when they are available in bag", Position = 9)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool UseLuckyEggConstantly;

        [NecroBotConfig(Description = "Number of Pokemon ready for evolve that can and are able to use lucky egg", Position = 10)]
        [DefaultValue(30)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public int UseLuckyEggsMinPokemonAmount{ get; set; }

        [NecroBotConfig(Description = "Allows bot to use lucky eggs when evolving pokemon", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public bool UseLuckyEggsWhileEvolving;

        /*Berries*/
        //[NecroBotConfig(Description = "Specify min CP will be use berries when catch", Position = 12)]
        //[DefaultValue(1000)]
        //[Range(0, 9999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        //public int UseBerriesMinCp { get; set; }

        //[NecroBotConfig(Description = "Specify min IV will be use berries when catch", Position = 13)]
        //[DefaultValue(90)]
        //[Range(0, 100)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        //public float UseBerriesMinIv { get; set; }

        //[NecroBotConfig(Description = "Specify max catch chance  will be use berries when catch", Position = 14)]
        //[DefaultValue(0.20)]
        //[Range(0, 1)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        //public double UseBerriesBelowCatchProbability { get; set; }

        //[NecroBotConfig(Description = "The operator logic for berry use", Position = 15)]
        //[DefaultValue("or")]
        //[EnumDataType(typeof(Operator))]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        //public string UseBerriesOperator { get; set; }

        //[NecroBotConfig(Description = "Number of berries can be used for 1 pokemon", Position = 16)]
        //[DefaultValue(30)]
        //[Range(0, 999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 16)]
        //public int MaxBerriesToUsePerPokemon { get; set; }

        /*Transfer*/
        [NecroBotConfig(Description = "Allows bot to transfer weak/low cp pokemon", Position = 17)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 17)]
        public bool TransferWeakPokemon;

        [NecroBotConfig(Description = "Allows bot to transfer all duplicate pokemon", Position = 18)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 18)]
        public bool TransferDuplicatePokemon { get; set; }

        [NecroBotConfig(Description = "Allows bot to transfer duplicated pokemon right after catch", Position = 19)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 19)]
        public bool TransferDuplicatePokemonOnCapture { get; set; }

        /*Rename*/
        [NecroBotConfig(Description = "Allows bot to rename pokemon after catch", Position = 20)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 20)]
        public bool RenamePokemon;

        [NecroBotConfig(Description = "Set Min IV for rename, bot will only rename pokemon that has IV higher then this value", Position = 21)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 21)]
        public bool RenameOnlyAboveIv;

        [NecroBotConfig(Description = "The template for pokemon rename", Position = 22)]
        [DefaultValue("{Name}_{IV}_Lv{Level}")]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 22)]
        public string RenameTemplate { get; set; }

        /*Favorite*/
        [NecroBotConfig(Description = "Set min IV for auto favoriting pokemon", Position = 23)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 23)]
        public float FavoriteMinIvPercentage { get; set; }


        [NecroBotConfig(Description = "Allows bot to auto favorite pokemon after catch", Position = 24)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 24)]
        public bool AutoFavoritePokemon;
        
        [NecroBotConfig(Description = "Allows bot to auto favorite any shiny pokemon on catch", Position = 25)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 25)]
        public bool AutoFavoriteShinyOnCatch;

        /*PokeBalls*/
        [NecroBotConfig(Description = "Number of balls that will be used to catch a pokemon", Position = 26)]
        [DefaultValue(6)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 26)]
        public int MaxPokeballsPerPokemon { get; set; }

        [NecroBotConfig(Description = "Min CP for using Great Ball instead of PokeBall", Position = 27)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 27)]
        public int UseGreatBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min CP for using Ultra Ball instead of Great Ball", Position = 28)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 28)]
        public int UseUltraBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min CP for using Master Ball instead of Ultra Ball", Position = 29)]
        [DefaultValue(1500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 29)]
        public int UseMasterBallAboveCp { get; set; }

        [NecroBotConfig(Description = "Min IV for using Great Ball instead of PokeBall", Position = 30)]
        [DefaultValue(85.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 30)]
        public double UseGreatBallAboveIv { get; set; }

        [NecroBotConfig(Description = "Min CP for using Ultra Ball instead of Great Ball", Position = 31)]
        [DefaultValue(95.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 31)]
        public double UseUltraBallAboveIv { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Ultra Ball instead of PokeBall", Position = 32)]
        [DefaultValue(0.2)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 32)]
        public double UseGreatBallBelowCatchProbability { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Ultra Ball instead of Great Ball", Position = 33)]
        [DefaultValue(0.1)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 33)]
        public double UseUltraBallBelowCatchProbability { get; set; }

        [NecroBotConfig(Description = "Min catch probability for using Master Ball instead of Ultra Ball", Position = 34)]
        [DefaultValue(0.05)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 34)]
        public double UseMasterBallBelowCatchProbability { get; set; }

        /*PoweUp*/
        [NecroBotConfig(Description = "Allows bot to power up Pokemon ", Position = 35)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 35)]
        public bool AutomaticallyLevelUpPokemon;

        [NecroBotConfig(Description = "Only allow the bot to upgrade favorited pokemon", Position = 36)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 36)]
        public bool OnlyUpgradeFavorites { get; set; }

        [NecroBotConfig(Description = "Use level up on this list of pokemon", Position = 37)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 37)]
        public bool UseLevelUpList { get; set; }

        [NecroBotConfig(Description = "Number of times to upgrade a Pokemon", Position = 38)]
        [DefaultValue(5)]
        [Range(0, 99)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 38)]
        public int AmountOfTimesToUpgradeLoop { get; set; }

        [NecroBotConfig(Description = "Min stardust to keep for auto power up", Position = 39)]
        [DefaultValue(5000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 39)]
        public int GetMinStarDustForLevelUp { get; set; }

        [NecroBotConfig(Description = "Select pokemon to powerup by IV or CP", Position = 40)]
        [DefaultValue("iv")]
        [EnumDataType(typeof(CpIv))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 40)]
        public string LevelUpByCPorIv { get; set; }

        [NecroBotConfig(Description = "Min CP for pokemon upgrade", Position = 41)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 41)]
        public float UpgradePokemonCpMinimum { get; set; }

        [NecroBotConfig(Description = "Min IV for pokemon upgrade", Position = 42)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 42)]
        public float UpgradePokemonIvMinimum { get; set; }

        [NecroBotConfig(Description = "Logic operator for selecting pokemon for upgrade", Position = 43)]
        [DefaultValue("and")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 43)]
        public string UpgradePokemonMinimumStatsOperator { get; set; }

        /*Evolve*/
        [NecroBotConfig(Description = "Specify min IV for evolving pokemon", Position = 44)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 44)]
        public float EvolveAboveIvValue { get; set; }

        [NecroBotConfig(Description = "Allows bot to evolve all pokemon above this IV", Position = 45)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 45)]
        public bool EvolveAllPokemonAboveIv;

        [NecroBotConfig(Description = "When enabled, bot will evolve pokemon when it has enough candy", Position = 46)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 46)]
        public bool EvolveAllPokemonWithEnoughCandy { get; set; }

        [NecroBotConfig(Description = "Specify the max storage pokemon bag for triggering evolve", Position = 47)]
        [DefaultValue(90)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 47)]
        public double EvolveKeptPokemonsAtStorageUsagePercentage { get; set; }

        [NecroBotConfig(Description = "Specify the pokemon to keep for mass evolve", Position = 48)]
        [DefaultValue(120)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 48)]
        public int EvolveKeptPokemonIfBagHasOverThisManyPokemon = 120;

        /*Keep*/
        [NecroBotConfig(Description = "Allows bot to keep low candy pokemon for evolve", Position = 49)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 49)]
        public bool KeepPokemonsThatCanEvolve;

        [NecroBotConfig(Description = "Specify min CP to not transfer pokemon", Position = 50)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 50)]
        public int KeepMinCp { get; set; }

        [NecroBotConfig(Description = "Specify min IV to not transfer pokemon", Position = 51)]
        [DefaultValue(90)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 51)]
        public float KeepMinIvPercentage { get; set; }

        [NecroBotConfig(Description = "Specify min LV to not transfer pokemon", Position = 52)]
        [DefaultValue(6)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 52)]
        public int KeepMinLvl { get; set; }

        [NecroBotConfig(Description = "Logic operator for keep pokemon check", Position = 53)]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 53)]
        public string KeepMinOperator { get; set; }

        [NecroBotConfig(Description = "Tell bot to check level before transfer", Position = 54)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 54)]
        public bool UseKeepMinLvl;

        [NecroBotConfig(Description = "Keep pokemon has higher IV then CP to not transfer pokemon", Position = 55)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 55)]
        public bool PrioritizeIvOverCp { get; set; }

        [NecroBotConfig(Description = "Min number of duplicated pokemon to keep", Position = 56)]
        [DefaultValue(1)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 56)]
        public int KeepMinDuplicatePokemon { get; set; }

        [NecroBotConfig(Description = "Max number of duplicated pokemon to keep", Position = 57)]
        [DefaultValue(1000)]
        [Range(0, 100000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 57)]
        public int KeepMaxDuplicatePokemon { get; set; }

        /*NotCatch*/
        [NecroBotConfig(Description = "Use the list pokemon not catch filter", Position = 58)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 58)]
        public bool UsePokemonToNotCatchFilter { get; set; }

        [NecroBotConfig(Description = "Use the Pokemon To Catch Local List", Position = 59)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 59)]
        public bool UsePokemonToCatchLocallyListOnly { get; set; }

        /*Dump Stats*/
        [NecroBotConfig(Description = "Allows bot to dump list pokemon to csv file", Position = 60)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 60)]
        public bool DumpPokemonStats;

        [DefaultValue(10000)]
        [NecroBotConfig(Description = "Delay time between pokemon upgrades", Position = 61)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 61)]
        public int DelayBetweenPokemonUpgrade { get; set; }

        [DefaultValue(5)]
        [NecroBotConfig(Description = "Temporarily disable catching pokemon for certain minutes if bot runs out of balls", Position = 62)]
        [Range(0, 120)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 62)]
        public int OutOfBallCatchBlockTime { get; set; }

        [DefaultValue(50)]
        [NecroBotConfig(Description = "Number of balls you want to save for snipe or manual play - it means if total balls is less than this value, catch pokemon will be deactivated", Position = 63)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 63)]
        public int PokeballToKeepForSnipe { get; set; }

        [DefaultValue(true)]
        [NecroBotConfig(Description = "Transfer multiple pokemon at once - this will increase bot speed and reduce api call", Position = 64)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 64)]
        public bool UseBulkTransferPokemon { get; set; }

        [DefaultValue(10)]
        [NecroBotConfig(Description = "Bot will transfer pokemons only when MaxStogare < pokemon + buffer", Position = 65)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 65)]
        public int BulkTransferStogareBuffer { get; set; }

        [DefaultValue(100)]
        [NecroBotConfig(Description = "Maximun number of pokemon in a transfer", Position = 66)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 66)]
        [Range(1,100)]
        public int BulkTransferSize { get; set; }

        [DefaultValue(Operator.or)]
        [NecroBotConfig(Description = "Use ball operator between IV and CP ", Position = 67)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 67)]
        public Operator UseBallOperator  { get; set; }


        /*Favorite CP*/
        [NecroBotConfig(Description = "Set min CP for auto favoriting pokemon", Position = 68)]
        [DefaultValue(0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 68)]
        public float FavoriteMinCp { get; set; }

        [NecroBotConfig(Description = "Set Buddy pokemon", Position = 69)]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 69)]
        public string DefaultBuddyPokemon { get; set; }

        [NecroBotConfig(Description = "Min level to use favoriting", Position = 70)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 70)]
        public int FavoriteMinLevel { get; set; }

        [NecroBotConfig(Description = "The logic operator to check compbo IV, CP, Level to favorite pokemon", Position = 71)]
        [DefaultValue(Operator.and)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 71)]
        public Operator FavoriteOperator { get; set; }

        [NecroBotConfig(Description = "If Enabled, bot will only rename pokemon not meeting transfer settings, otherwise, the bot will rename all pokemon in bag", Position = 72)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 72)]
        public  bool RenamePokemonRespectTransferRule { get;  set; }

        [NecroBotConfig(Description = "Minimum pokemon level to upgrade", Position = 73)]
        [DefaultValue(30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 73)]
        public double UpgradePokemonLvlMinimum { get;  set; }

        [NecroBotConfig(Description = "Global settting - Allows bot to only evolve favorited pokemons", Position = 74)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 74)]

        public bool EvolveFavoritedOnly { get; set; }
        [NecroBotConfig(Description = "The logic check to evolve pokemon", Position = 75)]
        [DefaultValue("and")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 75)]
        public string EvolveOperator { get; set; }

        [NecroBotConfig(Description = "Set Min IV for bot to evolve", Position = 76)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 76)]
        [Range(0,101)]
        public double EvolveMinIV { get; set; }
        [NecroBotConfig(Description = "Set Min CP for bot to evolve", Position = 77)]
        [DefaultValue(2000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 77)]
        [Range(0,5000)]

        public double EvolveMinCP { get; set; }
        [NecroBotConfig(Description = "Set Min Level for bot to evolve", Position = 78)]
        [DefaultValue(25)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 78)]
        [Range(0, 5000)]
        public double EvolveMinLevel { get;  set; }

        [NecroBotConfig(Description = "Allows bot to bypass catchflee - not recommended to use this feature", Position = 79)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 79)]
        public bool ByPassCatchFlee{ get; set; }

    }
}
