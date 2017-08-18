using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Window.Model
{
    public class EvolutionToPokemon : ViewModelBase
    {
        public int CandyNeed { get; set; }
        public ulong OriginPokemonId { get; set; }
        public PokemonId Pokemon { get; set; }
        public bool AllowEvolve { get; set; }
        public ItemId ItemNeed { get; set; }
    }
    public class PokemonDataViewModel : ViewModelBase
    {
        private PokemonSettings setting;
        public PokemonDataViewModel(ISession session, PokemonData pokemon)
        {
            Session = session;
            PokemonData = pokemon;
            Displayed = true;
            var pkmSettings = session.Inventory.GetPokemonSettings().Result;
            setting = pkmSettings.FirstOrDefault(x => x.PokemonId == pokemon.PokemonId);

            EvolutionBranchs = new List<EvolutionToPokemon>();

            //TODO - implement the candy count for enable evolution
            foreach (var item in setting.EvolutionBranch)
            {
                EvolutionBranchs.Add(new EvolutionToPokemon()
                {
                    CandyNeed = item.CandyCost,
                    ItemNeed = item.EvolutionItemRequirement,
                    Pokemon = item.Evolution,
                    AllowEvolve = session.Inventory.CanEvolvePokemon(pokemon).Result,
                    OriginPokemonId = pokemon.Id
                });

            }
        }

        public List<EvolutionToPokemon> EvolutionBranchs { get; set; }
        internal void UpdateWith(PokemonData item)
        {
            PokemonData = item;
        }
        public string Types
        {
            get
            {
                return setting.Type.ToString() + ((setting.Type2 != PokemonType.None) ? "," + setting.Type2.ToString() : "");
            }
        }
        public string Shiny => pokemonData.PokemonDisplay.Shiny ? "Yes" : "No";
        public string Form => pokemonData.PokemonDisplay.Form.ToString().Replace("Unown", "").Replace("Unset", "Normal");
        public string Costume => pokemonData.PokemonDisplay.Costume.ToString().Replace("Unset", "Regular");
        public string Sex => pokemonData.PokemonDisplay.Gender.ToString().Replace("Less", "Genderless");
        public ulong Id
        {
            get
            {
                return PokemonData.Id;
            }
        }

        public string PokemonName
        {
            get
            {
                return string.IsNullOrEmpty(PokemonData.Nickname) ? PokemonData.PokemonId.ToString() : PokemonData.Nickname;
            }

            set
            {
                if (PokemonData.Nickname != value)
                {
                    // Fire off the rename
                    Task.Run(async () =>
                    {
                        await RenameSinglePokemonTask.Execute(
                            Session,
                            PokemonData.Id,
                            value,
                            Session.CancellationTokenSource.Token);
                    });
                }
            }
        }

        public PokemonId PokemonId
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
                return Session.Inventory.GetPokemonSettings().Result.FirstOrDefault(x => x.PokemonId == PokemonId);
            }
        }

        public int Candy
        {
            get
            {
                return Session.Inventory.GetCandyCount(PokemonData.PokemonId).Result;
            }
        }

        public bool AllowPowerup
        {
            get
            {
                return Session.Inventory.CanUpgradePokemon(PokemonData).Result;
            }
        }

        public bool AllowEvolve
        {
            get
            {
                return Session.Inventory.CanEvolvePokemon(PokemonData).Result;
            }
        }

        public bool AllowTransfer
        {
            get
            {
                return Session.Inventory.CanTransferPokemon(PokemonData);
            }
        }

        public DateTime CaughtTime => TimeUtil.GetDateTimeFromMilliseconds((long)pokemonData.CreationTimeMs).ToLocalTime();

        private GeoLocation geoLocation;
        public GeoLocation GeoLocation
        {
            get
            {
                return geoLocation;
            }

            set
            {
                geoLocation = value;
                RaisePropertyChanged("CaughtLocation");
            }
        }

        public string CaughtLocation
        {
            get
            {
                if (geoLocation == null)
                {
                    // Just return latitude, longitude string
                    return new GeoLocation(pokemonData.CapturedCellId).ToString();
                }

                return geoLocation.ToString();
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

        private bool isUpgrading;
        public bool IsUpgrading
        {
            get { return isUpgrading; }
            set
            {
                isUpgrading = value;
                RaisePropertyChanged("IsUpgrading");
                RaisePropertyChanged("PowerupText");
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

        public bool Slashed => PokemonData.IsBad ? true : false;

        public string PokemonIcon
        {
            get
            {

                if (Slashed)
                    return $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/pokemon/slashed.png";
                else
                {
                    var additional = "";
                    additional = additional + ("-" + pokemonData.PokemonDisplay.Costume.ToString()).Replace("-Unset", "");
                    additional = additional + ("-" + pokemonData.PokemonDisplay.Form.ToString().Replace("Unown", "").Replace("-ExclamationPoint", "-ExclamationPoint").Replace("-QuestionMark", "-QuestionMark")).Replace("-Unset", "");
                    additional += pokemonData.PokemonDisplay.Shiny ? "-shiny" : "";
                    return $"https://cdn.rawgit.com/NecroBot-Private/PokemonGO-Assets/master/pokemon/{(int)PokemonData.PokemonId}{additional}.png";
                }
            }
        }

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

        public string PowerupText
        {
            get
            {
                if (IsUpgrading)
                    return "Upgrading...";
                else
                    return "Upgrade";
            }
        }

        public bool Displayed { get; set; }
    }
}
