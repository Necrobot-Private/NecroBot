using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoGo.NecroBot.Logic.Service
{
    public class TelegramUtils
    {
        private const int MaxTelegramMsgLength = 4096;

        private readonly TelegramBotClient _bot;
        private readonly ISession _session;

        public TelegramUtils(TelegramBotClient bot, ISession session)
        {
            _bot = bot;
            _session = session;
        }

        public async Task SendLocation(GeoCoordinate geo, Message telegramMessage)
        {
            await SendLocation(geo, telegramMessage.MessageId);
        }

        public async Task SendLocation(GeoCoordinate geo, long chatId)
        {
            if (chatId == 0)
            {
                _session.EventDispatcher.Send(new WarnEvent { Message = String.Format("Could not send location to 'Telegram', because given Chat id was '{0}'", 0) });
            }
            await _bot.SendLocationAsync(chatId, (float) geo.Latitude, (float) geo.Longitude);
        }

        public async Task SendMessage(string message, Message telegramMessage)
        {
            await SendMessage(message, telegramMessage.MessageId);
        }

        public async Task SendMessage(string message, long chatId)
        {
            if (chatId == 0)
            {
                _session.EventDispatcher.Send(new WarnEvent { Message = String.Format("Could not send message to 'Telegram', because given Chat id was '{0}'", 0) });
            }
            else if (string.IsNullOrEmpty(message))
            {
                return;
            }

            foreach (var msg in Split(message, MaxTelegramMsgLength))
            {
                await _bot.SendTextMessageAsync(chatId, msg, replyMarkup: new ReplyKeyboardHide());
            }
        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}