using POGOProtos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.State
{
    public class GymTeamState: IDisposable
    {
        public PokemonData attacker { get; set; }
        public int HpState { get; set; }

        public void Dispose()
        {
            attacker = null;
        }
    }
}
