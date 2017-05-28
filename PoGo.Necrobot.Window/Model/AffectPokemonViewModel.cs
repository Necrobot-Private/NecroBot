using POGOProtos.Enums;

namespace PoGo.Necrobot.Window.Model
{
    public class AffectPokemonViewModel : ViewModelBase
    {
        public PokemonId Pokemon { get; set; }
        public bool Selected { get; set; }
    }
}
