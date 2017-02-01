#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UpgradeSinglePokemonTask
    {
        public static async Task<bool> UpgradeSinglePokemon(ISession session, PokemonData pokemon)
        {
            if (!session.Inventory.CanUpgradePokemon(pokemon))
                return false;

            var upgradeResult = await session.Inventory.UpgradePokemon(pokemon.Id);

            if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.Success && upgradeResult.UpgradedPokemon != null)
            {
                var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                        ? session.Inventory.GetHighestPokemonOfTypeByIv(upgradeResult
                                            .UpgradedPokemon)
                                        : session.Inventory.GetHighestPokemonOfTypeByCp(upgradeResult
                                            .UpgradedPokemon)) ?? upgradeResult.UpgradedPokemon;

                session.EventDispatcher.Send(new UpgradePokemonEvent()
                {
                    Candy = session.Inventory.GetCandyCount(pokemon.PokemonId),
                    Pokemon = upgradeResult.UpgradedPokemon,
                    PokemonId = upgradeResult.UpgradedPokemon.PokemonId,
                    Cp = upgradeResult.UpgradedPokemon.Cp,
                    Id = upgradeResult.UpgradedPokemon.Id,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    Perfection = PokemonInfo.CalculatePokemonPerfection(upgradeResult.UpgradedPokemon)
                });

                return true;
            }
            return false;            
        }

        public static async Task Execute(ISession session, ulong pokemonId, bool isMax = false)
        {
            using (var block = new BlockableScope(session, BotActions.Upgrade))
            {
                PokemonData pokemonToUpgrade = null;
                try
                {
                    if (await block.WaitToRun())
                    {
                        //await session.Inventory.RefreshCachedInventory();

                        if (session.Inventory.GetStarDust() <= session.LogicSettings.GetMinStarDustForLevelUp)
                            return;

                        pokemonToUpgrade = session.Inventory.GetSinglePokemon(pokemonId);
                        if (pokemonToUpgrade == null)
                            return;

                        bool upgradable = false;
                        int upgradeTimes = 0;
                        do
                        {
                            try
                            {
                                upgradable = await UpgradeSinglePokemon(session, pokemonToUpgrade);

                                if (upgradable)
                                {
                                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonUpgrade);
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
                        } while (upgradable && (isMax || upgradeTimes < session.LogicSettings.AmountOfTimesToUpgradeLoop));
                    }
                }
                finally
                {
                    // Reload pokemon after upgrade.
                    var upgradedPokemon = session.Inventory.GetSinglePokemon(pokemonId);
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