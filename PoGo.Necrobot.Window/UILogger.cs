using PoGo.NecroBot.Logic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;

namespace PoGo.Necrobot.Window
{
    public class UILogger : ILogger
    {
        public Action<string, LogLevel, string> LogToUI;

        public void lineSelect(int lineChar = 0, int linesUp = 1)
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
            if(LogToUI != null)
            {
                message = Logger.GetFinalMessage(message, level, color);
                LogToUI(message, level, color.ToString());
            }
        }
    }
}
