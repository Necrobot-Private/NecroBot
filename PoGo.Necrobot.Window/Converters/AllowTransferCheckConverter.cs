using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class AllowTransferCheckConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return !System.Convert.ToBoolean(values[0]) && !System.Convert.ToBoolean(values[1]);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}