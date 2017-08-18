using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using POGOProtos.Data;
using POGOProtos.Inventory;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.Window.Model
{
    public class EggsListViewModel : ViewModelBase
    {
        public ObservableCollection<EggViewModel> Eggs { get; set; }
        public ObservableCollection<IncubatorViewModel> Incubators { get; set; }
        public EggsListViewModel()
        {
            Eggs = new ObservableCollection<EggViewModel>();
            Incubators = new ObservableCollection<IncubatorViewModel>();
        }

        public void OnInventoryRefreshed(IEnumerable<InventoryItem> inventory)
        {
            var eggs = inventory
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.IsEgg)
                .ToList();

            var incubators = inventory
                    .Where(x => x.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => i.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);

            foreach (var incu in incubators)
            {
                AddOrUpdateIncubator(incu);
            }

            foreach (var egg in eggs)
            {
                var incu = Incubators.FirstOrDefault(x => x.PokemonId == egg.Id);

                AddOrUpdate(egg, incu);
            }


        }

        private void AddOrUpdateIncubator(EggIncubator incu)
        {
            var incuModel = new IncubatorViewModel(incu);
            var existing = Incubators.FirstOrDefault(x => x.Id == incu.Id);
            if (existing != null)
            {
                existing.UpdateWith(incuModel);
            }
            else
            {
                Incubators.Add(incuModel);
            }
        }

        public void AddOrUpdate(PokemonData egg, IncubatorViewModel incu = null)
        {
            var eggModel = new EggViewModel(egg)
            {
                Hatchable = incu == null
            };
            var existing = Eggs.FirstOrDefault(x => x.Id == eggModel.Id);
            if (existing != null)
            {
                // Do not update, it overwrites OnEggIncubatorStatus Status updates
                // existing.UpdateWith(eggModel);
            }
            else
            {
                Eggs.Add(eggModel);
            }
        }

        internal void OnEggIncubatorStatus(EggIncubatorStatusEvent e)
        {
            var egg = Eggs.FirstOrDefault(t => t.Id == e.PokemonId);
            var incu = Incubators.FirstOrDefault(t => t.Id == e.IncubatorId);

            if (egg == null) return;

            egg.Hatchable = false;
            incu.InUse = true;
            egg.KM = e.KmToWalk - e.KmWalked; //Still in the works(TheWizard1328)

            egg.RaisePropertyChanged("KM");
            egg.RaisePropertyChanged("Hatchable");
            incu.RaisePropertyChanged("InUse");
        }
    }
}
