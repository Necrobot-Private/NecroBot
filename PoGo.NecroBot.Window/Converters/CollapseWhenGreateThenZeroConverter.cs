using System;
using System.Windows;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class CollapseWhenGreateThenZeroConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //if (targetType != typeof(bool))
            //    throw new InvalidOperationException("The target must be a boolean");

            int x = System.Convert.ToInt32(value);

            if (x > 0) return Visibility.Collapsed;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
