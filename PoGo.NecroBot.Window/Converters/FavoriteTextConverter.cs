using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class FavoriteTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
           bool isFavoriting = (bool)values[0];
           bool isFavorited = (bool)values[1];

            if (isFavoriting) return "Favoriting...";

            if (isFavorited) return "Un-Favorite";

            return "Favorite";

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
