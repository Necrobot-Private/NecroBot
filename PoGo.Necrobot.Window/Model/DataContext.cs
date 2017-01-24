using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Data;
using System.Collections.ObjectModel;
using PoGo.NecroBot.Logic.Common;

namespace PoGo.Necrobot.Window.Model
{
    public class DataContext : ViewModelBase
    {
        public UIViewModel UI { get; set; }
        public PlayerInfoModel PlayerInfo { get; set; }
        public string AA { get; set; }
        public List<PokemonData> internalPokemons;

        public PokemonListModel PokemonList { get; set; }
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
                this.internalPokemons = value;
                RaisePropertyChanged("Pokemons");
                RaisePropertyChanged("PokemonTabHeader");
            }
        }

        public int MaxItemStogare { get; set; }
        public DataContext()
        {
            UI = new UIViewModel();
            Map = new MapViewModel();

            MaxItemStogare = 350;
            ItemsList = new ItemsListViewModel();
            Sidebar = new SidebarViewModel();
            internalPokemons = new List<PokemonData>();
            SnipeList = new SnipeListViewModel();
            EggsList = new EggsListViewModel();

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

        internal void Reset()
        {
            this.PokemonList.Pokemons.Clear();
            this.ItemsList.Items.Clear();
            this.ItemsList.RaisePropertyChanged("TotalItem");

        }

        public string ItemsTabHeader
        {
            get
            {
                return $"   ITEMS ({ ItemsList.Items.Sum(x=>x.ItemCount)} / {MaxItemStogare})   ";

            }
        }

    }
}
