using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoGo.Necrobot.Window.Converters
{
    public class ItemIdToImageConverter : IValueConverter
    {
        //http://imgur.com/gallery/DmlpQ

        Dictionary<ItemId, string> resources = new Dictionary<ItemId, string>()
        {
            { ItemId.ItemIncubatorBasic,"https://raw.githubusercontent.com/Superviral/Pokemon-GO-App-Assets-and-Images/master/App%20Converted%20Images/EggIncubator.png" },
            {ItemId.ItemIncubatorBasicUnlimited,"http://i.imgur.com/ZBRdMuy.png" },
            {ItemId.ItemPokeBall, "https://raw.githubusercontent.com/Superviral/Pokemon-GO-App-Assets-and-Images/master/App%20Converted%20Images/pokeball_store.png" },
            {ItemId.ItemGreatBall, "http://i.imgur.com/ZP3DlMs.png" },
            {ItemId.ItemUltraBall, "http://i.imgur.com/nGsWvcR.png" },
            {ItemId.ItemMasterBall,"http://i.imgur.com/cyLuJEq.png" } ,
            {ItemId.ItemPotion,"http://i.imgur.com/6MQCO4g.png" } ,
            {ItemId.ItemSuperPotion,"http://i.imgur.com/LFaT9Lw.png" } ,
            {ItemId.ItemHyperPotion,"http://i.imgur.com/ay1jWQF.png" },
            {ItemId.ItemMaxPotion,"http://i.imgur.com/qEqi8Su.png" } ,
            {ItemId.ItemRevive,"http://i.imgur.com/tswFn3H.png" } ,
            {ItemId.ItemMaxRevive,"http://i.imgur.com/qa2Rqnw.png" } ,
            {ItemId.ItemRazzBerry,"http://i.imgur.com/9lyjJHn.png" } ,
            {ItemId.ItemBlukBerry,"http://i.imgur.com/80WZSnw.png" } ,
            {ItemId.ItemNanabBerry,"http://www.serebii.net/pokemongo/items/nanabberry.png" } ,
            {ItemId.ItemPinapBerry,"http://www.serebii.net/pokemongo/items/pinapberry.png" } ,
            {ItemId.ItemWeparBerry,"http://i.imgur.com/DOhPGVh.png" },
            {ItemId.ItemIncenseOrdinary,"http://i.imgur.com/KqdjZTf.png" } ,
            {ItemId.ItemTroyDisk,"http://i.imgur.com/qJedIld.png" } ,
            {ItemId.ItemLuckyEgg,"http://i.imgur.com/fEPh6Eu.png" } ,
            {ItemId.ItemDragonScale,"http://www.serebii.net/pokemongo/items/dragonscale.png" } ,
            {ItemId.ItemUpGrade,"http://www.serebii.net/pokemongo/items/up-grade.png" },
            {ItemId.ItemKingsRock, "http://www.serebii.net/pokemongo/items/kingsrock.png" },
            { ItemId.ItemMetalCoat, "http://www.serebii.net/pokemongo/items/metalcoat.png" },
            {ItemId.ItemSunStone, "http://www.serebii.net/pokemongo/items/sunstone.png" }
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemId = (ItemId)Enum.Parse(typeof(ItemId), value.ToString());
            if (resources.ContainsKey(itemId))
                return resources[itemId];

            return "http://i.imgur.com/qJedIld.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
