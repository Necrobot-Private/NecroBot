
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.PoGoUtils;
using WebSocket4Net;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class SnipeMSniperTask
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
            PCount = 2,
            PokemonList = 3,

        }

        public static SocketCmd GetSocketCmd(this MessageReceivedEventArgs e)
        {
            try
            {
                return (SocketCmd)Enum.Parse(typeof(SocketCmd), e.Message.Split(':')[0]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GetSocketData(this MessageReceivedEventArgs e)
        {
            try
            {
                return e.Message.Split(':')[1];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static double minIvPercent = 1.0;

        public static List<EncounterInfo> PkmnLocations = new List<EncounterInfo>();

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
            if (msocket == null || msocket.State == WebSocketState.Closed)
            {
                msocket = new WebSocket("ws://localhost:56000/WebSockets/NecroBotServer.ashx", "basic", WebSocketVersion.Rfc6455);
                msocket.MessageReceived += Msocket_MessageReceived;
                msocket.Closed += Msocket_Closed;
                msocket.Open();
            }
        }

        private static void Msocket_Closed(object sender, EventArgs e)
        {

        }

        private static void Msocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            SocketCmd cmd = e.GetSocketCmd();
            switch (cmd)
            {
                case SocketCmd.Identity:
                    UniequeId = e.GetSocketData();
                    break;

                case SocketCmd.PokemonList:
                    var x = PkmnLocations.GroupBy(p => p.PokemonId)
                        .Select(s => new PokemonCount { PokemonId = s.First().PokemonId, Count = s.Count() })
                        .ToList();
                    msocket.Send(JsonConvert.SerializeObject(x));
                    break;
            }
        }

        private static void SendToMSniperServer(string message)
        {
            try
            {
                msocket.Send($"{UniequeId}:{message}");
            }
            catch (Exception ex)
            {
                msocket.Close();
                throw ex;
            }
        }

        public static void AddToList(EncounterResponse eresponse)
        {
            if (!(PokemonInfo.CalculatePokemonPerfection(eresponse.WildPokemon.PokemonData) >= minIvPercent) &&
                blackList.FindIndex(p => p == eresponse.WildPokemon.PokemonData.PokemonId) != -1 &&
                PkmnLocations.FirstOrDefault(p => p.EncounterId == eresponse.WildPokemon.EncounterId) != null)
                return;
            using (var newdata = new EncounterInfo())
            {
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
