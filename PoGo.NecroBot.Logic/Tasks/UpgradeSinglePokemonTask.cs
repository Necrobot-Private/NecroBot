#region using directives

using System;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Data;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI.Helpers;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UpgradeSinglePokemonTask
    {
        public static async Task<bool> UpgradeSinglePokemon(ISession session, PokemonData pokemon)
        {
            if (!(await session.Inventory.CanUpgradePokemon(pokemon).ConfigureAwait(false)))
                return false;

            var upgradeResult = await session.Inventory.UpgradePokemon(pokemon.Id).ConfigureAwait(false);

            if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.Success && upgradeResult.UpgradedPokemon != null)
            {
                var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                        ? await session.Inventory.GetHighestPokemonOfTypeByIv(upgradeResult
                                            .UpgradedPokemon).ConfigureAwait(false)
                                        : await session.Inventory.GetHighestPokemonOfTypeByCp(upgradeResult
                                            .UpgradedPokemon).ConfigureAwait(false)) ?? upgradeResult.UpgradedPokemon;

                //stardust from what I've gathered is supposed to be - not + for AdditionalCpMultiplier
                var stardust = -PokemonCpUtils.GetStardustCostsForPowerup(upgradeResult.UpgradedPokemon.CpMultiplier);
                var stardust2 = -PokemonCpUtils.GetStardustCostsForPowerup(upgradeResult.UpgradedPokemon.CpMultiplier - upgradeResult.UpgradedPokemon.AdditionalCpMultiplier);
                var totalStarDust = session.Inventory.UpdateStarDust(stardust);
                Logging.Logger.Write($"SD1: {stardust,5:0} | SD2: {stardust2,5:0} | TotalSD: {totalStarDust,5:0}", Logging.LogLevel.Error);

                session.EventDispatcher.Send(new UpgradePokemonEvent()
                {
                    Candy = await session.Inventory.GetCandyCount(pokemon.PokemonId).ConfigureAwait(false),
                    Pokemon = upgradeResult.UpgradedPokemon,
                    PokemonId = upgradeResult.UpgradedPokemon.PokemonId,
                    Cp = upgradeResult.UpgradedPokemon.Cp,
                    Id = upgradeResult.UpgradedPokemon.Id,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    Perfection = PokemonInfo.CalculatePokemonPerfection(upgradeResult.UpgradedPokemon),
                    USD = stardust,
                    Lvl = upgradeResult.UpgradedPokemon.Level(),
                });
                return true;
            }
            return false;
        }

        public static async Task Execute(ISession session, ulong pokemonId, bool isMax = false, int numUpgrades = -1)
        {
            using (var block = new BlockableScope(session, BotActions.Upgrade))
            {
                if (numUpgrades == -1)
                    numUpgrades = session.LogicSettings.AmountOfTimesToUpgradeLoop;

                PokemonData pokemonToUpgrade = null;
                try
                {
                    if (await block.WaitToRun().ConfigureAwait(false))
                    {
                        if (session.Inventory.GetStarDust() <= session.LogicSettings.GetMinStarDustForLevelUp)
                            return;

                        pokemonToUpgrade = await session.Inventory.GetSinglePokemon(pokemonId).ConfigureAwait(false);
                        if (pokemonToUpgrade == null)
                            return;

                        bool upgradable = false;
                        int upgradeTimes = 0;
                        do
                        {
                            try
                            {
                                upgradable = await UpgradeSinglePokemon(session, pokemonToUpgrade).ConfigureAwait(false);

                                if (upgradable)
                                {
                                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonUpgrade).ConfigureAwait(false);
                                }
                                upgradeTimes++;
                            }
                            catch (CaptchaException cex)
                            {
                                throw cex;
                            }
                            catch (Exception)
                            {
                                //make sure no exception happen
                            }
                        } while (upgradable && (isMax || upgradeTimes < numUpgrades));
                    }
                }
                finally
                {
                    // Reload pokemon after upgrade.
                    var upgradedPokemon = await session.Inventory.GetSinglePokemon(pokemonId).ConfigureAwait(false);
                    session.EventDispatcher.Send(new FinishUpgradeEvent()
                    {
                        PokemonId = pokemonId,
                        Pokemon = upgradedPokemon
                    });
                }
            }
        }
    }
}
