using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class VisibleIfTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)(Visibility)((bool)value ? 0 : 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}