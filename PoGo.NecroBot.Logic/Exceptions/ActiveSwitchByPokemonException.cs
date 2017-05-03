using System;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Model;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchByPokemonException : Exception
    {
        public double LastLatitude { get; set; }
        public double LastLongitude { get; set; }
        public PokemonId LastEncounterPokemonId { get; set; }
        public Account Bot { get; set; }
        public bool Snipe { get; set; }
        public EncounteredEvent EncounterData { get; set; }
    }
}