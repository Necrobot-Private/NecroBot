using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Data;
using System.Collections.ObjectModel;

namespace PoGo.Necrobot.Window.Model
{
    public class DataContext : ViewModelBase
    {
        public PlayerInfoModel PlayerInfo { get; set; }
        public string AA { get; set; }
        public List<PokemonData> internalPokemons;

        public PokemonListModel PokemonList { get; set; }
        public List<PokemonData> Pokemons
        {
            get
            {
                return internalPokemons;
            } 
            set
            {
                this.internalPokemons = value;
                RaisePropertyChanged("Pokemons");
                RaisePropertyChanged("PokemonTabHeader");
            }
        }

        public DataContext()
        {
            internalPokemons = new List<PokemonData>();
            PokemonList = new PokemonListModel()
            {
                Pokemons = new ObservableCollection<PokemonDataViewModel>()
            };

        }
        public string PokemonTabHeader
        {
            get
            {
                return $"   POKEMONS ({PokemonList.Pokemons.Count} / 250)   ";
            }
        }


    }
}
