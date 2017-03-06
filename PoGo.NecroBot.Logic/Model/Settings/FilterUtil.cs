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
            if (source == null) return GetDefault<T>();

            try
            {
                if (source.ContainsKey(forPokemonId)) return source[forPokemonId];
                foreach (var item in source)
                {
                    if (item.Value.AffectToPokemons != null && item.Value.AffectToPokemons.Contains(forPokemonId)) return item.Value;
                }
                return GetDefault<T>();
            }
            catch (Exception)
            {
                return GetDefault<T>();
            }
        }

        private static T GetDefault<T>() where T : IPokemonFilter
        {
            var globalFilter = Activator.CreateInstance<T>();
            return (T)globalFilter.GetGlobalFilter();
        }

        public static void UpdateFilterSetting<T>(GlobalSettings globalSettings, Dictionary<PokemonId, T> pokemonsTransferFilter, PokemonId id, T f)
        {
            if(pokemonsTransferFilter.ContainsKey(id))
            {
                pokemonsTransferFilter[id] = f;
            }
            else
            {
                pokemonsTransferFilter.Add(id, f);

            }
            string configFile = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config\\config.json");
            globalSettings.Save(configFile);
        }
    }
}
