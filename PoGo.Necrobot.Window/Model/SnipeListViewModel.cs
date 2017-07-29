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

namespace PoGo.NecroBot.Window.Model
{
    public class SnipeListViewModel : ViewModelBase
    {
        public ObservableCollectionExt<SnipePokemonViewModel> IV100List { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> RareList { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> OtherList { get; set; }
        public ObservableCollectionExt<SnipePokemonViewModel> PokedexSnipeItems { get; set; }

        public AddManualSnipeCoordViewModel ManualSnipe { get; set; }

        public int TotalOtherList => OtherList.Count;

        public ObservableCollectionExt<SnipePokemonViewModel> SnipeQueueItems { get; set; }

        public SnipeListViewModel()
        {
            ManualSnipe = new AddManualSnipeCoordViewModel() { Latitude = 123 };
            RareList = new ObservableCollectionExt<SnipePokemonViewModel>();
            OtherList = new ObservableCollectionExt<SnipePokemonViewModel>();
            SnipeQueueItems = new ObservableCollectionExt<SnipePokemonViewModel>();
            PokedexSnipeItems = new ObservableCollectionExt<SnipePokemonViewModel>();
            IV100List = new ObservableCollectionExt<Model.SnipePokemonViewModel>();

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
            //RefreshList();
#pragma warning restore 4014
        }
        public async Task RefreshList()
        {
            while (true)
            {
                Refresh("PokedexSnipeItems", PokedexSnipeItems);
                Refresh("IV100List", IV100List);
                Refresh("SnipeQueueItems", SnipeQueueItems);
                Refresh("OtherList", OtherList);
                await Task.Delay(3000);
            }
        }

        ObservableCollectionExt<EncounteredEvent> pending = new ObservableCollectionExt<EncounteredEvent>();

        private DateTime lastUpdateTime = DateTime.Now;

        internal void OnSnipeData(EncounteredEvent e)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            if (!e.IsRecievedFromSocket) return;
            lock (pending)
            {
                pending.Add(e);

                if (lastUpdateTime > DateTime.Now.AddSeconds(-session.LogicSettings.UIConfig.SnipeListRefreshInterval))
                {
                    return;
                }

                foreach (var item in pending)
                {
                    var model = new SnipePokemonViewModel(item);
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
                }
                pending.RemoveAll(x => true);
                lastUpdateTime = DateTime.Now;
            }
        }

        public void OnPokemonSnipeStarted(MSniperServiceTask.MSniperInfo2 pokemon)
        {
            RemoveFromSnipeQueue(pokemon.UniqueIdentifier);
        }

        private void RemoveFromSnipeQueue(string uniqueIdentifier)
        {
            var find = SnipeQueueItems.FirstOrDefault(x => x.UniqueId == uniqueIdentifier);

            if (find != null)
            {
                SnipeQueueItems.Remove(find);
            }
        }

        private void HandlePokedex(SnipePokemonViewModel model)
        {
            PokedexSnipeItems.RemoveAll(x => ShouldRemove(x, model));

            if (pokedex != null && !pokedex.Any(p => p.PokemonId == model.PokemonId))
            {
                PokedexSnipeItems.Insert(0, model);
            }
            Refresh("PokedexSnipeItems", PokedexSnipeItems);

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
            if (x.EncounterId == 0 &&
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
            OtherList.RemoveAll(x => ShouldRemove(x, model));

            OtherList.Insert(0, model);

            RaisePropertyChanged("TotalOtherList");
            Refresh("OtherList", OtherList);

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
            PokedexSnipeItems.RemoveAll(x => pokedex.Any(p => p.PokemonId == x.PokemonId));
            RaisePropertyChanged("PokedexSnipeItems");
        }

        private void HandleRarePokemon(SnipePokemonViewModel model)
        {
            RareList.RemoveAll(x => ShouldRemove(x, model));
            RareList.Insert(0, model);
            Refresh("RareList", RareList);

        }

        private void Handle100IV(SnipePokemonViewModel e)
        {
            IV100List.RemoveAll(x => ShouldRemove(x, e));
            IV100List.Insert(0, e);
            Refresh("IV100List", IV100List);
        }

        internal void OnSnipeItemQueue(EncounteredEvent encounteredEvent)
        {
            if (!encounteredEvent.IsRecievedFromSocket) return;

            var model = new SnipePokemonViewModel(encounteredEvent)
            {
                AllowSnipe = false
            };
            HandleSnippingList(model);

        }

        private void HandleSnippingList(SnipePokemonViewModel model)
        {
            SnipeQueueItems.Insert(0, model);
            Refresh("SnipeQueueItems", SnipeQueueItems);
        }
    }
}
