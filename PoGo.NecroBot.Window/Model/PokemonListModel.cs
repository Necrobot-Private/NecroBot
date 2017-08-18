using POGOProtos.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Model;

namespace PoGo.NecroBot.Window.Model
{
    public class PokemonViewFilter : ViewModelBase
    {
        public PokemonViewFilter() : base()
        {
            MaxCP = 5000;
            MaxLevel = 50;
            MaxIV = 100;
        }

        public string Name { get; set; }
        public int MinIV { get; set; }

        public int MaxIV { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int MinCP { get; set; }
        public int MaxCP { get; set; }

        internal bool Check(PokemonDataViewModel item)
        {
            if(!string.IsNullOrEmpty(Name))
            {
                if (!Name.ToLower().Contains(item.PokemonId.ToString().ToLower())) return false;
            }

            if (item.IV < MinIV || item.IV > MaxIV) return false;
            if (item.Level < MinLevel || item.Level > MaxLevel) return false;
            if (item.CP < MinCP || item.CP > MaxCP) return false;
            return true;
        }
    }
    public class PokemonListViewModel : ViewModelBase
    {
        public PokemonListViewModel(ISession session)
        {
            Filter = new PokemonViewFilter();
            Session = Session;
        }
        
        public ObservableCollection<PokemonDataViewModel> Pokemons { get; set; }
        public PokemonViewFilter Filter { get; set; }
        internal void Update(IEnumerable<PokemonData> pokemons)
        {
            foreach (var item in pokemons)
            {
                var existing = Pokemons.FirstOrDefault(x => x.Id == item.Id);

                if (existing != null)
                {
                    existing.Displayed = Filter.Check(existing);
                    existing.UpdateWith(item);
                    
                }
                else
                {
                    var pokemonDataViewModel = new PokemonDataViewModel(Session, item);
                    pokemonDataViewModel.Displayed = Filter.Check(pokemonDataViewModel);

                    Pokemons.Add(pokemonDataViewModel);
                    Task.Run(async () =>
                    {
                        GeoLocation geoLocation = await GeoLocation.FindOrUpdateInDatabase(pokemonDataViewModel.PokemonData.CapturedCellId);
                        if (geoLocation != null)
                            pokemonDataViewModel.GeoLocation = geoLocation;
                    });
                }
            }

            // Remove missing pokemon
            List<PokemonDataViewModel> modelsToRemove = new List<PokemonDataViewModel>();
            foreach (var item in Pokemons)
            {
                var existing = pokemons.FirstOrDefault(x => x.Id == item.Id);
                if (existing == null)
                {
                    modelsToRemove.Add(item);
                }
            }

            foreach (var model in modelsToRemove)
            {
                Pokemons.Remove(model);
            }
        }

        internal void OnUpgradeEnd(FinishUpgradeEvent e)
        {
            var id = e.PokemonId;
            var model = Get(id);
            if (model == null)
                return;
            model.IsUpgrading = false;
            model.PokemonData = e.Pokemon;
        }

        internal void OnUpgraded(UpgradePokemonEvent e)
        {
            var id = e.Pokemon.Id;
            var model = Get(id);
            if (model == null)
                return;
            model.IsUpgrading = false;
            model.PokemonData = e.Pokemon;
        }

        public void OnFavorited(FavoriteEvent ev)
        {
            var result = ev.FavoritePokemonResponse.Result == POGOProtos.Networking.Responses.SetFavoritePokemonResponse.Types.Result.Success;
            var id = ev.Pokemon.Id;
            var model = Get(id);
            if (model == null)
                return;
            model.IsFavoriting = false;
            model.PokemonData = ev.Pokemon;
        }

        private PokemonDataViewModel Get(ulong id)
        {
            return Pokemons.FirstOrDefault(x => x.Id == id);
        }

        internal void OnEvolved(PokemonEvolveEvent ev)
        {
            var exist = Get(ev.OriginalId);
            if(ev.Cancelled && exist!=null)
            {
                exist.IsEvolving = false;
            } 
            if (ev.Result == POGOProtos.Networking.Responses.EvolvePokemonResponse.Types.Result.Success)
            {
                Candy candy = Session.Inventory.GetCandyFamily(ev.EvolvedPokemon.PokemonId).Result;
                if (candy != null)
                {
                    var familyId = candy.FamilyId;
                    foreach (var item in Pokemons.Where(p => p.FamilyId == familyId))
                    {
                        item.RaisePropertyChanged("Candy");
                    }
                }
            }
            else
            {
                exist.IsEvolving = false;
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
            var pkm = Pokemons.FirstOrDefault(x => x.Id == pokemonId && Session.Inventory.CanTransferPokemon(x.PokemonData));

            if (pkm != null)
            {
                pkm.IsTransfering = true;
            }
        }

        internal void Remove(ulong id)
        {
            var pkm = Pokemons.FirstOrDefault(x => x.Id == id);

            if (pkm != null)
                Pokemons.Remove(pkm);
        }

        internal void OnRename(RenamePokemonEvent e)
        {
            var pkm = Get(e.Id);
            pkm.PokemonData.Nickname = e.NewNickname;
            pkm.RaisePropertyChanged("PokemonName");
        }

        internal void OnTransfer(TransferPokemonEvent e)
        {
            Remove(e.Id);
            foreach (var item in Pokemons.Where(p => p.FamilyId == e.FamilyId))
            {
                item.RaisePropertyChanged("Candy");
            }
        }

        internal bool Favorite(ulong pokemonId)
        {
            var pkm = Get(pokemonId);

            if (pkm != null)
            {
                pkm.IsFavoriting = true;
            }

            return pkm.Favorited;
        }

        internal void Evolve(ulong pokemonId)
        {
            var pkm = Pokemons.FirstOrDefault(x => x.Id == pokemonId);

            if (pkm != null)
            {
                pkm.IsEvolving = true;
            }
        }


        public void Powerup(ulong pokemonId)
        {
            var pkm = Pokemons.FirstOrDefault(x => x.Id == pokemonId);

            if (pkm != null)
            {
                pkm.IsUpgrading = true;
            }
        }

        internal void ApplyFilter(bool select = false)
        {
            foreach (var item in Pokemons)
            {
                item.Displayed = Filter.Check(item);
                item.RaisePropertyChanged("Displayed");
                if(select && item.Displayed)
                {
                    item.IsSelected = true;
                    item.RaisePropertyChanged("IsSelected");
                }
            }
        }
    }
}
