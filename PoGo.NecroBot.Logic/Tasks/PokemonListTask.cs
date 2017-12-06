#region using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class PokemonListTask
    {
        public static async Task Execute(ISession session)
        {
            // Refresh inventory so that the player stats are fresh
            //await session.Inventory.RefreshCachedInventory().ConfigureAwait(false);

            var myPokemonSettings = await session.Inventory.GetPokemonSettings().ConfigureAwait(false);
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await session.Inventory.GetPokemonFamilies().ConfigureAwait(false);
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var allPokemonInBag = await session.Inventory.GetHighestsCp(1000).ConfigureAwait(false);

            var pkmWithIv = allPokemonInBag.Select(p =>
            {
                var settings = pokemonSettings.Single(x => x.PokemonId == p.PokemonId);
                return Tuple.Create(
                    p,
                    PokemonInfo.CalculatePokemonPerfection(p),
                    pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId).Candy_
                );
            });

            session.EventDispatcher.Send(
                new PokemonListEvent
                {
                    PokemonList = pkmWithIv.ToList()
                });

            await DelayingUtils.DelayAsync(session.LogicSettings.DelayBetweenPlayerActions, 0, session.CancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}