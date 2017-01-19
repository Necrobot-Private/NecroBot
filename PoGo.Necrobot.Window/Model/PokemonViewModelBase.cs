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
        public string PokemonName => PokemonId.ToString();

        public string PokemonRarityColor => PokemonGradeHelper.GetGradeColor(PokemonId);

        public string PokemonIcon
        {
            get
            {
                if ((int)PokemonId > 151)
                {
                    //http://www.serebii.net/pokemongo/pokemon/145.png maybe better
                    return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons2/{(int)PokemonId:000}.png";
                }

                return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons/{(int)PokemonId:000}.png";
            }
        }

    }
}
