using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoGo.NecroBot.CLI
{
    public class BotDataSocketClient
    {
        private static Queue<EncounteredEvent> events = new Queue<EncounteredEvent>();
        private const int POLLING_INTERVAL = 10000;
        public static void Listen(IEvent evt, Session session)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve);
            }
            catch
            {
            }
        }

        private static void HandleEvent(EncounteredEvent eve)
        {
            lock (events)
            {
                events.Enqueue(eve);
            }
        }

        private static string Serialize(dynamic evt)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            // Add custom seriaizer to convert uong to string (ulong shoud not appear to json according to json specs)
            jsonSerializerSettings.Converters.Add(new IdToStringConverter());

            string json = JsonConvert.SerializeObject(evt, Formatting.None, jsonSerializerSettings);
            //json = Regex.Replace(json, @"\\\\|\\(""|')|(""|')", match => {
            //    if (match.Groups[1].Value == "\"") return "\""; // Unescape \"
            //    if (match.Groups[2].Value == "\"") return "'";  // Replace " with '
            //    if (match.Groups[2].Value == "'") return "\\'"; // Escape '
            //    return match.Value;                             // Leave \\ and \' unchanged
            //});
            return json;
        }

        private static int retries = 0;
        static List<EncounteredEvent> processing = new List<EncounteredEvent>();

        public static async Task Start(Session session, CancellationToken cancellationToken)
        {

            await Task.Delay(30000);//delay running 30s

            System.Net.ServicePointManager.Expect100Continue = false;

            cancellationToken.ThrowIfCancellationRequested();

            var socketURL = session.LogicSettings.DataSharingDataUrl;

            using (var ws = new WebSocketSharp.WebSocket(socketURL))
            {
                ws.Log.Level = WebSocketSharp.LogLevel.Fatal;
                ws.Log.Output = (logData, message) =>
                 {
                     //silenly, no log exception message to screen that scare people :)
                 };
                
                //ws.OnMessage += (sender, e) =>
                // Console.WriteLine("New message from controller: " + e.Data);

                while (true)
                {
                    try
                    {
                        if (retries++ == 5) //failed to make connection to server  times contiuing, temporary stop for 10 mins.
                        {
                            session.EventDispatcher.Send(new WarnEvent()
                            {
                                Message = "Couldn't establish the connection to necro socket server, Bot will re-connect after 10 mins"
                            });

                            await Task.Delay(10 * 1000 * 60);
                        }

                        ws.Connect();
                        if (ws.ReadyState == WebSocketSharp.WebSocketState.Open)
                        {
                            Logger.Write("Pokemon spawn point data service connection established.");
                            retries = 0;

                            while (ws.IsAlive)
                            {
                                lock (events)
                                {
                                    while (events.Count > 0)
                                    {
                                        processing.Add(events.Dequeue());
                                    }
                                }

                                while (processing.Count > 0)
                                {
                                    if (ws.IsAlive)
                                    {
                                        var item = processing.FirstOrDefault();
                                        var data = Serialize(item);
                                        ws.Send($"42[\"pokemon\",{data}]");
                                        processing.Remove(item);
                                        await Task.Delay(processing.Count > 0 ? 3000 : 0);
                                    }
                                }
                            }
                            await Task.Delay(POLLING_INTERVAL);
                            ws.Ping();
                        }
                    }
                    catch (IOException)
                    {
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = "Disconnect to necro socket. New connection will be established when service available..."
                        });
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        //everytime disconnected with server bot wil reconnect after 15 sec
                        await Task.Delay(POLLING_INTERVAL, cancellationToken);
                    }
                }

            }

        }


        internal static Task StartAsync(Session session, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(session, cancellationToken), cancellationToken);
        }
    }
}
