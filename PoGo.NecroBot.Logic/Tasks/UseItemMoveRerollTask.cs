using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseItemMoveRerollTask
    {
        public static async Task Execute(ISession session, ItemData item, PokemonData pokemondata)
        {
            var response = await session.Client.Inventory.UseItemMoveReroll(item.ItemId, pokemondata.Id).ConfigureAwait(false);
            switch (response.Result)
            {
                case UseItemMoveRerollResponse.Types.Result.Success:
                    Logger.Write($"Success to use {item.ItemId}", LogLevel.Info);
                    break;
                case UseItemMoveRerollResponse.Types.Result.InvalidPokemon:
                    Logger.Write($"Failed Invalid Pokemon!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.ItemNotInInventory:
                    Logger.Write($"Error Item Not In Inventory!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.NoOtherMoves:
                    Logger.Write($"Error No Other Moves!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.NoPlayer:
                    Logger.Write($"No Player!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.NoPokemon:
                    Logger.Write($"No Pokemon!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.WrongItemType:
                    Logger.Write($"Wrong Item Type!", LogLevel.Error);
                    break;
                case UseItemMoveRerollResponse.Types.Result.Unset:
                    Logger.Write($"Unset!", LogLevel.Warning);
                    break;
                default:
                    Logger.Write($"Failed to use {item.ItemId}!", LogLevel.Warning);
                    break;
            }
        }
    }
}
