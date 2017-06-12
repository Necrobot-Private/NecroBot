using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;
using POGOProtos.Networking.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public abstract class BaseTransferPokemonTask
    {
        public static string SP { get; private set; }
        public static int maxL { get; private set; }
        public static int userL { get; private set; }

        public static async Task Execute(ISession session, IEnumerable<PokemonData> pokemonsToTransfer, CancellationToken cancellationToken)
        {
            if (pokemonsToTransfer.Count() > 0)
            {
                if (session.LogicSettings.UseBulkTransferPokemon)
                {
                    int page = pokemonsToTransfer.Count() / session.LogicSettings.BulkTransferSize + 1;
                    for (int i = 0; i < page; i++)
                    {
                        TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                        var batchTransfer = pokemonsToTransfer.Skip(i * session.LogicSettings.BulkTransferSize).Take(session.LogicSettings.BulkTransferSize);
                        var t = await session.Client.Inventory.TransferPokemons(batchTransfer.Select(x => x.Id).ToList()).ConfigureAwait(false);
                        if (t.Result == ReleasePokemonResponse.Types.Result.Success)
                        {
                            foreach (var duplicatePokemon in batchTransfer)
                            {
                                await PrintPokemonInfo(session, duplicatePokemon).ConfigureAwait(false);
                            }
                        }
                        else session.EventDispatcher.Send(new WarnEvent() { Message = session.Translation.GetTranslation(TranslationString.BulkTransferFailed, pokemonsToTransfer.Count()) });
                    }
                }
                else
                {
                    foreach (var pokemon in pokemonsToTransfer)
                    {
                        TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                        cancellationToken.ThrowIfCancellationRequested();

                        await session.Client.Inventory.TransferPokemon(pokemon.Id).ConfigureAwait(false);

                        await PrintPokemonInfo(session, pokemon).ConfigureAwait(false);

                        // Padding the TransferEvent with player-choosen delay before instead of after.
                        // This is to remedy too quick transfers, often happening within a second of the
                        // previous action otherwise

                        await DelayingUtils.DelayAsync(session.LogicSettings.TransferActionDelay, 0, cancellationToken).ConfigureAwait(false);
                    }
                }
                userL = 0;
                maxL = 0;
                foreach (var pokemon in pokemonsToTransfer)
                {
                    userL = pokemon.PokemonId.ToString().Length;
                    if (userL > maxL)
                    {
                        maxL = userL;
                    }
                }
            }
        }

        public static async Task PrintPokemonInfo(ISession session, PokemonData duplicatePokemon)
        {
            var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                        ? await session.Inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon).ConfigureAwait(false)
                                        : await session.Inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon).ConfigureAwait(false)) ??
                                    duplicatePokemon;
            SP = "";
            for (int i = 0; i < maxL - userL + 1; i++)
            {
                SP += " ";
            }
            var PokeID = duplicatePokemon.PokemonId.ToString() + SP;

            var ev = new TransferPokemonEvent
            {
                Id = duplicatePokemon.Id,
                PokemonId = duplicatePokemon.PokemonId,
                Perfection = PokemonInfo.CalculatePokemonPerfection(duplicatePokemon),
                Cp = duplicatePokemon.Cp,
                BestCp = bestPokemonOfType.Cp,
                BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                Candy = await session.Inventory.GetCandyCount(duplicatePokemon.PokemonId).ConfigureAwait(false)
            };

            if (await session.Inventory.GetCandyFamily(duplicatePokemon.PokemonId).ConfigureAwait(false) != null)
            {
                ev.FamilyId = (await session.Inventory.GetCandyFamily(duplicatePokemon.PokemonId).ConfigureAwait(false)).FamilyId;
            }

            session.EventDispatcher.Send(ev);
        }
    }
}
