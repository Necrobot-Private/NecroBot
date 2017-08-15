using POGOProtos.Inventory.Item;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.NecroBot.Window.Converters
{
    public class ItemIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ItemId itemId = (ItemId)Enum.Parse(typeof(ItemId), value.ToString());
            return $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/items/{(int)itemId}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
