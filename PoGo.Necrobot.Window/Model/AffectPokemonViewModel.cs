using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class AffectPokemonViewModel : ViewModelBase
    {
        public PokemonId Pokemon { get; set; }
        public bool Selected { get; set; }
    }
}
