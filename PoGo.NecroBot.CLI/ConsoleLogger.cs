#region using directives

using System;
using System.Text;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.CLI
{
    /// <summary>
    ///     The ConsoleLogger is a simple logger which writes all logs to the Console.
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        private readonly LogLevel _maxLogLevel;

        /// <summary>
        ///     To create a ConsoleLogger, we must define a maximum log level.
        ///     All levels above won't be logged.
        /// </summary>
        /// <param name="maxLogLevel"></param>
        internal ConsoleLogger(LogLevel maxLogLevel)
        {
            _maxLogLevel = maxLogLevel;
        }

        public void TurnOffLogBuffering()
        {
            // No need for buffering
        }

        public void SetSession(ISession session)
        {
            // No need for session
        }

        /// <summary>
        ///     Log a specific message by LogLevel. Won't log if the LogLevel is greater than the maxLogLevel set.
        /// </summary>
        /// <param name="message">The message to log. The current time will be prepended.</param>
        /// <param name="level">Optional. Default <see cref="LogLevel.Info" />.</param>
        /// <param name="color">Optional. Default is auotmatic</param>
        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            // Remember to change to a font that supports your language, otherwise it'll still show as ???.
            Console.OutputEncoding = Encoding.UTF8;
            if (level > _maxLogLevel)
                return;

            var finalMessage = Logger.GetFinalMessage(message, level, color);
            Console.WriteLine(finalMessage);
        }

        public void lineSelect(int lineChar = 0, int linesUp = 1)
        {
            Console.SetCursorPosition(lineChar, Console.CursorTop - linesUp);
        }
    }
}