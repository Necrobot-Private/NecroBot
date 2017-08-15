using PoGo.NecroBot.Logic.Common;
using System;
using System.Windows;
using System.Windows.Data;
using TinyIoC;

namespace PoGo.NecroBot.Window.Converters
{
    public class I18NConveter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if(! (Application.Current is App))
            {
                return value.ToString();
            }

            if (value == null) return "NO KEY";

            if (TinyIoCContainer.Current == null) return "";

            string key = (string)value;
            var uiTranslation = TinyIoCContainer.Current.Resolve<UITranslation>();
            if (uiTranslation == null) return key;
            string resourceValue =  uiTranslation.GetTranslation(key);
            return resourceValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
