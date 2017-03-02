using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Event
{
    public class PokestopLimitUpdate : IEvent
    {
        public int Limit;
        public int Value;

        public PokestopLimitUpdate(int v, int pokeStopLimit)
        {
            this.Value = v;
            this.Limit = pokeStopLimit;
        }
    }

    public class CatchLimitUpdate: IEvent
    {
        public int Limit;
        public int Value;

        public CatchLimitUpdate(int v, int catchLimit)
        {
            this.Value = v;
            this.Limit = catchLimit;
        }

    }
}
