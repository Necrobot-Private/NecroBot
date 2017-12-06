namespace PoGo.NecroBot.Logic.Event
{
    public class PokestopLimitUpdate : IEvent
    {
        public int Limit;
        public int Value;

        public PokestopLimitUpdate(int v, int pokeStopLimit)
        {
            Value = v;
            Limit = pokeStopLimit;
        }
    }

    public class CatchLimitUpdate: IEvent
    {
        public int Limit;
        public int Value;

        public CatchLimitUpdate(int v, int catchLimit)
        {
            Value = v;
            Limit = catchLimit;
        }

    }
}
