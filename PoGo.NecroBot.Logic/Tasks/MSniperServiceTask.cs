
using GeoCoordinatePortable;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Inventory.Item;
using WebSocket4Net;
using PoGo.NecroBot.Logic.Exceptions;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class MSniperServiceTask
    {
        #region signalr msniper service
        public class MSniperInfo2
        {
            public short PokemonId { get; set; }
            public ulong EncounterId { get; set; }
            public string SpawnPointId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Iv { get; set; }
            public PokemonMove Move1 { get;  set; }
            public PokemonMove Move2 { get;  set; }
        }
          
        public class HubData
        {
            [JsonProperty("H")]
            public string HubName { get; set; }

            [JsonProperty("M")]
            public string Method { get; set; }

            [JsonProperty("A")]
            public List<string> List { get; set; }
        }

        private static HubConnection _connection;
        private static IHubProxy _msniperHub;
        private static string _botIdentiy;
        private static string _msniperServiceUrl = "http://msniper.com/signalr";
        public static double minIvPercent = 1.0;
        public static bool isConnected = false;
        public static void AsyncConnectToService()
        {
            Task.Run(() => ConnectToService());
        }

        public static void ConnectToService()
        {
            Thread.Sleep(10000);

            while (true)
            {
                try
                {
                    if (!isConnected)
                    {
                        _connection = new HubConnection(_msniperServiceUrl, useDefaultUrl: false);
                        _msniperHub = _connection.CreateHubProxy("msniperHub");
                        _connection.Received += Connection_Received;
                        _connection.Reconnecting += Connection_Reconnecting;
                        //_connection.Reconnected += Connection_Reconnected;
                        _connection.Closed += Connection_Closed;
                        _connection.Start().Wait();
                        //Logger.Write("connecting", LogLevel.Service);
                        _msniperHub.Invoke("Identity");
                        isConnected = true;
                    }
                    break;
                }
                catch (Exception)
                {
                    //Logger.Write("service: " +e.Message, LogLevel.Error);
                    Thread.Sleep(500);
                }
            }

        }
        private static void Connection_Closed()
        {
            //Logger.Write("connection closed, trying to reconnect in 10secs", LogLevel.Service);
            ConnectToService();
        }

        private static void Connection_Reconnecting()
        {
            isConnected = false;
            _connection.Stop();
            ConnectToService();
            //Logger.Write("reconnecting", LogLevel.Service);
        }

        //private static void Connection_Reconnected()
        //{
        //    Logger.Write("reconnected", LogLevel.Service);
        //}

        private static void Connection_Received(string obj)
        {
            try
            {
                HubData xx = _connection.JsonDeserializeObject<HubData>(obj);
                switch (xx.Method)
                {
                    case "sendIdentity":
                        _botIdentiy = xx.List[0];
                        Logger.Write($"(Identity) [ {_botIdentiy} ] connection establisted", LogLevel.Service);
                        //Console.WriteLine($"[{numb}]now waiting pokemon request (15sec)");
                        break;

                    case "sendPokemon":
                        RefreshLocationQueue();
                        if (LocationQueue.Count > 0)
                        {
                            LocationQueue = LocationQueue.OrderByDescending(p => p.Iv).ToList();
                            //Logger.Write($"pokemons are sending.. {LocationQueue.Count} count", LogLevel.Service);
                            AddToVisited(LocationQueue.Select(p => p.GetEncounterId()).ToList());
                            _msniperHub.Invoke("RecvPokemons", LocationQueue);
                            LocationQueue.RemoveRange(0, LocationQueue.Count);
                        }
                        break;

                    case "Exceptions":
                        var defaultc = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Write("ERROR: " + xx.List.FirstOrDefault(), LogLevel.Service);
                        Console.ForegroundColor = defaultc;
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
        #region Classes

        public enum SocketCmd
        {
            None = 0,
            Identity = 1,
            PokemonCount = 2,
            SendPokemon = 3,
            SendOneSpecies = 4,
            Brodcaster = 5,
            IpLimit = 6,
            ServerLimit = 7
        }

        public class EncounterInfo : IDisposable
        {
            public string EncounterId { get; set; }
            public double Iv { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string PokemonName { get; set; }
            public string Move1 { get; set; }
            public string Move2 { get; set; }
            public string SpawnPointId { get; set; }
            public long Expiration { get; set; }


            //public long LastModifiedTimestampMs { get; set; }
            public int TimeTillHiddenMs { get; set; }

            public PokemonId GetPokemonName()
            {
                return (PokemonId)Enum.Parse(typeof(PokemonId), PokemonName);
            }

            public ulong GetEncounterId()
            {
                return Convert.ToUInt64(EncounterId);
            }

            public double GetLatitude()
            {
                return double.Parse(Latitude, CultureInfo.InvariantCulture);
            }
            public double GetLongitude()
            {
                return double.Parse(Longitude, CultureInfo.InvariantCulture);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }

        public class MSniperInfo
        {
            public PokemonId Id { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
        public class PokemonCount
        {
            public int Count { get; set; }
            public PokemonId PokemonId { get; set; }
        }
        #endregion

        #region Variables

        //public static WebSocket socket;
        public static List<EncounterInfo> LocationQueue = new List<EncounterInfo>();
        //public static List<EncounterInfo> ReceivedPokemons = new List<EncounterInfo>();
        public static List<ulong> VisitedEncounterIds = new List<ulong>();
        //public static string UserUniequeId { get; set; } //only info
        //public static DateTime lastNotify { get; set; }
        private static bool inProgress = false;
        #endregion

        #region MSniper Location Feeder

        public static void AddToList(ISession session, EncounterResponse eresponse)
        {
            if ((PokemonGradeHelper.GetPokemonGrade(eresponse.WildPokemon.PokemonData.PokemonId) == PokemonGrades.VeryRare ||
                 PokemonGradeHelper.GetPokemonGrade(eresponse.WildPokemon.PokemonData.PokemonId) == PokemonGrades.Epic ||
                 PokemonGradeHelper.GetPokemonGrade(eresponse.WildPokemon.PokemonData.PokemonId) == PokemonGrades.Legendary))
            {
                //access for rare pokemons
            }
            else if (PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData) < minIvPercent)
            {
                return;
            }

            if (LocationQueue.FirstOrDefault(p => p.EncounterId == eresponse.WildPokemon.EncounterId.ToString()) != null)
            {
                return;
            }

            using (var newdata = new EncounterInfo())
            {
                newdata.EncounterId = eresponse.WildPokemon.EncounterId.ToString();
                newdata.Iv = Math.Round(PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData), 2);
                newdata.Latitude = eresponse.WildPokemon.Latitude.ToString("G17", CultureInfo.InvariantCulture);
                newdata.Longitude = eresponse.WildPokemon.Longitude.ToString("G17", CultureInfo.InvariantCulture);
                newdata.PokemonName = eresponse.WildPokemon.PokemonData.PokemonId.ToString();
                newdata.SpawnPointId = eresponse.WildPokemon.SpawnPointId;
                newdata.Move1 = eresponse.WildPokemon.PokemonData.Move1.ToString();
                newdata.Move2 = eresponse.WildPokemon.PokemonData.Move2.ToString();


                newdata.TimeTillHiddenMs = eresponse.WildPokemon.TimeTillHiddenMs;
                if (newdata.TimeTillHiddenMs == 0)
                {
                    Random rn = new Random();
                    newdata.TimeTillHiddenMs = rn.Next(450, 481) * 1000;
                }
                newdata.Expiration = eresponse.WildPokemon.LastModifiedTimestampMs + newdata.TimeTillHiddenMs;

                LocationQueue.Add(newdata);
            }
        }

        public static void AddToVisited(List<ulong> encounterIds)
        {
            encounterIds.ForEach(p =>
            {
                if (!VisitedEncounterIds.Contains(p))
                {
                    VisitedEncounterIds.Add(p);
                }
            });
        }

        public static async Task CatchFromService(ISession session, CancellationToken cancellationToken, MSniperInfo2 encounterId)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double lat = session.Client.CurrentLatitude;
            double lon = session.Client.CurrentLongitude;

            EncounterResponse encounter;
            try
            {
                await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                   new GeoCoordinate(encounterId.Latitude, encounterId.Longitude, session.Client.CurrentAltitude), 0); // Speed set to 0 for random speed.

                await Task.Delay(1000, cancellationToken);

                encounter = await session.Client.Encounter.EncounterPokemon(encounterId.EncounterId, encounterId.SpawnPointId);

                await Task.Delay(1000, cancellationToken);
            }
            finally
            {
                await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                    new GeoCoordinate(lat, lon, session.Client.CurrentAltitude), 0);  // Speed set to 0 for random speed.
            }

            PokemonData encounteredPokemon;

            // Catch if it's a WildPokemon (MSniping not allowed for Incense pokemons)
            if (encounter?.Status == EncounterResponse.Types.Status.EncounterSuccess)
            {
                encounteredPokemon = encounter.WildPokemon?.PokemonData;
            }
            else
            {
                Logger.Write($"Pokemon despawned or wrong link format!", LogLevel.Service, ConsoleColor.Gray);
                return;// No success to work with
            }

            var pokemon = new MapPokemon
            {
                EncounterId = encounterId.EncounterId,
                Latitude = encounterId.Latitude,
                Longitude = encounterId.Longitude,
                PokemonId = encounteredPokemon.PokemonId,
                SpawnPointId = encounterId.SpawnPointId
            };

            await CatchPokemonTask.Execute(session, cancellationToken, encounter, pokemon, currentFortData: null, sessionAllowTransfer: true);
        }

        public static List<EncounterInfo> FindNew(List<EncounterInfo> received)
        {
            List<EncounterInfo> newOne = new List<EncounterInfo>();
            received.ForEach(x =>
            {
                if (!VisitedEncounterIds.Contains(x.GetEncounterId()))
                {
                    newOne.Add(x);
                }
            });
            return newOne;
        }

        //public static SocketCmd GetSocketCmd(this MessageReceivedEventArgs e)
        //{
        //    try
        //    {
        //        return (SocketCmd)Enum.Parse(typeof(SocketCmd), e.Message.Split(new string[] { "||" }, StringSplitOptions.None).First());
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
        //        throw ex;
        //    }
        //}

        //public static string[] GetSocketData(this MessageReceivedEventArgs e)
        //{
        //    try
        //    {
        //        return e.Message.Split(new string[] { "||" }, StringSplitOptions.None)[1].Split('|');
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
        //        throw ex;
        //    }
        //}

        //public static void OpenSocket()
        //{
        //    if (socket == null/* || msocket.State == WebSocketState.Closed*/)
        //    {
        //        try
        //        {
        //            Thread.Sleep(30000);
        //            //msniper.com
        //            socket = new WebSocket("ws://msniper.com/WebSockets/NecroBotServer.ashx", "", WebSocketVersion.Rfc6455);
        //            socket.MessageReceived += Msocket_MessageReceived;
        //            socket.Closed += Msocket_Closed;
        //            socket.Open();
        //            lastNotify = DateTime.Now;
        //            //Logger.Write($"Connecting to MSniperService", LogLevel.Service);
        //        }
        //        catch (Exception)
        //        {
        //            TimeSpan ts = DateTime.Now - lastNotify;
        //            if (ts.TotalMinutes > 5)
        //            {
        //                //Logger.Write(ex.Message + "  (may be offline)", LogLevel.Service, ConsoleColor.Red);
        //            }
        //            socket?.Dispose();
        //            socket = null;
        //        }
        //    }
        //}

        public static DateTime TimeStampToDateTime(double timeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(timeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        //private static void Msocket_Closed(object sender, EventArgs e)
        //{
        //    socket.Dispose();
        //    socket = null;
        //    TimeSpan ts = DateTime.Now - lastNotify;
        //    if (ts.TotalMinutes > 5)
        //    {
        //        //Logger.Write("connection lost  (may be offline)", LogLevel.Service, ConsoleColor.Red);
        //    }
        //    //throw new Exception("msniper socket closed");
        //    ////need delay or clear PkmnLocations

        //}

        private static void RefreshLocationQueue()
        {
            var pkmns = LocationQueue
                .Where(p => TimeStampToDateTime(p.Expiration) > DateTime.Now)
                .ToList();
            LocationQueue.Clear();
            LocationQueue.AddRange(pkmns);
        }

        //private static void Msocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        //{
        //    try
        //    {
        //        SocketCmd cmd = e.GetSocketCmd();
        //        switch (cmd)
        //        {
        //            case SocketCmd.IpLimit:
        //                Logger.Write("(IpLimit) " + e.GetSocketData().First(), LogLevel.Service, ConsoleColor.Red);
        //                break;
        //            case SocketCmd.ServerLimit:
        //                Logger.Write("(ServerLimit) " + e.GetSocketData().First(), LogLevel.Service, ConsoleColor.Red);
        //                break;
        //            case SocketCmd.Identity://first request
        //                UserUniequeId = e.GetSocketData().First();
        //                SendToMSniperServer(UserUniequeId);//confirm
        //                Logger.Write($"(Identity) [ {UserUniequeId} ] connection establisted", LogLevel.Service);
        //                break;

        //            case SocketCmd.PokemonCount://server asks what is in your hand (every 3 minutes)
        //                RefreshLocationQueue();
        //                var x = LocationQueue.GroupBy(p => p.PokemonName)
        //                    .Select(s => new PokemonCount { PokemonId = s.First().GetPokemonName(), Count = s.Count() })
        //                    .ToList();
        //                SendToMSniperServer(JsonConvert.SerializeObject(x));
        //                break;

        //            case SocketCmd.SendPokemon://sending encounters
        //                RefreshLocationQueue();
        //                LocationQueue = LocationQueue.OrderByDescending(p => p.Iv).ToList();
        //                int rq = 1;
        //                if (LocationQueue.Count < int.Parse(e.GetSocketData().First()))
        //                {
        //                    rq = LocationQueue.Count;
        //                }
        //                else
        //                {
        //                    rq = int.Parse(e.GetSocketData().First());
        //                }
        //                var selected = LocationQueue.GetRange(0, rq);
        //                SendToMSniperServer(JsonConvert.SerializeObject(selected));
        //                AddToVisited(selected.Select(p => p.GetEncounterId()).ToList());
        //                LocationQueue.RemoveRange(0, rq);
        //                break;

        //            case SocketCmd.SendOneSpecies://server needs one type pokemon
        //                RefreshLocationQueue();
        //                PokemonId speciesId = (PokemonId)Enum.Parse(typeof(PokemonId), e.GetSocketData().First());
        //                int requestCount = int.Parse(e.GetSocketData()[1]);
        //                var onespecies = LocationQueue.Where(p => p.GetPokemonName() == speciesId).ToList();
        //                onespecies = onespecies.OrderByDescending(p => p.Iv).ToList();
        //                if (onespecies.Count > 0)
        //                {
        //                    List<EncounterInfo> oneType;
        //                    if (onespecies.Count > requestCount)
        //                    {
        //                        oneType = LocationQueue.GetRange(0, requestCount);
        //                        AddToVisited(oneType.Select(p => p.GetEncounterId()).ToList());
        //                        LocationQueue.RemoveRange(0, requestCount);
        //                    }
        //                    else
        //                    {
        //                        oneType = LocationQueue.GetRange(0, LocationQueue.Count);
        //                        LocationQueue.Clear();
        //                    }
        //                    SendToMSniperServer(JsonConvert.SerializeObject(oneType));
        //                }
        //                break;

        //            case SocketCmd.Brodcaster://receiving encounter information from server

        //                //// disabled fornow
        //                //var xcoming = JsonConvert.DeserializeObject<List<EncounterInfo>>(e.GetSocketData().First());
        //                //xcoming = FindNew(xcoming);
        //                //ReceivedPokemons.AddRange(xcoming);
        //                //
        //                //RefreshReceivedPokemons();
        //                //TimeSpan ts = DateTime.Now - lastNotify;
        //                //if (ts.TotalMinutes >= 5)
        //                //{
        //                //    Logger.Write($"total active spawns:[ {ReceivedPokemons.Count} ]", LogLevel.Service);
        //                //    lastNotify = DateTime.Now;
        //                //}
        //                break;
        //            case SocketCmd.None:
        //                Logger.Write("UNKNOWN ERROR", LogLevel.Service, ConsoleColor.Red);
        //                //throw Exception
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        socket.Close();
        //        Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
        //        //throw ex;
        //    }
        //}
        //private static void RefreshReceivedPokemons()
        //{
        //    var pkmns = ReceivedPokemons
        //        .Where(p => TimeStampToDateTime(p.Expiration) > DateTime.Now)
        //        .ToList();
        //    ReceivedPokemons.Clear();
        //    ReceivedPokemons.AddRange(pkmns);
        //}
        //private static void SendToMSniperServer(string message)
        //{
        //    try
        //    {
        //        socket.Send($"{message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        socket.Close();
        //        Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
        //        //throw ex;
        //    }
        //}
        #endregion
        public static void AddSnipeItem(ISession session, MSniperInfo2 item)
        {
            SnipeFilter filter = new SnipeFilter()
            {
                SnipeIV = session.LogicSettings.MinIVForAutoSnipe
            };

            var pokemonId = (PokemonId)item.PokemonId;

            if (session.LogicSettings.PokemonSnipeFilters.ContainsKey(pokemonId))
            {
                filter = session.LogicSettings.PokemonSnipeFilters[pokemonId];
            }
            //hack, this case we can't determite move :)

            if(filter.SnipeIV < item.Iv && item.Move1 == PokemonMove.Absorb && item.Move2 == PokemonMove.Absorb )
            {
                autoSnipePokemons.Add(item);
                return;
            }
            //ugly but readable
            if ((string.IsNullOrEmpty(filter.Operator) || filter.Operator == Operator.or.ToString()) &&
                (filter.SnipeIV < item.Iv
                || (filter.Moves != null
                    && filter.Moves.Count > 0
                    && filter.Moves.Any(x => x[0] == item.Move1 && x[1] == item.Move2))
                ))

            {
                autoSnipePokemons.Add(item);
            }


            if (filter.Operator == Operator.and.ToString() &&
               (filter.SnipeIV < item.Iv
               && (filter.Moves != null
                   && filter.Moves.Count > 0
                   && filter.Moves.Any(x => x[0] == item.Move1 && x[1] == item.Move2))
               ))
            {
                autoSnipePokemons.Add(item);
            }

        }

        private static List<MSniperInfo2> autoSnipePokemons = new List<MSniperInfo2>();

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (inProgress)
                return;
            inProgress = true;
            //if (session.LogicSettings.ActivateMSniper)
            //    OpenSocket();

            //return;//NEW SNIPE METHOD WILL BE ACTIVATED

            var pth = Path.Combine(Directory.GetCurrentDirectory(), "SnipeMS.json");
            try
            {
                if (!File.Exists(pth) && autoSnipePokemons.Count == 0)
                {
                    inProgress = false;
                    return;
                }

                if (!await SnipePokemonTask.CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session, cancellationToken))
                {
                    inProgress = false;
                    return;
                }
                List<MSniperInfo2> mSniperLocation2 = new List<MSniperInfo2>();
                if (File.Exists(pth))
                {
                    var sr = new StreamReader(pth, Encoding.UTF8);
                    var jsn = sr.ReadToEnd();
                    sr.Close();

                    mSniperLocation2 = JsonConvert.DeserializeObject<List<MSniperInfo2>>(jsn);
                    File.Delete(pth);
                    if (mSniperLocation2 == null) mSniperLocation2 = new List<MSniperInfo2>();
                }

                mSniperLocation2.AddRange(autoSnipePokemons);
                autoSnipePokemons.Clear();

                foreach (var location in mSniperLocation2)
                {
                    if (session.Cache[location.EncounterId.ToString()] != null) continue;

                    session.Cache.Add(location.EncounterId.ToString(), true, DateTime.Now.AddMinutes(15)); 

                    cancellationToken.ThrowIfCancellationRequested();

                    session.EventDispatcher.Send(new SnipeScanEvent
                    {
                        Bounds = new Location(location.Latitude, location.Longitude),
                        PokemonId = (PokemonId)location.PokemonId,
                        Source = "MSniperService",
                        Iv = location.Iv
                    });
                    if (location.EncounterId != 0)
                    {
                        await CatchFromService(session, cancellationToken, location);
                    }
                    else
                    {
                        await CatchWithSnipe(session, cancellationToken, location);
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (ActiveSwitchByRuleException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                File.Delete(pth);
                var ee = new ErrorEvent { Message = ex.Message };
                if (ex.InnerException != null) ee.Message = ex.InnerException.Message;
                session.EventDispatcher.Send(ee);
            }
            finally
            {
                inProgress = false;
            }
        }

        public static async Task CatchWithSnipe(ISession session, CancellationToken cancellationToken, MSniperInfo2 encounterId)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await
                  SnipePokemonTask.Snipe(session, new List<PokemonId>() { (PokemonId)encounterId.PokemonId }, encounterId.Latitude, encounterId.Longitude, cancellationToken);
        }
    }
}