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
                    session.LogicSettings.KeepPokemonsThatCanEvolve,
                    session.LogicSettings.PrioritizeIvOverCp).ConfigureAwait(false);

            var orderedPokemon = weakPokemon.OrderBy(poke => poke.Cp);

            await Execute(session, orderedPokemon, cancellationToken).ConfigureAwait(false);

            // Evolve after transfer.
            await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
        }
    }
}