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

                var userL = 0;
                var maxL = 0;
                foreach (var pokemon in pokemonToTransfer)
                {
                    userL = pokemon.PokemonId.ToString().Length;
                    if (userL > maxL)
                    {
                        maxL = userL;
                    }
                }

                foreach (var pokemon in pokemonToTransfer)
                {
                    var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                                ? await session.Inventory.GetHighestPokemonOfTypeByIv(pokemon).ConfigureAwait(false)
                                                : await session.Inventory.GetHighestPokemonOfTypeByCp(pokemon).ConfigureAwait(false)) ??
                                            pokemon;
                    var SP = "";
                    var user = pokemon.PokemonId.ToString();
                    for (int i = 0; i < maxL - user.Length + 1; i++)
                    {
                        SP += " ";
                    }
                    var PokeID = pokemon.PokemonId.ToString() + SP;

                    // Broadcast event as everyone would benefit

                    var ev = new TransferPokemonEvent
                    {
                        Id = pokemon.Id,
                        PokemonId = PokeID,
                        Perfection = PokemonInfo.CalculatePokemonPerfection(pokemon),
                        Cp = pokemon.Cp,
                        BestCp = bestPokemonOfType.Cp,
                        BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                        Candy = await session.Inventory.GetCandyCount(pokemon.PokemonId).ConfigureAwait(false)
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