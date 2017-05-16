using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Enums;
using System;

namespace PoGo.NecroBot.Logic.State
{
    public class BotSwitcherState : IState
    {
        private EncounteredEvent encounterData;
        private PokemonId pokemonToCatch;

        public BotSwitcherState(PokemonId pokemon)
        {
            pokemonToCatch = pokemon;
        }

        public BotSwitcherState(PokemonId pokemon, EncounteredEvent encounterData) : this(pokemon)
        {
            this.encounterData = encounterData;
        }

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            if (encounterData == null)
            {
                session.Client.Player.UpdatePlayerLocation(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, session.Client.CurrentAltitude, 10);
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                await CatchNearbyPokemonsTask.Execute(session, cancellationToken, pokemonToCatch).ConfigureAwait(false);
                await CatchLurePokemonsTask.Execute(session, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                //snipe pokemon 
                await MSniperServiceTask.CatchWithSnipe(session, new MSniperServiceTask.MSniperInfo2()
                {
                    AddedTime = DateTime.Now,
                    Latitude = encounterData.Latitude, 
                    Longitude = encounterData.Longitude,
                    Iv = encounterData.IV, 
                    PokemonId =(short)encounterData.PokemonId,
                    SpawnPointId = encounterData.SpawnPointId,
                    EncounterId = Convert.ToUInt64(encounterData.EncounterId)
                }, session.CancellationTokenSource.Token).ConfigureAwait(false);
            }
            return new InfoState();
        }
    }
}