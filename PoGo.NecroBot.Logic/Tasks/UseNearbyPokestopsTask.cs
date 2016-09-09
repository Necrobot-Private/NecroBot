#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Strategies.Walk;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Event.Gym;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public delegate void UpdateTimeStampsPokestopDelegate();

    public class UseNearbyPokestopsTask
    {
        private static int _stopsHit;
        private static int _randomStop;
        private static Random _rc; //initialize pokestop random cleanup counter first time
        private static int _storeRi;
        private static int _randomNumber;
        private static List<FortData> _pokestopList;
        public static event UpdateTimeStampsPokestopDelegate UpdateTimeStampsPokestop;

        internal static void Initialize()
        {
            _stopsHit = 0;
            _randomStop = 0;
            _rc = new Random();
            _storeRi = _rc.Next(8, 15);
            _randomNumber = _rc.Next(4, 11);
            _pokestopList = new List<FortData>();
        }

        private static bool SearchThresholdExceeds(ISession session)
        {
            if (!session.LogicSettings.UsePokeStopLimit) return false;
            if (session.Stats.PokeStopTimestamps.Count >= session.LogicSettings.PokeStopLimit)
            {
                // delete uesless data
                int toRemove = session.Stats.PokeStopTimestamps.Count - session.LogicSettings.PokeStopLimit;
                if (toRemove > 0)
                {
                    session.Stats.PokeStopTimestamps.RemoveRange(0, toRemove);
                    UpdateTimeStampsPokestop?.Invoke();
                }
                var sec = (DateTime.Now - new DateTime(session.Stats.PokeStopTimestamps.First())).TotalSeconds;
                var limit = session.LogicSettings.PokeStopLimitMinutes * 60;
                if (sec < limit)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.PokeStopExceeds, Math.Round(limit - sec)) });
                    return true;
                }
            }

            return false;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //request map objects to referesh data. keep all fort in session

            var mapObjectTupe = await GetPokeStops(session);
            _pokestopList = mapObjectTupe.Item2;
            var pokeStop = await GetNextPokeStop(session);

            while (pokeStop != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await SnipeMSniperTask.CheckMSniperLocation(session, cancellationToken);

                var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                await WalkingToPokeStop(session, cancellationToken, pokeStop, fortInfo);

                await DoActionAtPokeStop(session, cancellationToken, pokeStop, fortInfo);

                await VisitNearByGymTask.Execute(session, cancellationToken, pokeStop, fortInfo);

                if (session.LogicSettings.SnipeAtPokestops || session.LogicSettings.UseSnipeLocationServer)
                    await SnipePokemonTask.Execute(session, cancellationToken);

                await SnipeMSniperTask.CheckMSniperLocation(session, cancellationToken);

                if (session.LogicSettings.EnableHumanWalkingSnipe)
                {
                    await HumanWalkSnipeTask.Execute(session, cancellationToken, pokeStop);
                }
                pokeStop.CooldownCompleteTimestampMs = DateTime.UtcNow.ToUnixTime() + (pokeStop.Type == FortType.Gym ? session.LogicSettings.GymVisitTimeout : 5) * 60 * 1000; //5 minutes to cooldown
                session.AddForts(new List<FortData>() { pokeStop }); //replace object in memory.
                pokeStop = await GetNextPokeStop(session);
            }

            //await VisitNearByGymTask.UpdateGymList(session, mapObjectTupe.Item2);
            //while (_pokestopList.Any())
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    await SnipeMSniperTask.CheckMSniperLocation(session, cancellationToken);

            //    _pokestopList =
            //        _pokestopList.OrderBy(
            //            i =>
            //                session.Navigation.WalkStrategy.CalculateDistance(
            //                    session.Client.CurrentLatitude, session.Client.CurrentLongitude, i.Latitude, i.Longitude, session)).ToList();

            //    // randomize next pokestop between first and second by distance
            //    var pokestopListNum = 0;
            //    if (_pokestopList.Count > 1)
            //        pokestopListNum = _rc.Next(0, 2);

            //    var pokeStop = _pokestopList[pokestopListNum];
            //    _pokestopList.RemoveAt(pokestopListNum);

            //    // this logic should only be called when we reach a pokestop either via GPX path or normal walking
            //    // as when walk-sniping, we want to get to the snipe ASAP rather than stop for lured pokemon upon
            //    // calling FarmPokestop; in that situation we are also always within 40m of the pokestop, so no
            //    // need to walk to it
            //    var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

            //    // we only move to the PokeStop, and send the associated FortTargetEvent, when not using GPX
            //    // also, GPX pathing uses its own EggWalker and calls the CatchPokemon tasks internally.
            //    if (!session.LogicSettings.UseGpxPathing)
            //    {
            //        // Will modify Lat,Lng and Name to fake position
            //        SetMoveToTargetTask.CheckSetMoveToTargetStatus(ref fortInfo, ref pokeStop);

            //        var eggWalker = new EggWalker(1000, session);

            //        var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
            //            session.Client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
            //        cancellationToken.ThrowIfCancellationRequested();

            //        if (!session.LogicSettings.UseGoogleWalk && !session.LogicSettings.UseYoursWalk)
            //            session.EventDispatcher.Send(new FortTargetEvent { Name = fortInfo.Name, Distance = distance, Route = "NecroBot" });
            //        else
            //            BaseWalkStrategy.FortInfo = fortInfo;

            //        await session.Navigation.Move(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude,
            //            LocationUtils.getElevation(session, pokeStop.Latitude, pokeStop.Longitude)),
            //        async () =>
            //        {
            //            if (SetMoveToTargetTask.CheckStopforSetMoveToTarget())
            //                return false;
            //            // Catch normal map Pokemon
            //            await CatchNearbyPokemonsTask.Execute(session, cancellationToken);
            //            //Catch Incense Pokemon
            //            await CatchIncensePokemonsTask.Execute(session, cancellationToken);
            //            // Minor fix google route ignore pokestop
            //            await LookPokestops(session, pokeStop, cancellationToken);
            //            return true;
            //        },
            //        session,
            //        cancellationToken);

            //        // we have moved this distance, so apply it immediately to the egg walker.
            //        await eggWalker.ApplyDistance(distance, cancellationToken);
            //    }

            //    if (SetMoveToTargetTask.CheckReachTarget(session))
            //        return;

            //    await FortAction(session, pokeStop, fortInfo, cancellationToken);

            //    if (session.LogicSettings.SnipeAtPokestops || session.LogicSettings.UseSnipeLocationServer)
            //        await SnipePokemonTask.Execute(session, cancellationToken);
            //    //samuraitruong: since we has duplication code for gym. I temporary comment this line to disable my feature. keep the code as reference, will remove later.

            //    //await VisitNearByGymTask.Execute(session, cancellationToken);

            //    if (session.LogicSettings.EnableHumanWalkingSnipe)
            //    {
            //        //refactore to move this code inside the task later.
            //        await HumanWalkSnipeTask.Execute(session, cancellationToken,
            //            async (lat, lng) =>
            //            {
            //                //idea of this function is to spin pokestop on way. maybe risky.
            //                var reachablePokestops = _pokestopList.Where(i =>
            //                    LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
            //                        session.Client.CurrentLongitude, i.Latitude, i.Longitude) < 40
            //                        && i.CooldownCompleteTimestampMs == 0
            //                        ).ToList();
            //                reachablePokestops = reachablePokestops.OrderBy(i =>
            //                LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
            //                session.Client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();

            //                foreach (var ps in reachablePokestops)
            //                {
            //                    if (!session.LogicSettings.UseGpxPathing)
            //                        _pokestopList.Remove(ps);
            //                    var fi = await session.Client.Fort.GetFort(ps.Id, ps.Latitude, ps.Longitude);
            //                    await FarmPokestop(session, ps, fi, cancellationToken, true);
            //                    await Task.Delay(2000, cancellationToken);
            //                }
            //            },
            //            async () =>
            //            {
            //                // if using GPX we have to move back to the original pokestop, to resume the path.
            //                // we do not try to use pokest;ops on the way back, as we will have used them getting
            //                // here.
            //                if (session.LogicSettings.UseGpxPathing)
            //                {
            //                    var eggWalker = new EggWalker(1000, session);

            //                    var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
            //                        session.Client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
            //                    var geo = new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude);

            //                    await session.Navigation.Move(geo,
            //                        async () =>
            //                        {
            //                            await CatchNearbyPokemonsTask.Execute(session, cancellationToken);
            //                            //Catch Incense Pokemon
            //                            await CatchIncensePokemonsTask.Execute(session, cancellationToken);
            //                        },
            //                        session,
            //                        cancellationToken);

            //                    await eggWalker.ApplyDistance(distance, cancellationToken);
            //                    return;
            //                }

            //                var nearestStop = _pokestopList.OrderBy(i =>
            //                    LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
            //                        session.Client.CurrentLongitude, i.Latitude, i.Longitude)).FirstOrDefault();

            //                if (nearestStop != null)
            //                {
            //                    var walkedDistance = LocationUtils.CalculateDistanceInMeters(nearestStop.Latitude, nearestStop.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude);
            //                    if (walkedDistance > session.LogicSettings.HumanWalkingSnipeWalkbackDistanceLimit)
            //                    {
            //                        await Task.Delay(3000, cancellationToken);
            //                        var nearbyPokeStops = await UpdateFortsData(session);
            //                        var notexists = nearbyPokeStops.Where(p => _pokestopList.All(x => x.Id != p.Id)).ToList();
            //                        _pokestopList.AddRange(notexists);
            //                        session.EventDispatcher.Send(new PokeStopListEvent { Forts = _pokestopList });
            //                        session.EventDispatcher.Send(new HumanWalkSnipeEvent
            //                        {
            //                            Type = HumanWalkSnipeEventTypes.PokestopUpdated,
            //                            Pokestops = notexists,
            //                            NearestDistance = walkedDistance
            //                        });
            //                    }
            //                }
            //            });
            //    }
            //}
        }

        private static async Task WalkingToPokeStop(ISession session, CancellationToken cancellationToken, FortData pokeStop, FortDetailsResponse fortInfo)
        {
            var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
            
            // we only move to the PokeStop, and send the associated FortTargetEvent, when not using GPX
            // also, GPX pathing uses its own EggWalker and calls the CatchPokemon tasks internally.
            if (!session.LogicSettings.UseGpxPathing)
            {
                // Will modify Lat,Lng and Name to fake position
                //Need refactor it to speparate from pokestop logic -> samuraitruong will do it.
                SetMoveToTargetTask.CheckSetMoveToTargetStatus(ref fortInfo, ref pokeStop);

                var eggWalker = new EggWalker(1000, session);

                cancellationToken.ThrowIfCancellationRequested();

                if (!session.LogicSettings.UseGoogleWalk && !session.LogicSettings.UseYoursWalk)
                    session.EventDispatcher.Send(new FortTargetEvent { Name = fortInfo.Name, Distance = distance, Route = "NecroBot" });
                else
                    BaseWalkStrategy.FortInfo = fortInfo;
                var pokeStopDestination = new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude,
                    LocationUtils.getElevation(session, pokeStop.Latitude, pokeStop.Longitude));

                if (pokeStop.Type == FortType.Gym)
                {
                    session.EventDispatcher.Send(new GymWalkToTargetEvent()
                    {
                        Name = fortInfo.Name,
                        Distance = distance,
                        Latitude = fortInfo.Latitude,
                        Longitude = fortInfo.Longitude
                    });
                }

                await session.Navigation.Move(pokeStopDestination,
                 async () =>
                 {
                     await OnWalkingToPokeStopOrGym(session, pokeStop, cancellationToken);
                 },
                             session,
                             cancellationToken);

                // we have moved this distance, so apply it immediately to the egg walker.
                await eggWalker.ApplyDistance(distance, cancellationToken);
            }
        }
        private static async Task OnWalkingToPokeStopOrGym(ISession session, FortData pokeStop, CancellationToken cancellationToken)
        {
            //TODO - refactore this call to somewhere else
            if (SetMoveToTargetTask.CheckStopforSetMoveToTarget())
            {
                return;
            }

            // Catch normal map Pokemon
            await CatchNearbyPokemonsTask.Execute(session, cancellationToken);
            //Catch Incense Pokemon
            await CatchIncensePokemonsTask.Execute(session, cancellationToken);

            // Minor fix google route ignore pokestop
            if (session.LogicSettings.UseGoogleWalk && !session.LogicSettings.UseYoursWalk && !session.LogicSettings.UseGpxPathing)
            {
                await SpinPokestopNearBy(session, cancellationToken, pokeStop);
            }
        }
        public static async Task<FortData> GetNextPokeStop(ISession session)
        {
            if (session.Forts == null ||
                session.Forts.Count == 0 ||
                session.Forts.Count(p => p.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()) == 0)
            {
                //non pokestop . should we init or return nul?
            };

            var pokeStopes = session.Forts.Where(p => p.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()).ToList();
            pokeStopes = pokeStopes.OrderBy(
                        p =>
                            session.Navigation.WalkStrategy.CalculateDistance(
                                session.Client.CurrentLatitude,
                                session.Client.CurrentLongitude,
                                p.Latitude,
                                p.Longitude,
                                session)
                                ).ToList();
            if (session.LogicSettings.UseGpxPathing)
            {
                pokeStopes = pokeStopes.Where(p => LocationUtils.CalculateDistanceInMeters(p.Latitude, p.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) < 40).ToList();
            }
            if (pokeStopes.Count == 1) return pokeStopes.FirstOrDefault();
            if (session.LogicSettings.GymAllowed)
            {
                var gyms = pokeStopes.Where(x => x.Type == FortType.Gym &&
                LocationUtils.CalculateDistanceInMeters(x.Latitude, x.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) < session.LogicSettings.GymMaxDistance
                && x.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());

                if (gyms.Count() > 0) return gyms.FirstOrDefault();
            }
            return pokeStopes.Skip((int)DateTime.Now.Ticks % 2).FirstOrDefault();
        }

        public static async Task SpinPokestopNearBy(ISession session, CancellationToken cancellationToken, FortData destinationFort = null)
        {
            var allForts = session.Forts.Where(p => p.Type == FortType.Checkpoint).ToList();

            if (allForts.Count > 1)
            {
                var spinablePokestops = allForts.Where(
                    i =>
                        (
                            LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                i.Latitude, i.Longitude) < 40 &&
                                i.CooldownCompleteTimestampMs == 0 &&
                                (destinationFort == null || destinationFort.Id != i.Id))
                ).ToList();

                List<FortData> spinedPokeStops = new List<FortData>();
                if (spinablePokestops.Count >= 1)
                {
                    foreach (var pokeStop in spinablePokestops)
                    {
                        var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                        await FarmPokestop(session, pokeStop, fortInfo, cancellationToken, true);
                        pokeStop.CooldownCompleteTimestampMs = DateTime.UtcNow.ToUnixTime() + 5 * 60 * 1000;
                        spinedPokeStops.Add(pokeStop);
                        if (spinablePokestops.Count > 1)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
                session.AddForts(spinablePokestops);
            }
        }

        private static async Task DoActionAtPokeStop(ISession session, CancellationToken cancellationToken, FortData pokeStop, FortDetailsResponse fortInfo, bool doNotTrySpin = false)
        {
            if (pokeStop.Type != FortType.Checkpoint) return;

            //Catch Lure Pokemon
            if (pokeStop.LureInfo != null)
            {
                // added for cooldowns
                await Task.Delay(Math.Min(session.LogicSettings.DelayBetweenPlayerActions, 3000));
                await CatchLurePokemonsTask.Execute(session, pokeStop, cancellationToken);
            }

            await FarmPokestop(session, pokeStop, fortInfo, cancellationToken, doNotTrySpin);

            if (++_stopsHit >= _storeRi) //TODO: OR item/pokemon bag is full //check stopsHit against storeRI random without dividing.
            {
                _storeRi = _rc.Next(6, 12); //set new storeRI for new random value
                _stopsHit = 0;

                if (session.LogicSettings.UseNearActionRandom)
                {
                    await HumanRandomActionTask.Execute(session, cancellationToken);
                }
                else
                {
                    await RecycleItemsTask.Execute(session, cancellationToken);

                    if (session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                        session.LogicSettings.EvolveAllPokemonAboveIv ||
                        session.LogicSettings.UseLuckyEggsWhileEvolving ||
                        session.LogicSettings.KeepPokemonsThatCanEvolve)
                        await EvolvePokemonTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.UseLuckyEggConstantly)
                        await UseLuckyEggConstantlyTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.UseIncenseConstantly)
                        await UseIncenseConstantlyTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.TransferDuplicatePokemon)
                        await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.TransferWeakPokemon)
                        await TransferWeakPokemonTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.RenamePokemon)
                        await RenamePokemonTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.AutomaticallyLevelUpPokemon)
                        await LevelUpPokemonTask.Execute(session, cancellationToken);

                    await GetPokeDexCount.Execute(session, cancellationToken);
                }
            }
        }

        private static async Task FarmPokestop(ISession session, FortData pokeStop, FortDetailsResponse fortInfo, CancellationToken cancellationToken, bool doNotRetry = false)
        {
            if (pokeStop.CooldownCompleteTimestampMs != 0) return;

            FortSearchResponse fortSearch;
            var timesZeroXPawarded = 0;
            var fortTry = 0; //Current check
            const int retryNumber = 50; //How many times it needs to check to clear softban
            const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (SearchThresholdExceeds(session))
                {
                    break;
                }

                fortSearch =
                    await session.Client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                if (fortSearch.ExperienceAwarded == 0)
                {
                    timesZeroXPawarded++;

                    if (timesZeroXPawarded > zeroCheck)
                    {
                        if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                        {
                            break; // Check if successfully looted, if so program can continue as this was "false alarm".
                        }

                        fortTry += 1;

                        session.EventDispatcher.Send(new FortFailedEvent
                        {
                            Name = fortInfo.Name,
                            Try = fortTry,
                            Max = retryNumber - zeroCheck,
                            Looted = false
                        });
                        if (doNotRetry)
                        {
                            break;
                        }
                        if (!session.LogicSettings.FastSoftBanBypass)
                        {
                            DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
                        }
                    }
                }
                else
                {
                    if (fortTry != 0)
                    {
                        session.EventDispatcher.Send(new FortFailedEvent
                        {
                            Name = fortInfo.Name,
                            Try = fortTry + 1,
                            Max = retryNumber - zeroCheck,
                            Looted = true
                        });
                    }

                    session.EventDispatcher.Send(new FortUsedEvent
                    {
                        Id = pokeStop.Id,
                        Name = fortInfo.Name,
                        Exp = fortSearch.ExperienceAwarded,
                        Gems = fortSearch.GemsAwarded,
                        Items = StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded),
                        Latitude = pokeStop.Latitude,
                        Longitude = pokeStop.Longitude,
                        Altitude = session.Client.CurrentAltitude,
                        InventoryFull = fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull
                    });

                    if (fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull)
                        _storeRi = 1;

                    if (session.LogicSettings.UsePokeStopLimit)
                    {
                        session.Stats.PokeStopTimestamps.Add(DateTime.Now.Ticks);
                        UpdateTimeStampsPokestop?.Invoke();
                        Logger.Write($"(POKESTOP LIMIT) {session.Stats.PokeStopTimestamps.Count}/{session.LogicSettings.PokeStopLimit}",
                            LogLevel.Info, ConsoleColor.Yellow);
                    }
                    break; //Continue with program as loot was succesfull.
                }
            } while (fortTry < retryNumber - zeroCheck);
            //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.

            if (session.LogicSettings.RandomlyPauseAtStops && !doNotRetry)
            {
                if (++_randomStop >= _randomNumber)
                {
                    _randomNumber = _rc.Next(4, 11);
                    _randomStop = 0;
                    int randomWaitTime = _rc.Next(30, 120);
                    await Task.Delay(randomWaitTime, cancellationToken);
                }
            }

        }

        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters for GPX pathing, as we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        //For non GPX pathing, it returns all pokestops in range.
        private static async Task<Tuple<List<FortData>, List<FortData>>> GetPokeStops(ISession session)
        {
            List<FortData> mapObjects = await UpdateFortsData(session);
            session.AddForts(mapObjects);

            if (!session.LogicSettings.UseGpxPathing)
            {
                if (mapObjects.Count <= 0)
                {
                    // only send this for non GPX because otherwise we generate false positives
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.FarmPokestopsNoUsableFound)
                    });
                }

                var pokeStops = mapObjects.Where(p => p.Type == FortType.Checkpoint).ToList();
                session.AddVisibleForts(pokeStops);
                session.EventDispatcher.Send(new PokeStopListEvent { Forts = mapObjects });

                var gyms = mapObjects.Where(p => p.Type == FortType.Gym).ToList();
                //   session.EventDispatcher.Send(new PokeStopListEvent { Forts = mapObjects });
                return Tuple.Create(pokeStops, gyms);
            }

            if (mapObjects.Count > 0)
            {
                // only send when there are stops for GPX because otherwise we send empty arrays often
                session.EventDispatcher.Send(new PokeStopListEvent { Forts = mapObjects });
            }
            // Wasn't sure how to make this pretty. Edit as needed.
            return Tuple.Create(
                mapObjects.Where(
                    i => i.Type == FortType.Checkpoint &&
                        ( // Make sure PokeStop is within 40 meters or else it is pointless to hit it
                            LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                i.Latitude, i.Longitude) < 40) ||
                        session.LogicSettings.MaxTravelDistanceInMeters == 0
                ).ToList(),
                mapObjects.Where(p => p.Type == FortType.Gym && LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                p.Latitude, p.Longitude) < 40).ToList()
                );
        }

        public static async Task<List<FortData>> UpdateFortsData(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects();
            session.AddForts(mapObjects.Item1.MapCells.SelectMany(p => p.Forts).ToList());

            var pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        (i.Type == FortType.Checkpoint || i.Type == FortType.Gym) &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        (
                            LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                i.Latitude, i.Longitude) < session.LogicSettings.MaxTravelDistanceInMeters) ||
                        session.LogicSettings.MaxTravelDistanceInMeters == 0
                );

            return pokeStops.ToList();
        }
    }
}