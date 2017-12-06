using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Gym Config", Description = "Setup rules for bot for gyms", ItemRequired = Required.DisallowNull)]
    public class GymConfig  : BaseConfig
    {
        public GymConfig() : base()
        {
        }

        internal enum TeamColor
        {
            Neutral,
            Yellow,
            Red,
            Blue
        }

        [NecroBotConfig(Description = "Allows bot go to a gym for berries, defense or battle with other teams.", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Enable { get; set; }

        [NecroBotConfig(Description = "If Enabled, bot will select a gym to go to instead of a pokestop if gym distance is valid", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrioritizeGymOverPokestop { get; set; }

        [NecroBotConfig(Description = "Maximum distance bot is allowed to walk to a gym.", Position = 3)]
        [DefaultValue(1500.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxDistance { get; set; }

        [NecroBotConfig(Description = "Default team color for bot to join.", Position = 4)]
        [DefaultValue("Neutral")]
        [EnumDataType(typeof(TeamColor))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public string DefaultTeam { get; set; }

        [NecroBotConfig(Description = "Max CP that pokemon will be selected for defending gyms.", Position = 5)]
        [DefaultValue(4000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxCPToDeploy { get; set; }

        [NecroBotConfig(Description = "Max LV of pokemon that can be put into a gym.", Position = 6)]
        [DefaultValue(40)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxLevelToDeploy { get; set; }

        [NecroBotConfig(Description = "Enables attacking raids", Position = 7)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableAttackRaid { get; set; }

        [NecroBotConfig(Description = "Use random pokemon for gym.", Position = 8)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseRandomPokemon { get; set; }

        [NecroBotConfig(Description = "Enables attacking gyms", Position = 9)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableAttackGym { get; set; }

        [NecroBotConfig(Description = "Enables deploy pokemon", Position = 10)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableDeployPokemon { get; set; }

        [NecroBotConfig(Description = "Heal defenders before applying to gym", Position = 11)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool HealDefendersBeforeApplyToGym { get; set; }

        [NecroBotConfig(Description = "Enables gym berries", Position = 12)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableGymBerries { get; set; }

        [NecroBotConfig(Description = "Min CP pokemon to use in attacking gyms", Position = 13)]
        [DefaultValue(1000)]
        [Range(1, 3500)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinCpToUseInAttack { get; set; }

        [NecroBotConfig(Description = "But not less than defender's percent", Position = 14)]
        [DefaultValue(0.75)]
        [Range(0.01, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double ButNotLessThanDefenderPercent { get; set; }

        [NecroBotConfig(Description = "Use Pokemon to attack only by their CP", Position = 15)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokemonToAttackOnlyByCp { get; set; }

        [NecroBotConfig(Description = "Do not use dodge in gyms", Position = 16)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseDodge { get; set; }

        [NecroBotConfig(Description = "Minimum revive potions to use gym module", Position = 17)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinRevivePotions { get; set; }

        [NecroBotConfig(Description = "Prioritize Gym with free slot", Position = 18)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrioritizeGymWithFreeSlot { get; set; }

        [NecroBotConfig(Description = "Save Max Revives", Position = 19)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SaveMaxRevives { get; set; }

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [NecroBotConfig(Description = "List of Defenders to use for Gyms", Position = 20)]
        public List<TeamMemberConfig> Defenders { get; set; } = TeamMemberConfig.GetDefaultDefenders();

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [NecroBotConfig(Description = "List of Attackers to use for Gyms", Position = 21)]
        public List<TeamMemberConfig> Attackers { get; set; } = TeamMemberConfig.GetDefaultAttackers();
    }

    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)]
    public class TeamMemberConfig : BaseConfig
    {
        public TeamMemberConfig() : base()
        {
        }

        [NecroBotConfig(Description = "Pokemon to use in Gyms", Position = 1)]
        [JsonProperty(Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate)]
        public PokemonId Pokemon { get; set; }

        [NecroBotConfig(Description = "Min CP to use in a team", Position = 2)]
        [Range(1, 5000)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? MinCP { get; set; }

        [NecroBotConfig(Description = "Max CP to use in a team", Position = 3)]
        [Range(1, 5000)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? MaxCP { get; set; }

        [NecroBotConfig(Description = "Priority", Position = 4)]
        [Range(1, 100)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Priority { get; set; }

        [NecroBotConfig(Key = "Moves", Description = "List of Moves to use in Gyms", Position = 5)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [DefaultValue(null)]
        public List<PokemonMove[]> Moves { get; set; }

        internal static List<TeamMemberConfig> GetDefaultDefenders()
        {
            return new List<TeamMemberConfig>()
            {
                { new TeamMemberConfig() { Pokemon=PokemonId.Lapras, MinCP=1000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Snorlax, MinCP=1000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Vaporeon, MinCP=2000, Moves = new List<PokemonMove[]>() { new PokemonMove[2] { PokemonMove.MoveUnset, PokemonMove.HydroPump } } } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Dragonite, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Charizard, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Alakazam, MinCP=2000 } }
            };
        }

        internal static List<TeamMemberConfig> GetDefaultAttackers()
        {
            return new List<TeamMemberConfig>()
            {
                { new TeamMemberConfig() { Pokemon=PokemonId.Dragonite, MinCP=2500 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Vaporeon, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Gyarados, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Snorlax, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Charizard, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Lapras, MinCP=2000 } }
            };
        }

        internal bool IsMoveMatch(PokemonMove move1, PokemonMove move2)
        {
            if(Moves!=null && Moves.Count > 0)
            {
                return Moves.Find(f => (f[0] == move1 || f[0] == PokemonMove.MoveUnset) && (f[1] == move2 || f[1] == PokemonMove.MoveUnset)) != null;
            }
            return true;
        }
    }
}
