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
        private static DateTime AttackStart { get; set; }

        private static int _startBattleCounter = 3;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static FortDetailsResponse _gymInfo { get; set; }
        private static GymGetInfoResponse _gymDetails { get; set; }
        private static IEnumerable<PokemonData> _deployedPokemons { get; set; }
        private static IEnumerable<PokemonData> _defenders { get; set; }
        private static FortData _gym { get; set; }
        private static ISession _session;

        public static int MaxPlayers = 6;

        public static async Task Execute(ISession session, CancellationToken cancellationToken, FortData gym, FortDetailsResponse fortInfo, GymGetInfoResponse fortDetails)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            if (!session.LogicSettings.GymConfig.Enable || gym.Type != FortType.Gym) return;

            _session = session;
            _gymInfo = fortInfo;
            _gym = gym;
            _gymDetails = fortDetails;
            _deployedPokemons = await session.Inventory.GetDeployedPokemons().ConfigureAwait(false);
            _defenders = _gymDetails.GymStatusAndDefenders.GymDefender.Select(p => p.MotivatedPokemon.Pokemon).ToList();


            if (session.GymState.MoveSettings == null)
            {
                session.GymState.MoveSettings = await session.Inventory.GetMoveSettings().ConfigureAwait(false);
            }

            await session.GymState.LoadMyPokemons(session).ConfigureAwait(false);

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

                var player = session.Profile.PlayerData;
                await EnsureJoinTeam(player).ConfigureAwait(false);

                session.EventDispatcher.Send(new GymDetailInfoEvent()
                {
                    Team = gym.OwnedByTeam,
                    Players = _gymDetails.GymStatusAndDefenders.GymDefender.Count(),
                    Name = _gymDetails.Name,
                });

                if (gym.OwnedByTeam == player.Team || gym.OwnedByTeam == TeamColor.Neutral)
                {
                    if (CanDeployToGym())
                        await DeployPokemonToGym().ConfigureAwait(false);

                    if (CanBerrieGym())
                        SendBerriesLogic();
                }
                else
                {
                    if (CanAttackGym())
                        await StartGymAttackLogic().ConfigureAwait(false);
                }

                if (CanAttackRaid())
                    StartRaidAttackLogic();
            }
            else
            {
                Logger.Write($"You are not level 5 yet, come back later...", LogLevel.Gym, ConsoleColor.White);
            }
        }

        private static void SendBerriesLogic()
        {
            //for dev
            Logger.Write("Send Berries not yet released.", LogLevel.Gym, ConsoleColor.Red);
        }

        private static void StartRaidAttackLogic()
        {
            //Check if raid or normal battle
            try
            {
                if (_gym?.RaidInfo != null)
                {
                    DateTime expires = new DateTime(0);
                    TimeSpan time = new TimeSpan(0);

                    if (_gym.RaidInfo.RaidBattleMs > DateTime.UtcNow.ToUnixTime())
                    {
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_gym.RaidInfo.RaidBattleMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            string str = $"Next RAID starts in: {time.Hours:00}h:{time.Minutes:00}m at: {(DateTime.Now + time).Hour:00}:{(DateTime.Now + time).Minute:00} Local time";
                            Logger.Write($"{str}.", LogLevel.Gym);
                        }
                    }

                    if (_gym.RaidInfo.RaidPokemon.PokemonId != PokemonId.Missingno)
                    {
                        //Raid modes 
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_gym.RaidInfo.RaidEndMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            string boss = $"Boss: {_session.Translation.GetPokemonTranslation(_gym.RaidInfo.RaidPokemon.PokemonId)} CP: {_gym.RaidInfo.RaidPokemon.Cp}";
                            string str = $"Local RAID ends in: {time.Hours:00}h:{time.Minutes:00}m at: {(DateTime.Now + time).Hour:00}:{(DateTime.Now + time).Minute:00} Local time {boss}";
                            Logger.Write($"{str}.", LogLevel.Gym);

                            //for dev
                            Logger.Write("Raid boos is present. Raids battle not yet released.", LogLevel.Gym, ConsoleColor.Red);
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

                    if (_gym.RaidInfo.RaidSpawnMs > DateTime.UtcNow.ToUnixTime())
                    {
                        expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_gym.RaidInfo.RaidSpawnMs);
                        time = expires - DateTime.UtcNow;
                        if (!(expires.Ticks == 0 || time.TotalSeconds < 0))
                        {
                            Logger.Write("Raid battle is runing...", LogLevel.Gym);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message, LogLevel.Gym, ConsoleColor.Red);
            }
        }

        private static async Task StartGymAttackLogic()
        {
            if (_defenders.Count() < 1)
                return;

            /*if (_gym.IsInBattle)
            {
                Logger.Write("This gym is under attack now, we will skip it");
                return false;
            }*/

            var badassPokemon = await CompleteAttackTeam(_defenders).ConfigureAwait(false);
            if (badassPokemon == null)
            {
                Logger.Write("Check gym settings, we can't compete against attackers team. Exiting.", LogLevel.Warning, ConsoleColor.Magenta);
                return;
            }
            var pokemonDatas = badassPokemon as PokemonData[] ?? badassPokemon.ToArray();

            Logger.Write($"Gym global CP: {GetGymAllCpOnGym()}", LogLevel.Gym);
            Logger.Write("Starting battle with: " + string.Join(", ", _defenders.Select(x => x.PokemonId.ToString())), LogLevel.Gym);

            foreach (var pokemon in pokemonDatas)
            {
                if (pokemon.Stamina <= 0)
                    await RevivePokemon(pokemon).ConfigureAwait(false);

                if (pokemon.Stamina <= 0)
                {
                    Logger.Write("You are out of revive potions! Can't revive attacker", LogLevel.Gym, ConsoleColor.Magenta);
                    return;
                }

                if (pokemon.Stamina < pokemon.StaminaMax)
                    await HealPokemon(pokemon).ConfigureAwait(false);

                if (pokemon.Stamina < pokemon.StaminaMax)
                    Logger.Write(string.Format("You are out of healing potions! {0} ({1} CP) was not fully healed", pokemon.PokemonId.ToString(), pokemon.Cp), LogLevel.Gym, ConsoleColor.Magenta);
            }
            //await Task.Delay(2000).ConfigureAwait(false);

            var index = 0;
            List<BattleAction> battleActions = new List<BattleAction>();
            ulong defenderPokemonId = _defenders.First().Id;
            Logger.Write("Attacking Team consists of:\n", LogLevel.Gym);

            while (index < _defenders.Count())
            {
                Logger.Write(string.Join(", ",
                    _session.GymState.MyTeam.Select(s => string.Format("\n{0} ({1} HP / {2} CP)",
                    s.Attacker.PokemonId.ToString(),
                    s.HpState,
                    s.Attacker.Cp))), LogLevel.Info, ConsoleColor.Yellow);

                var thisAttackActions = new List<BattleAction>();

                GymStartSessionResponse result = null;
                try
                {
                    result = await GymStartSession(pokemonDatas, defenderPokemonId).ConfigureAwait(false);
                }
                catch (APIBadRequestException)
                {
                    Logger.Write("Can't start battle", LogLevel.Gym, ConsoleColor.Red);
                    _startBattleCounter--;

                    Logger.Write("Starting battle Results: " + result, LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write("FortDetails: " + _gymDetails, LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write("PokemonDatas: " + string.Join(", ", pokemonDatas.Select(s => string.Format("Id: {0} Name: {1} CP: {2} HP: {3}", s.Id, s.PokemonId.ToString(), s.Cp, s.Stamina))), LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write("DefenderId: " + defenderPokemonId.ToString(), LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write("ActionsLog -> " + string.Join(Environment.NewLine, battleActions), LogLevel.Gym, ConsoleColor.Red);

                    break;
                }

                if (result.Result != GymStartSessionResponse.Types.Result.Success)
                {
                    _session.EventDispatcher.Send(new GymErrorUnset { GymName = _gymInfo.Name });
                    _startBattleCounter--;
                    break;
                }

                switch (result.Battle.BattleLog.State)
                {
                    case BattleState.Active:
                        _startBattleCounter = 3;
                        AttackStart = DateTime.Now.AddSeconds(120);
                        Logger.Write($"Time to start Attack Mode", LogLevel.Gym, ConsoleColor.DarkYellow);
                        thisAttackActions = await AttackGym(result, index).ConfigureAwait(false);
                        battleActions.AddRange(thisAttackActions);
                        break;
                    case BattleState.Defeated:
                        battleActions.Add(new BattleAction() { Type = BattleActionType.ActionDefeat });
                        break;
                    case BattleState.StateUnset:
                        battleActions.Add(new BattleAction() { Type = BattleActionType.ActionUnset });
                        break;
                    case BattleState.TimedOut:
                        battleActions.Add(new BattleAction() { Type = BattleActionType.ActionTimedOut });
                        break;
                    case BattleState.Victory:
                        battleActions.Add(new BattleAction() { Type = BattleActionType.ActionVictory });
                        break;
                    default:
                        battleActions.Add(new BattleAction() { Type = BattleActionType.ActionUnset });
                        break;
                }

                var rewarded = battleActions.Select(x => x.BattleResults?.PlayerXpAwarded).Where(x => x != null);
                var faintedPKM = battleActions.Where(x => x != null && x.Type == BattleActionType.ActionFaint).Select(x => x.ActivePokemonId).Distinct();
                var livePokemons = pokemonDatas.Where(x => !faintedPKM.Any(y => y == x.Id));
                var faintedPokemons = pokemonDatas.Where(x => faintedPKM.Any(y => y == x.Id));
                pokemonDatas = livePokemons.Concat(faintedPokemons).ToArray();
                index++;
            }

            Logger.Write(string.Join(Environment.NewLine, battleActions.OrderBy(o => o.ActionStartMs).Select(s => s).Distinct()), LogLevel.Gym, ConsoleColor.White);

            var bAction = battleActions.LastOrDefault();
            if (bAction != null)
            {
                if (bAction.Type == BattleActionType.ActionVictory)
                {
                    var lastAction = battleActions.LastOrDefault();
                    var exp = lastAction.BattleResults.PlayerXpAwarded;
                    defenderPokemonId = unchecked((ulong)lastAction.BattleResults.NextDefenderPokemonId);

                    await Task.Delay(5000).ConfigureAwait(false);

                    Logger.Write($"(Battle) XP: {exp} | Players: {_defenders.Count(),2:#0} | Next defender Id: {defenderPokemonId.ToString()}", LogLevel.Gym, ConsoleColor.Magenta);

                    if (_session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                        await PushNotificationClient.SendNotification(_session, $"Gym Battle",
                                                                               $"We were victorious!\n" +
                                                                               $"XP: {exp}" +
                                                                               $"Players: {_defenders.Count(),2:#0}", true).ConfigureAwait(false); // +

                    await Execute(_session, _session.CancellationTokenSource.Token, _gym, _gymInfo, _gymDetails).ConfigureAwait(false);
                }
                else if (bAction.Type == BattleActionType.ActionTimedOut)
                {
                    Logger.Write("TimeOut to try again (10 sec)");
                    if (_session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                        await PushNotificationClient.SendNotification(_session, "Gym Battle", $"Our attack timed out...:", true).ConfigureAwait(false);
                    await Task.Delay(10000).ConfigureAwait(false);
                    await Execute(_session, _session.CancellationTokenSource.Token, _gym, _gymInfo, _gymDetails).ConfigureAwait(false);
                }
                else if (bAction.Type == BattleActionType.ActionDefeat)
                {
                    Logger.Write("Defeat to try again (10 sec)");
                    await Task.Delay(10000).ConfigureAwait(false);
                    await Execute(_session, _session.CancellationTokenSource.Token, _gym, _gymInfo, _gymDetails).ConfigureAwait(false);
                }
                else if (bAction.Type == BattleActionType.ActionUnset)
                {
                    Logger.Write("Gym Unset to try again (10 sec)");
                    await Task.Delay(10000).ConfigureAwait(false);
                    await Execute(_session, _session.CancellationTokenSource.Token, _gym, _gymInfo, _gymDetails).ConfigureAwait(false);
                }
            }

            if (_startBattleCounter <= 0)
                _startBattleCounter = 3;
        }

        private static async Task DeployPokemonToGym()
        {
           var availableSlots = MaxPlayers - _gymDetails.GymStatusAndDefenders.GymDefender.Count();

            if (availableSlots > 0)
            {
                var deployed = await _session.Inventory.GetDeployedPokemons().ConfigureAwait(false);
                if (!deployed.Any(a => a.DeployedFortId == _gymInfo.FortId))
                {
                    var pokemon = await GetDeployablePokemon().ConfigureAwait(false);
                    var response = new GymDeployResponse();
                    if (pokemon != null)
                    {
                        try
                        {
                            response = await _session.Client.Fort.GymDeploy(_gymInfo.FortId, pokemon.Id).ConfigureAwait(false);
                        }
                        catch (APIBadRequestException)
                        {
                            Logger.Write("Failed to deploy pokemon. Trying again...", LogLevel.Gym, ConsoleColor.Magenta);
                            await Execute(_session, _session.CancellationTokenSource.Token, _gym, _gymInfo, _gymDetails).ConfigureAwait(false);
                            return;
                        }

                        if (response.Result == GymDeployResponse.Types.Result.Success)
                        {
                            _session.EventDispatcher.Send(new GymDeployEvent()
                            {
                                PokemonId = pokemon.PokemonId,
                                Name = _gymDetails.Name
                            });

                            _session.GymState.CapturedGymId = _gym.Id;
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
                int allCp = GetGymAllCpOnGym();
                string message = string.Format("No FREE slots in GYM: {0}/{1} (All Cp: {2})", _gymDetails.GymStatusAndDefenders.GymDefender.Count(), MaxPlayers, allCp);
                Logger.Write(message, LogLevel.Gym, ConsoleColor.White);
            }
        }

        private static async Task<IEnumerable<PokemonData>> CompleteAttackTeam(IEnumerable<PokemonData> defenders)
        {
            /*
             *  While i'm trying to make this gym attack i've made an error and complete team with the same one pokemon 6 times. 
             *  Guess what, it was no error. More, fight in gym was successfull and this one pokemon didn't died once but after faint got max hp again and fight again. 
             *  So after all we used only one pokemon.
             *  Maybe we can use it somehow.
             */
            _session.GymState.MyTeam.Clear();

            List<PokemonData> attackers = new List<PokemonData>();

            if (_session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp && defenders.Count() > 1)
            {
                attackers.AddRange(GetBestToTeam(attackers));
                attackers.ForEach(attacker =>
                {
                    _session.GymState.AddToTeam(_session, attacker);
                });
            }
            else
            {
                while (attackers.Count() < MaxPlayers)
                {
                    foreach (var defender in defenders)
                    {
                        var attacker = await GetBestAgainst(attackers, defender).ConfigureAwait(false);
                        if (attacker != null)
                        {
                            //Trying to make bot only select pokemon that are more than 75% of full CP to battle. Still needs some work(The Wizard1328)
                            //if (attacker.Cp >= attacker.Cp * 0.75)
                            //{
                            attackers.Add(attacker);
                            _session.GymState.AddToTeam(_session, attacker);
                            if (attackers.Count == MaxPlayers)
                                break;
                            //}
                        }
                        else return null;
                    }
                }
            }
            return attackers;
        }

        private static async Task<PokemonData> GetBestAgainst(List<PokemonData> myTeam, PokemonData defender)
        {
            Logger.Write(string.Format("Checking pokemon for {0} ({1} CP). Already collected team has: {2}", defender.PokemonId.ToString(), defender.Cp, string.Join(", ", myTeam.Select(s => string.Format("{0} ({1} CP)", s.PokemonId.ToString(), s.Cp)))), LogLevel.Gym, ConsoleColor.White);
            _session.GymState.AddPokemon(_session, defender, false);
            AnyPokemonStat defenderStat = _session.GymState.OtherDefenders.FirstOrDefault(f => f.Data.Id == defender.Id);

            if (_session.LogicSettings.GymConfig.Attackers != null && _session.LogicSettings.GymConfig.Attackers.Count > 0)
            {
                var allPokemons = await _session.Inventory.GetPokemons().ConfigureAwait(false);
                foreach (var def in _session.LogicSettings.GymConfig.Attackers.OrderByDescending(o => o.Priority))
                {
                    var attackersFromConfig = allPokemons.Where(w =>
                        w.PokemonId == def.Pokemon &&
                        w.Id != _session.Profile.PlayerData.BuddyPokemon?.Id &&
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

            MyPokemonStat myAttacker = _session.GymState.MyPokemons
                .Where(w =>
                        !myTeam.Any(a => a.Id == w.Data.Id) && //not already in team
                        string.IsNullOrEmpty(w.Data.DeployedFortId) && //not already deployed
                        _session.Profile.PlayerData.BuddyPokemon?.Id != w.Data.Id //not a buddy
                    )
                .OrderByDescending(o => o.TypeFactor[defenderStat.MainType] + o.TypeFactor[defenderStat.ExtraType])
                .ThenByDescending(o => o.Data.Cp)
                .FirstOrDefault();
            if (myAttacker == null || myAttacker.Data.Cp < (defender.Cp * _session.LogicSettings.GymConfig.ButNotLessThanDefenderPercent))
            {
                var other = GetBestToTeam(myTeam).FirstOrDefault();
                Logger.Write(string.Format("Best against {0} {6} CP with is {1} {5} can't be found, will use top by CP instead: {2} ({7} CP) with attacks {3} and {4}",
                    defender.PokemonId.ToString(), defenderStat.MainType, other?.PokemonId.ToString(), other?.Move1, other?.Move2, defenderStat.ExtraType, defender.Cp, other?.Cp), LogLevel.Gym, ConsoleColor.Cyan);
                return other;
            }
            else
                Logger.Write(string.Format("Best against {0} {7} CP with is {1} {5} type will be {2} ({6} CP) with attacks {3} and {4} (Factor for main type {8}, second {9}, CP {10})",
                    defender.PokemonId.ToString(), defenderStat.MainType, myAttacker.Data.PokemonId.ToString(), myAttacker.Data.Move1, myAttacker.Data.Move2, defenderStat.ExtraType, myAttacker.Data.Cp, defender.Cp, myAttacker.TypeFactor[defenderStat.MainType], myAttacker.TypeFactor[defenderStat.ExtraType]), LogLevel.Gym, ConsoleColor.Cyan);
            return myAttacker.Data;
        }

        private static PokemonData GetBestInBattle(PokemonData defender)
        {
            _session.GymState.AddPokemon(_session, defender, false);
            AnyPokemonStat defenderStat = _session.GymState.OtherDefenders.FirstOrDefault(f => f.Data.Id == defender.Id);
            List<PokemonType> attacks = new List<PokemonType>(GetBestTypes(defenderStat.MainType));

            Logger.Write(string.Format("Searching for a new attacker against {0} ({1})", defender.PokemonId.ToString(), defenderStat.MainType), LogLevel.Gym, ConsoleColor.Blue);

            var moves = _session.GymState.MoveSettings.Where(w => attacks.Any(a => a == w.PokemonType));

            PokemonData newAttacker = _session.GymState.MyTeam.Where(w =>
                        moves.Any(a => a.MovementId == w.Attacker.Move1 || a.MovementId == w.Attacker.Move2) && //by move
                        w.HpState > 0
                    )
                .OrderByDescending(o => o.Attacker.Cp)
                .Select(s => s.Attacker)
                .FirstOrDefault();

            if (newAttacker == null)
            {
                Logger.Write("No best found, takeing by CP", LogLevel.Gym, ConsoleColor.Green);
                newAttacker = _session.GymState.MyTeam.Where(w => w.HpState > 0)
                .OrderByDescending(o => o.Attacker.Cp)
                .Select(s => s.Attacker)
                .FirstOrDefault();
            }

            if (newAttacker != null)
                Logger.Write(string.Format("New atacker to switch will be {0} {1} CP {2}", newAttacker.PokemonId.ToString(), newAttacker.Cp, newAttacker.Id), LogLevel.Gym, ConsoleColor.Green);

            return newAttacker;
        }

        private static IEnumerable<PokemonData> GetBestToTeam(List<PokemonData> myTeam)
        {
            var data = _session.GymState.MyPokemons.Where(w =>
                        !myTeam.Any(a => a.Id == w.Data.Id) && //not already in team
                        string.IsNullOrEmpty(w.Data.DeployedFortId) && //not already deployed
                        _session.Profile.PlayerData.BuddyPokemon?.Id != w.Data.Id //not a buddy
                    )
                .Select(s => s.Data)
                .OrderByDescending(o => o.Cp)
                .Take(MaxPlayers - myTeam.Count());
            Logger.Write("Best others are: " + string.Join(", ", data.Select(s => s.PokemonId.ToString())), LogLevel.Gym, ConsoleColor.Green);
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

        private static async Task RevivePokemon(PokemonData pokemon)
        {
            int healPower = 0;

            if (_session.LogicSettings.GymConfig.SaveMaxRevives && await _session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false) > 0)
                healPower = Int32.MaxValue;
            else
            {
                var normalPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
                var superPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
                var hyperPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);

                healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;
            }

            var normalRevives = await _session.Inventory.GetItemAmountByType(ItemId.ItemRevive).ConfigureAwait(false);
            var maxRevives = await _session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive).ConfigureAwait(false);

            if ((healPower >= pokemon.StaminaMax / 2 || maxRevives == 0) && normalRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await _session.Client.Inventory.UseItemRevive(ItemId.ItemRevive, pokemon.Id).ConfigureAwait(false);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await _session.Inventory.UpdateInventoryItem(ItemId.ItemRevive).ConfigureAwait(false);
                        pokemon.Stamina = ret.Stamina;
                        _session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "normal",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (normalRevives - 1)
                        });
                        break;
                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Logger.Write(
                            $"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
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
                var ret = await _session.Client.Inventory.UseItemRevive(ItemId.ItemMaxRevive, pokemon.Id).ConfigureAwait(false);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        await _session.Inventory.UpdateInventoryItem(ItemId.ItemMaxRevive).ConfigureAwait(false);
                        pokemon.Stamina = ret.Stamina;
                        _session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "max",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (maxRevives - 1)
                        });
                        break;

                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Logger.Write($"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
            }
        }

        private static async Task<bool> UsePotion(PokemonData pokemon, int normalPotions)
        {
            var ret = await _session.Client.Inventory.UseItemPotion(ItemId.ItemPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    _session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "normal",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (normalPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseSuperPotion(PokemonData pokemon, int superPotions)
        {
            var ret = await _session.Client.Inventory.UseItemPotion(ItemId.ItemSuperPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    _session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "super",
                        PokemonCp = pokemon.Cp,

                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (superPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseHyperPotion(PokemonData pokemon, int hyperPotions)
        {
            var ret = await _session.Client.Inventory.UseItemPotion(ItemId.ItemHyperPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    _session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "hyper",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = (hyperPotions - 1)
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> UseMaxPotion(PokemonData pokemon, int maxPotions)
        {
            var ret = await _session.Client.Inventory.UseItemPotion(ItemId.ItemMaxPotion, pokemon.Id).ConfigureAwait(false);
            switch (ret.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    pokemon.Stamina = ret.Stamina;
                    _session.EventDispatcher.Send(new EventUsedPotion
                    {
                        Type = "max",
                        PokemonCp = pokemon.Cp,
                        PokemonId = pokemon.PokemonId.ToString(),
                        Remaining = maxPotions
                    });
                    break;

                case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                    Logger.Write($"Pokemon: {pokemon.PokemonId.ToString()} (CP: {pokemon.Cp}) is already deployed to a gym...");
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private static async Task<bool> HealPokemon(PokemonData pokemon)
        {
            var normalPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
            var superPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
            var hyperPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);
            var maxPotions = await _session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false);

            var healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;

            if (healPower < (pokemon.StaminaMax - pokemon.Stamina) && maxPotions > 0)
            {
                try
                {
                    if (await UseMaxPotion(pokemon, maxPotions).ConfigureAwait(false))
                    {
                        await _session.Inventory.UpdateInventoryItem(ItemId.ItemMaxPotion).ConfigureAwait(false);
                        return true;
                    }
                }
                catch (APIBadRequestException)
                {
                    Logger.Write(string.Format("Heal problem with max potions ({0}) on pokemon: {1}", maxPotions, pokemon.PokemonId.ToString()), LogLevel.Error, ConsoleColor.Magenta);
                }
            }

            while (normalPotions + superPotions + hyperPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                if (((pokemon.StaminaMax - pokemon.Stamina) > 200 || ((normalPotions * 20 + superPotions * 50) < (pokemon.StaminaMax - pokemon.Stamina))) && hyperPotions > 0)
                {
                    if (!await UseHyperPotion(pokemon, hyperPotions).ConfigureAwait(false))
                        return false;
                    hyperPotions--;
                    await _session.Inventory.UpdateInventoryItem(ItemId.ItemHyperPotion).ConfigureAwait(false);
                }
                else
                if (((pokemon.StaminaMax - pokemon.Stamina) > 50 || normalPotions * 20 < (pokemon.StaminaMax - pokemon.Stamina)) && superPotions > 0)
                {
                    if (!await UseSuperPotion(pokemon, superPotions).ConfigureAwait(false))
                        return false;
                    superPotions--;
                    await _session.Inventory.UpdateInventoryItem(ItemId.ItemSuperPotion).ConfigureAwait(false);
                }
                else
                {
                    if (!await UsePotion(pokemon, normalPotions).ConfigureAwait(false))
                        return false;
                    normalPotions--;
                    await _session.Inventory.UpdateInventoryItem(ItemId.ItemPotion).ConfigureAwait(false);
                }
            }

            return pokemon.Stamina == pokemon.StaminaMax;
        }

        private static int _currentAttackerEnergy;

        private static async Task<List<BattleAction>> AttackGym(GymStartSessionResponse startResponse, int counter)
        {
            long serverMs = startResponse.Battle.BattleLog.BattleStartTimestampMs;
            var lastActions = startResponse.Battle.BattleLog.BattleActions.ToList();

            Logger.Write($"Gym battle started; fighting trainer: {startResponse.Battle.Defender.TrainerPublicProfile.Name}", LogLevel.Gym, ConsoleColor.Green);
            Logger.Write($"We are attacking: {startResponse.Battle.Defender.ActivePokemon.PokemonData.PokemonId.ToString()} ({startResponse.Battle.Defender.ActivePokemon.PokemonData.Cp} CP), Lvl: {startResponse.Battle.Defender.ActivePokemon.PokemonData.Level():0.0}", LogLevel.Gym, ConsoleColor.White);
            Console.WriteLine(Environment.NewLine);

            if (_session.LogicSettings.NotificationConfig.EnablePushBulletNotification == true)
                await PushNotificationClient.SendNotification(_session, $"Gym battle started", $"Trainer: {startResponse.Battle.Defender.TrainerPublicProfile.Name}\n" +
                                                                       $"We are attacking: {startResponse.Battle.Defender.ActivePokemon.PokemonData.PokemonId.ToString()} ({startResponse.Battle.Defender.ActivePokemon.PokemonData.Cp} CP)\n" +
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
                    Logger.Write("Starts loop", LogLevel.Gym);
                    var last = lastActions.Where(w => !_session.GymState.MyTeam.Any(a => a.Attacker.Id.Equals(w.ActivePokemonId))).LastOrDefault();
                    BattleAction lastSpecialAttack = lastActions.Where(w => !_session.GymState.MyTeam.Any(a => a.Attacker.Id.Equals(w.ActivePokemonId)) && w.Type == BattleActionType.ActionSpecialAttack).LastOrDefault();

                    Logger.Write("Getting actions", LogLevel.Gym, ConsoleColor.White);
                    var attackActionz = last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyActions : GetActions(serverMs, attacker, defender, _currentAttackerEnergy, last, lastSpecialAttack);

                    Logger.Write(string.Format("Going to make attack : {0}",
                        string.Join(", ", attackActionz.Select(s => string.Format("{0} -> {1}", s.Type, s.DurationMs)))), LogLevel.Gym, ConsoleColor.Blue);

                    BattleAction a2 = (last == null || last.Type == BattleActionType.ActionVictory || last.Type == BattleActionType.ActionDefeat ? emptyAction : last);
                    GymBattleAttackResponse attackResult = null;
                    try
                    {
                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            Logger.Write("Etra wait before SWAP call", LogLevel.Gym);
                            await Task.Delay(1000).ConfigureAwait(false);
                        }

                        Logger.Write("Start making attack", LogLevel.Gym, ConsoleColor.Green);
                        long timeBefore = DateTime.UtcNow.ToUnixTime();
                        attackResult = await _session.Client.Fort.GymBattleAttak(_gym.Id, startResponse.Battle.BattleId, attackActionz, a2, serverMs).ConfigureAwait(false);
                        long timeAfter = DateTime.UtcNow.ToUnixTime();
                        Logger.Write(string.Format("Finished making attack call: {0}", timeAfter - timeBefore), LogLevel.Gym, ConsoleColor.White);

                        var attackTime = attackActionz.Sum(x => x.DurationMs);
                        int attackTimeCorrected = attackTime;

                        if (attackActionz.Any(a => a.Type != BattleActionType.ActionSpecialAttack))
                            attackTimeCorrected = attackTime - (int)(timeAfter - timeBefore);

                        Logger.Write(string.Format("Waiting for attack to be prepared: {0} (last call was {1}, after correction {2})",
                            attackTime, timeAfter, attackTimeCorrected > 0 ? attackTimeCorrected : 0), LogLevel.Gym, ConsoleColor.Yellow);
                        if (attackTimeCorrected > 0)
                            await Task.Delay(attackTimeCorrected).ConfigureAwait(false);

                        if (attackActionz.Any(a => a.Type == BattleActionType.ActionSwapPokemon))
                        {
                            Logger.Write("Etra wait after SWAP call", LogLevel.Gym, ConsoleColor.Green);
                            await Task.Delay(2000).ConfigureAwait(false);
                        }
                    }
                    catch (APIBadRequestException)
                    {
                        Logger.Write("Bad attack gym", LogLevel.Gym, ConsoleColor.Red);
                        Logger.Write(string.Format("Last retrieved action was: {0}", a2), LogLevel.Gym, ConsoleColor.Red);
                        Logger.Write(string.Format("Actions to perform were: {0}", string.Join(", ", attackActionz)), LogLevel.Gym, ConsoleColor.Red);
                        Logger.Write(string.Format("Attacker was: {0}, defender was: {1}", attacker, defender), LogLevel.Gym, ConsoleColor.Red);

                        continue;
                    };

                    loops++;

                    if (attackResult.Result == GymBattleAttackResponse.Types.Result.Success)
                    {
                        Logger.Write("Attack success", LogLevel.Gym, ConsoleColor.Green);
                        defender = attackResult.BattleUpdate.ActiveDefender?.PokemonData;
                        if (attackResult.BattleUpdate.BattleLog != null && attackResult.BattleUpdate.BattleLog.BattleActions.Count > 0)
                        {

                            /*
                             * Debug mode
                             * 
                            var result = attackResult.BattleUpdate.BattleLog.BattleActions.OrderBy(o => o.ActionStartMs).Distinct();
                            lastActions.AddRange(result);
                            try
                            {
                                Logger.Write("Result -> \r\n" + string.Join(Environment.NewLine, result), LogLevel.Gym, ConsoleColor.White);
                            }
                            catch (Exception) { }
                            */
                        }
                        serverMs = attackResult.BattleUpdate.BattleLog.ServerMs;

                        switch (attackResult.BattleUpdate.BattleLog.State)
                        {
                            case BattleState.Active:
                                _currentAttackerEnergy = attackResult.BattleUpdate.ActiveAttacker.CurrentEnergy;
                                if (attacker == null) //start battle
                                {
                                    if (counter == 1 || _gymDetails.GymStatusAndDefenders.GymDefender.Count == 1 || _session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //first iteration, we have good attacker
                                        attacker = attackResult.BattleUpdate.ActiveAttacker.PokemonData;
                                    else //next iteration so we should to swith to proper attacker for new defender
                                    {
                                        var newAttacker = GetBestInBattle(attackResult.BattleUpdate.ActiveDefender.PokemonData);
                                        if (newAttacker != null && newAttacker.Id != attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id)
                                        {
                                            _session.GymState.SwithAttacker = new SwitchPokemonData(attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id, newAttacker.Id);
                                            wasSwithed = true;
                                        }
                                    }
                                }
                                else if (attacker != null && attacker.Id != attackResult?.BattleUpdate.ActiveAttacker?.PokemonData.Id) //we died and pokemon is switched to next one
                                {
                                    bool informDie = true;
                                    bool extraWait = true;
                                    if (!_session.LogicSettings.GymConfig.UsePokemonToAttackOnlyByCp) //we should manually switch pokemon to best one
                                    {
                                        if (!wasSwithed && _gymDetails.GymStatusAndDefenders.GymDefender.Count > 1) //swap call wasn't already called, do job
                                        {
                                            _session.GymState.MyTeam.Where(w => w.Attacker.Id == attacker.Id).FirstOrDefault().HpState = 0;
                                            var newAttacker = GetBestInBattle(attackResult.BattleUpdate.ActiveDefender.PokemonData);
                                            if (newAttacker != null && newAttacker.Id != attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id)
                                            {
                                                _session.GymState.SwithAttacker = new SwitchPokemonData(attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id, newAttacker.Id);
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
                                        Logger.Write(string.Format("Our Pokemon has fainted in battle, our new attacker is: {0} ({1} CP)",
                                            attacker.PokemonId.ToString(), attacker.Cp), LogLevel.Gym, ConsoleColor.Magenta);
                                        Logger.Write("");
                                    }
                                    if (extraWait)
                                    {
                                        Logger.Write("Death penalty applied", LogLevel.Gym, ConsoleColor.Yellow);
                                        await Task.Delay(1000).ConfigureAwait(false);
                                    }
                                }

                                attacker = attackResult.BattleUpdate.ActiveAttacker.PokemonData;
                                defender = attackResult.BattleUpdate.ActiveDefender.PokemonData;

                                var player = _session.Profile.PlayerData;
                                await EnsureJoinTeam(player).ConfigureAwait(false);
                                var ev = _gym.OwnedByTeam;
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
                                    _session.GymState.MyTeam.Where(w => w.Attacker.Id == attackResult.BattleUpdate.ActiveAttacker.PokemonData.Id).FirstOrDefault().HpState = attackResult.BattleUpdate.ActiveAttacker.CurrentHealth;
                                break;
                            case BattleState.Defeated:
                                Logger.Write($"We have been defeated... (AttackGym)", LogLevel.Gym, ConsoleColor.DarkYellow);
                                return lastActions;
                            case BattleState.TimedOut:
                                Logger.Write($"Our attack timed out...", LogLevel.Gym, ConsoleColor.DarkYellow);
                                return lastActions;
                            case BattleState.StateUnset:
                                Logger.Write($"State was unset? {attackResult}", LogLevel.Gym, ConsoleColor.DarkYellow);
                                return lastActions;
                            case BattleState.Victory:
                                Logger.Write($"We were victorious!", LogLevel.Gym, ConsoleColor.Green);
                                await Task.Delay(2000).ConfigureAwait(false);
                                return lastActions;
                            default:
                                Logger.Write($"Unhandled attack response: {attackResult}", LogLevel.Gym, ConsoleColor.DarkYellow);
                                continue;
                        }
                    }
                    else
                    {
                        Logger.Write($"Unexpected attack result: {attackResult}", LogLevel.Gym, ConsoleColor.Yellow);
                        Logger.Write("Attack: " + string.Join(Environment.NewLine, attackActionz), LogLevel.Gym, ConsoleColor.Yellow);
                        break;
                    }

                    Logger.Write("Finished attack", LogLevel.Gym, ConsoleColor.Green);
                }
                catch (APIBadRequestException e)
                {
                    Logger.Write("Bad request sent to server -", LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write("NOT finished attack", LogLevel.Gym, ConsoleColor.Red);
                    Logger.Write(e.Message, LogLevel.Gym, ConsoleColor.Red);
                };
            }
            return lastActions;
        }

        private static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

        private static List<BattleAction> GetActions(long serverMs, PokemonData attacker, PokemonData defender, int energy, BattleAction lastAction, BattleAction lastSpecialAttack)
        {
            List<BattleAction> actions = new List<BattleAction>();
            DateTime now = DateTimeFromUnixTimestampMillis(serverMs);
            const int beforeDodge = 200;

            if (_session.GymState.SwithAttacker != null)
            {
                actions.Add(new BattleAction()
                {
                    Type = BattleActionType.ActionSwapPokemon,
                    DurationMs = _session.GymState.SwithAttacker.AttackDuration,
                    ActionStartMs = serverMs,
                    ActivePokemonId = _session.GymState.SwithAttacker.OldAttacker,
                    TargetPokemonId = _session.GymState.SwithAttacker.NewAttacker,
                    TargetIndex = -1,
                });
                Logger.Write(string.Format("Trying to switch pokemon: {0} to: {1}, serverMs: {2}", _session.GymState.SwithAttacker.OldAttacker, _session.GymState.SwithAttacker.NewAttacker, serverMs), LogLevel.Gym, ConsoleColor.White);
                _session.GymState.SwithAttacker = null;
                return actions;
            }

            if (lastSpecialAttack != null && lastSpecialAttack.DamageWindowsStartTimestampMs > serverMs)
            {
                long dodgeTime = lastSpecialAttack.DamageWindowsStartTimestampMs - beforeDodge;
                if (_session.GymState.TimeToDodge < dodgeTime)
                    _session.GymState.TimeToDodge = dodgeTime;
            }

            if (attacker != null && defender != null)
            {
                var normalMove = _session.GymState.MyPokemons.FirstOrDefault(f => f.Data.Id == attacker.Id).Attack;
                var specialMove = _session.GymState.MyPokemons.FirstOrDefault(f => f.Data.Id == attacker.Id).SpecialAttack;
                bool skipDodge = ((lastSpecialAttack?.DurationMs ?? 0) < normalMove.DurationMs + 550) || _session.LogicSettings.GymConfig.UseDodge; //if our normal attack is too slow and defender special is too fast so we should to only do dodge all the time then we totally skip dodge
                bool canDoSpecialAttack = Math.Abs(specialMove.EnergyDelta) <= energy && (!(_session.GymState.TimeToDodge > now.ToUnixTime() && _session.GymState.TimeToDodge < now.ToUnixTime() + specialMove.DurationMs) || skipDodge);
                bool canDoAttack = !canDoSpecialAttack && (!(_session.GymState.TimeToDodge > now.ToUnixTime() && _session.GymState.TimeToDodge < now.ToUnixTime() + normalMove.DurationMs) || skipDodge);

                if (_session.GymState.TimeToDodge > now.ToUnixTime() && !canDoAttack && !canDoSpecialAttack && !skipDodge)
                {
                    _session.GymState.LastWentDodge = now.ToUnixTime();

                    BattleAction dodge = new BattleAction()
                    {
                        Type = BattleActionType.ActionDodge,
                        ActionStartMs = now.ToUnixTime(),
                        DurationMs = 500,
                        TargetIndex = -1,
                        ActivePokemonId = attacker.Id,
                    };

                    Logger.Write(string.Format("Trying to dodge an attack {0}, lastSpecialAttack.DamageWindowsStartTimestampMs: {1}, serverMs: {2}",
                        dodge, lastSpecialAttack.DamageWindowsStartTimestampMs, serverMs), LogLevel.Gym, ConsoleColor.White);
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
                        Logger.Write(string.Format("Trying to make an special attack {0}, on: {1}, duration: {2}"
                            , specialMove.MovementId, _gymInfo.Name, specialMove.DurationMs), LogLevel.Gym, ConsoleColor.White);
                    }
                    else if (canDoAttack)
                    {
                        action2.Type = BattleActionType.ActionAttack;
                        action2.DurationMs = normalMove.DurationMs;
                        action2.DamageWindowsStartTimestampMs = normalMove.DamageWindowStartMs;
                        action2.DamageWindowsEndTimestampMs = normalMove.DamageWindowEndMs;
                        Logger.Write(string.Format("Trying to make an normal attack {0}, on: {1}, duration: {2}"
                            , normalMove.MovementId, _gymInfo.Name, normalMove.DurationMs), LogLevel.Gym, ConsoleColor.White);
                    }
                    else
                    {
                        Logger.Write("SHIT", LogLevel.Gym, ConsoleColor.Yellow);
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

        private static async Task<GymStartSessionResponse> GymStartSession(IEnumerable<PokemonData> attackers, ulong defenderId)
        {
            IEnumerable<PokemonData> currentPokemons = attackers;
            var pokemonDatas = currentPokemons as PokemonData[] ?? currentPokemons.ToArray();
            var attackerPokemons = pokemonDatas.Select(pokemon => pokemon.Id);
            var attackingPokemonIds = attackerPokemons as ulong[] ?? attackerPokemons.ToArray();
            GymStartSessionResponse result = null;

            try
            {
                result = await _session.Client.Fort.GymStartSession(_gym.Id, defenderId, attackingPokemonIds).ConfigureAwait(false);
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
                            return result;
                        case BattleState.TimedOut:
                            Logger.Write($"Error occoured: {result.Battle.BattleLog.State}");
                            return result;
                        default:
                            Logger.Write($"Unhandled occoured: {result.Battle.BattleLog.State}");
                            return result;
                    }
                }
            }
            catch (APIBadRequestException e)
            {
                Logger.Write("Gym Details: " + e.Message, LogLevel.Error);
                return result;
            }
            return result;
        }

        private static async Task EnsureJoinTeam(PlayerData player)
        {
            if (_session.Profile.PlayerData.Team == TeamColor.Neutral)
            {
                var defaultTeam = (TeamColor)Enum.Parse(typeof(TeamColor), _session.LogicSettings.GymConfig.DefaultTeam);
                var teamResponse = await _session.Client.Player.SetPlayerTeam(defaultTeam).ConfigureAwait(false);
                if (teamResponse.Status == SetPlayerTeamResponse.Types.Status.Success)
                {
                    player.Team = defaultTeam;
                }

                _session.EventDispatcher.Send(new GymTeamJoinEvent()
                {
                    Team = defaultTeam,
                    Status = teamResponse.Status
                });
            }
        }

        private static int GetGymAllCpOnGym()//
        {
            int allCp = 0;
            foreach (var x in _defenders)
                allCp = allCp + x.Cp;
            return allCp;
        }

        private static bool CanAttackGym()
        {
            if (!_session.LogicSettings.GymConfig.EnableAttackGym)
                return false;

            if (_gym?.RaidInfo != null)
            {
                if (_gym.RaidInfo.RaidPokemon.PokemonId != PokemonId.Missingno)
                    return false;
            }
            return true;
        }

        private static bool CanAttackRaid()
        {
            if (!_session.LogicSettings.GymConfig.EnableAttackRaid)
                return false;

            if (_gym?.RaidInfo != null)
            {
                if (_gym.RaidInfo.RaidPokemon.PokemonId != PokemonId.Missingno)
                    return true;
            }
            return false;
        }

        private static bool CanBerrieGym()
        {
            if (!_session.LogicSettings.GymConfig.EnableGymBerries)
                return false;

            if (!_deployedPokemons.Any(a => a.DeployedFortId.Equals(_gym.Id)))
                return false;
            return true;
        }

        private static bool CanDeployToGym()
        {
            if (!_session.LogicSettings.GymConfig.EnableDeployPokemon)
                return false;

            if (_deployedPokemons.Any(a => a.DeployedFortId.Equals(_gym.Id)))
                return false;

            if (!(_gymDetails.GymStatusAndDefenders.GymDefender.Count() < MaxPlayers))
                return false;

            if (_gym?.RaidInfo != null)
            {
                if (_gym.RaidInfo.RaidPokemon.PokemonId != PokemonId.Missingno)
                    return false;
            }

            return true;
        }

        private static async Task<PokemonData> GetDeployablePokemon()
        {
            PokemonData pokemon = null;
            List<ulong> excluded = new List<ulong>();
            var pokemonList = (await _session.Inventory.GetPokemons().ConfigureAwait(false)).ToList();

            if (_session.LogicSettings.GymConfig.Defenders != null && _session.LogicSettings.GymConfig.Defenders.Count > 0)
            {
                foreach (var def in _session.LogicSettings.GymConfig.Defenders.OrderByDescending(o => o.Priority))
                {
                    var defendersFromConfig = pokemonList.Where(w =>
                        w.PokemonId == def.Pokemon &&
                        w.Id != _session.Profile.PlayerData.BuddyPokemon?.Id &&
                        string.IsNullOrEmpty(w.DeployedFortId) &&
                        w.Cp >= (def.MinCP ?? 0) &&
                        w.Cp <= (def.MaxCP ?? 5000) &&
                        def.IsMoveMatch(w.Move1, w.Move2)
                    ).ToList();

                    if (defendersFromConfig != null && defendersFromConfig.Count > 0)
                        foreach (var _pokemon in defendersFromConfig.OrderByDescending(o => o.Cp))
                        {
                            if (_session.LogicSettings.GymConfig.HealDefendersBeforeApplyToGym)
                            {
                                if (_pokemon.Stamina <= 0)
                                    await RevivePokemon(_pokemon).ConfigureAwait(false);

                                if (_pokemon.Stamina < _pokemon.StaminaMax && _pokemon.Stamina > 0)
                                    await HealPokemon(_pokemon).ConfigureAwait(false);
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
                    .Where(w => !excluded.Contains(w.Id) && w.Id != _session.Profile.PlayerData.BuddyPokemon?.Id)
                    .OrderByDescending(p => p.Cp)
                    .ToList();

                if (pokemonList.Count == 0)
                    return null;

                if (pokemonList.Count == 1)
                    pokemon = pokemonList.FirstOrDefault();

                if (_session.LogicSettings.GymConfig.UseRandomPokemon && pokemon == null)
                    pokemon = pokemonList.ElementAt(new Random().Next(0, pokemonList.Count - 1));

                pokemon = pokemonList.FirstOrDefault(p =>
                    p.Cp <= _session.LogicSettings.GymConfig.MaxCPToDeploy &&
                    PokemonInfo.GetLevel(p) <= _session.LogicSettings.GymConfig.MaxLevelToDeploy &&
                    string.IsNullOrEmpty(p.DeployedFortId)
                );

                if (_session.LogicSettings.GymConfig.HealDefendersBeforeApplyToGym)
                {
                    if (pokemon.Stamina <= 0)
                        await RevivePokemon(pokemon).ConfigureAwait(false);

                    if (pokemon.Stamina < pokemon.StaminaMax && pokemon.Stamina > 0)
                        await HealPokemon(pokemon).ConfigureAwait(false);
                }

                if (pokemon.Stamina < pokemon.StaminaMax)
                {
                    excluded.Add(pokemon.Id);
                    pokemon = null;
                }
            }
            return pokemon;
        }
    }
}
