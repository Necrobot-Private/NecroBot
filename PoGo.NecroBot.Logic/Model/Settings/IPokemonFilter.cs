using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public interface IPokemonFilter
    {
        List<PokemonId> AffectToPokemons { get; set; }
        IPokemonFilter GetGlobalFilter();
    }
}
