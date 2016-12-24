using System.Threading.Tasks;
using PoGo.NecroBot.LIB.Logging;
using PoGo.NecroBot.LIB.State;
using PoGo.NecroBot.LIB.Utils;
using POGOProtos.Networking.Responses;

namespace PoGo.NecroBot.LIB.Tasks
{
    public class LevelUpSpecificPokemonTask
    {
        public static async Task Execute(ISession session, ulong pokemonId)
        {
            var upgradeResult = await session.Inventory.UpgradePokemon(pokemonId);
            if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.Success)
            {
                Logger.Write("Pokemon Upgraded: " +
                             session.Translation.GetPokemonTranslation(
                                 upgradeResult.UpgradedPokemon.PokemonId) + ": " +
                             upgradeResult.UpgradedPokemon.Cp, LogLevel.LevelUp);
            }
            else
            {
                Logger.Write("Pokemon Upgrade Failed.", LogLevel.Warning);
            }

            DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
        }
    }
}