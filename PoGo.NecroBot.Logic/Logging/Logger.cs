#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Logging
{
    public static class Logger
    {
        private static List<ILogger> _loggers = new List<ILogger>();
        private static string _path;
        private static DateTime _lastLogTime;
        private static readonly IList<string> LogbufferList = new List<string>();
        private static string _lastLogMessage;
        private static bool _isGui;

        public static void TurnOffLogBuffering()
        {
            foreach (var logger in _loggers)
            {
                logger?.TurnOffLogBuffering();
            }
        }

        private static void Log(string message, bool force = false)
        {
            lock (LogbufferList)
            {
                LogbufferList.Add(message);

                if (_lastLogTime.AddSeconds(60).Ticks > DateTime.Now.Ticks && !force)
                    return;

                using (
                    var log =
                        File.AppendText(Path.Combine(_path,
                            $"NecroBot2-{DateTime.Today.ToString("yyyy-MM-dd")}-{DateTime.Now.ToString("HH")}.txt"))
                    )
                {
                    foreach (var line in LogbufferList)
                    {
                        log.WriteLine(line);
                    }
                    _lastLogTime = DateTime.Now;
                    log.Flush();
                    LogbufferList.Clear();
                }
            }
        }

        /// <summary>
        ///   Add a logger.
        /// </summary>
        /// <param name="logger"></param>
        public static void AddLogger(ILogger logger, string subPath = "", bool isGui = false)
        {
            if (!_loggers.Contains(logger))
                _loggers.Add(logger);

            _isGui = isGui;
            if (!_isGui)
            {
                _path = Path.Combine(Directory.GetCurrentDirectory(), subPath, "Logs");
                Directory.CreateDirectory(_path);
                Log($"Initializing NecroBot2 logger at time {DateTime.Now}...");
            }
        }

        /// <summary>
        ///     Sets Context for the loggers
        /// </summary>
        /// <param name="session">Context</param>
        public static void SetLoggerContext(ISession session)
        {
            foreach(var logger in _loggers)
                logger?.SetSession(session);
        }

        /// <summary>
        ///     Log a specific message to the logger setup by <see cref="SetLogger(ILogger)" /> .
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">Optional level to log. Default <see cref="LogLevel.Info" />.</param>
        /// <param name="color">Optional. Default is automatic color.</param>
        public static void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black, bool force = false)
        {
            if (_loggers.Count == 0 || _lastLogMessage == message)
                return;

            _lastLogMessage = message;
            foreach(var logger in _loggers)
                logger?.Write(message, level, color);

            if (!_isGui)
            {
                if (level == LogLevel.Debug)
                {
                    Log(string.Concat($"[{DateTime.Now.ToString("HH:mm:ss")}] ", message), force);
                }
                else
                {
                    Log(string.Concat($"[{DateTime.Now.ToString("HH:mm:ss")}] ", message), force);
                }


            }

        }

        public static void lineSelect(int lineChar = 0, int linesUp = 1)
        {
            foreach(var logger in _loggers)
                logger?.lineSelect(lineChar, linesUp);
        }
    }

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Pokestop = 3,
        Farming = 4,
        Sniper = 5,
        Recycling = 6,
        Berry = 7,
        Caught = 8,
        Flee = 9,
        Transfer = 10,
        Evolve = 11,
        Egg = 12,
        Update = 13,
        Info = 14,
        New = 15,
        SoftBan = 16,
        LevelUp = 17,
        Gym = 18,
        Service = 19,
        Debug = 20,
    }
}