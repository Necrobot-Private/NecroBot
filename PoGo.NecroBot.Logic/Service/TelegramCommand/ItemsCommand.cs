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
                    await session.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry),
                    await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseOrdinary) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseSpicy) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseCool) +
                    await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseFloral),
                    await session.Inventory.GetItemAmountByType(ItemId.ItemLuckyEgg),
                    await session.Inventory.GetItemAmountByType(ItemId.ItemTroyDisk));
                callback(answerTextmessage);
                return true;
            }
            return false;
        }
    }
}