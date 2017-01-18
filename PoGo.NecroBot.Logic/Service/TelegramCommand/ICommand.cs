using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.State;
using Telegram.Bot.Types;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    internal interface ICommand
    {
        string Command { get; }
        bool StopProcess { get; }
        TranslationString DescriptionI18NKey { get; }
        TranslationString MsgHeadI18NKey { get; }

        Task<bool> OnCommand(ISession session, string cmd, Message telegramMessage);

        string GetDescription(Session session);
        string GetMsgHead(Session session);
    }
}