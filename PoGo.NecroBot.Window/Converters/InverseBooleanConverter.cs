using System;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //if (targetType != typeof(bool))
            //    throw new InvalidOperationException("The target must be a boolean");
            if (value == null)
                return true;

            if (value is int)
                return ((int)value) == 1 ? false : true;

            if (value is Int64)
                return ((Int64)value) == 1 ? false : true;

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
