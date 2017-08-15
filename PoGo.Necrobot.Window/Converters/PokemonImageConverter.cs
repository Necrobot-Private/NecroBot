using POGOProtos.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class PokemonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PokemonId pokemonId = (PokemonId)Enum.Parse(typeof(PokemonId), value.ToString());
            return $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/pokemon/{(int)pokemonId}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
