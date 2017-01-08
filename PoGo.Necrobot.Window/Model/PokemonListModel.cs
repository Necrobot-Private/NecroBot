using POGOProtos.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonListModel : ViewModelBase
    {
        public ObservableCollection<PokemonDataViewModel> Pokemons { get; set; }

        internal void Update(List<PokemonData> pokemons)
        {
            foreach (var item in pokemons)
            {
                var existing = Pokemons.FirstOrDefault(x => x.Id == item.Id);

                if(existing != null)
                {
                    existing.IsTransfering = false;
                    continue;
                }

                Pokemons.Add(new PokemonDataViewModel(item));
            }
        }
        public void Transfer(List<ulong> pokemonIds)
        {

            foreach (var item in pokemonIds)
            {
                Transfer(item);
            }

        }
        public void Transfer(ulong pokemonId)
        {
            var pkm = Pokemons.FirstOrDefault(x => x.Id == pokemonId);

            if (pkm != null)
            {
                pkm.IsTransfering = true;
            }
        }

        internal void Remove(ulong id)
        {
            var pkm = this.Pokemons.FirstOrDefault(x => x.Id == id);

            if (pkm != null)
                this.Pokemons.Remove(pkm);
        }
    }
}
