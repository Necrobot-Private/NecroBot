using POGOProtos.Enums;

namespace PoGo.NecroBot.Window.Model
{
    public class PokeDexEntryViewModel : ViewModelBase
    {
        public PokemonId PokemonId { get; set; }
        public int Seen { get; set; }
        public int Caught { get; set; }

        public int Id => (int)PokemonId;

        public string Name => PokemonId.ToString();

        public double Opacity { get; set; }
        public string TimelineDuration { get; set; }
    }
}
