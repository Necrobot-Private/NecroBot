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
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Data;
using POGOProtos.Data.Battle;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseGymBattleTask
    {
        // ReSharper disable once UnusedMember.Local
        private static Dictionary<FortData, DateTime> _gyms = new Dictionary<FortData, DateTime>();
        public static async Task Execute(ISession session, CancellationToken cancellationToken, FortData gym, FortDetailsResponse fortInfo)
        {
            if (!session.LogicSettings.GymAllowed || gym.Type != FortType.Gym) return;

            cancellationToken.ThrowIfCancellationRequested();
            // ReSharper disable once UnusedVariable
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

                var fortDetails = await session.Client.Fort.GetGymDetails(gym.Id, gym.Latitude, gym.Longitude);

                if (fortDetails.Result == GetGymDetailsResponse.Types.Result.Success)
                {
                    if (fortDetails.Result == GetGymDetailsResponse.Types.Result.Success)
                    {
                        var player = session.Profile.PlayerData;
                        await EnsureJoinTeam(session, player);

                        //Do gym tutorial - tobe coded

                        session.EventDispatcher.Send(new GymDetailInfoEvent()
                        {
                            Team = fortDetails.GymState.FortData.OwnedByTeam,
                            Point = gym.GymPoints,
                            Name = fortDetails.Name,
                        });

                        if (player.Team != TeamColor.Neutral && fortDetails.GymState.FortData.OwnedByTeam == player.Team)
                        {
                            //trainning logic will come here
                            await DeployPokemonToGym(session, fortInfo, fortDetails);
                        }
                        else
                        {
                            var badassPokemon = await session.Inventory.GetHighestCpForGym(6);
                            bool fighting = true;
                            var pokemonDatas = badassPokemon as PokemonData[] ?? badassPokemon.ToArray();
                            while (fighting)
                            {
                                // Heal pokemon
                                foreach (var pokemon in pokemonDatas)
                                {
                                    if (pokemon.Stamina <= 0)
                                        await RevivePokemon(session, pokemon);
                                    if (pokemon.Stamina < pokemon.StaminaMax)
                                        await HealPokemon(session, pokemon);
                                }
                                Thread.Sleep(4000);

                                var result = await StartBattle(session, pokemonDatas, gym);
                                if (result != null)
                                {
                                    if (result.Result == StartGymBattleResponse.Types.Result.Success)
                                    {
                                        switch (result.BattleLog.State)
                                        {
                                            case BattleState.Active:
                                                Debug.WriteLine($"Time to start the Attack Mode");
                                                await AttackGym(session, cancellationToken, gym, result);
                                                break;
                                            case BattleState.Defeated:
                                                break;
                                            case BattleState.StateUnset:
                                                break;
                                            case BattleState.TimedOut:
                                                break;
                                            case BattleState.Victory:
                                                fighting = false;
                                                break;
                                            default:
                                                Debug.WriteLine($"Unhandled result starting gym battle:\n{result}");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"Hmmm, no result?");
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                    fortDetails = await session.Client.Fort.GetGymDetails(gym.Id, gym.Latitude, gym.Longitude);
                                    if (fortDetails.GymState.FortData.OwnedByTeam == TeamColor.Neutral ||
                                        fortDetails.GymState.FortData.OwnedByTeam == session.Profile.PlayerData.Team)
                                        break;
                                }
                            }

                            // Finished battling.. OwnedByTeam should be neutral when we reach here
                            if (fortDetails.GymState.FortData.OwnedByTeam == TeamColor.Neutral ||
                                fortDetails.GymState.FortData.OwnedByTeam == session.Profile.PlayerData.Team)
                            {
                                await Execute(session, cancellationToken, gym, fortInfo);
                            }
                            else
                            {
                                Debug.WriteLine($"Hmmm, for some reason the gym was not taken over...");
                            }
                            // Logger.Write($"No action, This gym is defending by other color", LogLevel.Gym, ConsoleColor.Cyan);
                        }
                    }
                    else
                    {
                        Logger.Write($"You are not level 5 yet, come back later...", LogLevel.Gym, ConsoleColor.Cyan);
                    }
                }
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                Logger.Write($"Ignoring  Gym : {fortInfo.Name} - ", LogLevel.Gym, ConsoleColor.Cyan);
            }
        }

        private static async Task DeployPokemonToGym(ISession session, FortDetailsResponse fortInfo, GetGymDetailsResponse fortDetails)
        {
            var pokemon = await GetDeployablePokemon(session);
            if (pokemon != null)
            {
                var response = await session.Client.Fort.FortDeployPokemon(fortInfo.FortId, pokemon.Id);
                if (response.Result == FortDeployPokemonResponse.Types.Result.Success)
                {
                    session.EventDispatcher.Send(new GymDeployEvent()
                    {
                        PokemonId = pokemon.PokemonId,
                        Name = fortDetails.Name
                    });
                }
            }
        }

        public static async Task RevivePokemon(ISession session, PokemonData pokemon)
        {
            var normalRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemRevive);
            if (normalRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemRevive, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "normal",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (normalRevives - 1)
                        });
                        break;
                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine(
                            $"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;
                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;
                    default:
                        return;
                }
                return;
            }
            var maxRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive);
            if (maxRevives > 0 && pokemon.Stamina <= 0)
            {
                var ret = await session.Client.Inventory.UseItemRevive(ItemId.ItemMaxRevive, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedRevive
                        {
                            Type = "max",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (maxRevives - 1)
                        });
                        break;

                    case UseItemReviveResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
            }
        }

        public static async Task HealPokemon(ISession session, PokemonData pokemon)
        {
            var normalPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion);
            while (normalPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemPotion, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemPotionResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedPotion
                        {
                            Type = "normal",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (normalPotions - 1)
                        });
                        break;

                    case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
                normalPotions--;
            }

            var superPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
            while (superPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemSuperPotion, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemPotionResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedPotion
                        {
                            Type = "super",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (superPotions - 1)
                        });
                        break;

                    case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
                superPotions--;
            }

            var hyperPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion);
            while (hyperPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemHyperPotion, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemPotionResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedPotion
                        {
                            Type = "hyper",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (hyperPotions - 1)
                        });
                        break;

                    case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
                hyperPotions--;
            }

            var maxPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion);
            while (maxPotions > 0 && (pokemon.Stamina < pokemon.StaminaMax))
            {
                var ret = await session.Client.Inventory.UseItemPotion(ItemId.ItemMaxPotion, pokemon.Id);
                switch (ret.Result)
                {
                    case UseItemPotionResponse.Types.Result.Success:
                        session.EventDispatcher.Send(new EventUsedPotion
                        {
                            Type = "max",
                            PokemonCp = pokemon.Cp,
                            PokemonId = pokemon.PokemonId.ToString(),
                            Remaining = (maxPotions - 1)
                        });
                        break;

                    case UseItemPotionResponse.Types.Result.ErrorDeployedToFort:
                        Debug.WriteLine($"Pokemon: {pokemon.PokemonId} (CP: {pokemon.Cp}) is already deployed to a gym...");
                        return;

                    case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                        return;

                    default:
                        return;
                }
                maxPotions--;
            }
        }

        public static async Task EngageGymBattleTask(ISession session, CancellationToken cancellationToken, FortData currentFortData, GetGymDetailsResponse fortInfo)
        {
            bool fighting = true;
            var badassPokemon = await session.Inventory.GetHighestCpForGym(6);

            // Start Battle
            var gymInfo =
                await
                    session.Client.Fort.GetGymDetails(currentFortData.Id, currentFortData.Latitude,
                        currentFortData.Longitude);

            var pokemonDatas = badassPokemon as PokemonData[] ?? badassPokemon.ToArray();
            while (fighting)
            {
                // Heal pokemon
                foreach (var pokemon in pokemonDatas)
                {
                    if (pokemon.Stamina <= 0)
                        await RevivePokemon(session, pokemon);
                    if (pokemon.Stamina < pokemon.StaminaMax)
                        await HealPokemon(session, pokemon);

                    if (pokemon.Stamina < pokemon.StaminaMax)
                        return;
                }
                Thread.Sleep(4000);

                var result = await StartBattle(session, pokemonDatas, currentFortData);
                if (result != null)
                {
                    if (result.Result == StartGymBattleResponse.Types.Result.Success)
                    {
                        switch (result.BattleLog.State)
                        {
                            case BattleState.Active:
                                Debug.WriteLine($"Time to start the Attack Mode");
                                await AttackGym(session, cancellationToken, currentFortData, result);
                                break;
                            case BattleState.Defeated:
                                break;
                            case BattleState.StateUnset:
                                break;
                            case BattleState.TimedOut:
                                break;
                            case BattleState.Victory:
                                fighting = false;
                                break;
                            default:
                                Debug.WriteLine($"Unhandled result starting gym battle:\n{result}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Hmmm, no result?");
                        Thread.Sleep(5000);
                        continue;
                    }

                    gymInfo = await
                        session.Client.Fort.GetGymDetails(currentFortData.Id, currentFortData.Latitude,
                        currentFortData.Longitude);
                    if (gymInfo.GymState.FortData.OwnedByTeam == TeamColor.Neutral ||
                        gymInfo.GymState.FortData.OwnedByTeam == session.Profile.PlayerData.Team)
                        break;
                }
            }

            var fortDetailsResponse = currentFortData.Id == SetMoveToTargetTask.TARGET_ID ? SetMoveToTargetTask.FortInfo : await session.Client.Fort.GetFort(currentFortData.Id, currentFortData.Latitude, currentFortData.Longitude);
            // Finished battling.. OwnedByTeam should be neutral when we reach here
            if (gymInfo.GymState.FortData.OwnedByTeam == TeamColor.Neutral ||
                gymInfo.GymState.FortData.OwnedByTeam == session.Profile.PlayerData.Team)
            {
                await Execute(session, cancellationToken, currentFortData, fortDetailsResponse);
            }
            else
            {
                Debug.WriteLine($"Hmmm, for some reason the gym was not taken over...");
            }
        }

        private static int _currentAttackerEnergy;

        // ReSharper disable once UnusedParameter.Local
        private static async Task AttackGym(ISession session, CancellationToken cancellationToken, FortData currentFortData, StartGymBattleResponse startResponse)
        {
            long serverMs = startResponse.BattleLog.BattleStartTimestampMs;
            var lastActions = startResponse.BattleLog.BattleActions.ToList();

            Debug.WriteLine($"Gym battle started; fighting trainer: {startResponse.Defender.TrainerPublicProfile.Name}");
            Debug.WriteLine($"We are attacking: {startResponse.Defender.ActivePokemon.PokemonData.PokemonId}");
            int loops = 0;
            List<BattleAction> emptyActions = new List<BattleAction>();
            BattleAction emptyAction = new BattleAction();
            PokemonData attacker = null;

            while (true)
            {
                var attackActionz = GetActions(serverMs, attacker, _currentAttackerEnergy);
                var attackResult =
                    await session.Client.Fort.AttackGym
                    (
                        currentFortData.Id,
                        startResponse.BattleId,
                        (loops > 0 ? attackActionz : emptyActions),
                        (loops > 0 ? lastActions.Last() : emptyAction)
                    );
                loops++;

                if (attackResult.Result == AttackGymResponse.Types.Result.Success)
                {

                    switch (attackResult.BattleLog.State)
                    {
                        case BattleState.Active:
                            _currentAttackerEnergy = attackResult.ActiveAttacker.CurrentEnergy;
                            attacker = attackResult.ActiveAttacker.PokemonData;
                            Debug.WriteLine(
                                $"Successful attack! - They have {attackResult.ActiveDefender.CurrentHealth} health left, we have {attackResult.ActiveAttacker.CurrentHealth} health, energy: {attackResult.ActiveAttacker.CurrentEnergy}");
                            break;
                        case BattleState.Defeated:
                            Debug.WriteLine(
                                $"We were defeated... (AttackGym)");
                            return;
                        case BattleState.TimedOut:
                            Debug.WriteLine(
                                $"Our attack timed out...: {attackResult}");
                            return;
                        case BattleState.StateUnset:
                            Debug.WriteLine(
                                $"State was unset?: {attackResult}");
                            return;
                        case BattleState.Victory:
                            Debug.WriteLine(
                                $"We were victorious!: {attackResult}");
                            return;
                        default:
                            Debug.WriteLine(
                                $"Unhandled attack response: {attackResult}");
                            continue;
                    }
                    Debug.WriteLine($"{attackResult}");

                    Thread.Sleep(5000);
                    // Sleep until last sent battle action expired
                    //bool sleep = true;
                    //while (attackActionz.LastOrDefault() != null && sleep)
                    //{
                    //    Thread.Sleep(1000);
                    //    DateTime currentTime = DateTime.Now;
                    //    if (currentTime.ToUnixTime() > attackActionz.LastOrDefault().DamageWindowsEndTimestampMss)
                    //    {
                    //        sleep = false;
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        Debug.WriteLine($"Sleeping until next attack, {currentTime.ToUnixTime()} < {attackActionz.LastOrDefault().DamageWindowsEndTimestampMss}");
                    //    }
                    //}
                    Debug.WriteLine($"Finished sleeping, starting another attack");

                }
                else
                {
                    Debug.WriteLine($"Unexpected attack result:\n{attackResult}");
                    continue;
                }

                if (attackResult.BattleLog != null && attackResult.BattleLog.BattleActions.Count > 0)
                    lastActions.AddRange(attackResult.BattleLog.BattleActions);
                serverMs = attackResult.BattleLog.ServerMs;
            }
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

        private static int _pos;
        public static List<BattleAction> GetActions(long serverMs, PokemonData attacker, int energy)
        {
            Random rnd = new Random();
            List<BattleAction> actions = new List<BattleAction>();
            DateTime now = DateTimeFromUnixTimestampMillis(serverMs);
            Debug.WriteLine($"AttackGym Count: {_pos}");

            if (attacker != null)
            {
                var move1 = PokemonMoveMetaRegistry.GetMeta(attacker.Move1);
                var move2 = PokemonMoveMetaRegistry.GetMeta(attacker.Move2);
                Debug.WriteLine($"Retrieved Move Metadata, Move1: {move1.GetTime()} - Move2: {move2.GetTime()}");

                switch (_pos)
                {
                    case 0:
                        for (int x = 0; x < 3; x++)
                        {
                            BattleAction action = new BattleAction();
                            now = now.AddMilliseconds(move1.GetTime());
                            action.Type = BattleActionType.ActionAttack;
                            action.DurationMs = move1.GetTime();
                            action.ActionStartMs = now.ToUnixTime();
                            action.TargetIndex = -1;
                            action.ActivePokemonId = attacker.Id;
                            action.DamageWindowsStartTimestampMss = now.ToUnixTime() - 200;
                            action.DamageWindowsEndTimestampMss = now.ToUnixTime();
                            actions.Add(action);
                        }
                        _pos++;
                        break;

                    case 1:
                        _pos++;
                        break;

                    default:
                        for (int x = 0; x < 3; x++)
                        {
                            BattleAction action = new BattleAction();

                            //if (x == 2 && currentAttackerEnergy > move2.GetEnergy())
                            //{
                            //    // Special Attack
                            //    now = now.AddMilliseconds(move2.GetTime());
                            //    action.Type = BattleActionType.ActionSpecialAttack;
                            //    action.DurationMs = move2.GetTime();
                            //}
                            //else
                            //{
                            //    // Basic Attack
                            //    now = now.AddMilliseconds(move1.GetTime());
                            //    action.Type = BattleActionType.ActionAttack;
                            //    action.DurationMs = move1.GetTime();
                            //}

                            // Basic Attack
                            now = now.AddMilliseconds(move1.GetTime());
                            action.Type = BattleActionType.ActionAttack;
                            action.DurationMs = move1.GetTime();

                            action.ActionStartMs = now.ToUnixTime();
                            action.TargetIndex = -1;
                            action.ActivePokemonId = attacker.Id;
                            action.DamageWindowsStartTimestampMss = now.ToUnixTime() - 200;
                            action.DamageWindowsEndTimestampMss = now.ToUnixTime();


                            actions.Add(action);
                        }
                        _pos++;
                        break;
                }
            }



            return actions;
        }

        private static async Task<StartGymBattleResponse> StartBattle(ISession session, IEnumerable<PokemonData> pokemons, FortData currentFortData)
        {
            IEnumerable<PokemonData> currentPokemons = pokemons;
            var gymInfo = await session.Client.Fort.GetGymDetails(currentFortData.Id, currentFortData.Latitude, currentFortData.Longitude);
            int trys = 0;

            var pokemonDatas = currentPokemons as PokemonData[] ?? currentPokemons.ToArray();
            var defendingPokemon = gymInfo.GymState.Memberships.First().PokemonData.Id;
            var attackerPokemons = pokemonDatas.Select(pokemon => pokemon.Id);
            var result = await session.Client.Fort.StartGymBattle(currentFortData.Id, defendingPokemon, attackerPokemons);

            while (true)
            {
                trys++;
                if (result.Result == StartGymBattleResponse.Types.Result.Success)
                {
                    switch (result.BattleLog.State)
                    {
                        case BattleState.Active:
                            if (result.Result == StartGymBattleResponse.Types.Result.Success)
                            {
                                session.EventDispatcher.Send(new GymBattleStarted {GymName = gymInfo.Name});
                                return result;
                            }
                            else
                            {
                                Debug.WriteLine($"Unexpected result from Server: {result}");
                            }
                            break;
                        case BattleState.Defeated:
                            Debug.WriteLine($"We were defeated in battle.");
                            return result;
                        case BattleState.Victory:
                            Debug.WriteLine($"We were victorious");
                            _pos = 0;
                            return result;
                        case BattleState.StateUnset:
                            Debug.WriteLine($"Error occoured: {result.BattleLog.State}");
                            break;
                        case BattleState.TimedOut:
                            Debug.WriteLine($"Error occoured: {result.BattleLog.State}");
                            break;
                        default:
                            Debug.WriteLine($"Unhandled occoured: {result.BattleLog.State}");
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
                    session.EventDispatcher.Send(new GymErrorUnset {GymName = gymInfo.Name});
                }

                if (trys > 5)
                    return result;

                // Update the state of the Gym and try to call the battle again
                gymInfo = await session.Client.Fort.GetGymDetails(currentFortData.Id, currentFortData.Latitude, currentFortData.Longitude);
                result = await session.Client.Fort.StartGymBattle(currentFortData.Id,
                    gymInfo.GymState.Memberships.First().PokemonData.Id,
                    pokemonDatas.Select(pokemon => pokemon.Id));
            }
        }

        private static async Task EnsureJoinTeam(ISession session, PlayerData player)
        {
            if (session.Profile.PlayerData.Team == TeamColor.Neutral)
            {
                var defaultTeam = session.LogicSettings.GymDefaultTeam;
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

        private bool CanVisitGym()
        {
            return true;
        }



        private static async Task<PokemonData> GetDeployablePokemon(ISession session)
        {
            var pokemonList = (await session.Inventory.GetPokemons()).ToList();
            pokemonList = pokemonList.OrderByDescending(p => p.Cp).Skip(Math.Min(pokemonList.Count - 1, session.LogicSettings.GymNumberOfTopPokemonToBeExcluded)).ToList();

            if (pokemonList.Count == 1) return pokemonList.FirstOrDefault();
            if (session.LogicSettings.GymUseRandomPokemon)
            {

                return pokemonList.ElementAt(new Random().Next(0, pokemonList.Count - 1));
            }

            var pokemon = pokemonList.FirstOrDefault(p => p.Cp <= session.LogicSettings.GymMaxCPToDeploy && PokemonInfo.GetLevel(p) <= session.LogicSettings.GymMaxLevelToDeploy && string.IsNullOrEmpty(p.DeployedFortId));
            return pokemon;
        }
    }

}

