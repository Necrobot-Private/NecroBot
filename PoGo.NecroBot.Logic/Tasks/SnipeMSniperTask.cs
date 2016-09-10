
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
        #region MSniper Location Feeder

        public class PokemonCount
        {
            public PokemonId PokemonId { get; set; }
            public int Count { get; set; }
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        public class EncounterInfo : IDisposable
        {
            public string SpawnPointId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public PokemonMove Move1 { get; set; } = PokemonMove.MoveUnset;
            public PokemonMove Move2 { get; set; } = PokemonMove.MoveUnset;
            public double Iv { get; set; } = 0;
            public ulong EncounterId { get; set; }
            public PokemonId PokemonId { get; set; }
            public DateTime ExpirationTimestamp { get; set; }
            public DateTime LastVisitedTimeStamp { get; set; }
            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }
        public enum SocketCmd
        {
            None = 0,
            Identity = 1,
            PokemonCount = 2,
            SendPokemon = 3,
            SendOneSpecies = 4,
            Brodcaster = 5

        }

        public static SocketCmd GetSocketCmd(this MessageReceivedEventArgs e)
        {
            try
            {
                return (SocketCmd)Enum.Parse(typeof(SocketCmd), e.Message.Split('|')[0]);
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                throw ex;
            }
        }
        public static string GetSocketData(this MessageReceivedEventArgs e)
        {
            try
            {
                return e.Message.Split('|')[1];
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                throw ex;
            }
        }
        public static double minIvPercent = 1.0;

        public static List<EncounterInfo> PkmnLocations = new List<EncounterInfo>();
        public static List<ulong> VisitedEncounterIds = new List<ulong>();
        public static List<PokemonId> blackList = new List<PokemonId>()
        {
            //PokemonId.Pidgeot,
            //PokemonId.Pidgey,
            //PokemonId.Weedle,
            //PokemonId.Spearow

        };
        public static WebSocket msocket;

        public static string UniequeId { get; set; }

        public static void OpenSocket()
        {
            if (msocket == null/* || msocket.State == WebSocketState.Closed*/)
            {
                try
                {
                    //msniper.com
                    msocket = new WebSocket("ws://msniper.com/WebSockets/NecroBotServer.ashx", "", WebSocketVersion.Rfc6455);
                    msocket.MessageReceived += Msocket_MessageReceived;
                    msocket.Closed += Msocket_Closed;
                    msocket.Open();
                    Logger.Write($"Connection to LocationService", LogLevel.Info, ConsoleColor.White);

                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                }
            }
        }

        private static void Msocket_Closed(object sender, EventArgs e)
        {
            msocket.Dispose();
            msocket = null;
            Logger.Write("msniper socket closed", LogLevel.Error, ConsoleColor.Red);
            //throw new Exception("msniper socket closed");
            ////need delay or clear PkmnLocations

        }

        public static void AddToVisited(List<ulong> encounterIds)
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

        public static List<EncounterInfo> FindNew(List<EncounterInfo> received)
        {
            List<EncounterInfo> newOne = new List<EncounterInfo>();
            foreach (var VARIABLE in received)
            {
                int index = VisitedEncounterIds.FindIndex(p => p == VARIABLE.EncounterId);
                if (index == -1)
                {
                    newOne.Add(VARIABLE);
                }
            }
            return newOne;
        }

        private static void Msocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                SocketCmd cmd = e.GetSocketCmd();
                switch (cmd)
                {
                    case SocketCmd.Identity://first request
                        UniequeId = e.GetSocketData();
                        SendToMSniperServer(UniequeId);//confirm
                        Logger.Write($"Identity: [ {UniequeId} ] connection establisted with service", LogLevel.Info, ConsoleColor.White);
                        break;

                    case SocketCmd.PokemonCount://server asks what is in your hand (every 3 minutes)
                        var x = PkmnLocations.GroupBy(p => p.PokemonId)
                            .Select(s => new PokemonCount { PokemonId = s.First().PokemonId, Count = s.Count() })
                            .ToList();
                        SendToMSniperServer(JsonConvert.SerializeObject(x));
                        Logger.Write($"PokemonCount: Telling amount of pokemon [ {x.Count} ]", LogLevel.Info, ConsoleColor.White);
                        break;

                    case SocketCmd.SendPokemon://sending encounters
                        PkmnLocations = PkmnLocations.OrderByDescending(p => p.Iv).ToList();
                        int rq = 1;
                        if (PkmnLocations.Count < int.Parse(e.GetSocketData()))
                        {
                            rq = PkmnLocations.Count;
                        }
                        else
                        {
                            rq = int.Parse(e.GetSocketData());
                        }
                        var selected = PkmnLocations.GetRange(0, rq);
                        SendToMSniperServer(JsonConvert.SerializeObject(selected));
                        AddToVisited(selected.Select(p => p.EncounterId).ToList());
                        PkmnLocations.RemoveRange(0, rq);
                        Logger.Write($"SendPokemon: Sending {selected.Count} amount PokemonLocation", LogLevel.Info, ConsoleColor.White);
                        break;

                    case SocketCmd.SendOneSpecies://server needs one type pokemon
                        PokemonId speciesId = (PokemonId)Enum.Parse(typeof(PokemonId), e.GetSocketData().Split(',')[0]);
                        int requestCount = int.Parse(e.GetSocketData().Split(',')[1]);
                        var onespecies = PkmnLocations.Where(p => p.PokemonId == speciesId).ToList();
                        onespecies = onespecies.OrderByDescending(p => p.Iv).ToList();
                        if (onespecies.Count > 0)
                        {
                            List<EncounterInfo> oneType;
                            if (onespecies.Count > requestCount)
                            {
                                oneType = PkmnLocations.GetRange(0, requestCount);
                                AddToVisited(oneType.Select(p => p.EncounterId).ToList());
                                PkmnLocations.RemoveRange(0, requestCount);
                            }
                            else
                            {
                                oneType = PkmnLocations.GetRange(0, PkmnLocations.Count);
                                PkmnLocations.Clear();
                            }
                            SendToMSniperServer(JsonConvert.SerializeObject(oneType));
                            Logger.Write($"SendOneSpecies: Sending {oneType.First().PokemonId.ToString()} [ {oneType.Count} ] amount", LogLevel.Info, ConsoleColor.White);
                        }
                        break;

                    case SocketCmd.Brodcaster://receiving encounter information from server
                        List<EncounterInfo> POKEMON_FEED = JsonConvert.DeserializeObject<List<EncounterInfo>>(e.GetSocketData());
                        Logger.Write($"Brodcaster:  Received {POKEMON_FEED.Count} pokemon location from MSniper Service", LogLevel.Info, ConsoleColor.White);
                        POKEMON_FEED = FindNew(POKEMON_FEED);
                        Logger.Write($"Brodcaster:  AND {POKEMON_FEED.Count} amount pokemon haven't visited", LogLevel.Info, ConsoleColor.White);
                        break;
                    case SocketCmd.None:
                        Logger.Write("UNKNOWN ERROR", LogLevel.Info, ConsoleColor.White);
                        //throw Exception
                        break;
                }
            }
            catch (Exception ex)
            {
                msocket.Close();
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                //throw ex;
            }
        }

        private static void SendToMSniperServer(string message)
        {
            try
            {
                msocket.Send($"{message}");
            }
            catch (Exception ex)
            {
                msocket.Close();
                Logger.Write(ex.Message, LogLevel.Error, ConsoleColor.Red);
                //throw ex;
            }
        }

        public static void AddToList(EncounterResponse eresponse)
        {
            if (!(PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData) >= minIvPercent) &&
                blackList.FindIndex(p => p == eresponse.WildPokemon.PokemonData.PokemonId) != -1 &&
                PkmnLocations.FirstOrDefault(p => p.EncounterId == eresponse.WildPokemon.EncounterId) != null &&
                VisitedEncounterIds.FindIndex(p => p == eresponse.WildPokemon.EncounterId) != -1)
                return;

            //CONVERT TESTS
            string _id = eresponse.WildPokemon.PokemonData.PokemonId.ToString();
            string _lat = eresponse.WildPokemon.Latitude.ToString("G17", CultureInfo.InvariantCulture);
            string _lon = eresponse.WildPokemon.Longitude.ToString("G17", CultureInfo.InvariantCulture);
            long lastmodified = eresponse.WildPokemon.LastModifiedTimestampMs;
            DateTime _lastmodified = JavaTimeStampToDateTime(lastmodified);
            long hiddenms = eresponse.WildPokemon.TimeTillHiddenMs;
            DateTime _expiredtime = JavaTimeStampToDateTime(lastmodified + hiddenms);
            ///////////////////////////

            using (var newdata = new EncounterInfo())
            {
                //JavaTimeStampToDateTime WRONG CONVERTER !
                newdata.EncounterId = eresponse.WildPokemon.EncounterId;
                newdata.LastVisitedTimeStamp = JavaTimeStampToDateTime(eresponse.WildPokemon.LastModifiedTimestampMs);
                newdata.SpawnPointId = eresponse.WildPokemon.SpawnPointId;
                newdata.ExpirationTimestamp = JavaTimeStampToDateTime(eresponse.WildPokemon.LastModifiedTimestampMs + eresponse.WildPokemon.TimeTillHiddenMs);
                newdata.PokemonId = eresponse.WildPokemon.PokemonData.PokemonId;
                newdata.Iv = PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData);
                newdata.Latitude = eresponse.WildPokemon.Latitude;
                newdata.Longitude = eresponse.WildPokemon.Longitude;
                newdata.Move1 = eresponse.WildPokemon.PokemonData.Move1;
                newdata.Move2 = eresponse.WildPokemon.PokemonData.Move2;

                if (PkmnLocations.FirstOrDefault(p => p.EncounterId == newdata.EncounterId &&
                p.SpawnPointId == newdata.SpawnPointId) == null)
                    PkmnLocations.Add(newdata);
            }
        }

        #endregion
        public static async Task CheckMSniper(ISession session, CancellationToken cancellationToken)
        {
            OpenSocket();
            
            var pth = Path.Combine(session.LogicSettings.ProfilePath, "SnipeMS.json");
            try
            {
                if (!File.Exists(pth))
                    return;

                if (!await SnipePokemonTask.CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session, cancellationToken))
                    return;

                var currentLatitude = session.Client.CurrentLatitude;
                var currentLongitude = session.Client.CurrentLongitude;

                var sr = new StreamReader(pth, Encoding.UTF8);
                var jsn = sr.ReadToEnd();
                sr.Close();
                var mSniperLocation = JsonConvert.DeserializeObject<List<MSniperInfo>>(jsn);
                File.Delete(pth);
                foreach (var location in mSniperLocation)
                {
                    session.EventDispatcher.Send(new SnipeScanEvent
                    {
                        Bounds = new Location(location.Latitude, location.Longitude),
                        PokemonId = location.Id,
                        Source = "MSniper"
                    });

                    await SpecialSnipe(session, location.Id, location.Latitude, location.Longitude, cancellationToken);
                }
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude));
            }
            catch (Exception ex)
            {
                File.Delete(pth);
                var ee = new ErrorEvent { Message = ex.Message };
                if (ex.InnerException != null) ee.Message = ex.InnerException.Message;
                session.EventDispatcher.Send(ee);
            }
        }

        public static async Task SpecialSnipe(ISession session, PokemonId targetPokemonId, double latitude,
           double longitude, CancellationToken cancellationToken, bool sessionAllowTransfer = true)
        {
            var currentLatitude = session.Client.CurrentLatitude;
            var currentLongitude = session.Client.CurrentLongitude;

            session.EventDispatcher.Send(new SnipeModeEvent { Active = true });

            List<MapPokemon> catchablePokemon;
            try
            {
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(latitude, longitude, session.Client.CurrentAltitude));

                session.EventDispatcher.Send(new UpdatePositionEvent
                {
                    Longitude = longitude,
                    Latitude = latitude
                });

                var nearbyPokemons = await GetPokemons(session);
                catchablePokemon = nearbyPokemons.Where(p => p.PokemonId == targetPokemonId).ToList();

            }
            finally
            {
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude));
            }

            if (catchablePokemon.Count > 0)
            {
                foreach (var pokemon in catchablePokemon)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    EncounterResponse encounter;
                    try
                    {
                        await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(latitude, longitude, session.Client.CurrentAltitude));

                        encounter = await session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);
                    }
                    finally
                    {
                        await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude));
                    }

                    switch (encounter.Status)
                    {
                        case EncounterResponse.Types.Status.EncounterSuccess:
                            session.EventDispatcher.Send(new UpdatePositionEvent
                            {
                                Latitude = currentLatitude,
                                Longitude = currentLongitude
                            });

                            await CatchPokemonTask.Execute(session, cancellationToken, encounter, pokemon);
                            session.Stats.SnipeCount++;
                            session.Stats.LastSnipeTime = DateTime.Now;
                            break;
                        case EncounterResponse.Types.Status.PokemonInventoryFull:
                            if (session.LogicSettings.TransferDuplicatePokemon || session.LogicSettings.TransferWeakPokemon)
                            {
                                session.EventDispatcher.Send(new WarnEvent
                                {
                                    Message = session.Translation.GetTranslation(TranslationString.InvFullTransferring)
                                });
                                if (session.LogicSettings.TransferDuplicatePokemon)
                                    await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
                                if (session.LogicSettings.TransferWeakPokemon)
                                    await TransferWeakPokemonTask.Execute(session, cancellationToken);
                            }
                            else
                                session.EventDispatcher.Send(new WarnEvent
                                {
                                    Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                                });
                            break;
                        default:
                            session.EventDispatcher.Send(new WarnEvent
                            {
                                Message =
                                    session.Translation.GetTranslation(TranslationString.EncounterProblem, encounter.Status)
                            });
                            break;
                    }

                    if (!Equals(catchablePokemon.ElementAtOrDefault(catchablePokemon.Count() - 1), pokemon))
                    {
                        await Task.Delay(2000, cancellationToken);
                    }
                }
            }
            else
            {
                session.EventDispatcher.Send(new SnipeEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.NoPokemonToSnipe)
                });
            }
            await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude));

            session.EventDispatcher.Send(new SnipeModeEvent { Active = false });

            await Task.Delay(5000, cancellationToken);
        }

        private static async Task<List<MapPokemon>> GetPokemons(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects();

            session.AddForts(mapObjects.Item1.MapCells.SelectMany(p => p.Forts).ToList());
            var pokemons = mapObjects.Item1.MapCells.SelectMany(i => i.CatchablePokemons).ToList();
            return pokemons;
        }

    }

    public class MSniperInfo
    {
        public PokemonId Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
