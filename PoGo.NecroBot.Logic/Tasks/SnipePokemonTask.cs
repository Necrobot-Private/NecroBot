#region using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CloudFlareUtilities;
using GeoCoordinatePortable;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using Quobject.Collections.Immutable;
using Quobject.SocketIoClientDotNet.Client;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;
using PoGo.NecroBot.Logic.Logging;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class SniperInfo
    {
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; }
        public string SpawnPointId { get; set; }
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }

        [JsonIgnore]
        public DateTime TimeStampAdded { get; set; } = DateTime.Now;
    }

    public class PokemonLocation
    {
        public PokemonLocation(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
        }

        public long Id { get; set; }
        public double expires { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int pokemon_id { get; set; }
        public PokemonId pokemon_name { get; set; }

        [JsonIgnore]
        public DateTime TimeStampAdded { get; set; } = DateTime.Now;

        public bool Equals(PokemonLocation obj)
        {
            return Math.Abs(latitude - obj.latitude) < 0.0001 && Math.Abs(longitude - obj.longitude) < 0.0001;
        }

        public override bool Equals(object obj) // contains calls this here
        {
            var p = obj as PokemonLocation;
            if (p == null) // no cast available
            {
                return false;
            }

            return Math.Abs(latitude - p.latitude) < 0.0001 && Math.Abs(longitude - p.longitude) < 0.0001;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return latitude.ToString("0.00000000000") + ", " + longitude.ToString("0.00000000000");
        }
    }

    public class PokemonLocationPokezz
    {
        public double time { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string iv { get; set; }

        public double _iv
        {
            get
            {
                try
                {
                    return Convert.ToDouble(iv, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public PokemonId name { get; set; }
        public bool verified { get; set; }
    }

    public class PokemonLocationPokesnipers
    {
        public int id { get; set; }
        public double iv { get; set; }
        public PokemonId name { get; set; }
        public string until { get; set; }
        public string coords { get; set; }
    }

    public class PokemonLocationPokewatchers
    {
        public PokemonId pokemon { get; set; }
        public double timeadded { get; set; }
        public double timeend { get; set; }
        public string cords { get; set; }
    }

    public class ScanResult
    {
        public string Status { get; set; }
        public List<PokemonLocation> pokemons { get; set; }
    }

    public class ScanResultPokesnipers
    {
        public string Status { get; set; }

        [JsonProperty("results")]
        public List<PokemonLocationPokesnipers> pokemons { get; set; }
    }

    public class ScanResultPokewatchers
    {
        public string Status { get; set; }
        public List<PokemonLocationPokewatchers> pokemons { get; set; }
    }

    public static class SnipePokemonTask
    {
        public static List<PokemonLocation> LocsVisited = new List<PokemonLocation>();
        private static readonly List<SniperInfo> SnipeLocations = new List<SniperInfo>();
        private static DateTime _lastSnipe = DateTime.MinValue;

        public static Task AsyncStart(Session session, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(session, cancellationToken), cancellationToken);
        }

        public static bool CheckPokeballsToSnipe(int minPokeballs, ISession session,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            var pokeBallsCount = session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            pokeBallsCount += session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            pokeBallsCount += session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            pokeBallsCount += session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall);

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

        private static bool CheckSnipeConditions(ISession session)
        {
            if (!session.LogicSettings.UseSnipeLimit) return true;

            session.EventDispatcher.Send(new SnipeEvent
            {
                Message = session.Translation.GetTranslation(TranslationString.SniperCount, session.Stats.SnipeCount)
            });

            if (session.Stats.SnipeCount < session.LogicSettings.SnipeCountLimit)
                return true;

            if ((DateTime.Now - session.Stats.LastSnipeTime).TotalSeconds > session.LogicSettings.SnipeRestSeconds)
            {
                session.Stats.SnipeCount = 0;
            }
            else
            {
                session.EventDispatcher.Send(new SnipeEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.SnipeExceeds)
                });
                return false;
            }
            return true;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (_lastSnipe.AddMilliseconds(session.LogicSettings.MinDelayBetweenSnipes) > DateTime.Now)
                return;

            LocsVisited.RemoveAll(q => DateTime.Now > q.TimeStampAdded.AddMinutes(15));
            SnipeLocations.RemoveAll(x => DateTime.Now > x.TimeStampAdded.AddMinutes(15));

            if (CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsToSnipe, session, cancellationToken))
            {
                var listPokemonToSnipe = new List<PokemonId>();
                foreach (var item in session.LogicSettings.PokemonSnipeFilters)
                {
                    var f = item.Value;
                    listPokemonToSnipe.Add(item.Key);
                    listPokemonToSnipe.AddRange(f.AffectToPokemons);

                }
                if (listPokemonToSnipe.Count > 0)
                {
                    List<PokemonId> pokemonIds;
                    if (session.LogicSettings.SnipePokemonNotInPokedex)
                    {
                        var pokeDex = session.Inventory.GetPokeDexItems();
                        var pokemonOnlyList = listPokemonToSnipe;
                        var capturedPokemon =
                            pokeDex.Where(i => i.InventoryItemData.PokedexEntry.TimesCaptured >= 1)
                                .Select(i => i.InventoryItemData.PokedexEntry.PokemonId);
                        var pokemonToCapture =
                            Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().Except(capturedPokemon);
                        pokemonIds = pokemonOnlyList.Union(pokemonToCapture).ToList();
                    }
                    else
                    {
                        pokemonIds = listPokemonToSnipe;
                    }

                    if (session.LogicSettings.UseSnipeLocationServer)
                    {
                        var locationsToSnipe = SnipeLocations?.Where(q =>
                                q.IV >= session.LogicSettings.PokemonSnipeFilters.GetFilter<SnipeFilter>(q.Id).SnipeIV &&
                                !LocsVisited.Contains(new PokemonLocation(q.Latitude, q.Longitude))
                                && !(q.ExpirationTimestamp != default(DateTime) &&
                                     q.ExpirationTimestamp > new DateTime(2016) &&
                                     // make absolutely sure that the server sent a correct datetime
                                     q.ExpirationTimestamp < DateTime.Now) &&
                                (q.Id == PokemonId.Missingno || pokemonIds.Contains(q.Id)))
                            .ToList();

                        var _locationsToSnipe = locationsToSnipe.OrderBy(q => q.ExpirationTimestamp).ToList();
                        if (_locationsToSnipe.Any())
                        {
                            foreach (var location in _locationsToSnipe)
                            {
                                if (LocsVisited.Contains(new PokemonLocation(location.Latitude, location.Longitude)))
                                    continue;

                                session.EventDispatcher.Send(new SnipeScanEvent
                                {
                                    Bounds = new Location(location.Latitude, location.Longitude),
                                    PokemonId = location.Id,
                                    Source = session.LogicSettings.SnipeLocationServer,
                                    Iv = location.IV
                                });

                                if (
                                    !CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session,
                                        cancellationToken))
                                    return;
                                if (!CheckSnipeConditions(session)) return;
                                await
                                    Snipe(session, pokemonIds, location.Latitude, location.Longitude, cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        public static async Task<bool> Snipe(ISession session, IEnumerable<PokemonId> pokemonIds, double latitude,
            double longitude, CancellationToken cancellationToken)
        {
            //if (LocsVisited.Contains(new PokemonLocation(latitude, longitude)))
            //    return;

            var currentLatitude = session.Client.CurrentLatitude;
            var currentLongitude = session.Client.CurrentLongitude;
            var catchedPokemon = false;

            session.EventDispatcher.Send(new SnipeModeEvent { Active = true });

            List<MapPokemon> catchablePokemon;
            int retry = 3;

            bool isCaptchaShow = false;
            try
            {
                do
                {
                    retry--;
                    LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(latitude, longitude, 10d), 0); // Set speed to 0 for random speed.
                    latitude += 0.00000001;
                    longitude += 0.00000001;

                    session.EventDispatcher.Send(new UpdatePositionEvent
                    {
                        Longitude = longitude,
                        Latitude = latitude
                    });
                    var mapObjects = await session.Client.Map.GetMapObjects(true);
                    catchablePokemon =
                        mapObjects.MapCells.SelectMany(q => q.CatchablePokemons)
                            .Where(q => pokemonIds.Contains(q.PokemonId))
                            .OrderByDescending(pokemon => PokemonInfo.CalculateMaxCp(pokemon.PokemonId))
                            .ToList();
                } while (catchablePokemon.Count == 0 && retry > 0);
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
            finally
            {
                //if(!isCaptchaShow)
                LocationUtils.UpdatePlayerLocationWithAltitude(session,
                    new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude), 0); // Set speed to 0 for random speed.
            }

            if (catchablePokemon.Count == 0)
            {
                // Pokemon not found but we still add to the locations visited, so we don't keep sniping
                // locations with no pokemon.
                if (!LocsVisited.Contains(new PokemonLocation(latitude, longitude)))
                    LocsVisited.Add(new PokemonLocation(latitude, longitude));

                session.EventDispatcher.Send(new SnipeEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.NoPokemonToSnipe),
                });

                session.EventDispatcher.Send(new SnipeFailedEvent
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    PokemonId = pokemonIds.FirstOrDefault()
                });

                return false;
            }

            isCaptchaShow = false;
            foreach (var pokemon in catchablePokemon)
            {
                EncounterResponse encounter;
                try
                {
                    LocationUtils.UpdatePlayerLocationWithAltitude(session,
                        new GeoCoordinate(latitude, longitude, session.Client.CurrentAltitude), 0); // Set speed to 0 for random speed.

                    encounter =
                        session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId).Result;
                }
                catch (HasherException ex) { throw ex; }
                catch (CaptchaException ex)
                {
                    isCaptchaShow = true;
                    throw ex;
                }
                finally
                {
                    if (!isCaptchaShow)
                        LocationUtils.UpdatePlayerLocationWithAltitude(session,
                            // Set speed to 0 for random speed.
                            new GeoCoordinate(currentLatitude, currentLongitude, session.Client.CurrentAltitude), 0);
                }

                switch (encounter.Status)
                {
                    case EncounterResponse.Types.Status.EncounterSuccess:
                        if (!LocsVisited.Contains(new PokemonLocation(latitude, longitude)))
                            LocsVisited.Add(new PokemonLocation(latitude, longitude));

                        //Also add exact pokemon location to LocsVisited, some times the server one differ a little.
                        if (!LocsVisited.Contains(new PokemonLocation(pokemon.Latitude, pokemon.Longitude)))
                            LocsVisited.Add(new PokemonLocation(pokemon.Latitude, pokemon.Longitude));
                        session.EventDispatcher.Send(new UpdatePositionEvent
                        {
                            Latitude = currentLatitude,
                            Longitude = currentLongitude
                        });
                        catchedPokemon = await CatchPokemonTask.Execute(session, cancellationToken, encounter, pokemon,
                            currentFortData: null, sessionAllowTransfer: true);
                        break;

                    case EncounterResponse.Types.Status.PokemonInventoryFull:
                        if (session.LogicSettings.TransferDuplicatePokemon)
                        {
                            await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
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

                if (!Equals(catchablePokemon.ElementAtOrDefault(catchablePokemon.Count - 1), pokemon))
                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch, cancellationToken);
            }

            _lastSnipe = DateTime.Now;

            if (catchedPokemon)
            {
                session.Stats.SnipeCount++;
                session.Stats.LastSnipeTime = _lastSnipe;
            }
            session.EventDispatcher.Send(new SnipeModeEvent { Active = false });
            return true;
            //await Task.Delay(session.LogicSettings.DelayBetweenPlayerActions, cancellationToken);
        }

        public static async Task Start(Session session, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                try
                {
                    var lClient = new TcpClient();
                    lClient.Connect(session.LogicSettings.SnipeLocationServer,
                        session.LogicSettings.SnipeLocationServerPort);

                    var sr = new StreamReader(lClient.GetStream());

                    while (lClient.Connected)
                    {
                        try
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                                throw new Exception("Unable to ReadLine from sniper socket");

                            var info = JsonConvert.DeserializeObject<SniperInfo>(line);

                            if (SnipeLocations.Any(x =>
                                    Math.Abs(x.Latitude - info.Latitude) < 0.0001 &&
                                    Math.Abs(x.Longitude - info.Longitude) < 0.0001))
                                // we might have different precisions from other sources
                                continue;

                            SnipeLocations.RemoveAll(x => _lastSnipe > x.TimeStampAdded);
                            SnipeLocations.RemoveAll(x => DateTime.Now > x.TimeStampAdded.AddMinutes(15));
                            SnipeLocations.Add(info);
                            session.EventDispatcher.Send(new SnipePokemonFoundEvent { PokemonFound = info });
                        }
                        catch (IOException)
                        {
                            session.EventDispatcher.Send(new ErrorEvent
                            {
                                Message = "The connection to the sniping location server was lost."
                            });
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

                await Task.Delay(100, cancellationToken);
            }
        }
    }
}