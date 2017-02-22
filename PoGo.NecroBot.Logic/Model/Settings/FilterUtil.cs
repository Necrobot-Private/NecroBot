using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class FilterUtil
    {
        public static T GetApplyFilter<T>(Dictionary<PokemonId, T> source, PokemonId forPokemonId) where T : IPokemonFilter
        {
            if (source.ContainsKey(forPokemonId)) return source[forPokemonId];
            foreach (var item in source)
            {
                if (item.Value.AffectToPokemons != null && item.Value.AffectToPokemons.Contains(forPokemonId)) return item.Value;

            }
            return GetDefault<T>();
        }

        private static T GetDefault<T>() where T : IPokemonFilter
        {

            var globalFilter = Activator.CreateInstance<T>();
            return (T)globalFilter.GetGlobalFilter();
        }
    }
}
