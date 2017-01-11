 using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class SidebarViewModel   : ViewModelBase
    {
        public SidebarViewModel()
        {
            HistoryItems = new ObservableCollection<SidebarItemViewModel>();
        }
        public ObservableCollection<SidebarItemViewModel> HistoryItems
        {
            get;
            set;
        }
        public void AddItem(SidebarItemViewModel item)              
        {
                this.HistoryItems.Add(item);
        }

        public void AddOrUpdate(CatchPokemonViewModel item)
        {
            var existingItem = this.HistoryItems.FirstOrDefault(x => x.UUID == item.UUID);

            if (existingItem == null)
            {
                this.HistoryItems.Add(item);
            }
            else
            {
                var model = existingItem as CatchPokemonViewModel;

                model.CatchStatus = item.CatchStatus;
                model.GreateBalls += item.GreateBalls;
                
                model.PokeBalls += item.PokeBalls;
                model.UltraBalls += item.PokeBalls;
                model.GreateBalls += item.GreateBalls;
                model.RaisePropertyChanged("UltraBalls");
                model.RaisePropertyChanged("PokeBalls");
                model.RaisePropertyChanged("GreateBalls");
                model.RaisePropertyChanged("CatchStatus");
                //existingItem.CopyFrom(item);
                //update exising item and fire property change.....

            }
        }
    }
}
