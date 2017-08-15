using System;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class FriendlyTransferTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //if (targetType != typeof(string))
            //    throw new InvalidOperationException("The target must be a string");

            return (bool)value?"Transfering...": "Transfer";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
