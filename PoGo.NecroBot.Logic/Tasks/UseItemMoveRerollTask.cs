using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseItemMoveRerollTask
    {
        public static async Task Execute(ISession session, PokemonData pokemondata)
        {
            var response = await session.Client.Inventory.UseItemMoveReroll(pokemondata.Id).ConfigureAwait(false);
            switch (response.Result)
            {
                case ReleasePokemonResponse.Types.Result.Success:
                    Logger.Write($"Success");
                    break;
                case ReleasePokemonResponse.Types.Result.Failed:
                    Logger.Write($"Failed to use MoveReroll!", LogLevel.Error);
                    break;
                case ReleasePokemonResponse.Types.Result.ErrorPokemonIsBuddy:
                    break;
                case ReleasePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                    break;
                case ReleasePokemonResponse.Types.Result.PokemonDeployed:
                    break;
                case ReleasePokemonResponse.Types.Result.Unset:
                    break;
                default:
                    Logger.Write($"Failed to use MoveReroll!", LogLevel.Warning);
                    break;
            }
        }
    }
}
