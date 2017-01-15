using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class PokemonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var id = (int)value;

            if (id > 151)
            {

                return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons2/{id:000}.png";

            }

            return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons/{id:000}.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
