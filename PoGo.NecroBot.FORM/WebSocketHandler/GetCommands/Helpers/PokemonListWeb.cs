using PoGo.NecroBot.Logic.Mini.PoGoUtils;
using POGOProtos.Data;

namespace PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Helpers
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