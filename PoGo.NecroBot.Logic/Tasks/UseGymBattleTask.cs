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
        private static int _startBattleCounter = 3;
        private static readonly bool _logTimings = false;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static async Task Execute(ISession session, CancellationToken cancellationToken, FortData gym, FortDetailsResponse fortInfo)
        {
            if (!session.LogicSettings.GymConfig.Enable || gym.Type != FortType.Gym) return;

            if (session.GymState.moveSettings == null)
            {
                session.GymState.moveSettings = await session.Inventory.GetMoveSettings();
            }

            session.GymState.LoadMyPokemons(session);


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

                var fortDetails = session.GymState.getGymDetails(session, gym, true); //await session.Client.Fort.GetGymDetails(gym.Id, gym.Latitude, gym.Longitude);

                if (fortDetails.Result == GetGymDetailsResponse.Types.Result.Success)
                {
                    var player = session.Profile.PlayerData;
                    await EnsureJoinTeam(session, player);

                    session.EventDispatcher.Send(new GymDetailInfoEvent()
                    {
                        Team = fortDetails.GymState.FortData.OwnedByTeam,
                        Point = gym.GymPoints,
                        Name = fortDetails.Name,
                    });

                    if (player.Team != TeamColor.Neutral)
                    {
                        var deployedPokemons = session.Inventory.GetDeployedPokemons();
                        List<PokemonData> deployedList = new List<PokemonData>(deployedPokemons);

                        if (fortDetails.GymState.FortData.OwnedByTeam == player.Team || fortDetails.GymState.FortData.OwnedByTeam == TeamColor.Neutral)
                        {
                            if (!deployedPokemons.Any(a => a.DeployedFortId.Equals(fortInfo.FortId)))
                            {
                                FortDeployPokemonResponse response = await DeployPokemonToGym(session, fortInfo, fortDetails, cancellationToken);

                                if (response != null && response.Result == FortDeployPokemonResponse.Types.Result.Success)
                                {
                                    deployedPokemons = session.Inventory.GetDeployedPokemons();
                                    deployedList = new List<PokemonData>(deployedPokemons);
                                }
                            }

                            if (CanTrainGym(session, gym, deployedList))
                            {
                                if (string.IsNullOrEmpty(session.GymState.trainingGymId) || !session.GymState.trainingGymId.Equals(fortInfo.FortId))
                                {
                                    session.GymState.trainingGymId = fortInfo.FortId;
                                    session.GymState.trainingRound = 0;
                                }
                                session.GymState.trainingRound++;
                                if (session.GymState.trainingRound <= session.LogicSettings.GymConfig.MaxTrainingRoundsOnOneGym)
                                    await StartGymAttackLogic(session, fortInfo, fortDetails, gym, cancellationToken);
                            }
                        }
                        else
                        {
                            if (CanAttackGym(session, gym, deployedList))
                                await StartGymAttackLogic(session, fortInfo, fortDetails, gym, cancellationToken);
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
                Logger.Write($"Ignoring  Gym : {fortInfo?.Name} - ", LogLevel.Gym, ConsoleColor.Cyan);
            }
        }

        private static async Task StartGymAttackLogic(ISession session, FortDetailsResponse fortInfo, GetGymDetailsResponse fortDetails, FortData gym, CancellationToken cancellationToken)
        {
            var defenders = fortDetails.GymState.Memberships.Select(x => x.PokemonData).ToList();

            if (defenders.Count < 1)
                return;

            if (session.Profile.PlayerData.Team != gym.OwnedByTeam)
            {
                if (session.LogicSettings.GymConfig.MaxGymLevelToAttack < GetGymLevel(gym.GymPoints))
                {
                    Logger.Write($"This is gym level {GetGymLevel(gym.GymPoints)} > {session.LogicSettings.GymConfig.MaxGymLevelToAttack} in your config. Bot walk away...", LogLevel.Gym, ConsoleColor.Red);
                    return;
                }

                if (session.LogicSettings.GymConfig.MaxDefendersToAttack < defenders.Count)
                {
                    Logger.Write($"This is gym has   {defenders.Count} defender  > {session.LogicSettings.GymConfig.MaxDefendersToAttack} in your config. Bot walk away...", LogLevel.Gym, ConsoleColor.Red);
                    return;
                }
            }

            if (fortDetails.GymState.FortData.IsInBattle)
            {
                Logger.Write("This gym is under attack now, we will skip it");
                return;
            }

            bool isTraining = (session.Profile.PlayerData.Team == fortDetails.GymState.FortData.OwnedByTeam || (!string.IsNullOrEmpty(session.GymState.capturedGymId) && session.GymState.capturedGymId.Equals(fortDetails.GymState.FortData.Id)));
            var badassPokemon = CompleteAttackTeam(session, defenders, isTraining);
            if (badassPokemon == null)
            {
                Logger.Write("Check gym setting, we cant complete attackers team. Exiting.", LogLevel.Warning, ConsoleColor.Magenta);
                return;
            }
            var pokemonDatas = badassPokemon as PokemonData[] ?? badassPokemon.ToArray();
            if (defenders.Count == 0) return;

            Logger.Write("Start battle with : " + string.Join(", ", defenders.Select(x => x.PokemonId.ToString())));

            foreach (var pokemon in pokemonDatas)
            {
                if (pokemon.Stamina <= 0)
                    await RevivePokemon(session, pokemon);

                if (pokemon.Stamina <= 0)
                {
                    Logger.Write("You are out of revive potions! Can't resurect attacker", LogLevel.Gym, ConsoleColor.Magenta);
                    return;
                }

                if (pokemon.Stamina < pokemon.StaminaMax)
                    await HealPokemon(session, pokemon);

                if (pokemon.Stamina < pokemon.StaminaMax)
                    Logger.Write(string.Format("You are out of healing potions! {0} ({1} CP) haven't got fully healed", pokemon.PokemonId, pokemon.Cp), LogLevel.Gym, ConsoleColor.Magenta);
            }
            //await Task.Delay(2000);

            var index = 0;
            bool isVictory = true;
            bool isFailedToStart = false;
            List<BattleAction> battleActions = new List<BattleAction>();
            ulong defenderPokemonId = defenders.First().Id;

            while (index < defenders.Count())
            {
                TimedLog("Attacking team is: " + string.Join(", ", session.GymState.myTeam.Select(s => string.Format("{0} ({1} HP / {2} CP) [{3}]", s.attacker.PokemonId, s.hpState, s.attacker.Cp, s.attacker.Id))));
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                var thisAttackActions = new List<BattleAction>();

                StartGymBattleResponse result = null;
                try
                {
                    result = await StartBattle(session, gym, pokemonDatas, defenderPokemonId);
                }
                catch (APIBadRequestException)
                {
                    Logger.Write("Can't start battle", LogLevel.Gym);
                    isFailedToStart = true;
                    isVictory = false;
                    _startBattleCounter--;

                    TimedLog("Start battle result: " + result);
                    TimedLog("FortDetais: " + fortDetails);
                    TimedLog("PokemonDatas: " + string.Join(", ", pokemonDatas.Select(s => string.Format("Id: {0} Name: {1} CP: {2} HP: {3}", s.Id, s.PokemonId, s.Cp, s.Stamina))));
                    TimedLog("DefenderId: " + defenderPokemonId);
                    TimedLog("ActionsLog -> " + string.Join(Environment.NewLine, battleActions));

                    break;
                }

                index++;
                // If we can't start battle in 10 tries, let's skip the gym
                if (result == null || result.Result != StartGymBattleResponse.Types.Result.Success)
                {
                    session.EventDispatcher.Send(new GymErrorUnset { GymName = fortInfo.Name });
                    isVictory = false;
                    break;
                }

                switch (result.BattleLog.State)
                {
                    case BattleState.Active:
                        _startBattleCounter = 3;
                        Logger.Write($"Time to start Attack Mode", LogLevel.Gym, ConsoleColor.DarkYellow);
                        thisAttackActions = await AttackGym(session, cancellationToken, fortDetails, result, index);
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
                        break;
                    default:
                        Logger.Write($"Unhandled result starting gym battle:\n{result}");
                        break;
                }

                var rewarded = battleActions.Select(x => x.BattleResults?.PlayerExperienceAwarded).Where(x => x != null);
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
                        var exp = lastAction.BattleResults.PlayerExperienceAwarded;
                        var point = lastAction.BattleResults.GymPointsDelta;
                        gym.GymPoints += point;
                        defenderPokemonId = unchecked((ulong)lastAction.BattleResults.NextDefenderPokemonId);

                        await Task.Delay(2000);

                        Logger.Write(string.Format("Exp: {0}, Gym points: {1}, Next defender Id: {2}", exp, point, defenderPokemonId), LogLevel.Gym, ConsoleColor.Magenta);
                    }
                    continue;
                }
            }

            TimedLog(string.Join(Environment.NewLine, battleActions.OrderBy(o => o.ActionStartMs).Select(s => s).Distinct()));

            if (isVictory)
            {
                if (gym.GymPoints < 0)
                    gym.GymPoints = 0;
                await Execute(session, cancellationToken, gym, fortInfo);
            }

            if (isFailedToStart && _startBattleCounter > 0)
            {
                Logger.Write("Waiting extra time to try again (3 min)");
                await Task.Delay(180000);
                await Execute(session, cancellationToken, gym, fortInfo);
            }

            var bAction = battleActions.LastOrDefault();
            if (bAction != null)
                if ((bAction.Type == BattleActionType.ActionDefeat) || (bAction.Type == BattleActionType.ActionTimedOut))
                {
                    if (battleActions.Exists(p => p.Type == BattleActionType.ActionVictory))
                    {
                        await Execute(session, cancellationToken, gym, fortInfo);
                    }
                }

            if (_startBattleCounter <= 0)
                _startBattleCounter = 3;
        }

        private static async Task<FortDeployPokemonResponse> DeployPokemonToGym(ISession session, FortDetailsResponse fortInfo, GetGymDetailsResponse fortDetails, CancellationToken cancellationToken)
        {
            FortDeployPokemonResponse response = null;
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            var points = fortDetails.GymState.FortData.GymPoints;
            var maxCount = GetGymLevel(points);

            var availableSlots = maxCount - fortDetails.GymState.Memberships.Count();

            if (availableSlots > 0)
            {
                var deployed = session.Inventory.GetDeployedPokemons();
                if (!deployed.Any(a => a.DeployedFortId == fortInfo.FortId))
                {
                    var pokemon = await GetDeployablePokemon(session);
                    if (pokemon != null)
                    {
                        try
                        {
                            response = await session.Client.Fort.FortDeployPokemon(fortInfo.FortId, pokemon.Id);
                        }
                        catch (APIBadRequestException)
                        {
                            Logger.Write("Failed to deploy pokemon. Trying again...", LogLevel.Gym, ConsoleColor.Magenta);
                            await Execute(session, cancellationToken, fortDetails.GymState.FortData, fortInfo);
                            return null;
                        }
                        if (response?.Result == FortDeployPokemonResponse.Types.Result.Success)
                        {
                            session.EventDispatcher.Send(new GymDeployEvent()
                            {
                                PokemonId = pokemon.PokemonId,
                                Name = fortDetails.Name
                            });

                            session.GymState.capturedGymId = fortDetails.GymState.FortData.Id;

                            if (session.LogicSettings.GymConfig.CollectCoinAfterDeployed > 0)
                            {
                                var count = deployed.Count();
                                if (count >= session.LogicSettings.GymConfig.CollectCoinAfterDeployed)
                                {
                                    try
                                    {
                                        if (session.Profile.PlayerData.DailyBonus.NextDefenderBonusCollectTimestampMs <= DateTime.UtcNow.ToLocalTime().ToUnixTime())
                                        {
                                            var collectDailyBonusResponse = await session.Client.Player.CollectDailyDefenderBonus();
                                            if (collectDailyBonusResponse.Result == CollectDailyDefenderBonusResponse.Types.Result.Success)
                                            {
                                                Logger.Write($"Collected {count * 10} coins", LogLevel.Gym, ConsoleColor.DarkYellow);

                                                // jjskuld - Ignore CS4014 warning for now.
                                                #pragma warning disable 4014
                                                PushNotificationClient.SendNotification(session, $"Daily reward collected : {count * 10} coins", $"Congratulations, Your bot has worked hard and collected {count * 10} coins today.");
                                                #pragma warning restore 4014
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

                                        await Task.Delay(500);
                                    }
                                }
                                else
                                    Logger.Write(string.Format("You have only {0} defenders deployed but {1} required to get reward", count, session.LogicSettings.GymConfig.CollectCoinAfterDeployed), LogLevel.Gym, ConsoleColor.Magenta);
                            }
                            else
                                Logger.Write("You have disabled reward collecting in config file", LogLevel.Gym, ConsoleColor.Magenta);
                        }
                        else
                            Logger.Write(string.Format("Deploy pokemon failed with result: {0}", response.Result), LogLevel.Gym, ConsoleColor.Magenta);
                    }
                    else
                        Logger.Write($"You don't have pokemons to be deployed!", LogLevel.Gym);
                }
                else
                    Logger.Write($"You already have pokemon deployed here", LogLevel.Gym);
            }
            else
            {
                string message = string.Format("No action. No FREE slots in GYM {0}/{1} ({2})", fortDetails.GymState.Memberships.Count(), maxCount, points);
                Logger.Write(message, LogLevel.Gym, ConsoleColor.White);
            }
            return response;
        }

        private static IEnumerable<PokemonData> CompleteAttackTeam(ISession session, IEnumerable<PokemonData> defenders, bool isTraining)
        {
            /*
             *  While i'm trying to make this gym attack i've made an error and complete team with the same one pokemon 6 times. 
             *  Guess what, it was no error. More, fight in gym was successfull and this one pokemon didn't died once but after faint got max hp again and fight again. 
             *  So after all we used only one pokemon.
             *  Maybe we can use it somehow.
             */
            session.GymState.myTeam.Clear();

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
                        var attacker = GetBestAgainst(session, attackers, defender, isTraining);
                        if (attacker != null)
                        {
                            attackers.Add(attacker);
                            session.GymState.AddToTeam(session, attacker);
                            if (attackers.Count == 6)
                                break;
                        }
                        else return null;
                    }
                }
            }

            return attackers;
        }

        private static PokemonData GetBestAgainst(ISession session, List<PokemonData> myTeam, PokemonData defender, bool isTraining)
        {
            TimedLog(string.Format("Checking pokemon for {0} ({1} CP). Already collected team is: {2}", defender.PokemonId, defender.Cp, string.Join(", ", myTeam.Select(s => string.Format("{0} ({1} CP)", s.PokemonId, s.Cp)))));
            session.GymState.AddPokemon(session, defender, false);
            AnyPokemonStat defenderStat = session.GymState.otherDefenders.FirstOrDefault(f => f.data.Id == defender.Id);

            if(session.LogicSettings.GymConfig.Attackers !=null && session.LogicSettings.GymConfig.Attackers.Count > 0)
            {
                var allPokemons = session.Inventory.GetPokemons();
                var configs = isTraining ? session.LogicSettings.GymConfig.Trainers : session.LogicSettings.GymConfig.Attackers;
                foreach (var def in configs.OrderByDescending(o => o.Priority))
                {
                    var attackersFromConfig = allPokemons.Where(w =>
                        w.PokemonId == def.Pokemon &&
                        w.Id != session.Profile.PlayerData.BuddyPokemon?.Id &&
                        !myTeam.Any(a=> a.Id==w.Id) &&
                        string.IsNullOrEmpty(w.DeployedFortId) &&
                        w.Cp >= (def.MinCP ?? 0) &&
                        w.Cp <= (def.MaxCP ?? 5000) &&
                        def.IsMoveMatch(w.Move1, w.Move2)
                    ).ToList();

                    if (attackersFromConfig != null && attackersFromConfig.Count > 0)
                        return attackersFromConfig.OrderByDescending(o => o.Cp).FirstOrDefault();
                }

            }

            MyPokemonStat myAttacker = session.GymState.myPokemons
                .Where(w =>
                        !myTeam.Any(a => a.Id == w.data.Id) && //not already in team
                        string.IsNullOrEmpty(w.data.DeployedFortId) && //not already deployed
                        session.Profile.PlayerData.BuddyPokemon?.Id != w.data.Id //not a buddy
                    )
                .OrderByDescending(o => o.typeFactor[defenderStat.mainType] + o.typeFactor[defenderStat.extraType] + o.GetFactorAgainst(session, defender.Cp, isTraining))
                .ThenByDescending(o => o.data.Cp)
                .FirstOrDefault();
            if (myAttacker == null || myAttacker.data.Cp < (defender.Cp * session.LogicSettings.GymConfig.ButNotLessThanDefenderPercent))
            {
                var other = GetBestToTeam(session, myTeam).FirstOrDefault();
                TimedLog(string.Format("Best against {0} {6} CP with is {1} {5} can't be found, will be used top by CP instead: {2} ({7} CP) with attacks {3} and {4}", defender.PokemonId, defenderStat.mainType, other?.PokemonId, other?.Move1, other?.Move2, defenderStat.extraType, defender.Cp, other?.Cp));
                return other;
            }
            else
                TimedLog(string.Format("Best against {0} {7} CP with is {1} {5} type will be {2} ({6} CP) with attacks {3} and {4} (Factor for main type {8}, second {9}, CP {10})", defender.PokemonId, defenderStat.mainType, myAttacker.data.PokemonId, myAttacker.data.Move1, myAttacker.data.Move2, defenderStat.extraType, myAttacker.data.Cp, defender.Cp, myAttacker.typeFactor[defenderStat.mainType], myAttacker.typeFactor[defenderStat.extraType], myAttacker.GetFactorAgainst(session, defender.Cp, isTraining)));
            return myAttacker.data;
        }

        private static PokemonData GetBestInBattle(ISession session, PokemonData defender)
        {
            session.GymState.AddPokemon(session, defender, false);
            AnyPokemonStat defenderStat = session.GymState.otherDefenders.FirstOrDefault(f => f.data.Id == defender.Id);
            List<PokemonType> attacks = new List<PokemonType>(GetBestTypes(defenderStat.mainType));

            TimedLog(string.Format("Searching for new attacker against {0} ({1})", defender.PokemonId, defenderStat.mainType));

            var moves = session.GymState.moveSettings.Where(w => attacks.Any(a => a == w.PokemonType));

            PokemonData newAttacker = session.GymState.myTeam.Where(w =>
                        moves.Any(a => a.MovementId == w.attacker.Move1 || a.MovementId == w.attacker.Move2) && //by move
                        w.hpState > 0
                    )
                .OrderByDescending(o => o.attacker.Cp)
                .Select(s => s.attacker)
                .FirstOrDefault();

            if (newAttacker == null)
            {
                TimedLog("No best found, takeing by CP");
                newAttacker = session.GymState.myTeam.Where(w => w.hpState > 0)
                .OrderByDescending(o => o.attacker.Cp)
                .Select(s => s.attacker)
                .FirstOrDefault();
            }

            if (newAttacker != null)
                TimedLog(string.Format("New atacker to switch will be {0} {1} CP {2}", newAttacker.PokemonId, newAttacker.Cp, newAttacker.Id));

            return newAttacker;
        }

        private static IEnumerable<PokemonData> GetBestToTeam(ISession session, List<PokemonData> myTeam)
        {
            var data = session.GymState.myPokemons.Where(w =>
                        !myTeam.Any(a => a.Id == w.data.Id) && //not already in team
                        string.IsNullOrEmpty(w.data.DeployedFortId) && //not already deployed
                        session.Profile.PlayerData.BuddyPokemon?.Id != w.data.Id //not a buddy
                    )
                .Select(s => s.data)
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

            if (session.LogicSettings.GymConfig.SaveMaxRevives && session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion) > 0)
                healPower = Int32.MaxValue;
            else
            {
                var normalPotions = session.Inventory.GetItemAmountByType(ItemId.ItemPotion);
                var superPotions = session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
                var hyperPotions = session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion);

                healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;
            }

            var normalRevives = session.Inventory.GetItemAmountByType(ItemId.ItemRevive);
            var maxRevives = session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive);

            if ((healPower >= pokemon.StaminaMax / 2 || maxRevives == 0) && normalRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemRevive, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemRevive);
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
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemMaxRevive, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemMaxRevive);
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
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemPotion, pokemon.Id);
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
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemSuperPotion, pokemon.Id);
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
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemHyperPotion, pokemon.Id);
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
            var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemMaxPotion, pokemon.Id);
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
            var normalPotions = session.Inventory.GetItemAmountByType(ItemId.ItemPotion);
            var superPotions = session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
            var hyperPotions = session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion);
            var maxPotions = session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion);

            var healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;

            if (healPower < (pokemon.StaminaMax - pokemon.Stamina) && maxPotions > 0)
            {
                try
                {
                    if (await UseMaxPotion(session, pokemon, maxPotions))
                    {
                        await session.Inventory.UpdateInventoryItem(ItemId.ItemMaxPotion);
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
                    if (!await UseHyperPotion(session, pokemon, hyperPotions))
                        return false;
                    hyperPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemHyperPotion);
                }
                else
                if (((pokemon.StaminaMax - pokemon.Stamina) > 50 || normalPotions * 20 < (pokemon.StaminaMax - pokemon.Stamina)) && superPotions > 0)
                {
                    if (!await UseSuperPotion(session, pokemon, superPotions))
                        return false;
                    superPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemSuperPotion);
                }
                else
                {
                    if (!await UsePotion(session, pokemon, normalPotions))
                        return false;
                    normalPotions--;
                    await session.Inventory.UpdateInventoryItem(ItemId.ItemPotion);
                }
            }

            return pokemon.Stamina == pokemon.StaminaMax;
        }

        private static int _currentAttackerEnergy;

        private static async Task<List<BattleAction>> AttackGym(ISession session, CancellationToken cancellationToken, GetGymDetailsResponse currentFortData, StartGymBattleResponse startResponse, int counter)
        {
            long serverMs = startResponse.BattleLog.BattleStartTimestampMs;
            var lastActions = startResponse.BattleLog.BattleActions.ToList();

            Logger.Write($"Gym battle started; fighting trainer: {startResponse.Defender.TrainerPublicProfile.Name}", LogLevel.Gym, ConsoleColor.Green);
            Logger.Write($"We are attacking: {startResponse.Defender.ActivePokemon.PokemonData.PokemonId} ({startResponse.Defender.ActivePokemon.PokemonData.Cp} CP)", LogLevel.Gym, ConsoleColor.White);
            Console.WriteLine(Environment.NewLine);

            int loops = 0;
            List<BattleAction> emptyActions = new List<BattleAction>();
            BattleAction emptyAction = new BattleAction();
            PokemonData attacker = null;
            PokemonData defender = null;
            FortData gym = currentFortData.GymState.FortData;
            _currentAttackerEnergy = 0;
            bool wasSwithed = false;

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

                    TimedLog("Starts loop");
                    var last = lastActions.Where(w => !session.GymState.myTeam.Any(a => a.attacker.Id.Equals(w.ActivePokemonId))).LastOrDefault();
                    BattleAction lastSpecialAttack = lastActions.Where(w => !session.GymState.myTeam.Any(a => a.attacker.Id.Equals(w.ActivePokemonId)) && w.Type == BattleActionType.ActionSpecialAttack).LastOrDefault();

                    TimedLog("Getting actions");
                    var attackActionz = last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyActions : GetActions(session, serverMs, attacker, defender, _currentAttackerEnergy, last, lastSpecialAttack);

                    TimedLog(string.Format("Going to make attack : {0}", string.Join(", ", attackActionz.Select(s => string.Format("{0} -> {1}", s.Type, s.DurationMs)))));

                    BattleAction a2 = (last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyAction : last);
                    AttackGymResponse attackResult = null;
                    try
                    {
                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            TimedLog("Etra wait before SWAP call");
                            await Task.Delay(1000);
                        }

                        TimedLog("Start making attack");
                        long timeBefore = DateTime.UtcNow.ToUnixTime();
                        attackResult = await session.Client.Fort.AttackGym(gym.Id, startResponse.BattleId, attackActionz, a2);
                        long timeAfter = DateTime.UtcNow.ToUnixTime();
                        TimedLog(string.Format("Finished making attack call: {0}", timeAfter - timeBefore));

                        var attackTime = attackActionz.Sum(x => x.DurationMs);
                        int attackTimeCorrected = attackTime;

                        if (attackActionz.Any(a => a.Type != BattleActionType.ActionSpecialAttack))
                            attackTimeCorrected = attackTime - (int)(timeAfter - timeBefore);

                        TimedLog(string.Format("Waiting for attack to be prepared: {0} (last call was {1}, after correction {2})", attackTime, timeAfter, attackTimeCorrected > 0 ? attackTimeCorrected : 0));
                        if (attackTimeCorrected > 0)
                            await Task.Delay(attackTimeCorrected);

                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            TimedLog("Etra wait after SWAP call");
                            await Task.Delay(2000);
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

                    if (attackResult.Result == AttackGymResponse.Types.Result.Success)
                    {
                        TimedLog("Attack success");
                        defender = attackResult.ActiveDefender?.PokemonData;
                        if (attackResult.BattleLog != null && attackResult.BattleLog.BattleActions.Count > 0)
                        {
                            var result = attackResult.BattleLog.BattleActions.OrderBy(o => o.ActionStartMs).Distinct();
                            lastActions.AddRange(result);
                            try
                            {
                                TimedLog("Result -> \r\n" + string.Join(Environment.NewLine, result));
                            }
                            catch (Exception) { }
                        }
                        serverMs = attackResult.BattleLog.ServerMs;

                        switch (attackResult.BattleLog.State)
                        {
                            case BattleState.Active:
                                _currentAttackerEnergy = attackResult.ActiveAttacker.CurrentEnergy;
                                if (attacker == null) //start battle
                                {
                                    if (counter == 1 || currentFortData.GymState.Memberships.Count == 1 || session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //first iteration, we have good attacker
                                        attacker = attackResult.ActiveAttacker.PokemonData;
                                    else //next iteration so we should to swith to proper attacker for new defender
                                    {
                                        var newAttacker = GetBestInBattle(session, attackResult.ActiveDefender.PokemonData);
                                        if (newAttacker != null && newAttacker.Id != attackResult.ActiveAttacker.PokemonData.Id)
                                        {
                                            session.GymState.swithAttacker = new SwitchPokemonData(attackResult.ActiveAttacker.PokemonData.Id, newAttacker.Id);
                                            wasSwithed = true;
                                        }
                                    }
                                }
                                else if (attacker != null && attacker.Id != attackResult?.ActiveAttacker?.PokemonData.Id) //we died and pokemon is switched to next one
                                {
                                    bool informDie = true;
                                    bool extraWait = true;
                                    if (!session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //we should manually switch pokemon to best one
                                    {
                                        if (!wasSwithed && currentFortData.GymState.Memberships.Count > 1) //swap call wasn't already called, do job
                                        {
                                            session.GymState.myTeam.Where(w => w.attacker.Id == attacker.Id).FirstOrDefault().hpState = 0;
                                            var newAttacker = GetBestInBattle(session, attackResult.ActiveDefender.PokemonData);
                                            if (newAttacker != null && newAttacker.Id != attackResult.ActiveAttacker.PokemonData.Id)
                                            {
                                                session.GymState.swithAttacker = new SwitchPokemonData(attackResult.ActiveAttacker.PokemonData.Id, newAttacker.Id);
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
                                        Logger.Write(string.Format("We ware fainted in battle, new attacker is: {0} ({1} CP)", attacker.PokemonId, attacker.Cp), LogLevel.Info, ConsoleColor.Magenta);
                                        Logger.Write("");
                                    }
                                    if (extraWait)
                                    {
                                        TimedLog("Death penalty applied");
                                        await Task.Delay(1000);
                                    }
                                }
                                attacker = attackResult.ActiveAttacker.PokemonData;
                                //Console.SetCursorPosition(0, Console.CursorTop - 1);
                                Logger.Write($"(GYM ATTACK) : Defender {attackResult.ActiveDefender.PokemonData.PokemonId.ToString()  } HP {attackResult.ActiveDefender.CurrentHealth} - Attacker  {attackResult.ActiveAttacker.PokemonData.PokemonId.ToString()} ({attackResult.ActiveAttacker.PokemonData.Cp} CP)  HP/Sta {attackResult.ActiveAttacker.CurrentHealth}/{attackResult.ActiveAttacker.CurrentEnergy}        ");
                                if (attackResult != null && attackResult.ActiveAttacker != null)
                                    session.GymState.myTeam.Where(w => w.attacker.Id == attackResult.ActiveAttacker.PokemonData.Id).FirstOrDefault().hpState = attackResult.ActiveAttacker.CurrentHealth;
                                break;
                            case BattleState.Defeated:
                                Logger.Write($"We were defeated... (AttackGym)");
                                return lastActions;
                            case BattleState.TimedOut:
                                Logger.Write($"Our attack timed out...:");
                                return lastActions;
                            case BattleState.StateUnset:
                                Logger.Write($"State was unset?: {attackResult}");
                                return lastActions;

                            case BattleState.Victory:
                                Logger.Write($"We were victorious!: ");
                                await Task.Delay(2000);
                                return lastActions;
                            default:
                                Logger.Write($"Unhandled attack response: {attackResult}");
                                continue;
                        }
                        Debug.WriteLine($"{attackResult}", "GYM: " + DateTime.UtcNow.ToUnixTime());
                    }
                    else
                    {
                        Logger.Write($"Unexpected attack result:\n{attackResult}");
                        TimedLog("Attack: " + string.Join(Environment.NewLine, attackActionz), true);
                        break;
                    }

                    TimedLog("Finished attack");
                }
                catch (APIBadRequestException e)
                {
                    Logger.Write("Bad request send to server -", LogLevel.Warning);
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

            if (session.GymState.swithAttacker != null)
            {
                actions.Add(new BattleAction()
                {
                    Type = BattleActionType.ActionSwapPokemon,
                    DurationMs = session.GymState.swithAttacker.attackDuration,
                    ActionStartMs = serverMs,
                    ActivePokemonId = session.GymState.swithAttacker.oldAttacker,                    
                    TargetPokemonId = session.GymState.swithAttacker.newAttacker,
                    TargetIndex = -1,
                });
                TimedLog(string.Format("Trying to switch pokemon: {0} to: {1}, serverMs: {2}", session.GymState.swithAttacker.oldAttacker, session.GymState.swithAttacker.newAttacker, serverMs));
                session.GymState.swithAttacker = null;
                return actions;
            }

            if (lastSpecialAttack != null && lastSpecialAttack.DamageWindowsStartTimestampMs > serverMs)
            {
                long dodgeTime = lastSpecialAttack.DamageWindowsStartTimestampMs - beforeDodge;
                if (session.GymState.timeToDodge < dodgeTime)
                    session.GymState.timeToDodge = dodgeTime;
            }

            if (attacker != null && defender != null)
            {
                var normalMove = session.GymState.myPokemons.FirstOrDefault(f => f.data.Id == attacker.Id).attack;
                var specialMove = session.GymState.myPokemons.FirstOrDefault(f => f.data.Id == attacker.Id).specialAttack;

                bool skipDodge = ((lastSpecialAttack?.DurationMs ?? 0) < normalMove.DurationMs + 550) || session.LogicSettings.GymConfig.DontUseDodge; //if our normal attack is too slow and defender special is too fast so we should to only do dodge all the time then we totally skip dodge

                bool canDoSpecialAttack = Math.Abs(specialMove.EnergyDelta) <= energy && (!(session.GymState.timeToDodge > now.ToUnixTime() && session.GymState.timeToDodge < now.ToUnixTime() + specialMove.DurationMs) || skipDodge);
                if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == attacker.PokemonId && a.Value == specialMove.MovementId))
                    canDoSpecialAttack = false;

                bool canDoAttack = !canDoSpecialAttack && (!(session.GymState.timeToDodge > now.ToUnixTime() && session.GymState.timeToDodge < now.ToUnixTime() + normalMove.DurationMs) || skipDodge);

                if (session.GymState.timeToDodge > now.ToUnixTime() && !canDoAttack && !canDoSpecialAttack && !skipDodge)
                {
                        session.GymState.lastWentDodge = now.ToUnixTime();

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

        private static async Task<StartGymBattleResponse> StartBattle(ISession session, FortData gym, IEnumerable<PokemonData> attackers, ulong defenderId)
        {

            IEnumerable<PokemonData> currentPokemons = attackers;

            var pokemonDatas = currentPokemons as PokemonData[] ?? currentPokemons.ToArray();
            var attackerPokemons = pokemonDatas.Select(pokemon => pokemon.Id);
            var attackingPokemonIds = attackerPokemons as ulong[] ?? attackerPokemons.ToArray();

            try
            {
                var result = await session.Client.Fort.StartGymBattle(gym.Id, defenderId, attackingPokemonIds);
                await Task.Delay(2000);

                if (result.Result == StartGymBattleResponse.Types.Result.Success)
                {
                    switch (result.BattleLog.State)
                    {
                        case BattleState.Active:
                            Logger.Write("Start new battle...");
                            return result;
                        case BattleState.Defeated:
                            Logger.Write($"We were defeated in battle.");
                            return result;
                        case BattleState.Victory:
                            Logger.Write($"We were victorious");
                            return result;
                        case BattleState.StateUnset:
                            Logger.Write($"Error occoured: {result.BattleLog.State}");
                            break;
                        case BattleState.TimedOut:
                            Logger.Write($"Error occoured: {result.BattleLog.State}");
                            break;
                        default:
                            Logger.Write($"Unhandled occoured: {result.BattleLog.State}");
                            break;
                    }
                }
                else if (result.Result == StartGymBattleResponse.Types.Result.ErrorGymBattleLockout)
                {
                    return result;
                }
                else if (result.Result == StartGymBattleResponse.Types.Result.ErrorAllPokemonFainted)
                {
                    return result;
                }
                else if (result.Result == StartGymBattleResponse.Types.Result.Unset)
                {
                    return result;
                }
                return result;
            }
            catch (APIBadRequestException e)
            {
                TimedLog("Gym details: " + gym);
                throw e;
            }
        }

        private static async Task EnsureJoinTeam(ISession session, PlayerData player)
        {
            if (session.Profile.PlayerData.Team == TeamColor.Neutral)
            {
                var defaultTeam = (TeamColor)Enum.Parse(typeof(TeamColor), session.LogicSettings.GymConfig.DefaultTeam);
                var teamResponse = await session.Client.Player.SetPlayerTeam(defaultTeam);
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

        internal static int GetGymLevel(double points)
        {
            if (points < 2000) return 1;
            else
            if (points < 4000) return 2;
            else
                if (points < 8000) return 3;
            else if (points < 12000) return 4;
            else if (points < 16000) return 5;
            else if (points < 20000) return 6;
            else if (points < 30000) return 7;
            else if (points < 40000) return 8;
            else if (points < 50000) return 10;
            return 10;
        }

        internal static int GetGymMaxPointsOnLevel(int lvl)
        {
            if (lvl == 1) return 2000 - 1;
            else
            if (lvl == 2) return 4000 - 1;
            else
                if (lvl == 3) return 8000 - 1;
            else if (lvl == 4) return 12000 - 1;
            else if (lvl == 5) return 16000 - 1;
            else if (lvl == 6) return 20000 - 1;
            else if (lvl == 7) return 30000 - 1;
            else if (lvl == 8) return 40000 - 1;
            else if (lvl == 9) return 50000 - 1;
            return 52000;
        }

        internal static bool CanAttackGym(ISession session, FortData fort, IEnumerable<PokemonData> deployedPokemons)
        {
            if (!session.LogicSettings.GymConfig.EnableAttackGym)
                return false;
            if (fort.OwnedByTeam == session.Profile.PlayerData.Team)
                return false;
            if (GetGymLevel(fort.GymPoints) > session.LogicSettings.GymConfig.MaxGymLevelToAttack)
                return false;
            if (deployedPokemons != null && session.LogicSettings.GymConfig.DontAttackAfterCoinsLimitReached && deployedPokemons.Count() >= session.LogicSettings.GymConfig.CollectCoinAfterDeployed)
                return false;
            return true;
        }

        internal static bool CanTrainGym(ISession session, FortData fort, IEnumerable<PokemonData> deployedPokemons)
        {
            try
            {
                GetGymDetailsResponse gymDetails = session.GymState.getGymDetails(session, fort);
                if (session.GymState.capturedGymId.Equals(fort.Id) && gymDetails.GymState.FortData.OwnedByTeam != session.Profile.PlayerData.Team)
                    gymDetails = session.GymState.getGymDetails(session, fort, true);

                if (gymDetails?.Result == GetGymDetailsResponse.Types.Result.Success)
                    fort = gymDetails.GymState.FortData;

                if (session.GymState.capturedGymId.Equals(fort.Id))
                    fort.OwnedByTeam = session.Profile.PlayerData.Team;

                bool isDeployed = deployedPokemons != null && deployedPokemons.Count() > 0 ? deployedPokemons.Any(a => a?.DeployedFortId == fort.Id) : false;
                if (gymDetails != null && GetGymLevel(fort.GymPoints) > gymDetails.GymState.Memberships.Count && !isDeployed) // free slot should be used always but not always we know that...
                    return true;
                if (!session.LogicSettings.GymConfig.EnableGymTraining)
                    return false;
                if (fort.OwnedByTeam != session.Profile.PlayerData.Team)
                    return false;
                if (!session.LogicSettings.GymConfig.TrainAlreadyDefendedGym && isDeployed)
                    return false;
                if (GetGymLevel(fort.GymPoints) > session.LogicSettings.GymConfig.MaxGymLvlToTrain)
                    return false;
                if (GetGymMaxPointsOnLevel(GetGymLevel(fort.GymPoints)) - fort.GymPoints > session.LogicSettings.GymConfig.TrainGymWhenMissingMaxPoints)
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
            GetGymDetailsResponse gymDetails = session.GymState.getGymDetails(session, fort);
            if(gymDetails?.Result == GetGymDetailsResponse.Types.Result.Success)
                fort = gymDetails.GymState.FortData;

            if (deployedPokemons.Any(a => a.DeployedFortId.Equals(fort.Id)))
                return false;

            if (fort.OwnedByTeam == TeamColor.Neutral)
                return true;

            if (gymDetails != null && fort.OwnedByTeam == session.Profile.PlayerData.Team && gymDetails.GymState.Memberships.Count < GetGymLevel(fort.GymPoints))
                return true;

            return false;
        }

        private static async Task<PokemonData> GetDeployablePokemon(ISession session)
        {
            PokemonData pokemon = null;
            List<ulong> excluded = new List<ulong>();
            var pokemonList = session.Inventory.GetPokemons().ToList();
            pokemonList.RemoveAll(x => session.LogicSettings.GymConfig.ExcludeForGyms.Contains(x.PokemonId));

            if(session.LogicSettings.GymConfig.Defenders!=null && session.LogicSettings.GymConfig.Defenders.Count > 0)
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
                                    await RevivePokemon(session, _pokemon);

                                if (_pokemon.Stamina < _pokemon.StaminaMax && _pokemon.Stamina > 0)
                                    await HealPokemon(session, _pokemon);
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
                        await RevivePokemon(session, pokemon);

                    if (pokemon.Stamina < pokemon.StaminaMax && pokemon.Stamina > 0)
                        await HealPokemon(session, pokemon);
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