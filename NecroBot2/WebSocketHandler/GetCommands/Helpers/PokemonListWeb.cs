using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Data;

namespace NecroBot2.WebSocketHandler.GetCommands.Helpers
{
    public class PokemonListWeb
    {
        public PokemonData Base;

        public PokemonListWeb(PokemonData data)
        {
            Base = data;
        }

        public double IvPerfection
        {
            get { return PokemonInfo.CalculatePokemonPerfection(Base); }
        }
    }
}