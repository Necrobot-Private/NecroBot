#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Helpers;
using PoGo.NecroBot.Logic.PoGoUtils;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class LevelUpSpecificPokemonTask
    {
        //this task is duplicated, may need remove to clean up. 
        public static async Task Execute(ISession session, ulong pokemonId)
        {
            using (var blocker = new BlockableScope(session, BotActions.Upgrade))
            {
                if (!await blocker.WaitToRun().ConfigureAwait(false)) return;

                var all = await session.Inventory.GetPokemons().ConfigureAwait(false);
                var pokemons = all.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax);
                var pokemon = pokemons.FirstOrDefault(p => p.Id == pokemonId);

                if (pokemon == null) return;

                var upgradeResult = await session.Inventory.UpgradePokemon(pokemon.Id).ConfigureAwait(false);

                if (upgradeResult.Result.ToString().ToLower().Contains("success"))
                {
                    var stardust = -PokemonCpUtils.GetStardustCostsForPowerup(pokemon.CpMultiplier); //+ pokemon.AdditionalCpMultiplier);
                    var totalStarDust = session.Inventory.UpdateStarDust(stardust);

                    session.EventDispatcher.Send(new PokemonLevelUpEvent
                    {
                        Id = upgradeResult.UpgradedPokemon.PokemonId,
                        Cp = upgradeResult.UpgradedPokemon.Cp,
                        UniqueId = pokemon.Id,
                        PSD = stardust,
                        PCandies = await PokemonInfo.GetCandy(session, pokemon).ConfigureAwait(false),
                        Lvl = upgradeResult.UpgradedPokemon.Level(),
                    });
                }
                await DelayingUtils.DelayAsync(session.LogicSettings.DelayBetweenPlayerActions, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
