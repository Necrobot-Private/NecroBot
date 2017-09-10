#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Model.Settings;
using TinyIoC;
using PokemonGo.RocketAPI.Util;
using POGOProtos.Inventory.Item;
using GeoCoordinatePortable;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseNearbyPokestopsTask
    {
        //add delegate
        public delegate void LootPokestopDelegate(FortData pokestop);

        private static int _stopsHit;
        private static int _randomStop;
        private static Random _rc; //initialize pokestop random cleanup counter first time
        private static int _storeRi;
        private static int _randomNumber;
        public static bool _pokestopLimitReached;
        public static bool _pokestopTimerReached;
        //private static double lastPokestopLat =0;
        //private static double lastPokestopLng = 0;

        internal static void Initialize()
        {
            _stopsHit = 0;
            _randomStop = 0;
            _rc = new Random();
            _storeRi = _rc.Next(8, 15);
            _randomNumber = _rc.Next(4, 11);
            _pokestopLimitReached = false;
            _pokestopTimerReached = false;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            //request map objects to referesh data. keep all fort in session

            var mapObjectTupe = await GetPokeStops(session).ConfigureAwait(false);
            var pokeStop = await GetNextPokeStop(session).ConfigureAwait(false);

            while (pokeStop != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Exit this task if both catching and looting has reached its limits
                await CheckLimit(session).ConfigureAwait(false);

                var fortInfo = pokeStop.Id.StartsWith(SetMoveToTargetTask.TARGET_ID) ? SetMoveToTargetTask.FakeFortInfo(pokeStop) : await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude).ConfigureAwait(false);
                await WalkingToPokeStop(session, cancellationToken, pokeStop, fortInfo).ConfigureAwait(false);

                await DoActionAtPokeStop(session, cancellationToken, pokeStop, fortInfo).ConfigureAwait(false);

                try // Try to fix Error: System.NullReferenceException
                {
                    bool gymAttackSucceeded = await UseGymBattleTask.Execute(session, cancellationToken, pokeStop, fortInfo).ConfigureAwait(false);

                    var _fortstate = new POGOProtos.Data.Gym.GymState()
                    {
                        FortData = pokeStop
                    };


                    if (gymAttackSucceeded &&
                        fortInfo.Type == FortType.Gym &&
                        (_fortstate.FortData.OwnedByTeam == session.Profile.PlayerData.Team || session.GymState.CapturedGymId.Equals(fortInfo.FortId)) &&
                        session.LogicSettings.GymConfig.Enable &&
                        session.LogicSettings.GymConfig.EnableGymTraining)
                    {
                        if (string.IsNullOrEmpty(session.GymState.TrainingGymId) || !session.GymState.TrainingGymId.Equals(fortInfo.FortId))
                        {
                            session.GymState.TrainingGymId = fortInfo.FortId;
                            session.GymState.TrainingRound = 0;
                        }
                        session.GymState.TrainingRound++;
                        if (session.GymState.TrainingRound <= session.LogicSettings.GymConfig.MaxTrainingRoundsOnOneGym)
                            continue;
                    }
                }
                catch
                {
                    Logger.Write("Retry waiting, gym check please wait ...", LogLevel.Gym);
                    return;
                }

                if (!await SetMoveToTargetTask.IsReachedDestination(pokeStop, session, cancellationToken).ConfigureAwait(false))
                {
                    pokeStop.CooldownCompleteTimestampMs = DateTime.UtcNow.ToUnixTime() + (pokeStop.Type == FortType.Gym ? session.LogicSettings.GymConfig.VisitTimeout : 5) * 60 * 1000; //5 minutes to cooldown
                    session.AddForts(new List<FortData>() { pokeStop }); //replace object in memory.
                }

                await MSniperServiceTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (session.LogicSettings.EnableHumanWalkingSnipe)
                {
                    await HumanWalkSnipeTask.Execute(session, cancellationToken, pokeStop, fortInfo).ConfigureAwait(false);
                }

                pokeStop = await GetNextPokeStop(session).ConfigureAwait(false);
            }
        }

        private static async Task CheckLimit(ISession session)
        {
            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            bool allowSwitch = manager.AllowSwitch();
            var multiConfig = session.LogicSettings.MultipleBotConfig;

            if (session.Stats.CatchThresholdExceeds(session, false) &&
                multiConfig.SwitchOnCatchLimit &&
                manager.AllowMultipleBot() &&
                allowSwitch)
            {
                throw new ActiveSwitchByRuleException(SwitchRules.CatchLimitReached, session.LogicSettings.CatchPokemonLimit);
            }
            if (session.Stats.SearchThresholdExceeds(session, false) &&
                multiConfig.SwitchOnPokestopLimit &&
                manager.AllowMultipleBot() &&
                allowSwitch)
            {
                throw new ActiveSwitchByRuleException(SwitchRules.SpinPokestopReached, session.LogicSettings.PokeStopLimit);
            }

            if (session.Stats.CatchThresholdExceeds(session, false) &&
                session.Stats.SearchThresholdExceeds(session, false)
                )
            {
                if (manager.AllowMultipleBot() && allowSwitch)
                {
                    throw new ActiveSwitchByRuleException(SwitchRules.SpinPokestopReached, session.LogicSettings.PokeStopLimit);
                }
                else
                {
                    await Task.Delay(15 * 60 * 1000).ConfigureAwait(false);
                }
            }
        }

        private static async Task WalkingToPokeStop(ISession session, CancellationToken cancellationToken, FortData pokeStop, FortDetailsResponse fortInfo)
        {
            var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);

            // we only move to the PokeStop, and send the associated FortTargetEvent, when not using GPX
            // also, GPX pathing uses its own EggWalker and calls the CatchPokemon tasks internally.
            if (!session.LogicSettings.UseGpxPathing)
            {
                var eggWalker = new EggWalker(1000, session);

                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                // Always set the fort info in base walk strategy.

                var pokeStopDestination = new FortLocation(pokeStop.Latitude, pokeStop.Longitude,
                    await LocationUtils.GetElevation(session.ElevationService, pokeStop.Latitude, pokeStop.Longitude).ConfigureAwait(false), pokeStop, fortInfo);

                await session.Navigation.Move(pokeStopDestination,
                    async () =>
                    {
                        await OnWalkingToPokeStopOrGym(session, pokeStop, cancellationToken).ConfigureAwait(false);
                    },
                    session,
                    cancellationToken).ConfigureAwait(false);

                // we have moved this distance, so apply it immediately to the egg walker.
                await eggWalker.ApplyDistance(distance, cancellationToken).ConfigureAwait(false);
            }
        }
        private static DateTime lastCatch = DateTime.Now;
        private static async Task OnWalkingToPokeStopOrGym(ISession session, FortData pokeStop, CancellationToken cancellationToken)
        {
            await MSniperServiceTask.Execute(session, cancellationToken).ConfigureAwait(false);

            //to avoid api call when walking.
            //if (lastCatch < DateTime.Now.AddSeconds(-2))
            //{
                // Catch normal map Pokemon
                await CatchNearbyPokemonsTask.Execute(session, cancellationToken).ConfigureAwait(false);
                //Catch Incense Pokemon
                await CatchIncensePokemonsTask.Execute(session, cancellationToken).ConfigureAwait(false);
                lastCatch = DateTime.Now;
            //}

            if (!session.LogicSettings.UseGpxPathing)
            {
                // Spin as long as we haven't reached the user defined limits
                if (!_pokestopLimitReached && !_pokestopTimerReached)
                {
                    await SpinPokestopNearBy(session, cancellationToken, pokeStop).ConfigureAwait(false);
                }
            }
        }
        public static async Task<FortData> GetNextPokeStop(ISession session)
        {

            var priorityTarget = await SetMoveToTargetTask.GetTarget(session).ConfigureAwait(false);
            if (priorityTarget != null) return priorityTarget;


            if (session.Forts == null ||
                session.Forts.Count == 0 ||
                session.Forts.Count(p => p.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()) == 0)
            {
                //TODO : A logic need to be add for handle this  case?
            };

            var deployedPokemons = await session.Inventory.GetDeployedPokemons().ConfigureAwait(false);

            //NOTE : This code is killing perfomance of BOT if GYM is turn on, need to refactor to avoid this hummer call API

            var forts = session.Forts
                .Where(p => p.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                .Where(f => f.Type == FortType.Checkpoint ||
                       (session.LogicSettings.GymConfig.Enable && (
                            UseGymBattleTask.CanAttackGym(session, f, deployedPokemons) ||
                            UseGymBattleTask.CanTrainGym(session, f, deployedPokemons) ||
                            UseGymBattleTask.CanDeployToGym(session, f, deployedPokemons))))
                .ToList();

            if (session.LogicSettings.GymConfig.Enable &&
                ((session.LogicSettings.GymConfig.EnableAttackGym && forts.Where(w => w.Type == FortType.Gym && UseGymBattleTask.CanAttackGym(session, w, deployedPokemons)).Count() == 0) ||
                (session.LogicSettings.GymConfig.EnableGymTraining && forts.Where(w => w.Type == FortType.Gym && UseGymBattleTask.CanTrainGym(session, w, deployedPokemons)).Count() == 0)
                ))
            {
                //Logger.Write("No usable gym found. Trying to refresh list.", LogLevel.Gym, ConsoleColor.Magenta);
                await GetPokeStops(session).ConfigureAwait(false);
            }

            forts = forts.OrderBy(
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
                forts = forts.Where(p => LocationUtils.CalculateDistanceInMeters(p.Latitude, p.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) < 40).ToList();
            }

            var reviveCount = (await session.Inventory.GetItems().ConfigureAwait(false)).Where(w => w.ItemId == POGOProtos.Inventory.Item.ItemId.ItemRevive || w.ItemId == POGOProtos.Inventory.Item.ItemId.ItemMaxRevive).Select(s => s.Count).Sum();
            if (!session.LogicSettings.GymConfig.Enable
                || session.LogicSettings.GymConfig.MinRevivePotions > reviveCount
            /*|| session.Inventory.GetPlayerStats().FirstOrDefault().Level <= 5*/
            )
            {
                // Filter out the gyms
                forts = forts.Where(x => x.Type != FortType.Gym).ToList();
            }
            else if (session.LogicSettings.GymConfig.PrioritizeGymOverPokestop)
            {
                // Prioritize gyms over pokestops
                var gyms = forts.Where(x => x.Type == FortType.Gym &&
                    LocationUtils.CalculateDistanceInMeters(x.Latitude, x.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude) < session.LogicSettings.GymConfig.MaxDistance);
                //.OrderBy(x => LocationUtils.CalculateDistanceInMeters(x.Latitude, x.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude));

                if (session.LogicSettings.GymConfig.PrioritizeGymWithFreeSlot)
                {
                    var freeSlots = gyms.Where(w => w.OwnedByTeam == session.Profile.PlayerData.Team && UseGymBattleTask.CanDeployToGym(session, w, deployedPokemons));
                    if (freeSlots.Count() > 0)
                        return freeSlots.First();
                }

                // Return the first gym in range.
                if (gyms.Count() > 0)
                    return gyms.FirstOrDefault();
            }

            return forts.FirstOrDefault();
        }

        public static async Task SpinPokestopNearBy(ISession session, CancellationToken cancellationToken, FortData destinationFort = null)
        {
            var allForts = session.Forts.Where(p => p.Type == FortType.Checkpoint).ToList();

            if (allForts.Count > 0)
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

                if (spinablePokestops.Count > 0)
                {
                    foreach (var pokeStop in spinablePokestops)
                    {
                        var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude).ConfigureAwait(false);
                        await FarmPokestop(session, pokeStop, fortInfo, cancellationToken, true).ConfigureAwait(false);
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
                await Task.Delay(Math.Min(session.LogicSettings.DelayBetweenPlayerActions, 3000)).ConfigureAwait(false);
                await CatchLurePokemonsTask.Execute(session, pokeStop, cancellationToken).ConfigureAwait(false);
            }

            // Spin as long as we haven't reached the user defined limits
            if (!_pokestopLimitReached && !_pokestopTimerReached)
            {
                await FarmPokestop(session, pokeStop, fortInfo, cancellationToken, doNotTrySpin).ConfigureAwait(false);
            }
            else
            {
                // We hit the pokestop limit but not the pokemon limit. So we want to set the cooldown on the pokestop so that
                // we keep moving and don't walk back and forth between 2 pokestops.
                pokeStop.CooldownCompleteTimestampMs = DateTime.UtcNow.ToUnixTime() + 5 * 60 * 1000; // 5 minutes to cooldown for pokestop.
            }

            if (++_stopsHit >= _storeRi) //TODO: OR item/pokemon bag is full //check stopsHit against storeRI random without dividing.
            {
                _storeRi = _rc.Next(6, 12); //set new storeRI for new random value
                _stopsHit = 0;

                if (session.LogicSettings.UseNearActionRandom)
                {
                    await HumanRandomActionTask.Execute(session, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await RecycleItemsTask.Execute(session, cancellationToken).ConfigureAwait(false);

                    if (session.LogicSettings.UseLuckyEggConstantly)
                        await UseLuckyEggConstantlyTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    if (session.LogicSettings.UseIncenseConstantly)
                        await UseIncenseConstantlyTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    if (session.LogicSettings.TransferDuplicatePokemon)
                        await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    if (session.LogicSettings.TransferWeakPokemon)
                        await TransferWeakPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    if (session.LogicSettings.EvolveAllPokemonAboveIv ||
                        session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                        session.LogicSettings.UseLuckyEggsWhileEvolving ||
                        session.LogicSettings.KeepPokemonsThatCanEvolve)
                    {
                        await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    }
                    if (session.LogicSettings.AutomaticallyLevelUpPokemon)
                        await LevelUpPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                    if (session.LogicSettings.RenamePokemon)
                        await RenamePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);

                    await GetPokeDexCount.Execute(session, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static int softbanCount = 0;

        public static async Task FarmPokestop(ISession session, FortData pokeStop, FortDetailsResponse fortInfo, CancellationToken cancellationToken, bool doNotRetry = false)
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();

            // If the cooldown is in the future than don't farm the pokestop.
            if (pokeStop.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                return;

            if (session.Stats.SearchThresholdExceeds(session, true))
            {
                if (manager.AllowMultipleBot() && session.LogicSettings.MultipleBotConfig.SwitchOnPokestopLimit)
                {
                    throw new Exceptions.ActiveSwitchByRuleException(SwitchRules.SpinPokestopReached, session.LogicSettings.PokeStopLimit);
                }
                return;
            }

            //await session.Client.Map.GetMapObjects().ConfigureAwait(false);
            FortSearchResponse fortSearch;
            var timesZeroXPawarded = 0;
            var fortTry = 0; //Current check
            int retryNumber = session.LogicSettings.ByPassSpinCount; //How many times it needs to check to clear softban
            int zeroCheck = Math.Min(5, retryNumber); //How many times it checks fort before it thinks it's softban

            var distance = LocationUtils.CalculateDistanceInMeters(pokeStop.Latitude, pokeStop.Longitude, session.Client.CurrentLatitude, session.Client.CurrentLongitude);
            //This should be < ## not > ##. > makes bot jump to pokestop if < then when in range will just spin.
            if (distance < 50) //if (distance > 30)
            {
                await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude), 0).ConfigureAwait(false);
                await session.Client.Misc.RandomAPICall().ConfigureAwait(false);
            }

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                int retry = 3;
                double latitude = pokeStop.Latitude;
                double longitude = pokeStop.Longitude;
                do
                {
                    fortSearch = await session.Client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude).ConfigureAwait(false);
                    if (fortSearch.Result == FortSearchResponse.Types.Result.OutOfRange)
                    {
                        
                        if (retry > 2)
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                        }
                        else
                            await session.Client.Map.GetMapObjects(true).ConfigureAwait(false);

                        Logger.Debug($"Loot pokestop result: {fortSearch.Result}, distance to pokestop:[{pokeStop.Latitude}, {pokeStop.Longitude}] {distance:0.00}m, retry: #{4 - retry}");

                        latitude += 0.000003;
                        longitude += 0.000005;
                        await LocationUtils.UpdatePlayerLocationWithAltitude(session, new GeoCoordinate(latitude, longitude), 0).ConfigureAwait(false);

                        retry--;
                    }
                }
                while (fortSearch.Result == FortSearchResponse.Types.Result.OutOfRange && retry > 0);
                Logger.Debug($"Loot pokestop result: {fortSearch.Result}");
                if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                if (fortSearch.ExperienceAwarded == 0 && fortSearch.Result != FortSearchResponse.Types.Result.InventoryFull)
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
                            await DelayingUtils.DelayAsync(session.LogicSettings.DelayBetweenPlayerActions, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    softbanCount = 0;
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
                        Gems = fortSearch.GemsAwarded > 0 ? $"Yes {fortSearch.GemsAwarded}" : "No",
                        Items = StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded),
                        Badges = fortSearch.AwardedGymBadge != null ? fortSearch.AwardedGymBadge.GymBadgeType.ToString() : "No",
                        BonusLoot = fortSearch.BonusLoot != null ? StringUtils.GetSummedFriendlyNameOfGetLootList(fortSearch.BonusLoot.LootItem) : "No",
                        RaidTickets = fortSearch.RaidTickets > 0 ? $"{fortSearch.RaidTickets} tickets" : "No",
                        TeamBonusLoot = fortSearch.TeamBonusLoot != null ? StringUtils.GetSummedFriendlyNameOfGetLootList(fortSearch.TeamBonusLoot.LootItem) : "No",
                        PokemonDataEgg = fortSearch.PokemonDataEgg != null ? fortSearch.PokemonDataEgg : null,
                        Latitude = pokeStop.Latitude,
                        Longitude = pokeStop.Longitude,
                        Altitude = session.Client.CurrentAltitude,
                        InventoryFull = fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull,
                        Fort = pokeStop
                    });
                    if (fortSearch.Result == FortSearchResponse.Types.Result.Success)
                    {
                        mapEmptyCount = 0;
                        foreach (var item in fortSearch.ItemsAwarded)
                        {
                            await session.Inventory.UpdateInventoryItem(item.ItemId).ConfigureAwait(false);
                        }
                        if (fortSearch.PokemonDataEgg != null)
                        {
                            fortSearch.PokemonDataEgg.IsEgg = true;
                        }

                        // Update the cache
                        var fortFromCache = session.Client.Map.LastGetMapObjectResponse.MapCells.SelectMany(x => x.Forts).FirstOrDefault(f => f.Id == pokeStop.Id);

                        long newCooldown = TimeUtil.GetCurrentTimestampInMilliseconds() + (5 * 60 * 1000); /* 5 min */
                        fortFromCache.CooldownCompleteTimestampMs = newCooldown;
                        pokeStop.CooldownCompleteTimestampMs = newCooldown;

                        if (session.SaveBallForByPassCatchFlee)
                        {
                            var totalBalls = (await session.Inventory.GetItems().ConfigureAwait(false)).Where(x => x.ItemId == ItemId.ItemPokeBall || x.ItemId == ItemId.ItemGreatBall || x.ItemId == ItemId.ItemUltraBall).Sum(x => x.Count);
                            Logger.Write($"Ball requires for by pass catch flee {totalBalls}/{CatchPokemonTask.BALL_REQUIRED_TO_BYPASS_CATCHFLEE}");
                        }
                        else
                            MSniperServiceTask.UnblockSnipe(false);
                    }
                    if (fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull)
                    {
                        await RecycleItemsTask.Execute(session, cancellationToken).ConfigureAwait(false);
                        _storeRi = 1;
                    }

                    if (session.LogicSettings.UsePokeStopLimit)
                    {
                        session.Stats.AddPokestopTimestamp(DateTime.Now.Ticks);
                        session.EventDispatcher.Send(new PokestopLimitUpdate(session.Stats.GetNumPokestopsInLast24Hours(), session.LogicSettings.PokeStopLimit));
                    }
                    //add pokeStops to Map
                    OnLootPokestopEvent(pokeStop);
                    //end pokeStop to Map

                    break; //Continue with program as loot was succesfull.
                }
            } while (fortTry < retryNumber - zeroCheck);
            //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.

            if (manager.AllowMultipleBot())
            {
                if (fortTry >= retryNumber - zeroCheck)
                {
                    softbanCount++;

                    //only check if PokestopSoftbanCount > 0
                    if (MultipleBotConfig.IsMultiBotActive(session.LogicSettings, manager) &&
                        session.LogicSettings.MultipleBotConfig.PokestopSoftbanCount > 0 &&
                        session.LogicSettings.MultipleBotConfig.PokestopSoftbanCount <= softbanCount &&
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().AllowSwitch())
                    {
                        softbanCount = 0;

                        //Activate switcher by pokestop
                        throw new ActiveSwitchByRuleException()
                        {
                            MatchedRule = SwitchRules.PokestopSoftban,
                            ReachedValue = session.LogicSettings.MultipleBotConfig.PokestopSoftbanCount
                        };
                    }
                }
            }
            else
            {
                softbanCount = 0; //reset softban count
            }

            if (session.LogicSettings.RandomlyPauseAtStops && !doNotRetry)
            {
                if (++_randomStop >= _randomNumber)
                {
                    _randomNumber = _rc.Next(4, 11);
                    _randomStop = 0;
                    int randomWaitTime = _rc.Next(30, 120);
                    await Task.Delay(randomWaitTime, cancellationToken).ConfigureAwait(false);
                }
            }

        }
        private static int mapEmptyCount = 0;
        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters for GPX pathing, as we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        //For non GPX pathing, it returns all pokestops in range.
        private static async Task<Tuple<List<FortData>, List<FortData>>> GetPokeStops(ISession session)
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            List<FortData> mapObjects = await UpdateFortsData(session).ConfigureAwait(false);
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
                    mapEmptyCount++;
                    if (mapEmptyCount == 30 && 
                        manager.AllowMultipleBot() &&
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().AllowSwitch())
                    {
                        throw new ActiveSwitchByRuleException() { MatchedRule = SwitchRules.EmptyMap, ReachedValue = 30 };
                    }
                }
                else
                {
                    mapEmptyCount = 0;
                }

                var pokeStops = mapObjects.Where(p => p.Type == FortType.Checkpoint).ToList();
                session.AddVisibleForts(pokeStops);
                session.EventDispatcher.Send(new PokeStopListEvent(mapObjects));

                var gyms = mapObjects.Where(p => p.Type == FortType.Gym).ToList();
                return Tuple.Create(pokeStops, gyms);
            }

            if (mapObjects.Count > 0)
            {
                // only send when there are stops for GPX because otherwise we send empty arrays often
                session.EventDispatcher.Send(new PokeStopListEvent(mapObjects));
            }
            // Wasn't sure how to make this pretty. Edit as needed.
            return Tuple.Create(
                mapObjects.Where(
                    i => i.Type == FortType.Checkpoint &&
                        ( // Make sure PokeStop is within 40 meters or else it is pointless to hit it
                            LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                i.Latitude, i.Longitude) <= 40)
                ).ToList(),
                mapObjects.Where(p => p.Type == FortType.Gym && LocationUtils.CalculateDistanceInMeters(
                                session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                                p.Latitude, p.Longitude) <= 40).ToList()
                );
        }

        public static async Task<List<FortData>> UpdateFortsData(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects().ConfigureAwait(false);

            session.AddForts(mapObjects.MapCells.SelectMany(p => p.Forts).ToList());

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        (i.Type == FortType.Checkpoint || i.Type == FortType.Gym) &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        (
                            LocationUtils.CalculateDistanceInMeters(
                                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                                i.Latitude, i.Longitude) <= session.LogicSettings.MaxTravelDistanceInMeters)
                );

            return pokeStops.ToList();
        }
		        //add delegate event
        private static void OnLootPokestopEvent(FortData pokestop)
        {
            LootPokestopEvent?.Invoke(pokestop);
        }
        public static event LootPokestopDelegate LootPokestopEvent;
    }
}
