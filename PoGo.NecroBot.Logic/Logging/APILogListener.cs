using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.UI;
using PoGo.NecroBot.Logic.State;
using PokemonGo.RocketAPI.Logging;
using System;
using TinyIoC;

namespace PoGo.NecroBot.Logic.Logging
{
    public class APILogListener : PokemonGo.RocketAPI.ILogger
    {
        DateTime lastVerboseLog = DateTime.Now;
        public void InboxStatusUpdate(string message, ConsoleColor color = ConsoleColor.White)
        {
            Logger.Write(message, LogLevel.Service, color);
        }

        public void HashStatusUpdate(HashInfo info)
        {
            DateTime expired = Convert.ToDateTime(info.Expired).ToLocalTime();
            TimeSpan expiredTime = expired - DateTime.Now;
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            if (session.Settings.DisplayVerboseLog && lastVerboseLog < DateTime.Now.AddSeconds(-60))
            {
                lastVerboseLog = DateTime.Now;
                Logger.Write($"(HASH SERVER) Key[{info.MaskedAPIKey}] - Last Minute: {info.Last60MinAPICalles} RPM, AVG: {info.Last60MinAPIAvgTime:0.00} MS, Fastest: {info.Fastest}, Slowest: {info.Slowest}, Available: {info.HealthyRate:0.00%}, Expires: {expired.ToString("MM/dd/yyyy")} @ {expired.ToString("HH:mm:ss tt")} ({expiredTime.Days} Days {expiredTime.Hours} Hours {expiredTime.Minutes} Minutes)", LogLevel.Info, ConsoleColor.White);
            }
            session.EventDispatcher.Send(new StatusBarEvent($"[{info.MaskedAPIKey}] - {info.Last60MinAPICalles} RPM, AVG: {info.Last60MinAPIAvgTime:0.00} MS, Fastest: {info.Fastest}, Slowest: {info.Slowest}, Available {info.HealthyRate:0.00%}, Expires: {expired.ToString("MM/dd/yyyy")} @ {expired.ToString("HH:mm:ss tt")} ({expiredTime.Days} Days {expiredTime.Hours} Hours {expiredTime.Minutes} Minutes)"));
        }

        public void LogCritical(string message, dynamic data)
        {
        }

        public void LogDebug(string message)
        {
            Logger.Debug(message);
        }

        public void LogError(string message)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            session.EventDispatcher.Send(new ErrorEvent() { Message = message });
        }

        public void LogInfo(string message)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            session.EventDispatcher.Send(new InfoEvent() { Message = message });
        }
        
        public void LogFlaggedInit(string message)
        {
            Logger.Write(message, LogLevel.Warning);
        }

        public void LogErrorInit(string message)
        {
            Logger.Write(message, LogLevel.Error);
        }
    }
}
