using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)] //Dont set Title
    public class SnipeFilter
    {
        public SnipeFilter()
        {
            Moves = new List<List<PokemonMove>>();
        }

        public SnipeFilter(int keepMinIvPercentage, List<List<PokemonMove>> moves = null)
        {
            this.SnipeIV = keepMinIvPercentage;
            this.Moves = moves;
        }

        [ExcelConfig(Key = "AI")]
        [DefaultValue(1250)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeIV {get; set;}

        [ExcelConfig(Key = "AJ")]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public List<List<PokemonMove>> Moves { get; set; }
          
        internal static Dictionary<PokemonId, SnipeFilter> SniperFilterDefault()
        {
            return new Dictionary<PokemonId, SnipeFilter>
            {
                { PokemonId.Lapras, new SnipeFilter(0, new List<List<PokemonMove>>() { }) }
            };
        }
    }
}