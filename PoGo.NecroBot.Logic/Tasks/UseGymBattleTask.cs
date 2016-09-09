#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using PoGo.NecroBot.Logic.Utils;
using PoGo.NecroBot.Logic.Logging;
using GeoCoordinatePortable;
using POGOProtos.Networking.Responses;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Event.Gym;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseGymBattleTask
    {
        private static Dictionary<FortData, DateTime> gyms = new Dictionary<FortData, DateTime>();
        public static async Task Execute(ISession session, CancellationToken cancellationToken, FortData gym, FortDetailsResponse fortInfo)
        {
            if (!session.LogicSettings.GymAllowed || gym.Type != FortType.Gym) return;

            cancellationToken.ThrowIfCancellationRequested();
            var distance = session.Navigation.WalkStrategy.CalculateDistance(session.Client.CurrentLatitude, session.Client.CurrentLongitude, gym.Latitude, gym.Longitude);

            if (fortInfo != null)
            {
                //session.EventDispatcher.Send(new GymWalkToTargetEvent()
                //{
                //    Name = fortInfo.Name,
                //    Distance = distance,
                //    Latitude = fortInfo.Latitude,
                //    Longitude = fortInfo.Longitude
                //});

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
                            //Battle logic code come here
                            Logger.Write($"No action, This gym is defending by other color", LogLevel.Gym, ConsoleColor.Cyan);
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

        private static async Task EnsureJoinTeam(ISession session, POGOProtos.Data.PlayerData player)
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



        private static async Task<POGOProtos.Data.PokemonData> GetDeployablePokemon(ISession session)
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