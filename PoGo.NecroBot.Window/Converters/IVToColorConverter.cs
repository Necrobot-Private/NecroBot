using System;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class IVToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var iv = (double)value;
            if (iv < 25) return System.Windows.Media.Brushes.AntiqueWhite.ToString();

            if (iv < 50) return System.Windows.Media.Brushes.DarkGreen.ToString();


            if (iv < 75) return System.Windows.Media.Brushes.LightGreen.ToString();


            if (iv < 99) return System.Windows.Media.Brushes.Green.ToString();


            return System.Windows.Media.Brushes.DarkMagenta.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
