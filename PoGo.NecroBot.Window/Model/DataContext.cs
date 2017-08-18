using System.Collections.Generic;
using System.Linq;
using POGOProtos.Data;
using System.Collections.ObjectModel;

namespace PoGo.NecroBot.Window.Model
{
    public class DataContext : ViewModelBase
    {
        public UIViewModel UI { get; set; }
        public PlayerInfoModel PlayerInfo { get; set; }
        public List<PokemonData> internalPokemons;

        public PokemonListViewModel PokemonList { get; set; }
        public SidebarViewModel Sidebar { get; set; }

        public SnipeListViewModel SnipeList { get; set; }
        public ItemsListViewModel ItemsList { get; set; }
        public EggsListViewModel EggsList { get; set; }

        public MapViewModel Map { get; set; }
        public List<PokemonData> Pokemons
        {
            get
            {
                return internalPokemons;
            }
            set
            {
                internalPokemons = value;
                RaisePropertyChanged("Pokemons");
                RaisePropertyChanged("PokemonTabHeader");
            }
        }

        public int MaxItemStorage { get; set; }
        public int MaxPokemonStorage { get; set; }
        public int MaxEggStorage { get; set; }
        public DataContext()
        {
            UI = new UIViewModel();
            Map = new MapViewModel();

            MaxItemStorage = 0;
            MaxPokemonStorage = 0;
            MaxEggStorage = 0;
            ItemsList = new ItemsListViewModel();
            Sidebar = new SidebarViewModel();
            internalPokemons = new List<PokemonData>();
            SnipeList = new SnipeListViewModel();
            EggsList = new EggsListViewModel();

            PokemonList = new PokemonListViewModel(Session)
            {
                Pokemons = new ObservableCollection<PokemonDataViewModel>()
            };

        }
        public string PokemonTabHeader
        {
            get
            {
                var pokemonNum = PokemonList.Pokemons.Count() + EggsList.Eggs.Count();
                if (pokemonNum > MaxPokemonStorage)
                {
                    pokemonNum = MaxPokemonStorage;
                }
                return $"{pokemonNum}/{MaxPokemonStorage}";
            }
        }

        public string EggsTabHeader
        {
            get
            {
                var numIncubatorsInUse = EggsList.Incubators.Count(i => i.InUse);

                return $"{numIncubatorsInUse}/{EggsList.Eggs.Count()}";
            }
        }

        internal void Reset()
        {
            PokemonList.Pokemons.Clear();
            ItemsList.Items.Clear();
            EggsList.Eggs.Clear();
            EggsList.Incubators.Clear();

            ItemsList.RaisePropertyChanged("TotalItem");
        }

        public string ItemsTabHeader
        {
            get
            {
                return $"{ItemsList.Items.Sum(x=>x.ItemCount)}/{MaxItemStorage}";
            }
        }

    }
}
