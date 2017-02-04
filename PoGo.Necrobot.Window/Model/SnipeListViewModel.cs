using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Model;
using POGOProtos.Networking.Responses;
using POGOProtos.Data;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.PoGoUtils;
using POGOProtos.Inventory;

namespace PoGo.Necrobot.Window.Model
{
   public class SnipeListViewModel : ViewModelBase
    {
        public ObservableCollection<SnipePokemonViewModel> IV100List { get; set; }
        public ObservableCollection<SnipePokemonViewModel> RareList { get;  set; }
        public ObservableCollection<SnipePokemonViewModel> OtherList { get; set; }
        public ObservableCollection<SnipePokemonViewModel> PokedexSnipeItems { get; set; }

        public AddManualSnipeCoordViewModel ManualSnipe { get; set; }

        public int TotalOtherList => this.OtherList.Count;

        public ObservableCollection<SnipePokemonViewModel> SnipeQueueItems { get;  set; }

        public SnipeListViewModel()
        {
            ManualSnipe = new AddManualSnipeCoordViewModel() { Latitude = 123 };
            this.RareList = new ObservableCollection<SnipePokemonViewModel>();
            this.OtherList = new ObservableCollection<SnipePokemonViewModel>();
            this.SnipeQueueItems = new ObservableCollection<SnipePokemonViewModel>();
            this.PokedexSnipeItems = new ObservableCollection<SnipePokemonViewModel>();
            this.IV100List = new ObservableCollection<Model.SnipePokemonViewModel>()
            {
                
            };
            #pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
            RefreshList();
            #pragma warning restore 4014
        }
        public async Task RefreshList()
        {
            while (true)
            {

                Refresh(this.PokedexSnipeItems);
                Refresh(this.IV100List);
                Refresh(this.SnipeQueueItems);

                Refresh(this.OtherList);

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
                best= bestPokemons.FirstOrDefault(x => x.PokemonId == model.PokemonId);

            if (best == null || PokemonInfo.CalculatePokemonPerfection(best) < model.IV)
            {
                model.Recommend = true;
            }
            if (model.IV>=100)
            Handle100IV(model);
            else
                if(grade == PokemonGrades.Legendary || 
                grade == PokemonGrades.VeryRare || 
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

            if(find != null)
            {
                this.SnipeQueueItems.Remove(find);
            }
        }

        private void HandlePokedex(SnipePokemonViewModel model)
        {
            if(pokedex != null && !pokedex.Exists(p=>p.PokemonId == model.PokemonId))
            {
                this.PokedexSnipeItems.Insert(0, model);
            }
            Refresh(this.PokedexSnipeItems);
        }

        //HOPE WPF HANDLE PERFOMANCE WELL
        public void Refresh(ObservableCollection<SnipePokemonViewModel> list)
        {
            var toremove = list.Where(x => x.RemainTimes < 0);

            foreach (var item in toremove)
            {
                
                list.Remove(item);
            }

            foreach (var item in list)
            {
                
                item.RaisePropertyChanged("RemainTimes");
            }
        }
        private void HandleOthers(SnipePokemonViewModel model)
        {
            this.OtherList.Insert(0,model);
            this.Refresh(this.OtherList);
            this.RaisePropertyChanged("TotalOtherList");
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
        }

        private void HandleRarePokemon(SnipePokemonViewModel model)
        {
            this.RareList.Insert(0,model);
            this.Refresh(this.RareList);
        }

        private void Handle100IV(SnipePokemonViewModel e)
        {
            this.IV100List.Insert(0,e);
            this.Refresh(this.IV100List);
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
            this.Refresh(this.SnipeQueueItems);
        }
    }
}
