using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    [ValueConversion(typeof(List<PokemonId>), typeof(string))]
    public class ListPokemonIdToTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var list = (List<PokemonId>)value;

            return string.Join(";", list.Select(x => x.ToString()));

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var arr = value.ToString().Split(';');
            List<PokemonId> list = new List<PokemonId>();

            var pid = PokemonId.Missingno;
            foreach (var item in arr)
            {
                if(Enum.TryParse<PokemonId>(item, true, out pid))
                {
                    list.Add(pid);
                }
            }
            return list;
        }

        #endregion
    }
}
