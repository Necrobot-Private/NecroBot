using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoGo.Necrobot.Window
{
    public class ObservableCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var observableType = typeof(ObservableCollection<>).MakeGenericType(value.GetType().GetGenericArguments());
            return Activator.CreateInstance(observableType, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var observableType = typeof(HashSet<>).MakeGenericType(value.GetType().GetGenericArguments());
            return Activator.CreateInstance(observableType, value);
        }
    }
}
