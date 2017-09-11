#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferDuplicatePokemonTask : BaseTransferPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            if (!session.LogicSettings.TransferDuplicatePokemon) return;
            if (session.LogicSettings.UseBulkTransferPokemon)
            {
                int buff = session.LogicSettings.BulkTransferStogareBuffer;
                //check for bag, if bag is nearly full, then process bulk transfer.
                var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
                var totalPokemon = await session.Inventory.GetPokemons().ConfigureAwait(false);
                var totalEggs = await session.Inventory.GetEggs().ConfigureAwait(false);
                if ((maxStorage - totalEggs.Count() - buff) > totalPokemon.Count()) return;
            }

            if (session.LogicSettings.AutoFavoritePokemon)
                await FavoritePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
            
            var duplicatePokemons = await
                session.Inventory.GetDuplicatePokemonToTransfer(
                    session.LogicSettings.PokemonsNotToTransfer,
                    session.LogicSettings.PokemonEvolveFilters,
                    session.LogicSettings.KeepPokemonsThatCanEvolve,
                    session.LogicSettings.PrioritizeIvOverCp).ConfigureAwait(false);

            if (duplicatePokemons.Count() > 0)
            {
                Logging.Logger.Write($"Transferring {duplicatePokemons.Count()} Duplicate pokemon.",Logging.LogLevel.Transfer);
                await Execute(session, duplicatePokemons, cancellationToken).ConfigureAwait(false);
            }

            var maxPokemonsToTransfer = await
               session.Inventory.GetMaxPokemonToTransfer(
                   session.LogicSettings.PokemonsNotToTransfer,
                   session.LogicSettings.PrioritizeIvOverCp).ConfigureAwait(false);

            if (maxPokemonsToTransfer.Count() > 0)
            {
                Logging.Logger.Write($"Transferring {maxPokemonsToTransfer.Count()} pokemon over max limit.", Logging.LogLevel.Transfer);
                await Execute(session, maxPokemonsToTransfer, cancellationToken).ConfigureAwait(false);
            }

            var SlashedPokemonsToTransfer = await
               session.Inventory.GetSlashedPokemonToTransfer().ConfigureAwait(false);

            if (SlashedPokemonsToTransfer.Count() > 0)
            {
                Logging.Logger.Write($"Transferring {SlashedPokemonsToTransfer.Count()} Slashed pokemon.", Logging.LogLevel.Transfer);
                await Execute(session, SlashedPokemonsToTransfer, cancellationToken).ConfigureAwait(false);
            }

            // Evolve after transfer
            await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
        }
    }
}