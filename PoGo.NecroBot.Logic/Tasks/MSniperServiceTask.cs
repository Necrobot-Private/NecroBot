
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
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Inventory.Item;
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
            Brodcaster = 5,
            IpLimit = 6,
            ServerLimit = 7
        }

        public class EncounterInfo : IDisposable
        {
            public ulong EncounterId { get; set; }
            public double Iv { get; set; }
            public long LastModifiedTimestampMs { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public PokemonMove Move1 { get; set; }
            public PokemonMove Move2 { get; set; }
            public PokemonId PokemonId { get; set; }
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

        #region Variables

        public static double minIvPercent = 50.0;
        public static WebSocket socket;
        public static List<EncounterInfo> LocationQueue = new List<EncounterInfo>();
        public static List<EncounterInfo> ReceivedPokemons = new List<EncounterInfo>();
        public static List<ulong> VisitedEncounterIds = new List<ulong>();
        public static string UserUniequeId { get; set; } //only info
        public static DateTime lastNotify { get; set; }
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

            if (LocationQueue.FirstOrDefault(p => p.EncounterId == eresponse.WildPokemon.EncounterId) != null)
            {
                return;
            }

            using (var newdata = new EncounterInfo())
            {
                newdata.EncounterId = eresponse.WildPokemon.EncounterId;
                newdata.LastModifiedTimestampMs = eresponse.WildPokemon.LastModifiedTimestampMs;
                newdata.SpawnPointId = eresponse.WildPokemon.SpawnPointId;
                newdata.TimeTillHiddenMs = eresponse.WildPokemon.TimeTillHiddenMs;
                newdata.PokemonId = eresponse.WildPokemon.PokemonData.PokemonId;
                newdata.Iv = PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData);
                newdata.Latitude = eresponse.WildPokemon.Latitude;
                newdata.Longitude = eresponse.WildPokemon.Longitude;
                newdata.Move1 = eresponse.WildPokemon.PokemonData.Move1;
                newdata.Move2 = eresponse.WildPokemon.PokemonData.Move2;

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

        public static async Task CatchFromService(ISession session, CancellationToken cancellationToken, EncounterInfo encounterId)
        {
            //default to excellent throw
            var normalizedRecticleSize = 1.95;
            //default spin
            var spinModifier = 1.0;

            //round to 2 decimals
            normalizedRecticleSize = Math.Round(normalizedRecticleSize, 2);

            CatchPokemonResponse caughtPokemonResponse;
            double lat = session.Client.CurrentLatitude;
            double lon = session.Client.CurrentLongitude;
            CatchPokemonResponse.Types.CatchStatus lastThrow = CatchPokemonResponse.Types.CatchStatus.CatchSuccess;
            CatchPokemonTask.AmountOfBerries = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                    new GeoCoordinate(encounterId.Latitude, encounterId.Longitude, session.Client.CurrentAltitude), 0); // Speed set to 0 for random speed.

                await Task.Delay(1000, cancellationToken);

                var encounter = await session.Client.Encounter.EncounterPokemon(encounterId.EncounterId, encounterId.SpawnPointId);

                await Task.Delay(1000, cancellationToken);

                await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                    new GeoCoordinate(lat, lon, session.Client.CurrentAltitude), 0);  // Speed set to 0 for random speed.

                float probability = encounter.CaptureProbability.CaptureProbability_[0];
                int cp = encounter.WildPokemon.PokemonData.Cp;
                int maxcp = PokemonInfo.CalculateMaxCp(encounter.WildPokemon.PokemonData);
                double lvl = PokemonInfo.GetLevel(encounter.WildPokemon.PokemonData);

                PokemonData encounteredPokemon;

                // Catch if it's a WildPokemon (MSniping not allowed for Incense pokemons)
                if (encounter is EncounterResponse && (encounter?.Status == EncounterResponse.Types.Status.EncounterSuccess))
                {
                    encounteredPokemon = encounter.WildPokemon?.PokemonData;
                }
                else return; // No success to work with
                var bestBall = await CatchPokemonTask.GetBestBall(session, encounteredPokemon, probability);

                if (((session.LogicSettings.UseBerriesOperator.ToLower().Equals("and") &&
                       encounterId.Iv >= session.LogicSettings.UseBerriesMinIv &&
                       cp >= session.LogicSettings.UseBerriesMinCp &&
                       probability < session.LogicSettings.UseBerriesBelowCatchProbability) ||
                   (session.LogicSettings.UseBerriesOperator.ToLower().Equals("or") && (
                       encounterId.Iv >= session.LogicSettings.UseBerriesMinIv ||
                       cp >= session.LogicSettings.UseBerriesMinCp ||
                       probability < session.LogicSettings.UseBerriesBelowCatchProbability))) &&
                   lastThrow != CatchPokemonResponse.Types.CatchStatus.CatchMissed) // if last throw is a miss, no double berry
                {

                    CatchPokemonTask.AmountOfBerries++;
                    if (CatchPokemonTask.AmountOfBerries <= session.LogicSettings.MaxBerriesToUsePerPokemon)
                    {
                        await CatchPokemonTask.UseBerry(session,
                           encounter.WildPokemon.EncounterId,
                           encounter.WildPokemon.SpawnPointId);
                    }

                }

                caughtPokemonResponse = await session.Client.Encounter.CatchPokemon(encounterId.EncounterId, encounterId.SpawnPointId,
                    bestBall, normalizedRecticleSize, spinModifier, true);


                Logger.Write($"({caughtPokemonResponse.Status.ToString()})  {encounterId.PokemonId.ToString()}  IV: {encounterId.Iv}%  Lvl: {lvl}  CP: ({cp}/{maxcp})", LogLevel.Service, caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? ConsoleColor.Green : ConsoleColor.Red);
                //CatchPokemonTask.AmountOfBerries
                await Task.Delay(1000, cancellationToken);
                lastThrow = caughtPokemonResponse.Status;
            } while (lastThrow == CatchPokemonResponse.Types.CatchStatus.CatchMissed || lastThrow == CatchPokemonResponse.Types.CatchStatus.CatchEscape);

        }

        public static List<EncounterInfo> FindNew(List<EncounterInfo> received)
        {
            List<EncounterInfo> newOne = new List<EncounterInfo>();
            received.ForEach(x =>
            {
                if (!VisitedEncounterIds.Contains(x.EncounterId))
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
                return (SocketCmd)Enum.Parse(typeof(SocketCmd), e.Message.Split(new string[] { "||" }, StringSplitOptions.None).First());
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
                throw ex;
            }
        }

        public static string[] GetSocketData(this MessageReceivedEventArgs e)
        {
            try
            {
                return e.Message.Split(new string[] { "||" }, StringSplitOptions.None)[1].Split('|');
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
                throw ex;
            }
        }

        public static void OpenSocket()
        {
            if (socket == null/* || msocket.State == WebSocketState.Closed*/)
            {
                try
                {
                    Thread.Sleep(500);
                    //msniper.com
                    socket = new WebSocket("ws://msniper.com/WebSockets/NecroBotServer.ashx", "", WebSocketVersion.Rfc6455);
                    socket.MessageReceived += Msocket_MessageReceived;
                    socket.Closed += Msocket_Closed;
                    socket.Open();
                    lastNotify = DateTime.Now;
                    Logger.Write($"Connecting to MSniperService", LogLevel.Service);
                }
                catch (Exception ex)
                {
                    TimeSpan ts = DateTime.Now - lastNotify;
                    if (ts.TotalMinutes > 5)
                    {
                        Logger.Write(ex.Message + "  (may be offline)", LogLevel.Service, ConsoleColor.Red);
                    }
                    socket?.Dispose();
                    socket = null;
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
            TimeSpan ts = DateTime.Now - lastNotify;
            if (ts.TotalMinutes > 5)
            {
                Logger.Write("connection lost  (may be offline)", LogLevel.Service, ConsoleColor.Red);
            }
            //throw new Exception("msniper socket closed");
            ////need delay or clear PkmnLocations

        }

        private static void RefreshLocationQueue()
        {
            var pkmns = LocationQueue
                .Where(p => TimeStampToDateTime(p.LastModifiedTimestampMs + p.TimeTillHiddenMs) > DateTime.Now)
                .ToList();
            LocationQueue.Clear();
            LocationQueue.AddRange(pkmns);
        }

        private static void Msocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                SocketCmd cmd = e.GetSocketCmd();
                switch (cmd)
                {
                    case SocketCmd.IpLimit:
                        Logger.Write("(IpLimit) " + e.GetSocketData().First(), LogLevel.Service, ConsoleColor.Red);
                        break;
                    case SocketCmd.ServerLimit:
                        Logger.Write("(ServerLimit) " + e.GetSocketData().First(), LogLevel.Service, ConsoleColor.Red);
                        break;
                    case SocketCmd.Identity://first request
                        UserUniequeId = e.GetSocketData().First();
                        SendToMSniperServer(UserUniequeId);//confirm
                        Logger.Write($"(Identity) [ {UserUniequeId} ] connection establisted", LogLevel.Service);
                        break;

                    case SocketCmd.PokemonCount://server asks what is in your hand (every 3 minutes)
                        RefreshLocationQueue();
                        var x = LocationQueue.GroupBy(p => p.PokemonId)
                            .Select(s => new PokemonCount { PokemonId = s.First().PokemonId, Count = s.Count() })
                            .ToList();
                        SendToMSniperServer(JsonConvert.SerializeObject(x));
                        break;

                    case SocketCmd.SendPokemon://sending encounters
                        RefreshLocationQueue();
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
                        RefreshLocationQueue();
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

                        //// disabled fornow
                        //var xcoming = JsonConvert.DeserializeObject<List<EncounterInfo>>(e.GetSocketData().First());
                        //xcoming = FindNew(xcoming);
                        //ReceivedPokemons.AddRange(xcoming);
                        //
                        //RefreshReceivedPokemons();
                        //TimeSpan ts = DateTime.Now - lastNotify;
                        //if (ts.TotalMinutes >= 5)
                        //{
                        //    Logger.Write($"total active spawns:[ {ReceivedPokemons.Count} ]", LogLevel.Service);
                        //    lastNotify = DateTime.Now;
                        //}
                        break;
                    case SocketCmd.None:
                        Logger.Write("UNKNOWN ERROR", LogLevel.Service, ConsoleColor.Red);
                        //throw Exception
                        break;
                }
            }
            catch (Exception ex)
            {
                socket.Close();
                Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
                //throw ex;
            }
        }
        private static void RefreshReceivedPokemons()
        {
            var pkmns = ReceivedPokemons
                .Where(p => TimeStampToDateTime(p.LastModifiedTimestampMs + p.TimeTillHiddenMs) > DateTime.Now)
                .ToList();
            ReceivedPokemons.Clear();
            ReceivedPokemons.AddRange(pkmns);
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
                Logger.Write(ex.Message, LogLevel.Service, ConsoleColor.Red);
                //throw ex;
            }
        }
        #endregion

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (inProgress)
                return;
            inProgress = true;
            OpenSocket();

            //return;//NEW SNIPE METHOD WILL BE ACTIVATED

            var pth = Path.Combine(session.LogicSettings.ProfilePath, "SnipeMS.json");
            try
            {
                if (!File.Exists(pth))
                {
                    inProgress = false;
                    return;
                }

                if (!await SnipePokemonTask.CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session, cancellationToken))
                {
                    inProgress = false;
                    return;
                }

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
            catch (Exception ex)
            {
                File.Delete(pth);
                var ee = new ErrorEvent { Message = ex.Message };
                if (ex.InnerException != null) ee.Message = ex.InnerException.Message;
                session.EventDispatcher.Send(ee);
            }
            inProgress = false;
        }

        public static async Task CatchWithSnipe(ISession session, CancellationToken cancellationToken, EncounterInfo encounterId)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await
                  SnipePokemonTask.Snipe(session, new List<PokemonId>() { encounterId.PokemonId }, encounterId.Latitude, encounterId.Longitude, cancellationToken);
        }
    }
}
