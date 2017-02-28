#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Model.Settings;
using System;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferWeakPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            if (!session.LogicSettings.TransferWeakPokemon) return;

            if (session.LogicSettings.AutoFavoritePokemon)
                await FavoritePokemonTask.Execute(session, cancellationToken);

            var pokemons = session.Inventory.GetPokemons();

            if (session.LogicSettings.UseBulkTransferPokemon)
            {
                int buff = session.LogicSettings.BulkTransferStogareBuffer;
                //check for bag, if bag is nearly full, then process bulk transfer.
                var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
                var totalEggs = session.Inventory.GetEggs();
                if ((maxStorage - totalEggs.Count() - buff) > pokemons.Count()) return;
            }

            var pokemonsFiltered =
                pokemons.Where(pokemon => !session.LogicSettings.PokemonsNotToTransfer.Contains(pokemon.PokemonId) &&
                    session.Inventory.CanTransferPokemon(pokemon));

            if (session.LogicSettings.KeepPokemonsThatCanEvolve)
            {
                var pokemonToEvolve = session.Inventory.GetPokemonToEvolve(session.LogicSettings.PokemonEvolveFilters);

                pokemonsFiltered = pokemonsFiltered.Where(pokemon => !pokemonToEvolve.Any(p => p.Id == pokemon.Id));
            }

            var orderedPokemon = pokemonsFiltered.OrderBy(poke => poke.Cp);
            var pokemonToTransfers = new List<PokemonData>();
            foreach (var pokemon in orderedPokemon)
            {
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                cancellationToken.ThrowIfCancellationRequested();
                if ((pokemon.Cp >= session.LogicSettings.KeepMinCp) ||
                    (PokemonInfo.CalculatePokemonPerfection(pokemon) >= session.LogicSettings.KeepMinIvPercentage &&
                     session.LogicSettings.PrioritizeIvOverCp) ||
                     (PokemonInfo.GetLevel(pokemon) >= session.LogicSettings.KeepMinLvl && session.LogicSettings.UseKeepMinLvl))
                    continue;

                if (session.LogicSettings.UseBulkTransferPokemon)
                {
                    pokemonToTransfers.Add(pokemon);
                }
                else
                {
                    await session.Client.Inventory.TransferPokemon(pokemon.Id);
                    PrintTransferedPokemonInfo(session, pokemon);

                    await DelayingUtils.DelayAsync(session.LogicSettings.TransferActionDelay, 0, cancellationToken);
                }
            }
            if (session.LogicSettings.UseBulkTransferPokemon && pokemonToTransfers.Count > 0)
            {
                int page = orderedPokemon.Count() / session.LogicSettings.BulkTransferSize + 1;
                for (int i = 0; i < page; i++)
                {
                    var batchTransfer = orderedPokemon.Skip(i * session.LogicSettings.BulkTransferSize).Take(session.LogicSettings.BulkTransferSize);
                    var t = await session.Client.Inventory.TransferPokemons(batchTransfer.Select(x => x.Id).ToList());
                    if (t.Result == ReleasePokemonResponse.Types.Result.Success)
                    {
                        foreach (var duplicatePokemon in batchTransfer)
                        {
                            PrintTransferedPokemonInfo(session, duplicatePokemon);
                        }
                    }
                    else session.EventDispatcher.Send(new WarnEvent() { Message = session.Translation.GetTranslation(TranslationString.BulkTransferFailed, orderedPokemon.Count()) });
                }
            }

            // Evolve after transfer.
            await EvolvePokemonTask.Execute(session, cancellationToken);
        }

        private static void PrintTransferedPokemonInfo(ISession session, PokemonData pokemon)
        {
            var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                        ? session.Inventory.GetHighestPokemonOfTypeByIv(pokemon)
                                        : session.Inventory.GetHighestPokemonOfTypeByCp(pokemon)) ?? pokemon;

            var ev = new TransferPokemonEvent
            {
                Id = pokemon.Id,
                PokemonId = pokemon.PokemonId,
                Perfection = PokemonInfo.CalculatePokemonPerfection(pokemon),
                Cp = pokemon.Cp,
                BestCp = bestPokemonOfType.Cp,
                BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                Candy = session.Inventory.GetCandyCount(pokemon.PokemonId)
            };

            if (session.Inventory.GetCandyFamily(pokemon.PokemonId) != null)
            {
                ev.FamilyId = session.Inventory.GetCandyFamily(pokemon.PokemonId).FamilyId;
            }

            session.EventDispatcher.Send(ev);
        }
    }
}