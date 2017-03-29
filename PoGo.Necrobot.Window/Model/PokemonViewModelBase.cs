using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Model;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonViewModelBase : ViewModelBase
    {
        
        public PokemonId PokemonId { get; set; }
        public string PokemonName => this.PokemonId.ToString();
        
        public string PokemonRarityColor => PokemonGradeHelper.GetGradeColor(PokemonId);

        public string PokemonIcon
        {
            get
            {
                return $"https://raw.githubusercontent.com/Necrobot-Private/PokemonGO-Assets/master/pokemon/{(int)PokemonId:000}.png";
            }
        }

    }
}
