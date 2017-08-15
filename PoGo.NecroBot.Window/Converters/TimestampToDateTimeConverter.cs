using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Util;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class TimestampToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (long)value == 0)
                return null;

            long timestamp = (long)value;
            return TimeUtil.GetDateTimeFromMilliseconds(timestamp).ToLocalTime(); // We want to display the local time.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            DateTime dateTime = (DateTime)value;
            return dateTime.ToUnixTime();
        }
    }
}
