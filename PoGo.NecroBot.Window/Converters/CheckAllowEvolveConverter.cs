using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class CheckAllowEvolveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEvolving = (bool)values[1];
            bool isTransfering = (bool)values[2];
            bool allow = (bool)values[0];

            return !isEvolving && !isTransfering && allow; 
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
