#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferWeakPokemonTask : BaseTransferPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            if (!session.LogicSettings.TransferWeakPokemon) return;
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

            var weakPokemon = await
                session.Inventory.GetWeakPokemonToTransfer(
                    session.LogicSettings.PokemonsNotToTransfer,
                    session.LogicSettings.PokemonEvolveFilters,
                    session.LogicSettings.KeepPokemonsThatCanEvolve).ConfigureAwait(false);

            if (weakPokemon.Count() > 0)
            {
                Logging.Logger.Write($"Transferring {weakPokemon.Count()} Weak pokemon.", Logging.LogLevel.Transfer);
                await Execute(session, weakPokemon, cancellationToken).ConfigureAwait(false);
            }
            // Evolve after transfer.
            await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
        }
    }
}