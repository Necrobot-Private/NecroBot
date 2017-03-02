#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using POGOProtos.Networking.Responses;
using POGOProtos.Enums;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    internal class LevelUpPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //await session.Inventory.RefreshCachedInventory();

            if (session.Inventory.GetStarDust() <= session.LogicSettings.GetMinStarDustForLevelUp)
                return;

            IEnumerable<PokemonData> upgradablePokemon = session.Inventory.GetPokemonToUpgrade();

            if (upgradablePokemon.Count() == 0)
                return;

            var upgradedNumber = 0;
            List<PokemonId> PokemonToLevel = session.LogicSettings.PokemonsToLevelUp.ToList();
            PokemonToLevel.AddRange(session.LogicSettings.PokemonUpgradeFilters.Select(p => p.Key));
            foreach (var pokemon in upgradablePokemon)
            {
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                //code seem wrong. need need refactor to cleanup code here.
                if (session.LogicSettings.UseLevelUpList && PokemonToLevel != null)
                {
                    for (int i = 0; i < PokemonToLevel.Count; i++)
                    {
                        TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                        //unnessecsarily check, should remove
                        if (PokemonToLevel.Contains(pokemon.PokemonId))
                        {
                            bool upgradable = await UpgradeSinglePokemonTask.UpgradeSinglePokemon(session, pokemon);
                            if (!upgradable || upgradedNumber >= session.LogicSettings.AmountOfTimesToUpgradeLoop)
                                break;
                            await Task.Delay(session.LogicSettings.DelayBetweenPokemonUpgrade);
                            upgradedNumber++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    await UpgradeSinglePokemonTask.UpgradeSinglePokemon(session, pokemon);
                    await Task.Delay(session.LogicSettings.DelayBetweenPlayerActions);
                }
            }
        }
    }
}