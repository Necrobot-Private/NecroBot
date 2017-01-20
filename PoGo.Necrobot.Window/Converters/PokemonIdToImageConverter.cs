using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class PokemonIdToImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pokemonId = (PokemonId)value;
            if ((int)pokemonId > 151)
            {
                //http://www.serebii.net/pokemongo/pokemon/145.png maybe better
                return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons2/{(int)pokemonId:000}.png";
            }

            return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons/{(int)pokemonId:000}.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
