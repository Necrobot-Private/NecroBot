using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using System;
using System.Linq;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonDataViewModel : ViewModelBase
    {
        public PokemonDataViewModel(ISession session, PokemonData pokemon)
        {
            this.Session = session;
            this.PokemonData = pokemon;
            this.PowerupText = "Upgrade";
        }

        internal void UpdateWith(PokemonData item)
        {
            this.PokemonData = item;
            this.IsTransfering = false;
            this.IsEvolving = false;
            this.IsFavoriting = false;
        }

        public ulong Id
        {
            get
            {
                return PokemonData.Id;
            }
        }

        public PokemonId PokemonName
        {
            get
            {
                return PokemonData.PokemonId;
            }
        }

        public string Move1
        {
            get
            {
                return PokemonData.Move1.ToString().Replace("Fast", "");
            }
        }

        public string Move2
        {
            get
            {
                return PokemonData.Move2.ToString();
            }
        }

        public PokemonFamilyId FamilyId
        {
            get
            {
                return PokemonSettings.FamilyId;
            }
        }

        public PokemonSettings PokemonSettings
        {
            get
            {
                return this.Session.Inventory.GetPokemonSettings().Result.FirstOrDefault(x => x.PokemonId == PokemonName);
            }
        }

        public int Candy
        {
            get
            {
                return this.Session.Inventory.GetCandy(this.PokemonData.PokemonId);
            }
        }

        public bool AllowPowerup
        {
            get
            {
                return this.Session.Inventory.CanUpgradePokemon(this.PokemonData);
            }
        }

        public bool AllowEvolve
        {
            get
            {
                return this.Session.Inventory.CanEvolvePokemon(this.PokemonData).Result;
            }
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

        private bool isEvolving;
        public bool IsEvolving
        {
            get { return isEvolving; }
            set
            {
                isEvolving = value;
                RaisePropertyChanged("IsEvolving");
            }
        }

        private bool isFavoriting;
        public bool IsFavoriting
        {
            get { return isFavoriting; }
            set
            {
                isFavoriting = value;
                RaisePropertyChanged("IsFavoriting");
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        
        public double Level
        {
            get
            {
                return PokemonInfo.GetLevel(PokemonData);
            }
        }
        
        public int CP
        {
            get
            {
                return PokemonData.Cp;
            }
        }

        public double IV
        {
            get
            {
                return PokemonInfo.CalculatePokemonPerfection(PokemonData);
            }
        }

        public DateTime CaughtTime { get; set; }
        
        public int HP
        {
            get
            {
                return PokemonData.Stamina;
            }
        }
        
        public int MaxHP
        {
            get
            {
                return PokemonData.StaminaMax;
            }
        }

        public bool Favorited
        {
            get
            {
                return PokemonData.Favorite == 1;
            }
        }

        public string HPDisplay => $"{HP}/{MaxHP}";
        
        public string PokemonIcon
        {
            get
            {
                if ((int)PokemonData.PokemonId > 151)
                {

                    return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons2/{(int)PokemonData.PokemonId:000}.png";

                }

                return $"https://rankedboost.com/wp-content/plugins/ice/riot/poksimages/pokemons/{(int)PokemonData.PokemonId:000}.png";
            }
        }

        private PokemonData pokemonData;
        public PokemonData PokemonData {
            get
            {
                return pokemonData;
            }

            set
            {
                pokemonData = value;
                RaisePropertyChanged("Id");
                RaisePropertyChanged("PokemonName");
                RaisePropertyChanged("Candy");
                RaisePropertyChanged("AllowPowerup");
                RaisePropertyChanged("AllowEvolve");
                RaisePropertyChanged("Candy");
                RaisePropertyChanged("IV");
                RaisePropertyChanged("CP");
                RaisePropertyChanged("HP");
                RaisePropertyChanged("MaxHP");
                RaisePropertyChanged("HPDisplay");
                RaisePropertyChanged("Level");
                RaisePropertyChanged("Favorited");
                RaisePropertyChanged("Move1");
                RaisePropertyChanged("Move2");
                RaisePropertyChanged("PokemonIcon");

            }
        }
        public string PowerupText { get; internal set; }
    }
}
