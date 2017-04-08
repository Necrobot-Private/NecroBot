using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class ItemIdToImageConverter : IValueConverter
    {
        //http://imgur.com/gallery/DmlpQ

        Dictionary<ItemId, string> resources = new Dictionary<ItemId, string>()
        {
            {ItemId.ItemIncubatorBasic,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/901.png" },
            {ItemId.ItemIncubatorBasicUnlimited,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/902.png" },
            {ItemId.ItemPokeBall,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1.png" },
            {ItemId.ItemGreatBall,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/2.png" },
            {ItemId.ItemUltraBall,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/3.png" },
            {ItemId.ItemMasterBall,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/4.png" },
            {ItemId.ItemPotion,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/101.png" },
            {ItemId.ItemSuperPotion,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/102.png" },
            {ItemId.ItemHyperPotion,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/103.png" },
            {ItemId.ItemMaxPotion,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/104.png" },
            {ItemId.ItemRevive,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/201.png" },
            {ItemId.ItemMaxRevive,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/202.png" },
            {ItemId.ItemRazzBerry,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/701.png" },
            {ItemId.ItemBlukBerry,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/702.png" },
            {ItemId.ItemNanabBerry,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/703.png" },
            {ItemId.ItemPinapBerry,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/705.png" },
            {ItemId.ItemWeparBerry,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/704.png" },
            {ItemId.ItemIncenseOrdinary,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/401.png" },
            {ItemId.ItemTroyDisk,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/501.png" },
            {ItemId.ItemLuckyEgg,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/301.png" },
            {ItemId.ItemDragonScale,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1104.png" },
            {ItemId.ItemUpGrade,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1105.png" },
            {ItemId.ItemKingsRock,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1102.png" },
            {ItemId.ItemMetalCoat,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1103.png" },
            {ItemId.ItemSunStone,"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/1101.png" }
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemId = (ItemId)Enum.Parse(typeof(ItemId), value.ToString());
            if (resources.ContainsKey(itemId))
                return resources[itemId];

            return $"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/items-icons/{itemId}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
