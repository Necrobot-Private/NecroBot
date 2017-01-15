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

namespace PoGo.Necrobot.Window.Model
{
   public class SnipeListViewModel : ViewModelBase
    {
        public ObservableCollection<SnipePokemonViewModel> IV100List { get; set; }
        public ObservableCollection<SnipePokemonViewModel> RareList { get;  set; }
        public ObservableCollection<SnipePokemonViewModel> OtherList { get; set; }

        public int TotalOtherList => this.OtherList.Count;

        public SnipeListViewModel()
        {
            this.RareList = new ObservableCollection<SnipePokemonViewModel>();
            this.OtherList = new ObservableCollection<SnipePokemonViewModel>();

            this.IV100List = new ObservableCollection<Model.SnipePokemonViewModel>()
            {
                
            };
        }

        internal void OnSnipeData(EncounteredEvent e)
        {
            if (!e.IsRecievedFromSocket) return;
            var model = new SnipePokemonViewModel(e);
            var grade = PokemonGradeHelper.GetPokemonGrade(model.PokemonId);

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
             //CHeck if pkm not in
        }
        //HOPE WPF HANDLE PERFOMANCE WELL
        public void Refresh(ObservableCollection<SnipePokemonViewModel> list)
        {
            var toremove = list.Where(x => x.Expired < DateTime.Now);

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
    }
}
