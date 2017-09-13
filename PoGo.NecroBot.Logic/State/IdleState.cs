using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.Logic.State
{
    public class IdleState : IState
    {
        public async Task<bool> Ping()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    await client.GetStringAsync("https://pokehash.buddyauth.com/api/hash/versions").ConfigureAwait(false);
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            session.EventDispatcher.Send(new WarnEvent()
            {
                Message = "Hash server may be down, Bot will enter IDLE state until service becomes available. Ping interval, 5 sec..."
            });

            Console.WriteLine();

            var start = DateTime.Now;
            var lastPing = DateTime.Now;

            bool alive = false;
            while (!alive)
            {
                alive = await Ping().ConfigureAwait(false);
                lastPing = DateTime.Now;
                if (!alive)
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    var ts = DateTime.Now - start;
                    session.EventDispatcher.Send(new ErrorEvent()
                    {
                        Message = $"Hash API server down time : {ts.ToString(@"hh\:mm\:ss")}   Last Ping: {lastPing.ToString("T")}"
                    });

                    await Task.Delay(5000).ConfigureAwait(false);
                }
            }

            return new LoginState();
        }
    }
}