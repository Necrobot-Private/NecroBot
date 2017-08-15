using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class DurationTextConverter : IValueConverter
    {
        public object Convert(object minutes, Type targetType, object parameter, CultureInfo culture)
        {
            if (minutes == null)
                return null;

            var seconds = (int)((double)minutes * 60);
            var duration = new TimeSpan(0, 0, seconds);

            return duration.ToString(@"dd\:hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
