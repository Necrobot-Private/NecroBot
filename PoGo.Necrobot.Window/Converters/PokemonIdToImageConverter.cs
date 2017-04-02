using POGOProtos.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class PokemonIdToImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pokemonId = (PokemonId)value;

            return $"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{(int)pokemonId:000}.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
