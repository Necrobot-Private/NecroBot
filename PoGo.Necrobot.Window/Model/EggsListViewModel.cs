using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Networking.Responses;
using POGOProtos.Data;
using POGOProtos.Inventory;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.Necrobot.Window.Model
{
    public class EggsListViewModel  :ViewModelBase
    {
        public ObservableCollection<EggViewModel> Eggs { get; set; }
        public ObservableCollection<IncubatorViewModel> Incubators { get; set; }
        public EggsListViewModel()
        {
            this.Eggs = new ObservableCollection<EggViewModel>();
            this.Incubators = new ObservableCollection<IncubatorViewModel>();
        }

        internal void OnInventoryRefreshed(GetInventoryResponse inventory)
        {
            var eggs = inventory.InventoryDelta.InventoryItems
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.IsEgg)
                .ToList();

            var incubators = inventory.InventoryDelta.InventoryItems
                    .Where(x => x.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => i.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);

            foreach (var incu in incubators)
            {
                AddOrUpdateIncubator(incu);
            }

            foreach (var egg in eggs)
            {
                var incu = this.Incubators.FirstOrDefault(x => x.PokemonId == egg.Id);

                AddOrUpdate(egg, incu);
            }

            
        }

        private void AddOrUpdateIncubator(EggIncubator incu)
        {
            var incuModel = new IncubatorViewModel(incu);
            var existing = this.Incubators.FirstOrDefault(x => x.Id == incu.Id);
            if (existing != null)
            {
                existing.UpdateWith(incuModel);
            }
            else
            {
                this.Incubators.Add(incuModel);
            }
        }

        public void AddOrUpdate(PokemonData egg, IncubatorViewModel incu = null)
        {
            var eggModel = new EggViewModel(egg);
            eggModel.Hatchable = incu == null;

            var existing = this.Eggs.FirstOrDefault(x => x.Id == eggModel.Id);
            if(existing != null )
            {
                existing.UpdateWith(eggModel);
            }
            else
            {
                this.Eggs.Add(eggModel);
            }
        }

        internal void OnEggIncubatorStatus(EggIncubatorStatusEvent e)
        {
            var egg = this.Eggs.FirstOrDefault(t => t.Id == e.PokemonId);
            var incu = this.Incubators.FirstOrDefault(t => t.Id == e.IncubatorId);

            egg.Hatchable = false;
            incu.InUse = true;
            egg.KM = e.KmWalked;

            egg.RaisePropertyChanged("KM");
            egg.RaisePropertyChanged("Hatchable");
            incu.RaisePropertyChanged("InUse");
        }
    }
}
