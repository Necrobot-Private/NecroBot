using System.Collections.Generic;
using POGOProtos.Map.Pokemon;

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class PokemonsEncounterEvent : IEvent
    {
        public List<MapPokemon> EncounterPokemons;
    }
}