using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;

namespace PoGo.NecroBot.Logic.Logging
{
    public class APILogListener : PokemonGo.RocketAPI.ILogger
    {
        public void HashStatusUpdate()
        {
            throw new NotImplementedException();
        }

        public void LogCritical(string message, dynamic data)
        {
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
