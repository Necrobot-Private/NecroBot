using System;
using System.Collections.Generic;

namespace PoGo.NecroBot.Logic.State
{
    public class SessionStats
    {
        public int SnipeCount { get; set; }
        public DateTime LastSnipeTime { get; set; }
        public List<Int64> PokeStopTimestamps { get; private set; }
        public List<Int64> PokemonTimestamps { get; private set; }

        public SessionStats()
        {
            PokemonTimestamps = new List<Int64>();
            PokeStopTimestamps = new List<Int64>();
        }
    }
}
