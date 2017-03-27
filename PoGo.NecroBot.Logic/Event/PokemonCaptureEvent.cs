#region using directives

using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using POGOProtos.Data;

#endregion

namespace PoGo.NecroBot.Logic.Event
{
    public class PokemonCaptureEvent : IEvent
    {
        public int Attempt;
        public int BallAmount;
        public string CatchType;
        public int Cp;
        public double Distance;
        public int Exp;
        public Candy Candy;
        public PokemonId Id;
        public ulong UniqueId;
        public double Level;
        public int MaxCp;
        public double Perfection;
        public ItemId Pokeball;
        public double Probability;
        public int Stardust;
        public CatchPokemonResponse.Types.CatchStatus Status;
        public CatchPokemonResponse.Types.CaptureReason CaptureReason;
        public double Latitude;
        public double Longitude;
        public string SpawnPointId;
        public ulong EncounterId;
        public PokemonMove Move1;
        public PokemonMove Move2;
        public long Expires;
        public string CatchTypeText;
        public string Rarity;
        public string Shiny => (pokemonData.PokemonDisplay.Shiny) ? "Yes" : "No";
        public string Gender { get; internal set; }
        private PokemonData pokemonData;
        public PokemonData PokemonData
        {
            get
            {
                return pokemonData;
            }

            set
            {
                PokemonData oldData = pokemonData;
                pokemonData = value;

                if (oldData != null)
                {
                    if (oldData.Id != pokemonData.Id)
                        RaisePropertyChanged("Id");

                    if (oldData.Nickname != pokemonData.Nickname)
                        RaisePropertyChanged("PokemonName");

                    if (oldData.Cp != pokemonData.Cp)
                    {
                        RaisePropertyChanged("CP");
                        RaisePropertyChanged("Level");
                    }

                    if (oldData.Stamina != pokemonData.Stamina)
                        RaisePropertyChanged("HP");

                    if (oldData.StaminaMax != pokemonData.StaminaMax)
                        RaisePropertyChanged("MaxHP");

                    if (oldData.Stamina != pokemonData.Stamina || oldData.StaminaMax != pokemonData.StaminaMax)
                        RaisePropertyChanged("HPDisplay");

                    RaisePropertyChanged("Candy");
                    RaisePropertyChanged("AllowPowerup");
                    RaisePropertyChanged("AllowEvolve");
                    RaisePropertyChanged("AllowTransfer");

                    // RaisePropertyChanged("IV");

                    if (oldData.Favorite != pokemonData.Favorite)
                        RaisePropertyChanged("Favorited");
                }
                else
                {
                    RaisePropertyChanged("Id");
                    RaisePropertyChanged("PokemonName");
                    RaisePropertyChanged("Candy");
                    RaisePropertyChanged("AllowPowerup");
                    RaisePropertyChanged("AllowEvolve");
                    RaisePropertyChanged("AllowTransfer");
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
        }        
    }
}
