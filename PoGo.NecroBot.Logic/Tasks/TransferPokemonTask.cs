#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken, List<ulong> pokemonIds)
        {
            using (var blocker = new BlockableScope(session, BotActions.Transfer))
            {
                if (!await blocker.WaitToRun().ConfigureAwait(false)) return;

                var all = await session.Inventory.GetPokemons().ConfigureAwait(false);
                List<PokemonData> pokemonToTransfer = new List<PokemonData>();
                var pokemons = all.OrderBy(x => x.Cp).ThenBy(n => n.StaminaMax);

                foreach (var item in pokemonIds)
                {
                    var pokemon = pokemons.FirstOrDefault(p => p.Id == item);

                    if (pokemon == null) return;
                    pokemonToTransfer.Add(pokemon);
                }

                var pokemonSettings = await session.Inventory.GetPokemonSettings().ConfigureAwait(false);
                var pokemonFamilies = await session.Inventory.GetPokemonFamilies().ConfigureAwait(false);

                await session.Client.Inventory.TransferPokemons(pokemonIds).ConfigureAwait(false);

                foreach (var pokemon in pokemonToTransfer)
                {
                    var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                                ? await session.Inventory.GetHighestPokemonOfTypeByIv(pokemon).ConfigureAwait(false)
                                                : await session.Inventory.GetHighestPokemonOfTypeByCp(pokemon).ConfigureAwait(false)) ??
                                            pokemon;

                    // Broadcast event as everyone would benefit
                    var ev = new TransferPokemonEvent
                    {
                        Id = pokemon.Id,
                        PokemonId = pokemon.PokemonId, //session.Translation.GetPokemonTranslation(pokemon.PokemonId),
                        Perfection = PokemonInfo.CalculatePokemonPerfection(pokemon),
                        Cp = pokemon.Cp,
                        BestCp = bestPokemonOfType.Cp,
                        BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                        Candy = await session.Inventory.GetCandyCount(pokemon.PokemonId).ConfigureAwait(false),
                        Level = PokemonInfo.GetLevel(pokemon)
                    };

                    if ((await session.Inventory.GetCandyFamily(pokemon.PokemonId).ConfigureAwait(false)) != null)
                    {
                        ev.FamilyId = (await session.Inventory.GetCandyFamily(pokemon.PokemonId).ConfigureAwait(false)).FamilyId;
                    }

                    session.EventDispatcher.Send(ev);
                }

                await DelayingUtils.DelayAsync(session.LogicSettings.TransferActionDelay, 0, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
