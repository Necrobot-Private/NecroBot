using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class DurationTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var duration = (double)value;
            int day = (int)duration / 1440;
            int hour = (int)(duration - (day * 1400)) / 60;
            int min = (int)(duration - (day * 1400) - hour * 60);
            return $"{day:00}:{hour:00}:{min:00}:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
