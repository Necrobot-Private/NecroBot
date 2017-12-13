using POGOProtos.Enums;
using System.Collections.Generic;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public interface IPokemonFilter
    {
        List<PokemonId> AffectToPokemons { get; set; }
        IPokemonFilter GetGlobalFilter();
    }
}
