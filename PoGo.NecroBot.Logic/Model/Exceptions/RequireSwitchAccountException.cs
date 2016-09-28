using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Exceptions
{
    public class RequireSwitchAccountException : Exception
    {
        public double LastLatitude { get; set; }
        public double LastLongitude { get; set; }
        public PokemonId LastEncounterPokemonId{ get; set; }
    }
}
