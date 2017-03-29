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

            return $"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{id}.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
