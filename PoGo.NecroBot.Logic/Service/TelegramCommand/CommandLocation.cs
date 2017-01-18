using System;
using System.Device.Location;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using Telegram.Bot.Types;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    public abstract class CommandLocation : ICommandGenerify<GeoCoordinate>
    {
        protected readonly TelegramUtils TelegramUtils;

        protected CommandLocation(TelegramUtils telegramUtils)
        {
            TelegramUtils = telegramUtils;
        }

        public abstract string Command { get; }
        public abstract string Description { get; }
        public abstract bool StopProcess { get; }

        public abstract Task<bool> OnCommand(ISession session, string cmd, Action<GeoCoordinate> callback);

        public Task<bool> OnCommand(ISession session, string cmd, Message telegramMessage)
        {
            Action<GeoCoordinate> callback = async geo =>
            {
                try
                {
                    await TelegramUtils.SendLocation(geo, telegramMessage.Chat.Id);
                }
                catch (Exception ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = ex.Message});
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Unkown Telegram Error occured. "});
                }
            };
            return OnCommand(session, cmd, callback);
        }
    }
}