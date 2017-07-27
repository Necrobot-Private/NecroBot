using System.Collections.Generic;
using System.Linq;

namespace PoGo.NecroBot.Window.Model
{
    public class SidebarViewModel   : ViewModelBase
    {
        public SidebarViewModel()
        {
            HistoryItems = new ObservableCollectionExt<SidebarItemViewModel>();
        }
        public ObservableCollectionExt<SidebarItemViewModel> HistoryItems
        {
            get;
            set;
        }
        public void AddItem(SidebarItemViewModel item)              
        {
            HistoryItems.Add(item);
        }

        public void AddOrUpdate(PokestopItemViewModel item)
        {
            var existingItem = HistoryItems.FirstOrDefault(x => x.UUID == item.UUID);

            if (existingItem == null)
            {
                HistoryItems.Insert(0,item);
            }
            Trim();
        }
        public void AddOrUpdate(CatchPokemonViewModel item)
        {
            var existingItem = HistoryItems.FirstOrDefault(x => x.UUID == item.UUID);

            if (existingItem == null)
            {
                HistoryItems.Insert(0, item);
                Trim();
            }
            else
            {
                var model = existingItem as CatchPokemonViewModel;

                model.CatchStatus = item.CatchStatus;
                model.GreatBalls += item.GreatBalls;
                
                model.PokeBalls += item.PokeBalls;
                model.UltraBalls += item.PokeBalls;
                model.RaisePropertyChanged("UltraBalls");
                model.RaisePropertyChanged("PokeBalls");
                model.RaisePropertyChanged("GreatBalls");
                model.RaisePropertyChanged("CatchStatus");
                //existingItem.CopyFrom(item);
                //update exising item and fire property change.....

            }
        }

        private void Trim()
        {
            if(HistoryItems.Count > 15)
            {
                HistoryItems.RemoveAt(HistoryItems.Count - 1);
            }
        }
    }
}
