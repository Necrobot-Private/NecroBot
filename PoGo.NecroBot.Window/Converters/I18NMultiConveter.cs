using PoGo.NecroBot.Logic.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using TinyIoC;
using System.Globalization;

namespace PoGo.NecroBot.Window.Converters
{
    public class I18NMultiConveter : IMultiValueConverter
    {                                         
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return "NO KEY";

            if (! (Application.Current is App))
            {
                return values[0].ToString();
            }

            
            if (TinyIoCContainer.Current == null) return "";

            string key = (string)values[0];

            var uiTranslation = TinyIoCContainer.Current.Resolve<UITranslation>();
            if (uiTranslation == null) return key;
            string resourceValue =  uiTranslation.GetTranslation(key);
            resourceValue = string.Format(resourceValue, values.Skip(1).ToArray());
            return resourceValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
