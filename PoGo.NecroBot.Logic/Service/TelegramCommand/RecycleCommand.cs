using System;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    public class RecycleCommand : CommandMessage
    {
        public override string Command => "/recycle";
        public override bool StopProcess => true;
        public override TranslationString DescriptionI18NKey => TranslationString.TelegramCommandLogsDescription;
        public override TranslationString MsgHeadI18NKey => TranslationString.TelegramCommandLogsMsgHead;

        public RecycleCommand(TelegramUtils telegramUtils) : base(telegramUtils)
        {
        }

        public override async Task<bool> OnCommand(ISession session, string commandText, Action<string> callback)
        {
            var cmd = commandText.Split(' ');

            if (cmd[0].ToLower() == Command)
            {
                await RecycleItemsTask.Execute(session, session.CancellationTokenSource.Token);
                callback("RECYCLE ITEM DONE!");
                return true;
            }
            return false;
        }
    }
}