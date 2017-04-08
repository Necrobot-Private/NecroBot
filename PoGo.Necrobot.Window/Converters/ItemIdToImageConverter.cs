using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class ItemIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemId = (ItemId)Enum.Parse(typeof(ItemId), value);
            return $"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/{(int)itemid.item.itemid}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
