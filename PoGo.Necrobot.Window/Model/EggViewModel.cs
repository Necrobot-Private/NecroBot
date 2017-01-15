using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Data;

namespace PoGo.Necrobot.Window.Model
{

    public class EggViewModel : ViewModelBase
    {
        Dictionary<double, string> icons = new Dictionary<double, string>()
        {
            {2.00, "https://hydra-media.cursecdn.com/pokemongo.gamepedia.com/2/26/Egg.png"             },
            {5.00,"https://hydra-media.cursecdn.com/pokemongo.gamepedia.com/6/67/Egg_5km.png" },
            {10.00 ,"https://hydra-media.cursecdn.com/pokemongo.gamepedia.com/5/5c/Egg_10km.png"
            }
        };

        private PokemonData egg;

        public ulong Id { get; set; }
        public double TotalKM { get; set; }
        public double KM { get; set; }

        public bool Hatchable { get; set; }
        public string Icon => icons[this.TotalKM];
        public EggViewModel(PokemonData egg)
        {
            this.Id = egg.Id;
            TotalKM = egg.EggKmWalkedTarget;
            KM = egg.EggKmWalkedStart;
            this.egg = egg;
        }
        public void UpdateWith(EggViewModel e)
        {
            this.KM = e.KM;
            this.RaisePropertyChanged("KM");
        }
    }
}
