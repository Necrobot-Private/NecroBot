using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;

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

        [NecrobotConfig(Description = "Allow bot catch pokemon", Position = 1)]
        /*Catch*/
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool CatchPokemon { get; set; }

        [NecrobotConfig(Description = "Delay time between 2 time catch pokemon ", Position = 2)]
        [DefaultValue(2000)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public int DelayBetweenPokemonCatch { get; set; }

        /*CatchLimit*/
        [NecrobotConfig(Description = "Check for daily limit catch rate - CatchPokemonLimit per CatchPokemonLimitMinutes", Position = 3)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public bool UseCatchLimit { get; set; }

        [NecrobotConfig(Description = "Number of pokemon allow for catch duration", Position = 4)]
        [DefaultValue(700)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public int CatchPokemonLimit { get; set; }

        [NecrobotConfig(Description = "Catch duration apply for catch limit & number", Position = 5)]
        [DefaultValue(60 * 22 + 30)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public int CatchPokemonLimitMinutes { get; set; }

        /*Incense*/
        [NecrobotConfig(Description = "Allow bot use Incense ", Position = 6)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public bool UseIncenseConstantly;

        /*Egg*/
        [NecrobotConfig(Description = "Allow bot put egg in Incubator for hatching", Position = 7)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public bool UseEggIncubators { get; set; }

        [NecrobotConfig(Description = "TUrn this on bot only put 10km egg in to non infinity incubator", Position = 8)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public bool UseLimitedEggIncubators { get; set; }

        [NecrobotConfig(Description = "Turn on to allow bot always use lucky egg when they are available in bag", Position = 9)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool UseLuckyEggConstantly;

        [NecrobotConfig(Description = "Number of pokemon ready for evolve that can use lucky egg", Position = 10)]
        [DefaultValue(30)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public int UseLuckyEggsMinPokemonAmount{ get; set; }

        [NecrobotConfig(Description = "Allow bot use lucky egg when evolve pokemon", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public bool UseLuckyEggsWhileEvolving;

        /*Berries*/
        //[NecrobotConfig(Description = "Specify min CP will be use berries when catch", Position = 12)]
        //[DefaultValue(1000)]
        //[Range(0, 9999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        //public int UseBerriesMinCp { get; set; }

        //[NecrobotConfig(Description = "Specify min IV will be use berries when catch", Position = 13)]
        //[DefaultValue(90)]
        //[Range(0, 100)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        //public float UseBerriesMinIv { get; set; }

        //[NecrobotConfig(Description = "Specify max catch chance  will be use berries when catch", Position = 14)]
        //[DefaultValue(0.20)]
        //[Range(0, 1)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        //public double UseBerriesBelowCatchProbability { get; set; }

        //[NecrobotConfig(Description = "The operator logic for berry use", Position = 15)]
        //[DefaultValue("or")]
        //[EnumDataType(typeof(Operator))]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        //public string UseBerriesOperator { get; set; }

        //[NecrobotConfig(Description = "Number of berries can be used for 1 pokemon", Position = 16)]
        //[DefaultValue(30)]
        //[Range(0, 999)]
        //[JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 16)]
        //public int MaxBerriesToUsePerPokemon { get; set; }

        /*Transfer*/
        [NecrobotConfig(Description = "Allow bot transfer weeak/low cp pokemon", Position = 17)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 17)]
        public bool TransferWeakPokemon;

        [NecrobotConfig(Description = "Alow bot transfer all duplicate pokemon", Position = 18)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 18)]
        public bool TransferDuplicatePokemon { get; set; }

        [NecrobotConfig(Description = "Allow bo transfer duplicated pokemon right after catch", Position = 19)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 19)]
        public bool TransferDuplicatePokemonOnCapture { get; set; }

        /*Rename*/
        [NecrobotConfig(Description = "Allow bot rename pokemon after catch", Position = 20)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 20)]
        public bool RenamePokemon;

        [NecrobotConfig(Description = "Set Min IV for rename , bot only rename pokemon has IV higher then this value", Position = 21)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 21)]
        public bool RenameOnlyAboveIv;

        [NecrobotConfig(Description = "The template for pokemon rename", Position = 22)]
        [DefaultValue("{Name}_{IV}_Lv{Level}")]
        [MinLength(0)]
        [MaxLength(32)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 22)]
        public string RenameTemplate { get; set; }

        /*Favorite*/
        [NecrobotConfig(Description = "Set min IV for auto favorite pokemon", Position = 23)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 23)]
        public float FavoriteMinIvPercentage { get; set; }


        [NecrobotConfig(Description = "Allow bot auto favorite pokemon after catch", Position = 24)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 24)]
        public bool AutoFavoritePokemon;
        
        [NecrobotConfig(Description = "Allow bot auto favorite any shiny pokemon on catch", Position = 25)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 25)]
        public bool AutoFavoriteShinyOnCatch;

        /*PokeBalls*/
        [NecrobotConfig(Description = "Number of balls will be use for catch a pokemon", Position = 26)]
        [DefaultValue(6)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 26)]
        public int MaxPokeballsPerPokemon { get; set; }

        [NecrobotConfig(Description = "Define min CP for use greate ball instead of PokeBall", Position = 27)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 27)]
        public int UseGreatBallAboveCp { get; set; }

        [NecrobotConfig(Description = "Define min CP for use Ultra Ball instead of Great Ball", Position = 28)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 28)]
        public int UseUltraBallAboveCp { get; set; }

        [NecrobotConfig(Description = "Define min CP for use master ball instead of Ultra Ball", Position = 29)]
        [DefaultValue(1500)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 29)]
        public int UseMasterBallAboveCp { get; set; }

        [NecrobotConfig(Description = "Define min IV for use Great Ball instead of PokeBall", Position = 30)]
        [DefaultValue(85.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 30)]
        public double UseGreatBallAboveIv { get; set; }

        [NecrobotConfig(Description = "Define min CP for use ultra ball instead of Great Ball", Position = 31)]
        [DefaultValue(95.0)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 31)]
        public double UseUltraBallAboveIv { get; set; }

        [NecrobotConfig(Description = "Define min catch probability for use Ultra Ball instead of pokemon ball", Position = 32)]
        [DefaultValue(0.2)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 32)]
        public double UseGreatBallBelowCatchProbability { get; set; }

        [NecrobotConfig(Description = "Define min catch probability for use Ultra Ball instead of greate ball", Position = 33)]
        [DefaultValue(0.1)]
        [Range(0, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 33)]
        public double UseUltraBallBelowCatchProbability { get; set; }

        [NecrobotConfig(Description = "Define min catch probability for use Master Ball instead of ultra ball", Position = 34)]
        [DefaultValue(0.05)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 34)]
        public double UseMasterBallBelowCatchProbability { get; set; }

        /*PoweUp*/
        [NecrobotConfig(Description = "Allow bot power up pokemon ", Position = 35)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 35)]
        public bool AutomaticallyLevelUpPokemon;

        [NecrobotConfig(Description = "Only allow bot upgrade favorited pokemon", Position = 36)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 36)]
        public bool OnlyUpgradeFavorites { get; set; }

        [NecrobotConfig(Description = "Use level up list pokemon", Position = 37)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 37)]
        public bool UseLevelUpList { get; set; }

        [NecrobotConfig(Description = "Number of time upgrade 1 pokemon", Position = 38)]
        [DefaultValue(5)]
        [Range(0, 99)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 38)]
        public int AmountOfTimesToUpgradeLoop { get; set; }

        [NecrobotConfig(Description = "Min startdust keep for auto power up", Position = 39)]
        [DefaultValue(5000)]
        [Range(0, 999999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 39)]
        public int GetMinStarDustForLevelUp { get; set; }

        [NecrobotConfig(Description = "Select pokemon to powerup by IV or CP", Position = 40)]
        [DefaultValue("iv")]
        [EnumDataType(typeof(CpIv))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 40)]
        public string LevelUpByCPorIv { get; set; }

        [NecrobotConfig(Description = "MIn CP for pokemon upgrade", Position = 41)]
        [DefaultValue(1000)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 41)]
        public float UpgradePokemonCpMinimum { get; set; }

        [NecrobotConfig(Description = "MIn IV for pokemon upgrade", Position = 42)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 42)]
        public float UpgradePokemonIvMinimum { get; set; }

        [NecrobotConfig(Description = "Logic operator for select pokemon for upgrade", Position = 43)]
        [DefaultValue("and")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 43)]
        public string UpgradePokemonMinimumStatsOperator { get; set; }

        /*Evolve*/
        [NecrobotConfig(Description = "Specify min IV for evolve pokemon", Position = 44)]
        [DefaultValue(95)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 44)]
        public float EvolveAboveIvValue { get; set; }

        [NecrobotConfig(Description = "Allow bot evolve all pokemon above this IV", Position = 45)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 45)]
        public bool EvolveAllPokemonAboveIv;

        [NecrobotConfig(Description = "When turn on, bot will evolve pokemon when has enought candy", Position = 46)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 46)]
        public bool EvolveAllPokemonWithEnoughCandy { get; set; }

        [NecrobotConfig(Description = "Specify the max storage pokemon bag for trigger evolve", Position = 47)]
        [DefaultValue(90)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 47)]
        public double EvolveKeptPokemonsAtStorageUsagePercentage { get; set; }

        [NecrobotConfig(Description = "Specify the pokemon to keep for mass evolve", Position = 48)]
        [DefaultValue(120)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 48)]
        public int EvolveKeptPokemonIfBagHasOverThisManyPokemon = 120;

        /*Keep*/
        [NecrobotConfig(Description = "Allow bot keep low candy pokemon for evolve", Position = 49)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 49)]
        public bool KeepPokemonsThatCanEvolve;

        [NecrobotConfig(Description = "Specify min CP to not transfer pokemon", Position = 50)]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 50)]
        public int KeepMinCp { get; set; }

        [NecrobotConfig(Description = "Specify min IV to not transfer pokemon", Position = 51)]
        [DefaultValue(90)]
        [Range(0, 101)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 51)]
        public float KeepMinIvPercentage { get; set; }

        [NecrobotConfig(Description = "Specify min LV to not transfer pokemon", Position = 52)]
        [DefaultValue(6)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 52)]
        public int KeepMinLvl { get; set; }

        [NecrobotConfig(Description = "Logic operator for keep pokemon check", Position = 53)]
        [DefaultValue("or")]
        [EnumDataType(typeof(Operator))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 53)]
        public string KeepMinOperator { get; set; }

        [NecrobotConfig(Description = "Tell bot to check level before transfer", Position = 54)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 54)]
        public bool UseKeepMinLvl;

        [NecrobotConfig(Description = "Keep pokemon has higher IV then CP to not transfer pokemon", Position = 55)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 55)]
        public bool PrioritizeIvOverCp { get; set; }

        [NecrobotConfig(Description = "Number of duplicated pokemon to keep", Position = 56)]
        [DefaultValue(1)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 56)]
        public int KeepMinDuplicatePokemon { get; set; }

        /*NotCatch*/
        [NecrobotConfig(Description = "Use list pokemon not catch filter", Position = 57)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 57)]
        public bool UsePokemonToNotCatchFilter { get; set; }

        [NecrobotConfig(Description = "UsePokemonToCatchLocallyListOnly", Position = 58)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 58)]
        public bool UsePokemonToCatchLocallyListOnly { get; set; }

        /*Dump Stats*/
        [NecrobotConfig(Description = "Allow bot dump list pokemon to csv file", Position = 59)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 59)]
        public bool DumpPokemonStats;

        [DefaultValue(10000)]
        [NecrobotConfig(Description = "Delay time between pokemon upgrade", Position = 60)]
        [Range(0, 99999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 60)]
        public int DelayBetweenPokemonUpgrade { get; set; }

        [DefaultValue(5)]
        [NecrobotConfig(Description = "Temporary disable catch pokemon for certain minutes if bot run out of balls", Position = 61)]
        [Range(0, 120)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 61)]
        public int OutOfBallCatchBlockTime { get; set; }

        [DefaultValue(50)]
        [NecrobotConfig(Description = "Number of balls you want to save for snipe or manual play - it mean if total ball less than this value, catch pokemon will be deactive", Position = 62)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 62)]
        public int PokeballToKeepForSnipe { get; set; }

        [DefaultValue(true)]
        [NecrobotConfig(Description = "Transfer multiple pokemon at 1 time - that will increase bot speed and reduce api call", Position = 63)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 63)]
        public bool UseBulkTransferPokemon { get; set; }

        [DefaultValue(10)]
        [NecrobotConfig(Description = "Bot will transfer pokemons only when MaxStogare < pokemon + buffer", Position = 64)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 64)]
        public int BulkTransferStogareBuffer { get; set; }

        [DefaultValue(100)]
        [NecrobotConfig(Description = "Maximun number of pokemon in 1 transfer", Position = 65)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 65)]
        [Range(1,100)]
        public int BulkTransferSize { get; set; }

        [DefaultValue(Operator.or)]
        [NecrobotConfig(Description = "Use ball operator between IV and CP ", Position = 66)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 66)]
        public Operator UseBallOperator  { get; set; }


        /*Favorite CP*/
        [NecrobotConfig(Description = "Set min CP for auto favorite pokemon", Position = 67)]
        [DefaultValue(0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 67)]
        public float FavoriteMinCp { get; set; }

        [NecrobotConfig(Description = "Set Buddy pokemon", Position = 68)]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 68)]
        public string DefaultBuddyPokemon { get; set; }

        [NecrobotConfig(Description = "Min level to favorite", Position = 69)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 70)]
        public int FavoriteMinLevel { get; set; }

        [NecrobotConfig(Description = "The logic operator to check compbo IV, CP, Level to favorite pokemon", Position = 71)]
        [DefaultValue(Operator.and)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 71)]

        public Operator FavoriteOperator { get; set; }

        [NecrobotConfig(Description = "If this option set to true, bot only rename pokemon not meet with transfer settings, otherwise, bot will rename all pokemon in bag", Position = 72)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 72)]

        public  bool RenamePokemonRespectTransferRule { get;  set; }

        [NecrobotConfig(Description = "Minium pokemon level to upgrade", Position = 73)]
        [DefaultValue(30)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 73)]
        public double UpgradePokemonLvlMinimum { get;  set; }

        [NecrobotConfig(Description = "Global settting - Allow bot only evolve favorited pokemons", Position = 74)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 74)]

        public bool EvolveFavoritedOnly { get; set; }
        [NecrobotConfig(Description = "The logic check to evolve pokemon", Position = 75)]
        [DefaultValue("and")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 75)]
        public string EvolveOperator { get; set; }

        [NecrobotConfig(Description = "Set MinIV  for bot to evolve", Position = 76)]
        [DefaultValue(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 76)]
        [Range(0,101)]
        public double EvolveMinIV { get; set; }
        [NecrobotConfig(Description = "Set MinCP  for bot to evolve", Position = 77)]
        [DefaultValue(2000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 77)]
        [Range(0,5000)]

        public double EvolveMinCP { get; set; }
        [NecrobotConfig(Description = "Set MinLevel for bot to evolve", Position = 78)]
        [DefaultValue(25)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 78)]
        [Range(0, 5000)]
        public double EvolveMinLevel { get;  set; }

        [NecrobotConfig(Description = "Allow bot bypass catchflee - not recomment use this feature", Position = 79)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 79)]
        public bool ByPassCatchFlee{ get; set; }

    }
}
