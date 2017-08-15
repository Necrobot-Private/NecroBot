using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class ListPokemonIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                var list = (List<PokemonId>)value;

                return string.Join(";", list.Select(x => x.ToString()));
            }
            return string.Empty;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();

            var arr = str.ToString().Split(new char[] { ';' });
            var list = new List<PokemonId>();
            foreach (var pname in arr)
            {
                PokemonId pi = PokemonId.Missingno;
                if (Enum.TryParse(pname, true, out pi))
                {
                    list.Add(pi);
                }
            }
            return list;
        }
    }
}
