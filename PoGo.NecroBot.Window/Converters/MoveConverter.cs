using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class MoveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //string display = "";
            if (value == null) return string.Empty;
            List<List<PokemonMove>> moves = value as List<List<PokemonMove>>;

            var list = moves.Select(x => string.Join(",", x.Select(y => y.ToString())));

            return string.Join(";", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<List<PokemonMove>> moves = new List<List<PokemonMove>>();

            var list = value.ToString().Split(new char
                [] { ';' }, StringSplitOptions.None);
            foreach (var item in list)
            {
                string[] movesStr = item.Split(new char
                [] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                PokemonMove move1 = PokemonMove.MoveUnset;
                PokemonMove move2 = PokemonMove.MoveUnset;

                if(movesStr.Length == 2)
                {
                    Enum.TryParse(movesStr[0], out move1);
                    Enum.TryParse(movesStr[0], out move2);

                    if(move1 != PokemonMove.MoveUnset && move2 != PokemonMove.MoveUnset)
                    {
                        moves.Add(new List<PokemonMove>() { move1, move2 });
                    }
                }
            }
            return moves;
        }
    }
}
