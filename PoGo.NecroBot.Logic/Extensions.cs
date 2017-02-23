using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Data;
using POGOProtos.Enums;
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

        public static T GetFilter<T>(this Dictionary<PokemonId, T> source, PokemonId pid) where T:IPokemonFilter
        {
            return FilterUtil.GetApplyFilter<T>(source, pid);
        }
    }
}
