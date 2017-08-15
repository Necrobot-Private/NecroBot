using PoGo.NecroBot.Logic.Logging;
using System;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Window
{
    public class UILogger : ILogger
    {
        public Action<string, LogLevel, string> LogToUI;

        public void LineSelect(int lineChar = 0, int linesUp = 1)
        {
        }

        public void SetSession(ISession session)
        {
        }

        public void TurnOffLogBuffering()
        {
        }

        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            if(LogToUI != null && level != LogLevel.Debug)
            {
                message = Logger.GetFinalMessage(message, level, color);
                LogToUI(message, level, color.ToString());
            }
        }
    }
}
