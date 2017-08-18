using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Snipe;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using System.Runtime.Caching;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Inventory.Item;
using GeoCoordinatePortable;
using PoGo.NecroBot.Logic.Model;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class MSniperServiceTask
    {
        #region Variables

        private const int SNIPE_SAFE_TIME = 180;
        public static List<EncounterInfo> LocationQueue = new List<EncounterInfo>();
        public static List<string> VisitedEncounterIds = new List<string>();
        private static List<MSniperInfo2> autoSnipePokemons = new List<MSniperInfo2>();
        private static List<MSniperInfo2> manualSnipePokemons = new List<MSniperInfo2>();
        private static List<MSniperInfo2> pokedexSnipePokemons = new List<MSniperInfo2>();
        private static bool inProgress = false;
        private static DateTime OutOffBallBlock = DateTime.MinValue;
        public static bool isConnected = false;
        public static double minIvPercent = 0.0; //no iv filter
        private static string _botIdentiy;
#pragma warning disable 0649
        private static HubConnection _connection;
        private static IHubProxy _msniperHub;
#pragma warning restore 0649
#pragma warning disable 0414
        private static string _msniperServiceUrl = "https://www.msniper.com/signalr";
#pragma warning restore 0414

        private static List<PokemonId> pokedexList = new List<PokemonId>();
        #endregion Variables

        #region signalr msniper service

        public static void ConnectToService()
        {
            //TODO - remove this line after MSniper.com back to work
            return;
            /*
            while (true)
            {
                try
                {
                    if (!isConnected)
                    {
                        Thread.Sleep(10000);
                        _connection = new HubConnection(_msniperServiceUrl, useDefaultUrl: false);
                        X509Certificate2 sertifika = new X509Certificate2();
                        sertifika.Import(Properties.Resources.msvc);
                        _connection.AddClientCertificate(sertifika);
                        _msniperHub = _connection.CreateHubProxy("msniperHub");
                        _msniperHub.On<MSniperInfo2>("msvc", p =>
                        {
                            using (await locker.LockAsync().ConfigureAwait(false))
                            {
                                autoSnipePokemons.Add(p);
                            }
                        });
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
                catch (CaptchaException cex)
                {
                    throw cex;
                }
                catch (Exception)
                {
                    //Logger.Write("service: " +e.Message, LogLevel.Error);
                    Thread.Sleep(500);
                }
            }
            */
        }

        private static void Connection_Closed()
        {
            //Logger.Write("connection closed, trying to reconnect in 10secs", LogLevel.Service);
            ConnectToService();
        }

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
                            //Logger.Write($"pokemons are sending.. {LocationQueue.Count} count", LogLevel.Service);
                            var findingSendables = FindNew(LocationQueue);
                            AddToVisited(findingSendables);
                            _msniperHub.Invoke("RecvPokemons", findingSendables);
                            findingSendables.ForEach(p => { LocationQueue.Remove(p); });
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

        private static void Connection_Reconnecting()
        {
            isConnected = false;
            _connection.Stop(); //removing server cache
            ConnectToService();
            //Logger.Write("reconnecting", LogLevel.Service);
        }

        //private static void Connection_Reconnected()
        //{
        //    Logger.Write("reconnected", LogLevel.Service);
        //}

        #endregion signalr msniper service

        #region Classes

        public class EncounterInfo : IEvent
        {
            public string EncounterId { get; set; }
            public long Expiration { get; set; }
            public double Iv { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string Move1 { get; set; }
            public string Move2 { get; set; }
            public int PokemonId { get; set; }
            public string PokemonName { get; set; }

            public string SpawnPointId { get; set; }
            //public long LastModifiedTimestampMs { get; set; }
            //public int TimeTillHiddenMs { get; set; }

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

            public PokemonId GetPokemonName()
            {
                return (PokemonId)PokemonId;
            }
        }

        private static bool isBlocking = true; //turn it on when account switching, do not add or run snipe

        public static void BlockSnipe()
        {
            pokedexList = new List<PokemonId>();
            isBlocking = true;
        }

        public class HubData
        {
            [JsonProperty("H")]
            public string HubName { get; set; }

            [JsonProperty("A")]
            public List<string> List { get; set; }

            [JsonProperty("M")]
            public string Method { get; set; }
        }

        public class MSniperInfo2
        {
            public string UniqueIdentifier { get; set; }
            public DateTime AddedTime { get; set; }
            public ulong EncounterId { get; set; }
            public double ExpiredTime { get; set; }
            public double Iv { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public PokemonMove Move1 { get; set; }
            public PokemonMove Move2 { get; set; }
            public short PokemonId { get; set; }
            public string SpawnPointId { get; set; }
            public int Priority { get; set; }
            public int Level { get; set; }
            public bool IsVerified()
            {
                return EncounterId > 0 && SpawnPointId.IndexOf('-') < 0;
            }
        }

        #endregion Classes

        #region MSniper Location Feeder

        public static void AddToList(IEvent evt)
        {
            if (evt is EncounterInfo)
            {
                var xx = TimeStampToDateTime((evt as EncounterInfo).Expiration);
                var ff = DateTime.Now;
                if ((ff - xx).TotalMinutes < 1)
                {
                    (evt as EncounterInfo).Expiration += 500000;
                    LocationQueue.Add(evt as EncounterInfo);
                }
                else
                {
                    //we need exact expiry time,so here is disabled
                }
            }
        }

        public static void AddToVisited(List<EncounterInfo> encounterIds)
        {
            encounterIds.ForEach(p =>
            {
                string query = $"{p.EncounterId}-{p.SpawnPointId}";
                if (!VisitedEncounterIds.Contains(query))
                    VisitedEncounterIds.Add(query);
            });
        }

        public static void UnblockSnipe(bool spinned = true)
        {
            isBlocking = false; //block release whenever first pokestop looted.

            snipeFailedCount = 0;
            waitNextPokestop = spinned;
        }

        private static DateTime lastPrintMessageTime = DateTime.Now;

        private static bool CheckSnipeConditions(ISession session)
        {
            if (session.SaveBallForByPassCatchFlee) return false;

            //if (waitNextPokestop) return false;
            if (session.LoggedTime > DateTime.Now.AddMinutes(1)) return false; //only snipe after login 1 min.

            if (snipeFailedCount >= 3) return false;
            if (session.Stats.CatchThresholdExceeds(session)) return false;

            if (inProgress || OutOffBallBlock > DateTime.Now)
                return false;

            if (!session.LogicSettings.UseSnipeLimit) return true;

            if (lastPrintMessageTime.AddMinutes(1) > DateTime.Now)
            {
                session.EventDispatcher.Send(new SnipeEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.SniperCount, session.Stats.SnipeCount)
                });
            }
            if (session.Stats.LastSnipeTime.AddMilliseconds(session.LogicSettings.MinDelayBetweenSnipes) > DateTime.Now)
                return false;

            if (session.Stats.SnipeCount < session.LogicSettings.SnipeCountLimit)
                return true;

            if ((DateTime.Now - session.Stats.LastSnipeTime).TotalSeconds > session.LogicSettings.SnipeRestSeconds)
            {
                session.Stats.SnipeCount = 0;
            }
            else
            {
                if (lastPrintMessageTime.AddMinutes(1) > DateTime.Now)
                {
                    lastPrintMessageTime = DateTime.Now;
                    session.EventDispatcher.Send(new SnipeEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.SnipeExceeds)
                    });
                }
                return false;
            }
            return true;
        }

        private static MemoryCache expiredCache = new MemoryCache("expired");

        public static void RemoveExpiredSnipeData(ISession session, string encounterId)
        {
            lock (expiredCache)
            {
                expiredCache.Add(encounterId, DateTime.Now, DateTime.Now.AddMinutes(15));
            }

            lock (autoSnipePokemons)
            {
                var find = autoSnipePokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounterId);
                if (find != null)
                {
                    session.EventDispatcher.Send(new SnipePokemonUpdateEvent(encounterId, true, find));
                    autoSnipePokemons.RemoveAll(x => x.EncounterId.ToString() == encounterId);
                }
            }

            lock (manualSnipePokemons)
            {
                var find = manualSnipePokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounterId);

                if (find != null)
                {
                    session.EventDispatcher.Send(new SnipePokemonUpdateEvent(encounterId, true, find));
                    manualSnipePokemons.RemoveAll(x => x.EncounterId.ToString() == encounterId);
                }
            }

            lock (pokedexSnipePokemons)
            {
                var find = pokedexSnipePokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounterId);

                if (find != null)
                {
                    session.EventDispatcher.Send(new SnipePokemonUpdateEvent(encounterId, true, find));
                    pokedexSnipePokemons.RemoveAll(x => x.EncounterId.ToString() == encounterId);
                }
            }
        }

        private static async Task ActionsWhenTravelToSnipeTarget(ISession session, CancellationToken cancellationToken,
            IGeoLocation pokemon, bool allowCatchPokemon, bool allowSpinPokeStop)
        {
            var distance = LocationUtils.CalculateDistanceInMeters(
                pokemon.Latitude,
                pokemon.Longitude,
                session.Client.CurrentLatitude,
                session.Client.CurrentLongitude
            );

            if (allowCatchPokemon && distance > 50.0)
            {
                // Catch normal map Pokemon
                await CatchNearbyPokemonsTask.Execute(session, cancellationToken, sessionAllowTransfer: false).ConfigureAwait(false);
            }
            if (allowSpinPokeStop)
            {
                //looking for neaby pokestop. spin it
                await UseNearbyPokestopsTask.SpinPokestopNearBy(session, cancellationToken, null).ConfigureAwait(false);
            }
        }

        public static async Task<bool> SnipeUnverifiedPokemon(ISession session, MSniperInfo2 sniperInfo, CancellationToken cancellationToken)
        {
            var latitude = sniperInfo.Latitude;
            var longitude = sniperInfo.Longitude;
            
            var originalLatitude = session.Client.CurrentLatitude;
            var originalLongitude = session.Client.CurrentLongitude;

            var catchedPokemon = false;

            session.EventDispatcher.Send(new SnipeModeEvent { Active = true });

            MapPokemon catchablePokemon;
            int retry = 3;

            bool useWalk = session.LogicSettings.EnableHumanWalkingSnipe;

            try
            {
                var distance = LocationUtils.CalculateDistanceInMeters(new GeoCoordinate(session.Client.CurrentLatitude, session.Client.CurrentLongitude), new GeoCoordinate(latitude, longitude));
                
                if (useWalk)
                {
                    Logger.Write($"Walking to snipe target. Distance: {distance}", LogLevel.Info);

                    await session.Navigation.Move(
                            new MapLocation(latitude, longitude, 0),
                            async () =>
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await ActionsWhenTravelToSnipeTarget(session, cancellationToken, new MapLocation(latitude, longitude, 0), session.LogicSettings.HumanWalkingSnipeCatchPokemonWhileWalking, session.LogicSettings.HumanWalkingSnipeSpinWhileWalking).ConfigureAwait(false);
                            },
                            session,
                            cancellationToken,
                            session.LogicSettings.HumanWalkingSnipeAllowSpeedUp ? session.LogicSettings.HumanWalkingSnipeMaxSpeedUpSpeed : 200
                        ).ConfigureAwait(false);
                }
                else
                {
                    Logger.Write($"Jumping to snipe target. Distance: {distance}", LogLevel.Info);

                    await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(latitude, longitude, 10d), 0).ConfigureAwait(false); // Set speed to 0 for random speed.

                    session.EventDispatcher.Send(new UpdatePositionEvent
                    {
                        Latitude = latitude,
                        Longitude = longitude
                    });
                }

                try
                {
                    do
                    {
                        retry--;

                        var mapObjects = await session.Client.Map.GetMapObjects(true, false).ConfigureAwait(false);

                        catchablePokemon =
                            mapObjects.MapCells.SelectMany(q => q.CatchablePokemons)
                                .Where(q => sniperInfo.PokemonId == (short)q.PokemonId)
                                .OrderByDescending(pokemon => PokemonInfo.CalculateMaxCp(pokemon.PokemonId))
                                .FirstOrDefault();
                    } while (catchablePokemon == null && retry > 0);
                }
                catch (HasherException ex) { throw ex; }
                catch (CaptchaException ex)
                {
                    throw ex;
                }
                catch (Exception e)
                {
                    Logger.Write($"Error: {e.Message}", LogLevel.Error);
                    throw e;
                }

                if (catchablePokemon == null)
                {
                    session.EventDispatcher.Send(new SnipeEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.NoPokemonToSnipe),
                    });

                    session.EventDispatcher.Send(new SnipeFailedEvent
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        PokemonId = (PokemonId)sniperInfo.PokemonId,
                        EncounterId = sniperInfo.EncounterId
                    });

                    return false;
                }

                if (catchablePokemon != null)
                {
                    EncounterResponse encounter;
                    try
                    {
                        await LocationUtils.UpdatePlayerLocationWithAltitude(session,
                            new GeoCoordinate(catchablePokemon.Latitude, catchablePokemon.Longitude, session.Client.CurrentAltitude), 0).ConfigureAwait(false); // Set speed to 0 for random speed.

                        encounter =
                            await session.Client.Encounter.EncounterPokemon(catchablePokemon.EncounterId, catchablePokemon.SpawnPointId).ConfigureAwait(false);
                    }
                    catch (HasherException ex) { throw ex; }
                    catch (CaptchaException ex)
                    {
                        throw ex;
                    }

                    switch (encounter.Status)
                    {
                        case EncounterResponse.Types.Status.EncounterSuccess:
                            catchedPokemon = await CatchPokemonTask.Execute(session, cancellationToken, encounter, catchablePokemon,
                                currentFortData: null, sessionAllowTransfer: true).ConfigureAwait(false);
                            break;

                        case EncounterResponse.Types.Status.PokemonInventoryFull:
                            if (session.LogicSettings.TransferDuplicatePokemon)
                            {
                                await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                session.EventDispatcher.Send(new WarnEvent
                                {
                                    Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                                });
                            }
                            return false;

                        default:
                            session.EventDispatcher.Send(new WarnEvent
                            {
                                Message =
                                    session.Translation.GetTranslation(
                                        TranslationString.EncounterProblem, encounter.Status)
                            });
                            break;
                    }

                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch, cancellationToken).ConfigureAwait(false);
                }

                if (catchedPokemon)
                {
                    session.Stats.SnipeCount++;
                }
                session.EventDispatcher.Send(new SnipeModeEvent { Active = false });
                return true;
            }
            finally
            {
                if (useWalk)
                {
                    Logger.Write($"Walking back to original location.", LogLevel.Info);

                    await session.Navigation.Move(
                        new MapLocation(originalLatitude, originalLongitude, 0),
                        async () =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await ActionsWhenTravelToSnipeTarget(session, cancellationToken, new MapLocation(latitude, longitude, 0), session.LogicSettings.HumanWalkingSnipeCatchPokemonWhileWalking, session.LogicSettings.HumanWalkingSnipeSpinWhileWalking).ConfigureAwait(false);
                        },
                        session,
                        cancellationToken,
                        session.LogicSettings.HumanWalkingSnipeAllowSpeedUp ? session.LogicSettings.HumanWalkingSnipeMaxSpeedUpSpeed : 200
                    ).ConfigureAwait(false);
                }
                else
                {
                    Logger.Write($"Jumping back to original location.", LogLevel.Info);

                    await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(originalLatitude, originalLongitude), 0).ConfigureAwait(false); // Set speed to 0 for random speed.

                    session.EventDispatcher.Send(new UpdatePositionEvent
                    {
                        Latitude = originalLatitude,
                        Longitude = originalLongitude
                    });

                    await session.Client.Map.GetMapObjects(true).ConfigureAwait(false);
                }
            }
        }

        // CatchFromService no longer works.
        /*
        public static async Task<bool> CatchFromService(ISession session,
            CancellationToken cancellationToken, MSniperInfo2 encounterId)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double originalLat = session.Client.CurrentLatitude;
            double originalLng = session.Client.CurrentLongitude;

            EncounterResponse encounter;
            try
            {
                // Speed set to 0 for random speed.
                await LocationUtils.UpdatePlayerLocationWithAltitude(
                    session,
                    new GeoCoordinate(encounterId.Latitude, encounterId.Longitude, session.Client.CurrentAltitude),
                    0
                ).ConfigureAwait(false);


                await session.Client.Misc.RandomAPICall().ConfigureAwait(false);

                encounter = await session.Client.Encounter.EncounterPokemon(encounterId.EncounterId, encounterId.SpawnPointId).ConfigureAwait(false);

                if (encounter != null && encounter.Status != EncounterResponse.Types.Status.EncounterSuccess)
                {
                    Logger.Debug($"{encounter}");
                }
                //pokemon has expired, send event to remove it.
                if (encounter != null && (encounter.Status == EncounterResponse.Types.Status.EncounterClosed ||
                    encounter.Status == EncounterResponse.Types.Status.EncounterNotFound))
                {
                    session.EventDispatcher.Send(new SnipePokemonUpdateEvent(encounterId.EncounterId.ToString(), false, null));
                }
            }
            catch (CaptchaException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                session.Client.Player.SetCoordinates(originalLat, originalLng, session.Client.CurrentAltitude); //only reset d
            }

            if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
            {
                Logger.Write("Pokemon bag full, snipe cancel");
                await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                return false;
            }

            if (encounter.Status == EncounterResponse.Types.Status.EncounterClosed)
            {
                Logger.Write("This pokemon has been expired");
                return true;
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
                return false;
                //return await CatchWithSnipe(session, encounterId, cancellationToken).ConfigureAwait(false);// No success to work with
            }

            var pokemon = new MapPokemon
            {
                EncounterId = encounterId.EncounterId,
                Latitude = encounterId.Latitude,
                Longitude = encounterId.Longitude,
                PokemonId = encounteredPokemon.PokemonId,
                SpawnPointId = encounterId.SpawnPointId
            };

            return await CatchPokemonTask.Execute(
                session, cancellationToken, encounter, pokemon, currentFortData: null, sessionAllowTransfer: true
            ).ConfigureAwait(false);
        }
        */

        public static List<EncounterInfo> FindNew(List<EncounterInfo> received)
        {
            List<EncounterInfo> newOne = new List<EncounterInfo>();
            received.ForEach(p =>
            {
                if (!VisitedEncounterIds.Contains($"{p.EncounterId}-{p.SpawnPointId}"))
                    newOne.Add(p);
            });
            return newOne;
        }

        public static DateTime TimeStampToDateTime(double timeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(timeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }

        private static void RefreshLocationQueue()
        {
            var pkmns = LocationQueue
                .Where(p => TimeStampToDateTime(p.Expiration) > DateTime.Now)
                .ToList();
            LocationQueue.Clear();
            LocationQueue.AddRange(pkmns);
        }

        #endregion MSniper Location Feeder

        private static AsyncLock locker = new AsyncLock();

        public static async Task<bool> AddSnipeItem(ISession session, MSniperInfo2 item, bool byPassValidation = false)
        {
            if (isBlocking) return false;
            //this pokemon has been recorded as expires
            if (item.EncounterId > 0 && expiredCache.Get(item.EncounterId.ToString()) != null) return false;

            //fake & annoy data
            if (Math.Abs(item.Latitude) > 90 || Math.Abs(item.Longitude) > 180 || item.Iv > 100) return false;

            using (await locker.LockAsync().ConfigureAwait(false))
            {
                Func<MSniperInfo2, bool> checkExisting = (MSniperInfo2 x) =>
                {
                    return (x.EncounterId > 0 && x.EncounterId == item.EncounterId) ||
                    (x.EncounterId == 0 && Math.Round(x.Latitude, 6) == Math.Round(item.Latitude, 6)
                                         && Math.Round(x.Longitude, 6) == Math.Round(item.Longitude, 6)
                                         && x.PokemonId == item.PokemonId);
                };

                //remove existing item that
                autoSnipePokemons.RemoveAll(x => checkExisting(x));
                pokedexSnipePokemons.RemoveAll(x => checkExisting(x));
                manualSnipePokemons.RemoveAll(x => checkExisting(x));
            }

            if (!byPassValidation &&
                session.LogicSettings.AutoSnipeMaxDistance > 0 &&
                LocationUtils.CalculateDistanceInMeters(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude, item.Latitude, item.Longitude) > session.LogicSettings.AutoSnipeMaxDistance * 1000) return false;

            using (await locker.LockAsync().ConfigureAwait(false))
            {
                item.AddedTime = DateTime.Now;
                //just keep pokemon in last 2 min
                autoSnipePokemons.RemoveAll(x => x.AddedTime.AddSeconds(SNIPE_SAFE_TIME) < DateTime.Now);
                pokedexSnipePokemons.RemoveAll(x => x.AddedTime.AddMinutes(SNIPE_SAFE_TIME) < DateTime.Now);
            }
            if (OutOffBallBlock > DateTime.Now ||
                autoSnipePokemons.Exists(x => x.EncounterId == item.EncounterId && item.EncounterId > 0) ||
                (item.EncounterId > 0 && session.Cache[CatchPokemonTask.GetEncounterCacheKey(item.EncounterId)] != null)) return false;

            item.Iv = Math.Round(item.Iv, 2);
            if (session.LogicSettings.SnipePokemonNotInPokedex)
            {
                //sometime the API return pokedex not correct, we need cahe this list, need lean everyetime peopellogi
                var pokedex = (await session.Inventory.GetPokeDexItems().ConfigureAwait(false)).Select(x => x.InventoryItemData?.PokedexEntry?.PokemonId).Where(x => x != null).ToList();
                var update = pokedex.Where(x => !pokedexList.Contains(x.Value)).ToList();

                pokedexList.AddRange(update.Select(x => x.Value));

                //Logger.Debug($"Pokedex Entry : {pokedexList.Count()}");

                if (pokedexList.Count > 0 &&
                    !pokedexList.Exists(x => x == (PokemonId)item.PokemonId) &&
                    !pokedexSnipePokemons.Exists(p => p.PokemonId == item.PokemonId) &&
                    (!session.LogicSettings.AutosnipeVerifiedOnly ||
                     (session.LogicSettings.AutosnipeVerifiedOnly && item.IsVerified())))
                {
                    session.EventDispatcher.Send(new WarnEvent()
                    {
                        Message = session.Translation.GetTranslation(TranslationString.SnipePokemonNotInPokedex,
                            session.Translation.GetPokemonTranslation((PokemonId)item.PokemonId))
                    });
                    item.Priority = 0;
                    pokedexSnipePokemons.Add(item); //Add as hight priority snipe entry
                    return true;
                }
            }
            var pokemonId = (PokemonId)item.PokemonId;
            SnipeFilter filter = session.LogicSettings.PokemonSnipeFilters.GetFilter<SnipeFilter>(pokemonId);

            using (await locker.LockAsync().ConfigureAwait(false))
            {
                if (byPassValidation)
                {
                    item.Priority = -1;
                    manualSnipePokemons.Add(item);

                    Logger.Write($"(MANUAL SNIPER) Pokemon added |  {(PokemonId)item.PokemonId} [{item.Latitude},{item.Longitude}] IV {item.Iv}%");
                    return true;
                }

                item.Priority = filter.Priority;

                if (filter.VerifiedOnly && item.EncounterId == 0) return false;

                //check candy
                int candy = await session.Inventory.GetCandyCount(pokemonId).ConfigureAwait(false);
                if (candy < filter.AutoSnipeCandy)
                {
                    autoSnipePokemons.Add(item);
                    return true;
                }

                if (filter.IsMatch(item.Iv, item.Move1, item.Move2, item.Level, item.EncounterId > 0))
                {
                    autoSnipePokemons.Add(item);
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> CatchWithSnipe(ISession session, MSniperInfo2 sniperInfo,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            return await SnipeUnverifiedPokemon(session, sniperInfo, cancellationToken).ConfigureAwait(false);
        }

        private static int snipeFailedCount = 0;
        private static bool waitNextPokestop = true;

        public static async Task<bool> CheckPokeballsToSnipe(int minPokeballs, ISession session,
           CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall).ConfigureAwait(false);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall).ConfigureAwait(false);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall).ConfigureAwait(false);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall).ConfigureAwait(false);

            if (pokeBallsCount >= minPokeballs)
                return true;

            session.EventDispatcher.Send(new SnipeEvent
            {
                Message =
                    session.Translation.GetTranslation(TranslationString.NotEnoughPokeballsToSnipe, pokeBallsCount,
                        minPokeballs)
            });

            return false;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (!CheckSnipeConditions(session)) return;

            inProgress = true;
            double originalLatitude = session.Client.CurrentLatitude;
            double originalLongitude = session.Client.CurrentLongitude;
            session.KnownLatitudeBeforeSnipe = originalLatitude;
            session.KnownLongitudeBeforeSnipe = originalLongitude;

            //Logger.Write($"DEBUG : Location before snipe : {originalLatitude},{originalLongitude}");

            var pth = Path.Combine(Directory.GetCurrentDirectory(), "SnipeMS.json");
            try
            {
                if (OutOffBallBlock > DateTime.Now || (
                        File.Exists(pth) && autoSnipePokemons.Count == 0 && manualSnipePokemons.Count == 0 &&
                        pokedexSnipePokemons.Count == 0))
                {
                    return;
                }

                if (autoSnipePokemons.Count > 0 && !(await CheckPokeballsToSnipe(
                        session.LogicSettings.MinPokeballsToSnipe + 1, session, cancellationToken).ConfigureAwait(false)))
                {
                    session.EventDispatcher.Send(new WarnEvent()
                    {
                        Message = session.Translation.GetTranslation(TranslationString.AutoSnipeDisabled,
                            session.LogicSettings.SnipePauseOnOutOfBallTime)
                    });

                    OutOffBallBlock = DateTime.Now.AddMinutes(session.LogicSettings.SnipePauseOnOutOfBallTime);
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
                using (await locker.LockAsync().ConfigureAwait(false))
                {
                    if (pokedexSnipePokemons.Count > 0)
                    {
                        mSniperLocation2.Add(pokedexSnipePokemons.OrderByDescending(x => x.PokemonId).FirstOrDefault());
                        pokedexSnipePokemons.Clear();
                    }
                    if (manualSnipePokemons.Count > 0)
                    {
                        mSniperLocation2.AddRange(manualSnipePokemons);
                        manualSnipePokemons.Clear();
                    }
                    else
                    {
                        autoSnipePokemons.RemoveAll(x => x.AddedTime.AddSeconds(SNIPE_SAFE_TIME) < DateTime.Now);
                        // || ( x.ExpiredTime >0 && x.ExpiredTime < DateTime.Now.ToUnixTime()));
                        autoSnipePokemons.OrderBy(x => x.Priority)
                            .ThenByDescending(x => PokemonGradeHelper.GetPokemonGrade((PokemonId)x.PokemonId))
                            .ThenByDescending(x => x.Iv)
                            .ThenByDescending(x => x.PokemonId)
                            .ThenByDescending(x => x.AddedTime);

                        var batch = autoSnipePokemons.Take(session.LogicSettings.AutoSnipeBatchSize);
                        if (batch != null && batch.Count() > 0)
                        {
                            mSniperLocation2.AddRange(batch);
                            autoSnipePokemons.RemoveAll(x => batch.Contains(x));
                        }
                    }
                }
                foreach (var location in mSniperLocation2)
                {
                    if (session.Stats.CatchThresholdExceeds(session) || isBlocking) break;
                    using (await locker.LockAsync().ConfigureAwait(false))
                    {
                        if (location.EncounterId > 0 && expiredCache.Get(location.EncounterId.ToString()) != null) continue;

                        if (pokedexSnipePokemons.Count > 0 || manualSnipePokemons.Count > 0)
                        {
                            break;
                            //should return item back to snipe list
                        }
                    }
                    session.EventDispatcher.Send(new SnipePokemonStarted(location));

                    if (location.EncounterId > 0 && session.Cache[CatchPokemonTask.GetEncounterCacheKey(location.EncounterId)] != null) continue;

                    if (!(await CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session, cancellationToken).ConfigureAwait(false)))
                    {
                        session.EventDispatcher.Send(new WarnEvent()
                        {
                            Message = session.Translation.GetTranslation(TranslationString.AutoSnipeDisabled)
                        });

                        OutOffBallBlock = DateTime.Now.AddMinutes(session.LogicSettings.SnipePauseOnOutOfBallTime);
                        break;
                    }

                    if (location.AddedTime.AddSeconds(SNIPE_SAFE_TIME) < DateTime.Now) continue;

                    //If bot already catch the same pokemon, and very close this location.
                    if (session.Cache.Get(CatchPokemonTask.GetUsernameGeoLocationCacheKey(session.Settings.Username, (PokemonId)location.PokemonId, location.Latitude, location.Longitude)) != null) continue;

                    session.Cache.Add(CatchPokemonTask.GetEncounterCacheKey(location.EncounterId), true, DateTime.Now.AddMinutes(15));

                    cancellationToken.ThrowIfCancellationRequested();
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                    session.EventDispatcher.Send(new SnipeScanEvent
                    {
                        Bounds = new Location(location.Latitude, location.Longitude),
                        PokemonId = (PokemonId)location.PokemonId,
                        Source = "InternalSnipe",
                        Iv = location.Iv
                    });

                    session.Stats.IsSnipping = true;
                    var result = await CatchWithSnipe(session, location, cancellationToken).ConfigureAwait(false);
                    
                    if (result)
                    {
                        snipeFailedCount = 0;
                    }
                    else
                    {
                        snipeFailedCount++;
                        if (snipeFailedCount >= 3) break; //maybe softban, stop snipe wait until verify it not been
                    }
                    //await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    session.Stats.LastSnipeTime = DateTime.Now;
                    session.Stats.SnipeCount++;
                    waitNextPokestop = true;
                }
            }
            catch (ActiveSwitchByPokemonException ex) { throw ex; }
            catch (ActiveSwitchAccountManualException ex)
            {
                throw ex;
            }
            catch (ActiveSwitchByRuleException ex)
            {
                throw ex;
            }
            catch (CaptchaException cex)
            {
                throw cex;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is CaptchaException) throw ex.InnerException;

                File.Delete(pth);
                var ee = new ErrorEvent { Message = ex.Message };
                if (ex.InnerException != null) ee.Message = ex.InnerException.Message;
                session.EventDispatcher.Send(ee);
            }
            finally
            {
                inProgress = false;
                session.Stats.IsSnipping = false;
                //Logger.Write($"DEBUG : Back to home location: {originalLatitude},{originalLongitude}");

                await LocationUtils.UpdatePlayerLocationWithAltitude(session,new GeoCoordinate(originalLatitude, originalLongitude),0).ConfigureAwait(false);

            }
        }
    }
}