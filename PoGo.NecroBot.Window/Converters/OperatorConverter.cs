using PoGo.NecroBot.Logic.Model.Settings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class OperatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Operator op = Operator.or;
            if (value == null) return op;
            Enum.TryParse<Operator>(value.ToString(), out op);
            return op;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
