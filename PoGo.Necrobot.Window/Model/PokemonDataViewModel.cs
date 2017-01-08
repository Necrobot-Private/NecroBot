using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Data;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonDataViewModel : ViewModelBase
    {
        public PokemonDataViewModel(PokemonData pokemon)
        {
            this.Id = pokemon.Id;
            this.PokemonName = pokemon.PokemonId;
            this.HP = pokemon.Stamina;
            this.MaxHP = pokemon.StaminaMax;
            this.IV = PokemonInfo.CalculatePokemonPerfection(pokemon);
            this.CP = PokemonInfo.CalculateCp(pokemon);
            this.Level = (int)PokemonInfo.GetLevel(pokemon);
            this.IsFavoried = pokemon.Favorite>0;
            this.IsSelected = false;
        }

        private bool isTransfering;
        public bool IsTransfering
        {
            get { return isTransfering; }
            set
            {
                isTransfering = value;
                RaisePropertyChanged("IsTransfering");

            }
        }

        public bool IsSelected { get; set; }
        public PokemonId PokemonName { get; set; }

        public int CP { get; set; }

        public int Level { get; set; }

        public int Candy { get; set; }

        public double IV { get; set; }

        public DateTime CaughtTime { get; set; }

        public ulong Id { get; set; }
        public int HP { get; private set; }
        public int MaxHP { get; private set; }

        public bool IsFavoried { get; set; }
        public string HPDisplay => $"{HP} ({Math.Round(((100.0* HP)/ MaxHP),2):P}";
    }
}
