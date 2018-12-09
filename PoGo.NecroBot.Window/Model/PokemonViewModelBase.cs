using PoGo.NecroBot.Logic.Common;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Window.Model
{
    public class PokemonViewModelBase : ViewModelBase
    {
        
        public PokemonId PokemonId { get; set; }
        public string PokemonName => PokemonId.ToString();
        
        public string PokemonRarityColor => PokemonGradeHelper.GetGradeColor(PokemonId);

        public string PokemonIcon
        {
            get
            {
                return $"https://cdn.jsdelivr.net/gh/NecroBot-Private/PokemonGO-Assets@master/pokemon/{(int)PokemonId}.png";
            }
        }

    }
}
