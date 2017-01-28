using System;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    public class ItemsCommand : CommandMessage
    {
        public override string Command => "/items";
        public override bool StopProcess => true;
        public override TranslationString DescriptionI18NKey => TranslationString.TelegramCommandItemsDescription;
        public override TranslationString MsgHeadI18NKey => TranslationString.TelegramCommandItemsMsgHead;

        public ItemsCommand(TelegramUtils telegramUtils) : base(telegramUtils)
        {
        }

        public override async Task<bool> OnCommand(ISession session, string cmd, Action<string> callback)
        {
            if (cmd.ToLower() == Command)
            {
                string answerTextmessage = GetMsgHead(session, session.Profile.PlayerData.Username) + "\r\n\r\n";
                var inventory = session.Inventory;
                answerTextmessage += session.Translation.GetTranslation(TranslationString.CurrentPokeballInv,
                    inventory.GetItemAmountByType(ItemId.ItemPokeBall),
                    inventory.GetItemAmountByType(ItemId.ItemGreatBall),
                    inventory.GetItemAmountByType(ItemId.ItemUltraBall),
                    inventory.GetItemAmountByType(ItemId.ItemMasterBall));
                answerTextmessage += "\n";
                answerTextmessage += session.Translation.GetTranslation(TranslationString.CurrentPotionInv,
                    inventory.GetItemAmountByType(ItemId.ItemPotion),
                    inventory.GetItemAmountByType(ItemId.ItemSuperPotion),
                    inventory.GetItemAmountByType(ItemId.ItemHyperPotion),
                    inventory.GetItemAmountByType(ItemId.ItemMaxPotion));
                answerTextmessage += "\n";
                answerTextmessage += session.Translation.GetTranslation(TranslationString.CurrentReviveInv,
                    inventory.GetItemAmountByType(ItemId.ItemRevive),
                    inventory.GetItemAmountByType(ItemId.ItemMaxRevive));
                answerTextmessage += "\n";
                answerTextmessage += session.Translation.GetTranslation(TranslationString.CurrentMiscItemInv,
                    session.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry),
                    session.Inventory.GetItemAmountByType(ItemId.ItemIncenseOrdinary) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemIncenseSpicy) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemIncenseCool) +
                    session.Inventory.GetItemAmountByType(ItemId.ItemIncenseFloral),
                    session.Inventory.GetItemAmountByType(ItemId.ItemLuckyEgg),
                    session.Inventory.GetItemAmountByType(ItemId.ItemTroyDisk));
                callback(answerTextmessage);
                return true;
            }
            return false;
        }
    }
}