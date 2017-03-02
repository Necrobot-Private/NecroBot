using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Model;
using POGOProtos.Data;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Inventory;
using PoGo.NecroBot.Logic.State;
using TinyIoC;

namespace PoGo.Necrobot.Window.Model
{
    public class SnipeListViewModel : ViewModelBase
    {
        public ObservableCollectionExt<SnipePokemonViewModel> IV100List { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> RareList { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> OtherList { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> PokedexSnipeItems { get; set; }

        public AddManualSnipeCoordViewModel ManualSnipe { get; set; }

        public int TotalOtherList => this.OtherList.Count;

        public ObservableCollectionExt<SnipePokemonViewModel> SnipeQueueItems { get; set; }

        public SnipeListViewModel()
        {
            ManualSnipe = new AddManualSnipeCoordViewModel() { Latitude = 123 };
            this.RareList = new ObservableCollectionExt<SnipePokemonViewModel>();
            this.OtherList = new ObservableCollectionExt<SnipePokemonViewModel>();
            this.SnipeQueueItems = new ObservableCollectionExt<SnipePokemonViewModel>();
            this.PokedexSnipeItems = new ObservableCollectionExt<SnipePokemonViewModel>();
            this.IV100List = new ObservableCollectionExt<Model.SnipePokemonViewModel>();

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
            //RefreshList();
#pragma warning restore 4014
        }
        public async Task RefreshList()
        {
            while (true)
            {
                Refresh("PokedexSnipeItems", this.PokedexSnipeItems);
                Refresh("IV100List", this.IV100List);
                Refresh("SnipeQueueItems", this.SnipeQueueItems);
                Refresh("OtherList", this.OtherList);
                await Task.Delay(3000);
            }
        }

        internal void OnSnipeData(EncounteredEvent e)
        {
            if (!e.IsRecievedFromSocket) return;
            var model = new SnipePokemonViewModel(e);
            var grade = PokemonGradeHelper.GetPokemonGrade(model.PokemonId);
            PokemonData best = null;

            if (bestPokemons != null)
                best = bestPokemons.FirstOrDefault(x => x.PokemonId == model.PokemonId);

            if (best == null || PokemonInfo.CalculatePokemonPerfection(best) < model.IV)
            {
                model.Recommend = true;
            }
            if (model.IV >= 100)
                Handle100IV(model);
            else
                if (grade == PokemonGrades.Legendary ||
                grade == PokemonGrades.VeryRare ||
                grade == PokemonGrades.Epic ||
                grade == PokemonGrades.Rare)
            {
                HandleRarePokemon(model);
            }
            else
            {
                HandleOthers(model);
            }

            HandlePokedex(model);
            //CHeck if pkm not in
        }

        public void OnPokemonSnipeStarted(MSniperServiceTask.MSniperInfo2 pokemon)
        {
            RemoveFromSnipeQueue(pokemon.UniqueIdentifier);
        }

        private void RemoveFromSnipeQueue(string uniqueIdentifier)
        {
            var find = this.SnipeQueueItems.FirstOrDefault(x => x.UniqueId == uniqueIdentifier);

            if (find != null)
            {
                this.SnipeQueueItems.Remove(find);
            }
        }

        private void HandlePokedex(SnipePokemonViewModel model)
        {
            this.PokedexSnipeItems.RemoveAll(x => ShouldRemove(x ,model));

            if (pokedex != null && !pokedex.Any(p => p.PokemonId == model.PokemonId))
            {
                this.PokedexSnipeItems.Insert(0, model);
            }
            Refresh("PokedexSnipeItems", this.PokedexSnipeItems);

        }

        //HOPE WPF HANDLE PERFOMANCE WELL
        public void Refresh(string propertyName, ObservableCollectionExt<SnipePokemonViewModel> list)
        {
            list.RemoveAll(x => x.RemainTimes < 0);

            foreach (var item in list)
            {
                item.RaisePropertyChanged("RemainTimes");
            }
            RaisePropertyChanged(propertyName);
        }
        private bool ShouldRemove(SnipePokemonViewModel x, SnipePokemonViewModel y)
        {
            //verified snipe data will
            if (x.EncounterId > 0 && x.EncounterId == y.EncounterId) return true;

            //unverified data
            if(x.EncounterId ==0 && 
                Math.Round(x.Latitude, 6) == Math.Round(y.Latitude, 6) &&
                Math.Round(x.Longitude, 6) == Math.Round(y.Longitude, 6) &&
                x.PokemonId == y.PokemonId) return true;

            if (x.EncounterId == 0 &&
                y.EncounterId > 0 &&
                Math.Round(x.Latitude, 6) == Math.Round(y.Latitude, 6) &&
                Math.Round(x.Longitude, 6) == Math.Round(y.Longitude, 6) &&
                x.PokemonId == y.PokemonId) return true;


            return false; 
        }
        private void HandleOthers(SnipePokemonViewModel model)
        {
            this.OtherList.RemoveAll(x => ShouldRemove(x, model));

            this.OtherList.Insert(0, model);

            this.RaisePropertyChanged("TotalOtherList");
            this.Refresh("OtherList", this.OtherList);

        }
        private List<PokedexEntry> pokedex;
        private List<PokemonData> bestPokemons;
        public void OnInventoryRefreshed(IEnumerable<InventoryItem> inventory)
        {
            var all = inventory.Select(x => x.InventoryItemData?.PokemonData).Where(x => x != null).ToList();
            pokedex = inventory.Select(x => x.InventoryItemData?.PokedexEntry).Where(x => x != null).ToList();
            bestPokemons = all.OrderByDescending(x => PokemonInfo.CalculatePokemonPerfection(x))
                             .GroupBy(x => x.PokemonId)
                             .Select(x => x.First())
                             .ToList();
                             
            // Remove pokedex items from pokemon snipe list.
            this.PokedexSnipeItems.RemoveAll(x => pokedex.Any(p => p.PokemonId == x.PokemonId));
            RaisePropertyChanged("PokedexSnipeItems");
        }

        private void HandleRarePokemon(SnipePokemonViewModel model)
        {
            this.RareList.RemoveAll(x => ShouldRemove(x, model));
            this.RareList.Insert(0, model);
            this.Refresh("RareList", this.RareList);

        }

        private void Handle100IV(SnipePokemonViewModel e)
        {
            this.IV100List.RemoveAll(x => ShouldRemove(x, e));
            this.IV100List.Insert(0, e);
            this.Refresh("IV100List", this.IV100List);
        }

        internal void OnSnipeItemQueue(EncounteredEvent encounteredEvent)
        {
            if (!encounteredEvent.IsRecievedFromSocket) return;

            var model = new SnipePokemonViewModel(encounteredEvent);
            model.AllowSnipe = false;
            HandleSnippingList(model);

        }

        private void HandleSnippingList(SnipePokemonViewModel model)
        {
            this.SnipeQueueItems.Insert(0, model);
            this.Refresh("SnipeQueueItems", this.SnipeQueueItems);
        }
    }
}
