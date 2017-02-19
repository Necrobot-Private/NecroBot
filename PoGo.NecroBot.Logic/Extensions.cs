using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic
{
    public static class Extensions
    {
        public static double Perfection(this PokemonData pkm)
        {
            return PokemonInfo.CalculatePokemonPerfection(pkm);
        }
        public static double Level(this PokemonData pkm)
        {
            return PokemonInfo.GetLevel(pkm);
        }

        public static double CP(this PokemonData pkm)
        {
            return PokemonInfo.CalculateCp(pkm);
        }
    }
}
