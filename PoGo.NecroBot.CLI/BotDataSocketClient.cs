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
        private static Queue<object> events = new Queue<object>();
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
            //Broadcast(Serialize(eve));
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

        public static async Task Start(Session session, CancellationToken cancellationToken)
        {

            System.Net.ServicePointManager.Expect100Continue = false;

            cancellationToken.ThrowIfCancellationRequested();

            var socketURL = session.LogicSettings.DataSharingDataUrl;

            //socketURL = "ws://127.0.0.1:5000/socket.io/?EIO=3&transport=websocket";
            using (var ws = new WebSocketSharp.WebSocket(socketURL))
            {
                ws.Log.Level = WebSocketSharp.LogLevel.Error;
                //ws.OnMessage += (sender, e) =>
                // Console.WriteLine("New message from controller: " + e.Data);

                while (true)
                {
                    try
                    {
                        ws.Connect();
                        Logger.Write("Pokemon spawn point data service connection established.");
                        while (ws.ReadyState == WebSocketSharp.WebSocketState.Open)
                        {
                            lock (events)
                            {
                                
                                while (events.Count > 0)
                                {
                                    if (ws.ReadyState == WebSocketSharp.WebSocketState.Open)
                                    {
                                        var item = events.Dequeue();
                                        var data = Serialize(item);
                                        ws.Send($"42[\"pokemon\",{data}]");
                                    }
                                }
                            }
                            await Task.Delay(3000);
                            ws.Ping();

                        }
                    }
                    catch (IOException)
                    {
                        session.EventDispatcher.Send(new ErrorEvent
                        {
                            Message = "The connection to the data sharing location server was lost."
                        });
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        //everytime disconnected with server bot wil reconnect after 15 sec
                        await Task.Delay(15000, cancellationToken);
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
