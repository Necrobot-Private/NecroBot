using POGOProtos.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.State;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonListModel : ViewModelBase
    {
        public PokemonListModel(ISession session)
        {
            this.Session = Session;
        }

        public ObservableCollection<PokemonDataViewModel> Pokemons { get; set; }

        internal void Update(IEnumerable<PokemonData> pokemons)
        {
            foreach (var item in pokemons)
            {
                var existing = Pokemons.FirstOrDefault(x => x.Id == item.Id);

                if (existing != null)
                {
                    existing.UpdateWith(item);
                }
                else
                {
                    Pokemons.Add(new PokemonDataViewModel(this.Session, item));
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
            
            foreach(var model in modelsToRemove)
            {
                Pokemons.Remove(model);
            }
        }

        internal void OnUpgradeEnd(FinishUpgradeEvent e)
        {
            var id = e.PokemonId;
            var model = Get(id);
            model.IsUpgrading = false;
            model.PokemonData = e.Pokemon;
        }

        internal void OnUpgraded(UpgradePokemonEvent e)
        {
            var id = e.Pokemon.Id;
            var model = Get(id);
            model.IsUpgrading = false;
            model.PokemonData = e.Pokemon;
        }

        public void OnFavorited(FavoriteEvent ev)
        {
            var result = ev.FavoritePokemonResponse.Result == POGOProtos.Networking.Responses.SetFavoritePokemonResponse.Types.Result.Success;
            var id = ev.Pokemon.Id;
            var model = Get(id);
            model.IsFavoriting = false;
            model.PokemonData = ev.Pokemon;
        }

        private PokemonDataViewModel Get(ulong id)
        {
            return this.Pokemons.FirstOrDefault(x => x.Id == id);
        }

        internal void OnEvolved(PokemonEvolveEvent ev)
        {
            var exist = Get(ev.OriginalId);
            if (ev.Result == POGOProtos.Networking.Responses.EvolvePokemonResponse.Types.Result.Success)
            {
                if (exist != null)
                    this.Pokemons.Remove(exist);

                var newItem = new PokemonDataViewModel(this.Session, ev.EvolvedPokemon);
                this.Pokemons.Add(newItem);

                foreach (var item in this.Pokemons.Where(p => p.FamilyId == newItem.FamilyId))
                {
                    item.RaisePropertyChanged("Candy");
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

        internal void OnTransfer(TransferPokemonEvent e)
        {
            this.Remove(e.Id);
            foreach (var item in this.Pokemons.Where(p => p.FamilyId == e.Candy.FamilyId))
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
    }
}
