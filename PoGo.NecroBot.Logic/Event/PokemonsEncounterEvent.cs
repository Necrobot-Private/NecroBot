using System.Collections.Generic;
using POGOProtos.Map.Pokemon;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.Logic.Event
{
    public class PokemonsEncounterEvent : IEvent
    {
        public List<MapPokemon> EncounterPokemons;
    }
}