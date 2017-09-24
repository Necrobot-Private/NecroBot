#region using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using PoGo.NecroBot.Logic.Logging;
using POGOProtos.Networking.Responses;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Event.Gym;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Data;
using POGOProtos.Data.Battle;
using PokemonGo.RocketAPI.Exceptions;
using PoGo.NecroBot.Logic.Utils;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseGymBattleTask
    {
        public static DateTime AttackStart { get; private set; }

        private static int _startBattleCounter = 3;
        private static readonly bool _logTimings = false;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static async Task<bool> Execute(ISession session, CancellationToken cancellationToken, FortData gym, FortDetailsResponse fortInfo)
        {
            if (!session.LogicSettings.GymConfig.Enable || gym.Type != FortType.Gym) return false;

            if (session.GymState.MoveSettings == null)
            {
                session.GymState.MoveSettings = await session.Inventory.GetMoveSettings().ConfigureAwait(false);
            }

            await session.GymState.LoadMyPokemons(session).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            var distance = session.Navigation.WalkStrategy.CalculateDistance(session.Client.CurrentLatitude, session.Client.CurrentLongitude, gym.Latitude, gym.Longitude);
            if (fortInfo != null)
            {
                session.EventDispatcher.Send(new GymWalkToTargetEvent()
                {
                    Name = fortInfo.Name,
                    Distance = distance,
                    Latitude = fortInfo.Latitude,
                    Longitude = fortInfo.Longitude
                });

                var fortDetails = session.GymState.GymGetInfo(session, gym, true); //await session.Client.Fort.GetGymDetails(gym.Id, gym.Latitude, gym.Longitude).ConfigureAwait(false);

                if (fortDetails.Result == GymGetInfoResponse.Types.Result.Success)
                {
                    var player = session.Profile.PlayerData;
                    await EnsureJoinTeam(session, player).ConfigureAwait(false);
                    var defenders = fortDetails.GymStatusAndDefenders.GymDefender.Select(x => x.MotivatedPokemon.Pokemon).ToList();

                    session.EventDispatcher.Send(new GymDetailInfoEvent()
                    {
                        Team = gym.OwnedByTeam,
                        Players = defenders.Count(),
                        Name = fortDetails.Name,
                    });

                    if (player.Team != TeamColor.Neutral)
                    {
                        var deployedPokemons = await session.Inventory.GetDeployedPokemons().ConfigureAwait(false);
                        List<PokemonData> deployedList = new List<PokemonData>(deployedPokemons);

                        if (gym.OwnedByTeam == player.Team || gym.OwnedByTeam == TeamColor.Neutral)
                        {
                            if (!deployedPokemons.Any(a => a.DeployedFortId.Equals(fortInfo.FortId)))
                            {
                                GymDeployResponse response = await DeployPokemonToGym(session, fortInfo, fortDetails, cancellationToken, gym).ConfigureAwait(false);

                                if (response != null && response.Result == GymDeployResponse.Types.Result.Success)
                                {
                                    deployedPokemons = await session.Inventory.GetDeployedPokemons().ConfigureAwait(false);
                                    deployedList = new List<PokemonData>(deployedPokemons);
                                }
                            }

                            if (CanBerrieGym(session, gym, deployedList))
                            {
                                if (string.IsNullOrEmpty(session.GymState.BerriesGymId) || !session.GymState.BerriesGymId.Equals(fortInfo.FortId))
                                {
                                    session.GymState.BerriesGymId = fortInfo.FortId;
                                    session.GymState.BerriesRound = 0;
                                }
                                session.GymState.BerriesRound++;
                                //Review this....
                                return false;
                                //if (session.GymState.BerriesRound <= session.LogicSettings.GymConfig.MaxBerriesRoundsOnOneGym)
                                //    return await StartGymAttackLogic(session, fortInfo, fortDetails, gym, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            if (CanAttackGym(session, gym, deployedList))
                                return await StartGymAttackLogic(session, fortInfo, fortDetails, gym, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    Logger.Write($"You are not level 5 yet, come back later...", LogLevel.Gym, ConsoleColor.White);
                }
            }
            else
            {
                Logger.Write($"Ignoring Gym: {fortInfo?.Name} - ", LogLevel.Gym, ConsoleColor.Cyan);
            }

            return false;
        }

        private static async Task<bool> StartGymAttackLogic(ISession session, FortDetailsResponse fortInfo, GymGetInfoResponse fortDetails, FortData gym, CancellationToken cancellationToken)
        {
            //Check if raid or normal battle
            try
            {
                if (gym.RaidInfo != null)
                {
                    DateTime expires = new DateTime(0);
                    TimeSpan time = new TimeSpan(0);

                    if (gym.RaidInfo.RaidBattleMs > DateTime.UtcNow.ToUnixTime())
                    {
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(gym.RaidInfo.RaidBattleMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            string str = $"Next RAID starts in: {time.Hours:00}h:{time.Minutes:00}m\nat: {(DateTime.Now + time).Hour:00}:{(DateTime.Now + time).Minute:00} Local time";
                            Logger.Write($"{str}.", LogLevel.Gym);
                        }
                    }

                    if (gym.RaidInfo.RaidPokemon.PokemonId != PokemonId.Missingno)
                    {
                        //Raid modes 
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(gym.RaidInfo.RaidEndMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            string boss = $"Boss: {session.Translation.GetPokemonTranslation(gym.RaidInfo.RaidPokemon.PokemonId)} CP: {gym.RaidInfo.RaidPokemon.Cp}";
                            string str = $"Local RAID ends in: {time.Hours:00}h:{time.Minutes:00}m\nat: {(DateTime.Now + time).Hour:00}:{(DateTime.Now + time).Minute:00} Local time\n\r{boss}";
                            Logger.Write($"{str}.", LogLevel.Gym);

                            //for dev
                            Logger.Write("Raid boos is present. Raids battle not yet released.", LogLevel.Gym, ConsoleColor.Red);
                            return false;
                        }

                        // new code or new task....

                        //var raidDetails = await session.Client.Fort.GetRaidDetails(gym.Id, gym.RaidInfo.RaidSeed).ConfigureAwait(false);
                        //var joinLobbyResult = await session.Client.Fort.JoinLobby(gym.Id, gym.RaidInfo.RaidSeed, false).ConfigureAwait(false);
                        //var setLobbyVisibility = await session.Client.Fort.SetLobbyVisibility(gym.Id, gym.RaidInfo.RaidSeed);
                        //var setLobbyPokemon = await session.Client.Fort.SetLobbyPokemon(gym.Id, gym.RaidInfo.RaidSeed);
                        //var startRaidBattle = await session.Client.Fort.StartRaidBattle(gym.Id, gym.RaidInfo.RaidSeed).ConfigureAwait(false);
                        //var attackRaid = await session.Client.Fort.AttackRaidBattle(gym.Id, gym.RaidInfo.RaidSeed).ConfigureAwait(false);
                        //var leaveLobbyResult = await session.Client.Fort.LeaveLobby(gym.Id, gym.RaidInfo.RaidSeed);
                        //
                    }

                    if (gym.RaidInfo.RaidSpawnMs > DateTime.UtcNow.ToUnixTime())
                    {
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(gym.RaidInfo.RaidSpawnMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            Logger.Write("Raid battle is runing...", LogLevel.Gym);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Logger.Write(ex.Message, LogLevel.Error);
#endif
                return false;
            }

            var defenders = fortDetails.GymStatusAndDefenders.GymDefender.Select(x => x.MotivatedPokemon.Pokemon).ToList();

            if (defenders.Count < 1)
                return false;

            if (session.Profile.PlayerData.Team != gym.OwnedByTeam)
            {
                if (session.LogicSettings.GymConfig.MaxGymPlayersToAttack < defenders.Count)
                {
                    Logger.Write($"This gym's Players is {defenders.Count} > {session.LogicSettings.GymConfig.MaxGymPlayersToAttack} in your config. Bot walk away...", LogLevel.Gym, ConsoleColor.Red);
                    return false;
                }

                if (session.LogicSettings.GymConfig.MaxDefendersToAttack < defenders.Count)
                {
                    Logger.Write($"This gym has {defenders.Count} defender(s) > {session.LogicSettings.GymConfig.MaxDefendersToAttack} in your config. Bot walk away...", LogLevel.Gym, ConsoleColor.Red);
                    return false;
                }
            }

            /*if (fortDetails.GymState.FortData.IsInBattle)
            {
                Logger.Write("This gym is under attack now, we will skip it");
                return false;
            }*/

            bool isTraining = (session.Profile.PlayerData.Team == gym.OwnedByTeam || (!string.IsNullOrEmpty(session.GymState.CapturedGymId) && session.GymState.CapturedGymId.Equals(gym.Id)));
            var badassPokemon = await CompleteAttackTeam(session, defenders, isTraining).ConfigureAwait(false);
            if (badassPokemon == null)
            {
                Logger.Write("Check gym settings, we can't compete against attackers team. Exiting.", LogLevel.Warning, ConsoleColor.Magenta);
                return false;
            }
            var pokemonDatas = badassPokemon as PokemonData[] ?? badassPokemon.ToArray();

            Logger.Write("Starting battle with: " + string.Join(", ", defenders.Select(x => x.PokemonId.ToString())));

            foreach (var pokemon in pokemonDatas)
            {
                if (pokemon.Stamina <= 0)
                    await RevivePokemon(session, pokemon).ConfigureAwait(false);

                if (pokemon.Stamina <= 0)
                {
                    Logger.Write("You are out of revive potions! Can't revive attacker", LogLevel.Gym, ConsoleColor.Magenta);
                    return false;
                }

                if (pokemon.Stamina < pokemon.StaminaMax)
                    await HealPokemon(session, pokemon).ConfigureAwait(false);

                if (pokemon.Stamina < pokemon.StaminaMax)
                    Logger.Write(string.Format("You are out of healing potions! {0} ({1} CP) was not fully healed", pokemon.PokemonId, pokemon.Cp), LogLevel.Gym, ConsoleColor.Magenta);
            }
            //await Task.Delay(2000).ConfigureAwait(false);

            var index = 0;
            bool isVictory = true;
            bool isFailedToStart = false;
            List<BattleAction> battleActions = new List<BattleAction>();
            ulong defenderPokemonId = defenders.First().Id;

            while (index < defenders.Count())
            {
                TimedLog("Attacking Team consists of: " + string.Join(", ", session.GymState.MyTeam.Select(s => string.Format("{0} ({1} HP / {2} CP) [{3}]", s.Attacker.PokemonId, s.HpState, s.Attacker.Cp, s.Attacker.Id))));
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                var thisAttackActions = new List<BattleAction>();

                GymStartSessionResponse result = null;
                try
                {
                    result = await StartBattle(session, gym, pokemonDatas, defenderPokemonId).ConfigureAwait(false);
                }
                catch (APIBadRequestException)
                {
                    Logger.Write("Can't start battle", LogLevel.Gym);
                    isFailedToStart = true;
                    isVictory = false;
                    _startBattleCounter--;

                    TimedLog("Starting battle Results: " + result);
                    TimedLog("FortDetails: " + fortDetails);
                    TimedLog("PokemonDatas: " + string.Join(", ", pokemonDatas.Select(s => string.Format("Id: {0} Name: {1} CP: {2} HP: {3}", s.Id, s.PokemonId, s.Cp, s.Stamina))));
                    TimedLog("DefenderId: " + defenderPokemonId);
                    TimedLog("ActionsLog -> " + string.Join(Environment.NewLine, battleActions));

                    break;
                }

                index++;
                // If we can't start battle in 10 tries, let's skip the gym
                if (result == null || result.Result != GymStartSessionResponse.Types.Result.Success)
                {
                    session.EventDispatcher.Send(new GymErrorUnset { GymName = fortInfo.Name });
                    isVictory = false;
                    break;
                }

                switch (result.Battle.BattleLog.State)
                {
                    case BattleState.Active:
                        _startBattleCounter = 3;
                        AttackStart = DateTime.Now.AddSeconds(120);
                        Logger.Write($"Time to start Attack Mode", LogLevel.Gym, ConsoleColor.DarkYellow);
                        thisAttackActions = await AttackGym(session, cancellationToken, fortDetails, result, index, gym).ConfigureAwait(false);
                        battleActions.AddRange(thisAttackActions);
                        break;
                    case BattleState.Defeated:
                        isVictory = false;
                        break;
                    case BattleState.StateUnset:
                        isVictory = false;
                        break;
                    case BattleState.TimedOut:
                        isVictory = false;
                        break;
                    case BattleState.Victory:
                        isVictory = true;
                        break;
                    default:
                        Logger.Write($"Unhandled result starting gym battle:\n{result}");
                        break;
                }

                var rewarded = battleActions.Select(x => x.BattleResults?.PlayerXpAwarded).Where(x => x != null);
                var lastAction = battleActions.LastOrDefault();

                if (lastAction.Type == BattleActionType.ActionTimedOut ||
                    lastAction.Type == BattleActionType.ActionUnset ||
                    lastAction.Type == BattleActionType.ActionDefeat)
                {
                    isVictory = false;
                    break;
                }

                var faintedPKM = battleActions.Where(x => x != null && x.Type == BattleActionType.ActionFaint).Select(x => x.ActivePokemonId).Distinct();
                var livePokemons = pokemonDatas.Where(x => !faintedPKM.Any(y => y == x.Id));
                var faintedPokemons = pokemonDatas.Where(x => faintedPKM.Any(y => y == x.Id));
                pokemonDatas = livePokemons.Concat(faintedPokemons).ToArray();

                if (lastAction.Type == BattleActionType.ActionVictory)
                {
                    if (lastAction.BattleResults != null)
                    {
                        var exp = lastAction.BattleResults.PlayerXpAwarded;
                        var point = lastAction.BattleResults.GymPointsDelta;
                        gym.GymPoints += point;
                        defenderPokemonId = unchecked((ulong)lastAction.BattleResults.NextDefenderPokemonId);

                        await Task.Delay(2000).ConfigureAwait(false);

                        Logger.Write($"(Battle) XP: {exp} | Gym points: {point} | Players: {defenders.Count,2:#0} | Next defender Id: {defenderPokemonId}", LogLevel.Gym, ConsoleColor.Magenta);

                        if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                            await PushNotificationClient.SendNotification(session, $"Gym Battle",
                                                                                   $"We were victorious!\n" +
                                                                                   $"XP: {exp}" +
                                                                                   $"Prest: {point}" +
                                                                                   $"Players: {defenders.Count,2:#0}", true).ConfigureAwait(false); // +
                                                                                                                                                                    //$"{startResponse.Defender.ActivePokemon.PokemonData.PokemonId}", true);
                    }
                    continue;
                }
            }

            TimedLog(string.Join(Environment.NewLine, battleActions.OrderBy(o => o.ActionStartMs).Select(s => s).Distinct()));

            if (isVictory)
            {
                if (gym.GymPoints < 0)
                    gym.GymPoints = 0;
                await Execute(session, cancellationToken, gym, fortInfo).ConfigureAwait(false);
            }

            if (isFailedToStart && _startBattleCounter > 0)
            {
                Logger.Write("Waiting extra time to try again (10 sec)");
                await Task.Delay(10000).ConfigureAwait(false);
                await Execute(session, cancellationToken, gym, fortInfo).ConfigureAwait(false);
            }

            var bAction = battleActions.LastOrDefault();
            if (bAction != null)
                if ((bAction.Type == BattleActionType.ActionDefeat) || (bAction.Type == BattleActionType.ActionTimedOut))
                {
                    if (battleActions.Exists(p => p.Type == BattleActionType.ActionVictory))
                    {
                        await Execute(session, cancellationToken, gym, fortInfo).ConfigureAwait(false);
                    }
                }

            if (_startBattleCounter <= 0)
                _startBattleCounter = 3;

            return true;
        }

        private static async Task<GymDeployResponse> DeployPokemonToGym(ISession session, FortDetailsResponse fortInfo, GymGetInfoResponse fortDetails, CancellationToken cancellationToken, FortData fort)
        {
            GymDeployResponse response = null;
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            var allCp = GetGymAllCpOnGym(fortDetails.GymStatusAndDefenders.GymDefender.Select(x => x.MotivatedPokemon.Pokemon).ToList());
            var maxCount = 6;

            var availableSlots = maxCount - fortDetails.GymStatusAndDefenders.GymDefender.Count();

            if (availableSlots > 0)
            {
                var deployed = await session.Inventory.GetDeployedPokemons().ConfigureAwait(false);
                if (!deployed.Any(a => a.DeployedFortId == fortInfo.FortId))
                {
                    var pokemon = await GetDeployablePokemon(session).ConfigureAwait(false);
                    if (pokemon != null)
                    {
                        try
                        {
                            response = await session.Client.Fort.GymDeploy(fortInfo.FortId, pokemon.Id).ConfigureAwait(false);
                        }
                        catch (APIBadRequestException)
                        {
                            Logger.Write("Failed to deploy pokemon. Trying again...", LogLevel.Gym, ConsoleColor.Magenta);
                            await Execute(session, cancellationToken, fort, fortInfo).ConfigureAwait(false);
                            return null;
                        }
                        if (response?.Result == GymDeployResponse.Types.Result.Success)
                        {
                            session.EventDispatcher.Send(new GymDeployEvent()
                            {
                                PokemonId = pokemon.PokemonId,
                                Name = fortDetails.Name
                            });

                            session.GymState.CapturedGymId = fort.Id;

                            if (session.LogicSettings.GymConfig.CollectCoinAfterDeployed > 0)
                            {
                                var count = deployed.Count();
                                if (count >= session.LogicSettings.GymConfig.CollectCoinAfterDeployed)
                                {
                                    /*
                                     * This is not used now
                                    try
                                    {
                                        if (session.Profile.PlayerData.DailyBonus.NextDefenderBonusCollectTimestampMs <= DateTime.UtcNow.ToLocalTime().ToUnixTime())
                                        {
                                            var collectDailyBonusResponse = await session.Client.Player.CollectDailyDefenderBonus().ConfigureAwait(false);
                                            if (collectDailyBonusResponse.Result == CollectDailyDefenderBonusResponse.Types.Result.Success)
                                            {
                                                Logger.Write($"Collected {count * 10} coins", LogLevel.Gym, ConsoleColor.DarkYellow);

                                                if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                                                    await PushNotificationClient.SendNotification(session, $"Daily reward collected: {count * 10} coins", $"Congratulations, Your bot has worked hard and collected {count * 10} coins today.", true).ConfigureAwait(false);
                                            }
                                            else
                                                Logger.Write($"Hmm, we have failed with gaining a reward: {collectDailyBonusResponse}", LogLevel.Gym, ConsoleColor.Magenta);
                                        }
                                        else
                                            Logger.Write($"You will be able to collect bonus at {DateTimeFromUnixTimestampMillis(session.Profile.PlayerData.DailyBonus.NextDefenderBonusCollectTimestampMs).ToLocalTime()}", LogLevel.Info, ConsoleColor.Magenta);
                                    }
                                    catch (APIBadRequestException)
                                    {
                                        Logger.Write("Can't get coins", LogLevel.Warning);
                                        //Debug.WriteLine(e.Message, "GYM");
                                        //Debug.WriteLine(e.StackTrace, "GYM");

                                        await Task.Delay(500).ConfigureAwait(false);
                                    }
                                    */
                                }
                                else
                                    Logger.Write(string.Format("You have {0} defenders deployed but {1} are required to get your reward", count, session.LogicSettings.GymConfig.CollectCoinAfterDeployed), LogLevel.Gym, ConsoleColor.Magenta);
                            }
                            else
                                Logger.Write("You have disabled reward collecting in your config file", LogLevel.Gym, ConsoleColor.Magenta);
                        }
                        else
                            Logger.Write(string.Format("Failed to deploy pokemon. Result: {0}", response.Result), LogLevel.Gym, ConsoleColor.Magenta);
                    }
                    else
                        Logger.Write($"You don't have any pokemon to be deployed!", LogLevel.Gym);
                }
                else
                    Logger.Write($"You already have pokemon deployed here", LogLevel.Gym);
            }
            else
            {
                string message = string.Format("No FREE slots in GYM: {0}/{1} (All Cp: {2})", fortDetails.GymStatusAndDefenders.GymDefender.Count(), maxCount, allCp);
                Logger.Write(message, LogLevel.Gym, ConsoleColor.White);
            }
            return response;
        }

        private static async Task<IEnumerable<PokemonData>> CompleteAttackTeam(ISession session, IEnumerable<PokemonData> defenders, bool isTraining)
        {
            /*
             *  While i'm trying to make this gym attack i've made an error and complete team with the same one pokemon 6 times. 
             *  Guess what, it was no error. More, fight in gym was successfull and this one pokemon didn't died once but after faint got max hp again and fight again. 
             *  So after all we used only one pokemon.
             *  Maybe we can use it somehow.
             */
            session.GymState.MyTeam.Clear();

            List<PokemonData> attackers = new List<PokemonData>();

            if (session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp && defenders.Count() > 1)
            {
                attackers.AddRange(GetBestToTeam(session, attackers));
                attackers.ForEach(attacker =>
                {
                    session.GymState.AddToTeam(session, attacker);
                });
            }
            else
            {
                while (attackers.Count() < 6)
                {
                    foreach (var defender in defenders)
                    {
                        var attacker = await GetBestAgainst(session, attackers, defender, isTraining).ConfigureAwait(false);
                        if (attacker != null)
                        {
                            //Trying to make bot only select pokemon that are more than 75% of full CP to battle. Still needs some work(The Wizard1328)
                            //if (attacker.Cp >= attacker.Cp * 0.75)
                            //{
                            attackers.Add(attacker);
                            session.GymState.AddToTeam(session, attacker);
                            if (attackers.Count == 6)
                                break;
                            //}
                        }
                        else return null;
                    }
                }
            }

            return attackers;
        }

        private static async Task<PokemonData> GetBestAgainst(ISession session, List<PokemonData> myTeam, PokemonData defender, bool isTraining)
        {
            TimedLog(string.Format("Checking pokemon for {0} ({1} CP). Already collected team has: {2}", defender.PokemonId, defender.Cp, string.Join(", ", myTeam.Select(s => string.Format("{0} ({1} CP)", s.PokemonId, s.Cp)))));
            session.GymState.AddPokemon(session, defender, false);
            AnyPokemonStat defenderStat = session.GymState.OtherDefenders.FirstOrDefault(f => f.Data.Id == defender.Id);

            if (session.LogicSettings.GymConfig.Attackers != null && session.LogicSettings.GymConfig.Attackers.Count > 0)
            {
                var allPokemons = await session.Inventory.GetPokemons().ConfigureAwait(false);
                var configs = isTraining ? session.LogicSettings.GymConfig.Trainers : session.LogicSettings.GymConfig.Attackers;
                foreach (var def in configs.OrderByDescending(o => o.Priority))
                {
                    var attackersFromConfig = allPokemons.Where(w =>
                        w.PokemonId == def.Pokemon &&
                        w.Id != session.Profile.PlayerData.BuddyPokemon?.Id &&
                        !myTeam.Any(a => a.Id == w.Id) &&
                        string.IsNullOrEmpty(w.DeployedFortId) &&
                        w.Cp >= (def.MinCP ?? 0) &&
                        w.Cp <= (def.MaxCP ?? 5000) &&
                        def.IsMoveMatch(w.Move1, w.Move2)
                    ).ToList();

                    if (attackersFromConfig != null && attackersFromConfig.Count > 0)
                        return attackersFromConfig.OrderByDescending(o => o.Cp).FirstOrDefault();
                }

            }

            MyPokemonStat myAttacker = session.GymState.MyPokemons
                .Where(w =>
                        !myTeam.Any(a => a.Id == w.Data.Id) && //not already in team
                        string.IsNullOrEmpty(w.Data.DeployedFortId) && //not already deployed
                        session.Profile.PlayerData.BuddyPokemon?.Id != w.Data.Id //not a buddy
                    )
                .OrderByDescending(o => o.TypeFactor[defenderStat.MainType] + o.TypeFactor[defenderStat.ExtraType] + o.GetFactorAgainst(session, defender.Cp, isTraining))
                .ThenByDescending(o => o.Data.Cp)
                .FirstOrDefault();
            if (myAttacker == null || myAttacker.Data.Cp < (defender.Cp * session.LogicSettings.GymConfig.ButNotLessThanDefenderPercent))
            {
                var other = GetBestToTeam(session, myTeam).FirstOrDefault();
                TimedLog(string.Format("Best against {0} {6} CP with is {1} {5} can't be found, will use top by CP instead: {2} ({7} CP) with attacks {3} and {4}", defender.PokemonId, defenderStat.MainType, other?.PokemonId, other?.Move1, other?.Move2, defenderStat.ExtraType, defender.Cp, other?.Cp));
                return other;
            }
            else
                TimedLog(string.Format("Best against {0} {7} CP with is {1} {5} type will be {2} ({6} CP) with attacks {3} and {4} (Factor for main type {8}, second {9}, CP {10})", defender.PokemonId, defenderStat.MainType, myAttacker.Data.PokemonId, myAttacker.Data.Move1, myAttacker.Data.Move2, defenderStat.ExtraType, myAttacker.Data.Cp, defender.Cp, myAttacker.TypeFactor[defenderStat.MainType], myAttacker.TypeFactor[defenderStat.ExtraType], myAttacker.GetFactorAgainst(session, defender.Cp, isTraining)));
            return myAttacker.Data;
        }

        private static PokemonData GetBestInBattle(ISession session, PokemonData defender)
        {
            session.GymState.AddPokemon(session, defender, false);
            AnyPokemonStat defenderStat = session.GymState.OtherDefenders.FirstOrDefault(f => f.Data.Id == defender.Id);
            List<PokemonType> attacks = new List<PokemonType>(GetBestTypes(defenderStat.MainType));

            TimedLog(string.Format("Searching for a new attacker against {0} ({1})", defender.PokemonId, defenderStat.MainType));

            var moves = session.GymState.MoveSettings.Where(w => attacks.Any(a => a == w.PokemonType));

            PokemonData newAttacker = session.GymState.MyTeam.Where(w =>
                        moves.Any(a => a.MovementId == w.Attacker.Move1 || a.MovementId == w.Attacker.Move2) && //by move
                        w.HpState > 0
                    )
                .OrderByDescending(o => o.Attacker.Cp)
                .Select(s => s.Attacker)
                .FirstOrDefault();

            if (newAttacker == null)
            {
                TimedLog("No best found, takeing by CP");
                newAttacker = session.GymState.MyTeam.Where(w => w.HpState > 0)
                .OrderByDescending(o => o.Attacker.Cp)
                .Select(s => s.Attacker)
                .FirstOrDefault();
            }

            if (newAttacker != null)
                TimedLog(string.Format("New atacker to switch will be {0} {1} CP {2}", newAttacker.PokemonId, newAttacker.Cp, newAttacker.Id));

            return newAttacker;
        }

        private static IEnumerable<PokemonData> GetBestToTeam(ISession session, List<PokemonData> myTeam)
        {
            var data = session.GymState.MyPokemons.Where(w =>
                        !myTeam.Any(a => a.Id == w.Data.Id) && //not already in team
                        string.IsNullOrEmpty(w.Data.DeployedFortId) && //not already deployed
                        session.Profile.PlayerData.BuddyPokemon?.Id != w.Data.Id //not a buddy
                    )
                .Select(s => s.Data)
                .OrderByDescending(o => o.Cp)
                .Take(6 - myTeam.Count());
            TimedLog("Best others are: " + string.Join(", ", data.Select(s => s.PokemonId)));
            return data;
        }

        public static IEnumerable<PokemonType> GetBestTypes(PokemonType defencTeype)
        {
            switch (defencTeype)
            {
                case PokemonType.Bug:
                    return new PokemonType[] { PokemonType.Rock, PokemonType.Fire, PokemonType.Flying };
                case PokemonType.Dark:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Fairy, PokemonType.Fighting };
                case PokemonType.Dragon:
                    return new PokemonType[] { PokemonType.Dragon, PokemonType.Fire, PokemonType.Ice };
                case PokemonType.Electric:
                    return new PokemonType[] { PokemonType.Ground };
                case PokemonType.Fairy:
                    return new PokemonType[] { PokemonType.Poison, PokemonType.Steel };
                case PokemonType.Fighting:
                    return new PokemonType[] { PokemonType.Fairy, PokemonType.Flying, PokemonType.Psychic };
                case PokemonType.Fire:
                    return new PokemonType[] { PokemonType.Ground, PokemonType.Rock, PokemonType.Water };
                case PokemonType.Flying:
                    return new PokemonType[] { PokemonType.Electric, PokemonType.Ice, PokemonType.Rock };
                case PokemonType.Ghost:
                    return new PokemonType[] { PokemonType.Dark, PokemonType.Ghost };
                case PokemonType.Grass:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Fire, PokemonType.Flying, PokemonType.Ice, PokemonType.Poison };
                case PokemonType.Ground:
                    return new PokemonType[] { PokemonType.Grass, PokemonType.Ice, PokemonType.Water };
                case PokemonType.Ice:
                    return new PokemonType[] { PokemonType.Fighting, PokemonType.Fire, PokemonType.Rock, PokemonType.Steel };
                case PokemonType.None:
                    return new PokemonType[] { };
                case PokemonType.Normal:
                    return new PokemonType[] { PokemonType.Fighting };
                case PokemonType.Poison:
                    return new PokemonType[] { PokemonType.Ground, PokemonType.Psychic };
                case PokemonType.Psychic:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Dark, PokemonType.Ghost };
                case PokemonType.Rock:
                    return new PokemonType[] { PokemonType.Fighting, PokemonType.Grass, PokemonType.Ground, PokemonType.Steel, PokemonType.Water };
                case PokemonType.Steel:
                    return new PokemonType[] { PokemonType.Fighting, PokemonType.Fire, PokemonType.Ground };
                case PokemonType.Water:
                    return new PokemonType[] { PokemonType.Electric, PokemonType.Grass };

                default:
                    return null;
            }
        }

        public static IEnumerable<PokemonType> GetWorstTypes(PokemonType defencTeype)
        {
            switch (defencTeype)
            {
                case PokemonType.Bug:
                    return new PokemonType[] { PokemonType.Fighting, PokemonType.Grass, PokemonType.Ground };
                case PokemonType.Dark:
                    return new PokemonType[] { PokemonType.Dark, PokemonType.Ghost };
                case PokemonType.Dragon:
                    return new PokemonType[] { PokemonType.Electric, PokemonType.Fire, PokemonType.Grass, PokemonType.Water };
                case PokemonType.Electric:
                    return new PokemonType[] { PokemonType.Electric, PokemonType.Flying, PokemonType.Steel };
                case PokemonType.Fairy:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Dark, PokemonType.Dragon, PokemonType.Fighting };
                case PokemonType.Fighting:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Dark, PokemonType.Rock };
                case PokemonType.Fire:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Fire, PokemonType.Fairy, PokemonType.Grass, PokemonType.Ice, PokemonType.Steel };
                case PokemonType.Flying:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Fighting, PokemonType.Grass };
                case PokemonType.Ghost:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Poison };
                case PokemonType.Grass:
                    return new PokemonType[] { PokemonType.Electric, PokemonType.Grass, PokemonType.Ground, PokemonType.Water };
                case PokemonType.Ground:
                    return new PokemonType[] { PokemonType.Poison, PokemonType.Rock };
                case PokemonType.Ice:
                    return new PokemonType[] { PokemonType.Ice };
                case PokemonType.None:
                    return new PokemonType[] { };
                case PokemonType.Normal:
                    return new PokemonType[] { };
                case PokemonType.Poison:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Fairy, PokemonType.Fighting, PokemonType.Grass, PokemonType.Poison };
                case PokemonType.Psychic:
                    return new PokemonType[] { PokemonType.Fighting, PokemonType.Psychic };
                case PokemonType.Rock:
                    return new PokemonType[] { PokemonType.Fire, PokemonType.Flying, PokemonType.Normal, PokemonType.Poison };
                case PokemonType.Steel:
                    return new PokemonType[] { PokemonType.Bug, PokemonType.Dragon, PokemonType.Fairy, PokemonType.Flying, PokemonType.Grass, PokemonType.Ice, PokemonType.Normal, PokemonType.Psychic, PokemonType.Rock, PokemonType.Steel };
                case PokemonType.Water:
                    return new PokemonType[] { PokemonType.Fire, PokemonType.Ice, PokemonType.Steel, PokemonType.Water };

                default:
                    return null;
            }
        }

        public static async Task RevivePokemon(ISession session, PokemonData pokemon)
        {
            int healPower = 0;

            if (session.LogicSettings.GymConfig.SaveMaxRevives && await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false) > 0)
                healPower = Int32.MaxValue;
            else
            {
                var normalPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
                var superPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
                var hyperPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);

                healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;
            }

            var normalRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemRevive).ConfigureAwait(false);
            var maxRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive).ConfigureAwait(false);

            if ((healPower >= pokemon.StaminaMax / 2 || maxRevives == 0) && normalRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemRevive, pokemon.Id).ConfigureAwait(false);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemRevive).ConfigureAwait(false);
                        pokemon.Stamina = ret.Stamina;
                        session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "normal",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (normalRevives - 1)
                        });
                        break;
                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Logger.Write(
                            $"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;
                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;
                    default:
                        return;
                }
                return;
            }

            if (maxRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemMaxRevive, pokemon.Id).ConfigureAwait(false);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemMaxRevive).ConfigureAwait(false);
                        pokemon.Stamina = ret.Stamina;
                        session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "max",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (maxRevives - 1)
                        });
                        break;

                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Logger.Write($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
            }
        }

        private static async Task<bool> UsePotion(ISession session, PokemonData pokemon, int normalPotions)
        {
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "normal",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (normalPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseSuperPotion(ISession session, PokemonData pokemon, int superPotions)
        {
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemSuperPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "super",
                        PokemonCp = pokemon.Cp,

                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (superPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseHyperPotion(ISession session, PokemonData pokemon, int hyperPotions)
        {
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemHyperPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "hyper",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (hyperPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseMaxPotion(ISession session, PokemonData pokemon, int maxPotions)
        {
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemMaxPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "max",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = maxPotions
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        public static async Task<bool> HealPokemon(ISession session, PokemonData pokemon)
        {
            var normalPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
            var superPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
            var hyperPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);
            var maxPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false);

            var healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;

            if (healPower < (pokemon.StaminaMax - pokemon.Stamina) && maxPotions > 0)
            {
                try
                {
                    if (await UseMaxPotion(session, pokemon, maxPotions).ConfigureAwait(false))
                    {
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemMaxPotion).ConfigureAwait(false);
                        return true;
                    }
                }
                catch (APIBadRequestException)
                {
                    Logger.Write(string.Format("Heal problem with max potions ({0}) on pokemon: {1}", maxPotions, pokemon), LogLevel.Error, ConsoleColor.Magenta);
                }
            }

            while (normalPotions + superPotions + hyperPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                if (((pokemon.StaminaMax - pokemon.Stamina) > 200 || ((normalPotions * 20 + superPotions * 50) < (pokemon.StaminaMax - pokemon.Stamina))) && hyperPotions > 0)
                {
                    if (!await UseHyperPotion(session, pokemon, hyperPotions).ConfigureAwait(false))
                        return false;
                    hyperPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemHyperPotion).ConfigureAwait(false);
                }
                else
                if (((pokemon.StaminaMax - pokemon.Stamina) > 50 || normalPotions * 20 < (pokemon.StaminaMax - pokemon.Stamina)) && superPotions > 0)
                {
                    if (!await UseSuperPotion(session, pokemon, superPotions).ConfigureAwait(false))
                        return false;
                    superPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemSuperPotion).ConfigureAwait(false);
                }
                else
                {
                    if (!await UsePotion(session, pokemon, normalPotions).ConfigureAwait(false))
                        return false;
                    normalPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemPotion).ConfigureAwait(false);
                }
            }

            return pokemon.Stamina == pokemon.StaminaMax;
        }

        private static int _currentAttackerEnergy;

        private static async Task<List<BattleAction>> AttackGym(ISession session, CancellationToken cancellationToken, GymGetInfoResponse currentFortData, GymStartSessionResponse startResponse, int counter, FortData fort)
        {
            long serverMs = startResponse.Battle.BattleLog.BattleStartTimestampMs;
            var lastActions = startResponse.Battle.BattleLog.BattleActions.ToList();

            Logger.Write($"Gym battle started; fighting trainer: {startResponse.Battle.Defender.TrainerPublicProfile.Name}", LogLevel.Gym, ConsoleColor.Green);
            Logger.Write($"We are attacking: {startResponse.Battle.Defender.ActivePokemon.PokemonData.PokemonId} ({startResponse.Battle.Defender.ActivePokemon.PokemonData.Cp} CP), Lvl: {startResponse.Battle.Defender.ActivePokemon.PokemonData.Level():0.0}", LogLevel.Gym, ConsoleColor.White);
            Console.WriteLine(Environment.NewLine);

            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                await PushNotificationClient.SendNotification(session, $"Gym battle started", $"Trainer: {startResponse.Battle.Defender.TrainerPublicProfile.Name}\n" +
                                                                       $"We are attacking: {startResponse.Battle.Defender.ActivePokemon.PokemonData.PokemonId} ({startResponse.Battle.Defender.ActivePokemon.PokemonData.Cp} CP)\n" +
                                                                       $"Lvl: {startResponse.Battle.Defender.ActivePokemon.PokemonData.Level():0.0}", true).ConfigureAwait(false);

            int loops = 0;
            List<BattleAction> emptyActions = new List<BattleAction>();
            BattleAction emptyAction = new BattleAction();
            PokemonData attacker = null;
            PokemonData defender = null;

            _currentAttackerEnergy = 0;
            bool wasSwithed = false;

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

                    TimedLog("Starts loop");
                    var last = lastActions.Where(w => !session.GymState.MyTeam.Any(a => a.Attacker.Id.Equals(w.ActivePokemonId))).LastOrDefault();
                    BattleAction lastSpecialAttack = lastActions.Where(w => !session.GymState.MyTeam.Any(a => a.Attacker.Id.Equals(w.ActivePokemonId)) && w.Type == BattleActionType.ActionSpecialAttack).LastOrDefault();

                    TimedLog("Getting actions");
                    var attackActionz = last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyActions : GetActions(session, serverMs, attacker, defender, _currentAttackerEnergy, last, lastSpecialAttack);

                    TimedLog(string.Format("Going to make attack : {0}", string.Join(", ", attackActionz.Select(s => string.Format("{0} -> {1}", s.Type, s.DurationMs)))));

                    BattleAction a2 = (last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyAction : last);
                    GymBattleAttackResponse attackResult = null;
                    try
                    {
                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            TimedLog("Etra wait before SWAP call");
                            await Task.Delay(1000).ConfigureAwait(false);
                        }

                        TimedLog("Start making attack");
                        long timeBefore = DateTime.UtcNow.ToUnixTime();
                        attackResult = await session.Client.Fort.GymBattleAttak(fort.Id, startResponse.Battle.BattleId, attackActionz, a2, serverMs).ConfigureAwait(false);
                        long timeAfter = DateTime.UtcNow.ToUnixTime();
                        TimedLog(string.Format("Finished making attack call: {0}", timeAfter - timeBefore));

                        var attackTime = attackActionz.Sum(x => x.DurationMs);
                        int attackTimeCorrected = attackTime;

                        if (attackActionz.Any(a => a.Type != BattleActionType.ActionSpecialAttack))
                            attackTimeCorrected = attackTime - (int)(timeAfter - timeBefore);

                        TimedLog(string.Format("Waiting for attack to be prepared: {0} (last call was {1}, after correction {2})", attackTime, timeAfter, attackTimeCorrected > 0 ? attackTimeCorrected : 0));
                        if (attackTimeCorrected > 0)
                            await Task.Delay(attackTimeCorrected).ConfigureAwait(false);

                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            TimedLog("Etra wait after SWAP call");
                            await Task.Delay(2000).ConfigureAwait(false);
                        }
                    }
                    catch (APIBadRequestException)
                    {
                        Logger.Write("Bad attack gym", LogLevel.Warning);
                        TimedLog(string.Format("Last retrieved action was: {0}", a2));
                        TimedLog(string.Format("Actions to perform were: {0}", string.Join(", ", attackActionz)));
                        TimedLog(string.Format("Attacker was: {0}, defender was: {1}", attacker, defender));

                        continue;
                    };

                    loops++;

                    if (attackResult.Result == GymBattleAttackResponse.Types.Result.Success)
                    {
                        TimedLog("Attack success");
                        defender = attackResult.BattleUpdate.ActiveDefender?.PokemonData;
                        if (attackResult.BattleUpdate.BattleLog != null && attackResult.BattleUpdate.BattleLog.BattleActions.Count > 0)
                        {
                            var result = attackResult.BattleUpdate.BattleLog.BattleActions.OrderBy(o => o.ActionStartMs).Distinct();
                            lastActions.AddRange(result);
                            try
                            {
                                TimedLog("Result -> \r\n" + string.Join(Environment.NewLine, result));
                            }
                            catch (Exception) { }
                        }
                        serverMs = attackResult.BattleUpdate.BattleLog.ServerMs;

                        switch (attackResult.BattleUpdate.BattleLog.State)
                        {
                            case BattleState.Active:
                                _currentAttackerEnergy = attackResult.BattleUpdate.ActiveAttacker.CurrentEnergy;
                                if (attacker == null) //start battle
                                {
                                    if (counter == 1 || currentFortData.GymStatusAndDefenders.GymDefender.Count == 1 || session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //first iteration, we have good attacker
                                        attacker = attackResult.BattleUpdate.ActiveAttacker.PokemonData;
                                    else //next iteration so we should to swith to proper attacker for new defender
                                    {
                                        var newAttacker = GetBestInBattle(session, attackResult.BattleUpdate.ActiveDefender.PokemonData);
                                        if (newAttacker != null && newAttacker.Id != attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id)
                                        {
                                            session.GymState.SwithAttacker = new SwitchPokemonData(attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id, newAttacker.Id);
                                            wasSwithed = true;
                                        }
                                    }
                                }
                                else if (attacker != null && attacker.Id != attackResult?.BattleUpdate.ActiveAttacker?.PokemonData.Id) //we died and pokemon is switched to next one
                                {
                                    bool informDie = true;
                                    bool extraWait = true;
                                    if (!session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //we should manually switch pokemon to best one
                                    {
                                        if (!wasSwithed && currentFortData.GymStatusAndDefenders.GymDefender.Count > 1) //swap call wasn't already called, do job
                                        {
                                            session.GymState.MyTeam.Where(w => w.Attacker.Id == attacker.Id).FirstOrDefault().HpState = 0;
                                            var newAttacker = GetBestInBattle(session, attackResult.BattleUpdate.ActiveDefender.PokemonData);
                                            if (newAttacker != null && newAttacker.Id != attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id)
                                            {
                                                session.GymState.SwithAttacker = new SwitchPokemonData(attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id, newAttacker.Id);
                                                wasSwithed = true;
                                                informDie = false; //don't inform, we just prepared swap call...
                                                extraWait = false; //don't wait, this is in swap call
                                            }
                                        }
                                        else
                                        {
                                            wasSwithed = false;
                                            informDie = false;
                                            extraWait = false; //we already waited in swap call
                                        }
                                    }
                                    if (informDie)
                                    {
                                        Logger.Write(string.Format("Our Pokemon has fainted in battle, our new attacker is: {0} ({1} CP)", attacker.PokemonId, attacker.Cp), LogLevel.Info, ConsoleColor.Magenta);
                                        Logger.Write("");
                                    }
                                    if (extraWait)
                                    {
                                        TimedLog("Death penalty applied");
                                        await Task.Delay(1000).ConfigureAwait(false);
                                    }
                                }

                                attacker = attackResult.BattleUpdate.ActiveAttacker.PokemonData;
                                defender = attackResult.BattleUpdate.ActiveDefender.PokemonData;

                                var fortDetails = session.GymState.GymGetInfo(session, fort, true); //await session.Client.Fort.GetGymDetails(gym.Id, gym.Latitude, gym.Longitude).ConfigureAwait(false);
                                var player = session.Profile.PlayerData;
                                await EnsureJoinTeam(session, player).ConfigureAwait(false);
                                var ev = fort.OwnedByTeam;
                                if (AttackStart > DateTime.Now) { AttackStart = DateTime.Now; }

                                //Console.SetCursorPosition(0, Console.CursorTop - 1);
                                Logger.Write($"(DEFENDER): {defender.PokemonId.ToString(),-12} | HP: {attackResult.BattleUpdate.ActiveDefender.CurrentHealth,3:##0} | Sta: {attackResult.BattleUpdate.ActiveDefender.CurrentEnergy,3:##0} | Lvl: {attackResult.BattleUpdate.ActiveDefender.PokemonData.Level(),4:#0.0}", LogLevel.Gym,
                                    (ev == TeamColor.Red)
                                        ? ConsoleColor.Red
                                        : (ev == TeamColor.Yellow ? ConsoleColor.Yellow : ConsoleColor.Blue));
                                Logger.Write($"(ATTACKER): {attacker.PokemonId.ToString(),-12} | HP: {attackResult.BattleUpdate.ActiveAttacker.CurrentHealth,3:##0} | Sta: {attackResult.BattleUpdate.ActiveAttacker.CurrentEnergy,3:##0} | Lvl: {attackResult.BattleUpdate.ActiveAttacker.PokemonData.Level(),4:#0.0}", LogLevel.Gym,
                                    (player.Team == TeamColor.Red)
                                        ? ConsoleColor.Red
                                        : (player.Team == TeamColor.Yellow ? ConsoleColor.Yellow : ConsoleColor.Blue));

                                TimeSpan BattleTimer = DateTime.Now.Subtract(AttackStart);

                                Logger.Write($"Battle Timer: {100 - BattleTimer.TotalSeconds,3:##0} Sec remaining.", LogLevel.Info, ConsoleColor.White);

                                if (attackResult != null && attackResult.BattleUpdate.ActiveAttacker != null)
                                    session.GymState.MyTeam.Where(w => w.Attacker.Id == attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id).FirstOrDefault().HpState = attackResult.BattleUpdate.ActiveAttacker.CurrentHealth;
                                break;
                            case BattleState.Defeated:
                                Logger.Write($"We have been defeated... (AttackGym)");
                                return lastActions;
                            case BattleState.TimedOut:
                                Logger.Write($"Our attack timed out...:");
                                if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                                    await PushNotificationClient.SendNotification(session, "Gym Battle", $"Our attack timed out...:", true).ConfigureAwait(false);
                                await Task.Delay(1000).ConfigureAwait(false);
                                Logger.Write($"Try again...:");
                                var fortInfo = await session.Client.Fort.GetFort(fort.Id, fort.Latitude, fort.Longitude).ConfigureAwait(false);
                                await Execute(session, session.CancellationTokenSource.Token, fort, fortInfo).ConfigureAwait(false);
                                break;// return lastActions;
                            case BattleState.StateUnset:
                                Logger.Write($"State was unset?: {attackResult}");
                                return lastActions;

                            case BattleState.Victory:
                                Logger.Write($"We were victorious!: ");
                                await Task.Delay(2000).ConfigureAwait(false);
                                return lastActions;
                            default:
                                Logger.Write($"Unhandled attack response: {attackResult}");
                                continue;
                        }
                        Debug.WriteLine($"{attackResult}", "GYM: " + DateTime.UtcNow.ToUnixTime());
                    }
                    else
                    {
                        Logger.Write($"Unexpected attack result: {attackResult}");
                        TimedLog("Attack: " + string.Join(Environment.NewLine, attackActionz), true);
                        break;
                    }

                    TimedLog("Finished attack");
                }
                catch (APIBadRequestException e)
                {
                    Logger.Write("Bad request sent to server -", LogLevel.Warning);
                    TimedLog("NOT finished attack");
                    TimedLog(e.Message);
                };
            }
            return lastActions;

        }

        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

        public static List<BattleAction> GetActions(ISession session, long serverMs, PokemonData attacker, PokemonData defender, int energy, BattleAction lastAction, BattleAction lastSpecialAttack)
        {
            List<BattleAction> actions = new List<BattleAction>();
            DateTime now = DateTimeFromUnixTimestampMillis(serverMs);
            const int beforeDodge = 200;

            if (session.GymState.SwithAttacker != null)
            {
                actions.Add(new BattleAction()
                {
                    Type = BattleActionType.ActionSwapPokemon,
                    DurationMs = session.GymState.SwithAttacker.AttackDuration,
                    ActionStartMs = serverMs,
                    ActivePokemonId = session.GymState.SwithAttacker.OldAttacker,
                    TargetPokemonId = session.GymState.SwithAttacker.NewAttacker,
                    TargetIndex = -1,
                });
                TimedLog(string.Format("Trying to switch pokemon: {0} to: {1}, serverMs: {2}", session.GymState.SwithAttacker.OldAttacker, session.GymState.SwithAttacker.NewAttacker, serverMs));
                session.GymState.SwithAttacker = null;
                return actions;
            }

            if (lastSpecialAttack != null && lastSpecialAttack.DamageWindowsStartTimestampMs > serverMs)
            {
                long dodgeTime = lastSpecialAttack.DamageWindowsStartTimestampMs - beforeDodge;
                if (session.GymState.TimeToDodge < dodgeTime)
                    session.GymState.TimeToDodge = dodgeTime;
            }

            if (attacker != null && defender != null)
            {
                var normalMove = session.GymState.MyPokemons.FirstOrDefault(f => f.Data.Id == attacker.Id).Attack;
                var specialMove = session.GymState.MyPokemons.FirstOrDefault(f => f.Data.Id == attacker.Id).SpecialAttack;

                bool skipDodge = ((lastSpecialAttack?.DurationMs ?? 0) < normalMove.DurationMs + 550) || session.LogicSettings.GymConfig.DontUseDodge; //if our normal attack is too slow and defender special is too fast so we should to only do dodge all the time then we totally skip dodge

                bool canDoSpecialAttack = Math.Abs(specialMove.EnergyDelta) <= energy && (!(session.GymState.TimeToDodge > now.ToUnixTime() && session.GymState.TimeToDodge < now.ToUnixTime() + specialMove.DurationMs) || skipDodge);
                if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == attacker.PokemonId && a.Value == specialMove.MovementId))
                    canDoSpecialAttack = false;

                bool canDoAttack = !canDoSpecialAttack && (!(session.GymState.TimeToDodge > now.ToUnixTime() && session.GymState.TimeToDodge < now.ToUnixTime() + normalMove.DurationMs) || skipDodge);

                if (session.GymState.TimeToDodge > now.ToUnixTime() && !canDoAttack && !canDoSpecialAttack && !skipDodge)
                {
                    session.GymState.LastWentDodge = now.ToUnixTime();

                    BattleAction dodge = new BattleAction()
                    {
                        Type = BattleActionType.ActionDodge,
                        ActionStartMs = now.ToUnixTime(),
                        DurationMs = 500,
                        TargetIndex = -1,
                        ActivePokemonId = attacker.Id,
                    };

                    TimedLog(string.Format("Trying to dodge an attack {0}, lastSpecialAttack.DamageWindowsStartTimestampMs: {1}, serverMs: {2}", dodge, lastSpecialAttack.DamageWindowsStartTimestampMs, serverMs));
                    actions.Add(dodge);
                }
                else
                {
                    BattleAction action2 = new BattleAction();
                    if (canDoSpecialAttack)
                    {
                        action2.Type = BattleActionType.ActionSpecialAttack;
                        action2.DurationMs = specialMove.DurationMs;
                        action2.DamageWindowsStartTimestampMs = specialMove.DamageWindowStartMs;
                        action2.DamageWindowsEndTimestampMs = specialMove.DamageWindowEndMs;
                        TimedLog(string.Format("Trying to make an special attack {0}, on: {1}, duration: {2}", specialMove.MovementId, serverMs, specialMove.DurationMs));
                    }
                    else if (canDoAttack)
                    {
                        action2.Type = BattleActionType.ActionAttack;
                        action2.DurationMs = normalMove.DurationMs;
                        action2.DamageWindowsStartTimestampMs = normalMove.DamageWindowStartMs;
                        action2.DamageWindowsEndTimestampMs = normalMove.DamageWindowEndMs;
                        TimedLog(string.Format("Trying to make an normal attack {0}, on: {1}, duration: {2}", normalMove.MovementId, serverMs, normalMove.DurationMs));
                    }
                    else
                    {
                        TimedLog("SHIT", true);
                    }
                    action2.ActionStartMs = now.ToUnixTime();
                    action2.TargetIndex = -1;
                    if (attacker.Stamina > 0)
                        action2.ActivePokemonId = attacker.Id;
                    action2.TargetPokemonId = defender.Id;
                    actions.Add(action2);
                }
                return actions;
            }
            BattleAction action1 = new BattleAction()
            {
                Type = BattleActionType.ActionDodge,
                DurationMs = 500,
                ActionStartMs = now.ToUnixTime(),
                TargetIndex = -1
            };
            if (defender != null && attacker != null)
                action1.ActivePokemonId = attacker.Id;

            actions.Add(action1);
            return actions;
        }

        private static async Task<GymStartSessionResponse> StartBattle(ISession session, FortData gym, IEnumerable<PokemonData> attackers, ulong defenderId)
        {

            IEnumerable<PokemonData> currentPokemons = attackers;

            var pokemonDatas = currentPokemons as PokemonData[] ?? currentPokemons.ToArray();
            var attackerPokemons = pokemonDatas.Select(pokemon => pokemon.Id);
            var attackingPokemonIds = attackerPokemons as ulong[] ?? attackerPokemons.ToArray();

            try
            {
                var result = await session.Client.Fort.GymStartSession(gym.Id, defenderId, attackingPokemonIds).ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);

                if (result.Result == GymStartSessionResponse.Types.Result.Success)
                {
                    switch (result.Battle.BattleLog.State)
                    {
                        case BattleState.Active:
                            Logger.Write("Starting new battle...");
                            return result;
                        case BattleState.Defeated:
                            Logger.Write($"We have been defeated in battle.");
                            return result;
                        case BattleState.Victory:
                            Logger.Write($"We were victorious");
                            return result;
                        case BattleState.StateUnset:
                            Logger.Write($"Error occoured: {result.Battle.BattleLog.State}");
                            break;
                        case BattleState.TimedOut:
                            Logger.Write($"Error occoured: {result.Battle.BattleLog.State}");
                            break;
                        default:
                            Logger.Write($"Unhandled occoured: {result.Battle.BattleLog.State}");
                            break;
                    }
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorGymBattleLockout)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorAllPokemonFainted)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorGymEmpty)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorGymNeutral)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorGymNotFound)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorGymWrongTeam)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorInvalidDefender)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorNotInRange)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorPlayerBelowMinimumLevel)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorPoiInaccessible)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorRaidActive)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorTooManyBattles)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorTooManyPlayers)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.ErrorTrainingInvalidAttackerCount)
                {
                    return result;
                }
                else if (result.Result == GymStartSessionResponse.Types.Result.Unset)
                {
                    return result;
                }
                return result;
            }
            catch (APIBadRequestException e)
            {
                TimedLog("Gym Details: " + gym);
                throw e;
            }
        }

        private static async Task EnsureJoinTeam(ISession session, PlayerData player)
        {
            if (session.Profile.PlayerData.Team == TeamColor.Neutral)
            {
                var defaultTeam = (TeamColor)Enum.Parse(typeof(TeamColor), session.LogicSettings.GymConfig.DefaultTeam);
                var teamResponse = await session.Client.Player.SetPlayerTeam(defaultTeam).ConfigureAwait(false);
                if (teamResponse.Status == SetPlayerTeamResponse.Types.Status.Success)
                {
                    player.Team = defaultTeam;
                }

                session.EventDispatcher.Send(new GymTeamJoinEvent()
                {
                    Team = defaultTeam,
                    Status = teamResponse.Status
                });
            }
        }

        internal static int GetGymAllCpOnGym(List<PokemonData> pokemonPlayers)//
        {
            int allCp = 0;
            foreach (var x in pokemonPlayers)
                allCp = allCp + x.Cp;
            return allCp;
        }

        internal static bool CanAttackGym(ISession session, FortData fort, IEnumerable<PokemonData> deployedPokemons)
        {
            if (!session.LogicSettings.GymConfig.EnableAttackGym)
                return false;
            if (fort.OwnedByTeam == session.Profile.PlayerData.Team)
                return false;
            if (session.LogicSettings.GymConfig.MaxGymPlayersToAttack <= 6)
                return false;
            if (deployedPokemons != null && session.LogicSettings.GymConfig.DontAttackAfterCoinsLimitReached && deployedPokemons.Count() >= session.LogicSettings.GymConfig.CollectCoinAfterDeployed)
                return false;
            return true;
        }

        internal static bool CanBerrieGym(ISession session, FortData fort, IEnumerable<PokemonData> deployedPokemons)
        {
            try
            {
                GymGetInfoResponse gymDetails = session.GymState.GymGetInfo(session, fort);
                FortDetailsResponse fortInfo = session.Client.Fort.GetFort(fort.Id, fort.Latitude, fort.Longitude).Result;

                if (session.Profile.PlayerData.Team == TeamColor.Neutral)
                    return false;

                if (session.GymState.CapturedGymId.Equals(fort.Id) && fort.OwnedByTeam != session.Profile.PlayerData.Team)
                    gymDetails = session.GymState.GymGetInfo(session, fort, true);

                if (gymDetails?.Result != GymGetInfoResponse.Types.Result.Success)
                    return false;

                if (session.GymState.CapturedGymId.Equals(fort.Id))
                    fort.OwnedByTeam = session.Profile.PlayerData.Team;

                bool isDeployed = deployedPokemons != null && deployedPokemons.Count() > 0 ? deployedPokemons.Any(a => a?.DeployedFortId == fort.Id) : false;
                if (gymDetails != null && gymDetails.GymStatusAndDefenders.GymDefender.Count < 6 && !isDeployed) // free slot should be used always but not always we know that...
                    return true;
                if (!session.LogicSettings.GymConfig.EnableGymBerries)
                    return false;
                if (fort.OwnedByTeam != session.Profile.PlayerData.Team)
                    return false;
                if (!session.LogicSettings.GymConfig.TrainAlreadyDefendedGym && isDeployed)
                    return false;
                if (session.LogicSettings.GymConfig.MaxGymPlayerToSendBerries <= gymDetails.GymStatusAndDefenders.GymDefender.Count)
                    return false;
                if (GetGymAllCpOnGym(gymDetails.GymStatusAndDefenders.GymDefender.Select(x => x.MotivatedPokemon.Pokemon).ToList()) > session.LogicSettings.GymConfig.BerriesGymWhenMissingMaxCP)
                    return false;
                if (deployedPokemons != null && session.LogicSettings.GymConfig.DontAttackAfterCoinsLimitReached && deployedPokemons.Count() >= session.LogicSettings.GymConfig.CollectCoinAfterDeployed)
                    return false;
            }
            catch (Exception ex)
            {
                TimedLog(string.Format("{0} -> {1} -> {2}", ex.Message, string.Join(", ", deployedPokemons), fort));
                return false;
            }
            return true;
        }

        internal static bool CanDeployToGym(ISession session, FortData fort, IEnumerable<PokemonData> deployedPokemons)
        {
            GymGetInfoResponse gymDetails = session.GymState.GymGetInfo(session, fort);

            if (deployedPokemons.Any(a => a.DeployedFortId.Equals(fort.Id)))
                return false;

            if (fort.OwnedByTeam == TeamColor.Neutral)
                return true;

            if (gymDetails != null && fort.OwnedByTeam == session.Profile.PlayerData.Team && gymDetails.GymStatusAndDefenders.GymDefender.Count < 6)
                return true;

            return false;
        }

        private static async Task<PokemonData> GetDeployablePokemon(ISession session)
        {
            PokemonData pokemon = null;
            List<ulong> excluded = new List<ulong>();
            var pokemonList = (await session.Inventory.GetPokemons().ConfigureAwait(false)).ToList();
            pokemonList.RemoveAll(x => session.LogicSettings.GymConfig.ExcludeForGyms.Contains(x.PokemonId));

            if (session.LogicSettings.GymConfig.Defenders != null && session.LogicSettings.GymConfig.Defenders.Count > 0)
            {
                foreach (var def in session.LogicSettings.GymConfig.Defenders.OrderByDescending(o => o.Priority))
                {
                    var defendersFromConfig = pokemonList.Where(w =>
                        w.PokemonId == def.Pokemon &&
                        w.Id != session.Profile.PlayerData.BuddyPokemon?.Id &&
                        string.IsNullOrEmpty(w.DeployedFortId) &&
                        w.Cp >= (def.MinCP ?? 0) &&
                        w.Cp <= (def.MaxCP ?? 5000) &&
                        def.IsMoveMatch(w.Move1, w.Move2)
                    ).ToList();

                    if (defendersFromConfig != null && defendersFromConfig.Count > 0)
                        foreach (var _pokemon in defendersFromConfig.OrderByDescending(o => o.Cp))
                        {
                            if (session.LogicSettings.GymConfig.HealDefendersBeforeApplyToGym)
                            {
                                if (_pokemon.Stamina <= 0)
                                    await RevivePokemon(session, _pokemon).ConfigureAwait(false);

                                if (_pokemon.Stamina < _pokemon.StaminaMax && _pokemon.Stamina > 0)
                                    await HealPokemon(session, _pokemon).ConfigureAwait(false);
                            }

                            if (_pokemon.Stamina < _pokemon.StaminaMax)
                                excluded.Add(pokemon.Id);
                            else
                                return _pokemon;
                        }
                }

            }

            while (pokemon == null)
            {
                pokemonList = pokemonList
                    .Where(w => !excluded.Contains(w.Id) && w.Id != session.Profile.PlayerData.BuddyPokemon?.Id)
                    .OrderByDescending(p => p.Cp)
                    .Skip(Math.Min(pokemonList.Count - 1, session.LogicSettings.GymConfig.NumberOfTopPokemonToBeExcluded))
                    .ToList();

                if (pokemonList.Count == 0)
                    return null;

                if (pokemonList.Count == 1)
                    pokemon = pokemonList.FirstOrDefault();

                if (session.LogicSettings.GymConfig.UseRandomPokemon && pokemon == null)
                    pokemon = pokemonList.ElementAt(new Random().Next(0, pokemonList.Count - 1));

                pokemon = pokemonList.FirstOrDefault(p =>
                    p.Cp <= session.LogicSettings.GymConfig.MaxCPToDeploy &&
                    PokemonInfo.GetLevel(p) <= session.LogicSettings.GymConfig.MaxLevelToDeploy &&
                    string.IsNullOrEmpty(p.DeployedFortId)
                );

                if (session.LogicSettings.GymConfig.HealDefendersBeforeApplyToGym)
                {
                    if (pokemon.Stamina <= 0)
                        await RevivePokemon(session, pokemon).ConfigureAwait(false);

                    if (pokemon.Stamina < pokemon.StaminaMax && pokemon.Stamina > 0)
                        await HealPokemon(session, pokemon).ConfigureAwait(false);
                }

                if (pokemon.Stamina < pokemon.StaminaMax)
                {
                    excluded.Add(pokemon.Id);
                    pokemon = null;
                }
            }
            return pokemon;
        }

        private static void TimedLog(string message, bool force)
        {
            if (_logTimings || force)
                Logger.Write(string.Format("{0} {1}", DateTime.UtcNow.ToUnixTime(), message), LogLevel.Gym, ConsoleColor.Magenta);
        }

        private static void TimedLog(string message)
        {
            if (_logTimings)
                Logger.Write(string.Format("{0} {1}", DateTime.UtcNow.ToUnixTime(), message), LogLevel.Gym, ConsoleColor.Magenta);
        }
    }
}
