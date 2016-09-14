using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
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
            lock(events)
            {
                events.Enqueue(eve);
            }
        }

        private static byte[] Serialize(dynamic evt)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            // Add custom seriaizer to convert uong to string (ulong shoud not appear to json according to json specs)
            jsonSerializerSettings.Converters.Add(new IdToStringConverter());

            string json = JsonConvert.SerializeObject(evt, Formatting.None, jsonSerializerSettings);
            return Encoding.Default.GetBytes(json);
        }

        private static ClientWebSocket socket;

        public static async Task Start(Session session, CancellationToken cancellationToken)
        {
            System.Net.ServicePointManager.Expect100Continue = false;


            //socket = new ClientWebSocket();
           // await socket.ConnectAsync(new Uri("ws://localhost:4000/"), cancellationToken);

            //while(false)
            //{
            //    //socket.SendAsync()
            //}
            //socket.o
            //WebSocket websocket = new WebSocket("ws://localhost:4000/");
            //websocket.o += new EventHandler(websocket_Opened);
            //websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            //websocket.Closed += new EventHandler(websocket_Closed);
            //websocket.MessageReceived += new EventHandler(websocket_MessageReceived);
            //websocket.Open();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    socket = new ClientWebSocket();
                    await socket.ConnectAsync(new Uri("ws://localhost:4000"), cancellationToken);
                    //var lClient = new TcpClient();
                    //lClient.Connect("http://localhost",//session.LogicSettings.SnipeLocationServer,
                    //    4000);///session.LogicSettings.SnipeLocationServerPort);

                    //Stream stream = lClient.GetStream();

                    while (socket.CloseStatus == WebSocketCloseStatus.Empty)
                    {
                        try
                        {
                            lock (events)
                            {
                                while (events.Count > 0)
                                {
                                    var item = events.Dequeue();
                                    var data = Serialize(item);
                                    Console.WriteLine("Send pokemon data to socket server");
                                    //stream.Write(data, 0, data.Length);     // write bytes to buffer
                                    //stream.Flush();                                // send bytes, clear buffer

                                    socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cancellationToken);
                                }
                                //int bytesAvailable = await stream.ReadAsync(by, 0, 2048);
                                //msg = Encoding.UTF8.GetString(by, 0, bytesAvailable);
                            }
                            //var line = sr.ReadLine();
                            //if (line == null)
                            //    throw new Exception("Unable to ReadLine from sniper socket");

                            //var info = JsonConvert.DeserializeObject<SniperInfo>(line);

                        }
                        catch (IOException ex)
                        {
                            session.EventDispatcher.Send(new ErrorEvent
                            {
                                Message = "The connection to the data sharing location server was lost."
                            });
                        }
                        finally
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                }
                catch (SocketException)
                {
                }
                catch (Exception ex)
                {
                    // most likely System.IO.IOException
                    session.EventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        internal  static Task StartAsync(Session session, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(session, cancellationToken), cancellationToken);
        }
    }
}
