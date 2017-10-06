#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Tasks;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public class FarmState : IState
    {
        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            if (session.LogicSettings.UseNearActionRandom)
            {
                await HumanRandomActionTask.Execute(session, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (session.LogicSettings.UseEggIncubators)
                    await UseIncubatorsTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (session.LogicSettings.TransferDuplicatePokemon)
                    await TransferDuplicatePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (session.LogicSettings.TransferWeakPokemon)
                    await TransferWeakPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (EvolvePokemonTask.IsActivated(session))
                    await EvolvePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (session.LogicSettings.UseLuckyEggConstantly)
                    await UseLuckyEggConstantlyTask.Execute(session, cancellationToken).ConfigureAwait(false);
                if (session.LogicSettings.UseIncenseConstantly)
                    await UseIncenseConstantlyTask.Execute(session, cancellationToken).ConfigureAwait(false);

                await GetPokeDexCount.Execute(session, cancellationToken).ConfigureAwait(false);

                if (session.LogicSettings.RenamePokemon)
                    await RenamePokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);

                await RecycleItemsTask.Execute(session, cancellationToken).ConfigureAwait(false);

                if (session.LogicSettings.AutomaticallyLevelUpPokemon)
                    await LevelUpPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);
            }

            await SelectBuddyPokemonTask.Execute(session, cancellationToken).ConfigureAwait(false);


            if (session.LogicSettings.UseGpxPathing)
                await FarmPokestopsGpxTask.Execute(session, cancellationToken).ConfigureAwait(false);
            else
                await FarmPokestopsTask.Execute(session, cancellationToken).ConfigureAwait(false);

            return this;
        }
    }
}