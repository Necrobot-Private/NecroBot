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
            this.pokemonToCatch = pokemon;
        }

        public BotSwitcherState(PokemonId pokemon, EncounteredEvent encounterData) : this(pokemon)
        {
            this.encounterData = encounterData;
        }

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            if (this.encounterData == null)
            {
                await session.Client.Player.UpdatePlayerLocation(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, session.Client.CurrentAltitude, 10);
                await Task.Delay(1000, cancellationToken);
                await CatchNearbyPokemonsTask.Execute(session, cancellationToken, this.pokemonToCatch);
                await CatchLurePokemonsTask.Execute(session, cancellationToken);
            }
            else
            {
                //snipe pokemon 
                await MSniperServiceTask.CatchFromService(session, session.CancellationTokenSource.Token, new MSniperServiceTask.MSniperInfo2()
                {
                    AddedTime = DateTime.Now,
                    Latitude = this.encounterData.Latitude, 
                    Longitude = this.encounterData.Longitude,
                    Iv = this.encounterData.IV, 
                    PokemonId =(short) this.encounterData.PokemonId,
                    SpawnPointId = this.encounterData.SpawnPointId,
                    EncounterId = Convert.ToUInt64( this.encounterData.EncounterId)
                });
            }
            return new InfoState();
        }
    }
}