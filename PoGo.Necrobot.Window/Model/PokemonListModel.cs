using POGOProtos.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using PoGo.NecroBot.Logic.Event.Inventory;

namespace PoGo.Necrobot.Window.Model
{
    public class PokemonListModel : ViewModelBase
    {
        public ObservableCollection<PokemonDataViewModel> Pokemons { get; set; }

        internal void Update(List<PokemonData> pokemons, List<Candy> candies, List<PokemonSettings> pokemonSettings)
        {
                //var families = Session.Inventory.GetPokemonFamilies().Result;
                //var settings = Session.Inventory.GetPokemonSettings().Result;

                foreach (var item in pokemons)
                {

                    var existing = Pokemons.FirstOrDefault(x => x.Id == item.Id);

                    if (existing != null)
                    {
                        existing.UpdateWith(item);
                        continue;
                    }
                    var setting = pokemonSettings.FirstOrDefault(x => x.PokemonId == item.PokemonId);

                    var family = candies.FirstOrDefault(x => x.FamilyId == setting.FamilyId);

                    Pokemons.Add(new PokemonDataViewModel(item, setting, family));
                }
          //  });
        }

        internal void OnUpgradeEnd(FinishUpgradeEvent e)
        {
            var id = e.PokemonId;
            var model = Get(id);
            model.AllowPowerup = e.AllowUpgrade;
            model.PowerupText = "Upgrade";
        }

        internal void OnUpgraded(UpgradePokemonEvent e)
        {
            var id = e.Pokemon.Id;
            var model = Get(id);
            model.PokemonData = e.Pokemon;
            model.CP = e.Cp;
            model.Candy = e.FamilyCandies;
            model.HP = e.Pokemon.Stamina;

        }
        public void OnFavorited(FavoriteEvent ev)
        {
            var result = ev.FavoritePokemonResponse.Result == POGOProtos.Networking.Responses.SetFavoritePokemonResponse.Types.Result.Success;
            var id = ev.Pokemon.Id;
            var model = Get(id);
            model.IsFavoriting = false;
            if (!result)
            {
                model.Favorited = !model.Favorited;
            }
        }

        private PokemonDataViewModel Get(ulong id)
        {
            return this.Pokemons.FirstOrDefault(x => x.Id == id);
        }
        internal void OnEvolved(PokemonEvolveEvent ev)
        {
            var exist = Get(ev.OriginalId);
            if(exist != null && ev.Result == POGOProtos.Networking.Responses.EvolvePokemonResponse.Types.Result.Success)
            {
                this.Pokemons.Remove(exist);
                var newItem = new PokemonDataViewModel(ev.EvolvedPokemon, ev.PokemonSetting, ev.Family);
                this.Pokemons.Add(newItem);

                foreach (var item in this.Pokemons)
                {
                    if (item.PokemonSettings != null && item.PokemonSettings.FamilyId == ev.Family?.FamilyId)
                    {
                        item.Candy = ev.Family.Candy_;
                    }
                }

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
            foreach (var item in this.Pokemons)
            {
                if(item.PokemonSettings != null && item.PokemonSettings.FamilyId == e.FamilyId)
                {
                    item.Candy = e.FamilyCandies;
                }
            }
        }
        internal bool Favorite(ulong pokemonId)
        {
            var pkm = Get(pokemonId);

            if (pkm != null)
            {
                pkm.IsFavoriting = true;
            }
            pkm.Favorited = !pkm.Favorited;

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
                pkm.AllowPowerup = false;
                pkm.PowerupText = "Upgrading...";
                pkm.RaisePropertyChanged("PowerupText");
                pkm.RaisePropertyChanged("AllowPowerup");
            }
        }
    }
}
