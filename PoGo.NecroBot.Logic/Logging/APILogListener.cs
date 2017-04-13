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
        public void HashStatusUpdate(HashInfo info)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            if (session.Settings.DisplayVerboseLog && lastVerboseLog< DateTime.Now.AddSeconds(-60))
            {
                lastVerboseLog = DateTime.Now;
                Logger.Write($"(HASH SERVER) Key[{info.MaskedAPIKey}] Last Minute:  {info.Last60MinAPICalles} RPM , AVG: {info.Last60MinAPIAvgTime:0.00} RPM, Fastest: {info.Fastest}, Slowest: {info.Slowest}, Available: {info.HealthyRate:0.00%}, Expires: {info.Expired}", LogLevel.Info, ConsoleColor.White);
            }
            session.EventDispatcher.Send(new StatusBarEvent($"[{info.MaskedAPIKey}] - {info.Last60MinAPICalles} RPM, AVG: {info.Last60MinAPIAvgTime:0.00} RPM, Fastest: {info.Fastest}, Slowest: {info.Slowest}, Available {info.HealthyRate:0.00%}, Expires: {info.Expired}"));
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
    }
}
