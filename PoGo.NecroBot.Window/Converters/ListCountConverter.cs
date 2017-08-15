using PoGo.NecroBot.Window.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class ListCountConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var t = (ObservableCollection<SnipePokemonViewModel>)value;
            return t.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
