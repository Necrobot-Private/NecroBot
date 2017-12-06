using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class GymFeedPokemonTask
    {
        public static async Task Execute(ISession session, FortData gym, ItemData item, PokemonData pokemon, int startingQuantity = 1)
        {
            var response = await session.Client.Fort.GymFeedPokemon(gym.Id, item.ItemId, pokemon.Id, startingQuantity).ConfigureAwait(false);
            switch (response.Result)
            {
                case GymFeedPokemonResponse.Types.Result.Success:
                    Logger.Write($"Succes", LogLevel.Info);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorCannotUse:
                    Logger.Write($"Error Cannot Use {item.ItemId}!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorGymBusy:
                    Logger.Write($"Error Gym Busy!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorGymClosed:
                    Logger.Write($"Error Gym Closed!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorNoBerriesLeft:
                    Logger.Write($"Error No Berries Left!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorNotInRange:
                    Logger.Write($"Error Not In Range!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorPokemonFull:
                    Logger.Write($"Error Pokemon Full!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorPokemonNotThere:
                    Logger.Write($"Error Pokemon Not There!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorRaidActive:
                    Logger.Write($"Error Raid Active!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorTooFast:
                    Logger.Write($"Error Too Fast!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorTooFrequent:
                    Logger.Write($"Error Too Frequent!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorWrongCount:
                    Logger.Write($"Error Wrong Count!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.ErrorWrongTeam:
                    Logger.Write($"Error Wrong Team!", LogLevel.Error);
                    break;
                case GymFeedPokemonResponse.Types.Result.Unset:
                    Logger.Write($"Unset!", LogLevel.Error);
                    break;
                default:
                    Logger.Write($"Failed to use {item.ItemId}!", LogLevel.Error);
                    break;
            }
        }
    }
}
