using POGOProtos.Enums;

namespace PoGo.NecroBot.Window.Model
{
    public class AffectPokemonViewModel : ViewModelBase
    {
        public PokemonId Pokemon { get; set; }
        public bool Selected { get; set; }
    }
}
