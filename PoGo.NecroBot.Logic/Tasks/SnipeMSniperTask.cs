
using GeoCoordinatePortable;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
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
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.PoGoUtils;
using WebSocket4Net;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class MSniperServiceTask
    {
        #region Classes

        public enum SocketCmd
        {
            None = 0,
            Identity = 1,
            PokemonCount = 2,
            SendPokemon = 3,
            SendOneSpecies = 4,
            Brodcaster = 5

        }

        public class EncounterInfo : IDisposable
        {
            public long EncounterId { get; set; }
            public double Iv { get; set; }
            public long LastModifiedTimestampMs { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public PokemonMove Move1 { get; set; } = PokemonMove.MoveUnset;
            public PokemonMove Move2 { get; set; } = PokemonMove.MoveUnset;
            public PokemonId PokemonId { get; set; } = PokemonId.Missingno;
            public string SpawnPointId { get; set; }
            public int TimeTillHiddenMs { get; set; }
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

        #region Properties
        public static List<PokemonId> blackList = new List<PokemonId>()
        {
            PokemonId.Pidgeot,
            PokemonId.Pidgey,
            PokemonId.Weedle,
            PokemonId.Spearow,
            PokemonId.Zubat
        };

        public static double minIvPercent = 50.0;
        public static WebSocket socket;
        public static List<EncounterInfo> LocationQueue = new List<EncounterInfo>();
        public static List<EncounterInfo> ReceivedPokemons = new List<EncounterInfo>();
        public static List<long> VisitedEncounterIds = new List<long>();
        public static string UserUniequeId { get; set; }
        #endregion

        #region MSniper Location Feeder

        public static void AddToList(EncounterResponse eresponse)
        {
            if (!(PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData) >= minIvPercent) &&
                blackList.FindIndex(p => p == eresponse.WildPokemon.PokemonData.PokemonId) != -1 &&
                LocationQueue.FirstOrDefault(p => p.EncounterId == (long)eresponse.WildPokemon.EncounterId) != null &&
                VisitedEncounterIds.FindIndex(p => p == (long)eresponse.WildPokemon.EncounterId) != -1)
                return;

            using (var newdata = new EncounterInfo())
            {
                newdata.EncounterId = (long)eresponse.WildPokemon.EncounterId;
                newdata.LastModifiedTimestampMs = eresponse.WildPokemon.LastModifiedTimestampMs;
                newdata.SpawnPointId = eresponse.WildPokemon.SpawnPointId;
                newdata.TimeTillHiddenMs = eresponse.WildPokemon.TimeTillHiddenMs;
                newdata.PokemonId = eresponse.WildPokemon.PokemonData.PokemonId;
                newdata.Iv = PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData);
                newdata.Latitude = eresponse.WildPokemon.Latitude;
                newdata.Longitude = eresponse.WildPokemon.Longitude;
                newdata.Move1 = eresponse.WildPokemon.PokemonData.Move1;
                newdata.Move2 = eresponse.WildPokemon.PokemonData.Move2;

                if (LocationQueue.FirstOrDefault(p => p.EncounterId == newdata.EncounterId &&
                p.SpawnPointId == newdata.SpawnPointId) == null)
                    LocationQueue.Add(newdata);
            }
        }

        public static void AddToVisited(List<long> encounterIds)
        {
            encounterIds.ForEach(p =>
            {
                int index = VisitedEncounterIds.FindIndex(x => x == p);
                if (index == -1)
                {
                    VisitedEncounterIds.Add(p);
                }
            });
        }

        public static async Task CatchFromService(ISession session, CancellationToken cancellationToken, ulong encounterId, string spawnPointGuid)
        {
            bool hitPokemon = true;

            //default to excellent throw
            var normalizedRecticleSize = 1.95;
            //default spin
            var spinModifier = 1.0;

            //round to 2 decimals
            normalizedRecticleSize = Math.Round(normalizedRecticleSize, 2);

            var caughtPokemonResponse = await
                 session.Client.Encounter.CatchPokemon(encounterId, spawnPointGuid,
                     POGOProtos.Inventory.Item.ItemId.ItemPokeBall, normalizedRecticleSize, spinModifier, hitPokemon);

            Logger.Write(caughtPokemonResponse.Status.ToString(), LogLevel.Info, ConsoleColor.White);
        }

        public static List<EncounterInfo> FindNew(List<EncounterInfo> received)
        {
            List<EncounterInfo> newOne = new List<EncounterInfo>();
            received.ForEach(x =>
            {
                int index = VisitedEncounterIds.FindIndex(p => p == x.EncounterId);
                if (index == -1)
                {
                    newOne.Add(x);
                }
            });
            return newOne;
        }

        public static SocketCmd GetSocketCmd(this MessageReceivedEventArgs e)
        {
            try
            {
                return (SocketCmd)Enum.Parse(typeof(SocketCmd), e.Message.Split('|').First());
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                throw ex;
            }
        }

        public static string[] GetSocketData(this MessageReceivedEventArgs e)
        {
            try
            {
                return e.Message.Split('|')[1].Split(',');
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                throw ex;
            }
        }

        public static void OpenSocket()
        {
            if (socket == null/* || msocket.State == WebSocketState.Closed*/)
            {
                try
                {
                    //msniper.com
                    socket = new WebSocket("ws://msniper.com/WebSockets/NecroBotServer.ashx", "", WebSocketVersion.Rfc6455);
                    socket.MessageReceived += Msocket_MessageReceived;
                    socket.Closed += Msocket_Closed;
                    socket.Open();
                    Logger.Write($"Connecting to NecroBot2Service", LogLevel.Info, ConsoleColor.White);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                }
            }
        }

        public static DateTime TimeStampToDateTime(double timeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(timeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        private static void Msocket_Closed(object sender, EventArgs e)
        {
            socket.Dispose();
            socket = null;
            Logger.Write("Nercobot2Service connection lost", LogLevel.Error, ConsoleColor.Red);
            //throw new Exception("msniper socket closed");
            ////need delay or clear PkmnLocations

        }
        private static void Msocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                SocketCmd cmd = e.GetSocketCmd();
                switch (cmd)
                {
                    case SocketCmd.Identity://first request
                        UserUniequeId = e.GetSocketData().First();
                        SendToMSniperServer(UserUniequeId);//confirm
                        Logger.Write($"Identity: [ {UserUniequeId} ] connection establisted with Nercobot2Service", LogLevel.Info, ConsoleColor.White);
                        break;

                    case SocketCmd.PokemonCount://server asks what is in your hand (every 3 minutes)
                        var x = LocationQueue.GroupBy(p => p.PokemonId)
                            .Select(s => new PokemonCount { PokemonId = s.First().PokemonId, Count = s.Count() })
                            .ToList();
                        SendToMSniperServer(JsonConvert.SerializeObject(x));
                        break;

                    case SocketCmd.SendPokemon://sending encounters
                        LocationQueue = LocationQueue.OrderByDescending(p => p.Iv).ToList();
                        int rq = 1;
                        if (LocationQueue.Count < int.Parse(e.GetSocketData().First()))
                        {
                            rq = LocationQueue.Count;
                        }
                        else
                        {
                            rq = int.Parse(e.GetSocketData().First());
                        }
                        var selected = LocationQueue.GetRange(0, rq);
                        SendToMSniperServer(JsonConvert.SerializeObject(selected));
                        AddToVisited(selected.Select(p => p.EncounterId).ToList());
                        LocationQueue.RemoveRange(0, rq);
                        break;

                    case SocketCmd.SendOneSpecies://server needs one type pokemon
                        PokemonId speciesId = (PokemonId)Enum.Parse(typeof(PokemonId), e.GetSocketData().First());
                        int requestCount = int.Parse(e.GetSocketData()[1]);
                        var onespecies = LocationQueue.Where(p => p.PokemonId == speciesId).ToList();
                        onespecies = onespecies.OrderByDescending(p => p.Iv).ToList();
                        if (onespecies.Count > 0)
                        {
                            List<EncounterInfo> oneType;
                            if (onespecies.Count > requestCount)
                            {
                                oneType = LocationQueue.GetRange(0, requestCount);
                                AddToVisited(oneType.Select(p => p.EncounterId).ToList());
                                LocationQueue.RemoveRange(0, requestCount);
                            }
                            else
                            {
                                oneType = LocationQueue.GetRange(0, LocationQueue.Count);
                                LocationQueue.Clear();
                            }
                            SendToMSniperServer(JsonConvert.SerializeObject(oneType));
                        }
                        break;

                    case SocketCmd.Brodcaster://receiving encounter information from server
                        var xcoming = JsonConvert.DeserializeObject<List<EncounterInfo>>(e.GetSocketData().First());
                        Logger.Write($"Brodcaster:  Received {xcoming.Count} pokemon from Nercobot2Service", LogLevel.Info, ConsoleColor.White);
                        xcoming = FindNew(xcoming);
                        ReceivedPokemons.AddRange(xcoming);
                        Logger.Write($"Brodcaster:  AND {xcoming.Count} amount pokemons haven't visited", LogLevel.Info, ConsoleColor.White);
                        break;
                    case SocketCmd.None:
                        Logger.Write("UNKNOWN ERROR", LogLevel.Info, ConsoleColor.White);
                        //throw Exception
                        break;
                }
            }
            catch (Exception ex)
            {
                socket.Close();
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                //throw ex;
            }
        }

        private static void SendToMSniperServer(string message)
        {
            try
            {
                socket.Send($"{message}");
            }
            catch (Exception ex)
            {
                socket.Close();
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                //throw ex;
            }
        }
        #endregion
        public static async Task CheckMSniper(ISession session, CancellationToken cancellationToken)
        {
            OpenSocket();

            //return;//NEW SNIPE METHOD WILL BE ACTIVATED

            var pth = Path.Combine(session.LogicSettings.ProfilePath, "SnipeMS.json");
            try
            {
                if (!File.Exists(pth))
                    return;

                if (!await SnipePokemonTask.CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session, cancellationToken))
                    return;

                var sr = new StreamReader(pth, Encoding.UTF8);
                var jsn = sr.ReadToEnd();
                sr.Close();

                var mSniperLocation2 = JsonConvert.DeserializeObject<List<EncounterInfo>>(jsn);
                File.Delete(pth);
                foreach (var location in mSniperLocation2)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    session.EventDispatcher.Send(new SnipeScanEvent
                    {
                        Bounds = new Location(location.Latitude, location.Longitude),
                        PokemonId = location.PokemonId,
                        Source = "MSniperService"
                    });

                    await CatchFromService(session, cancellationToken, (ulong)location.EncounterId, location.SpawnPointId);
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                File.Delete(pth);
                var ee = new ErrorEvent { Message = ex.Message };
                if (ex.InnerException != null) ee.Message = ex.InnerException.Message;
                session.EventDispatcher.Send(ee);
            }
        }
    }
}
