using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Inventory;

namespace PoGo.Necrobot.Window.Model
{
    public class IncubatorViewModel : ViewModelBase
    {

        public IncubatorViewModel(EggIncubator incu)
        {

            this.Id = incu.Id;
            this.InUse = incu.PokemonId > 0;
            this.KM = incu.StartKmWalked;
            this.TotalKM = incu.TargetKmWalked;
            this.PokemonId = incu.PokemonId;
            this.UsesRemaining = incu.UsesRemaining;
            this.IsUnlimited = incu.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncubatorBasicUnlimited;

        }
        public string Icon =>  "https://github.com/Superviral/Pokemon-GO-App-Assets-and-Images/blob/master/App%20Converted%20Images/EggIncubator.png?raw=true";
        public double Availbility => InUse ? 0 : 1;

        public string Id { get;  set; }
        public bool InUse { get;  set; }
        public bool IsUnlimited { get;  set; }
        public ulong PokemonId { get;  set; }
        public int UsesRemaining { get;  set; }
        public double KM { get; set; }
        public double TotalKM { get;  set; }

        public void UpdateWith(IncubatorViewModel incuModel)
        {
            this.InUse = incuModel.PokemonId > 0;
            this.PokemonId = incuModel.PokemonId;
            this.UsesRemaining = incuModel.UsesRemaining;
            this.KM = incuModel.KM;
            this.TotalKM = incuModel.TotalKM;

            this.RaisePropertyChanged("InUse");
            this.RaisePropertyChanged("PokemonId");
            this.RaisePropertyChanged("UsesRemaining");
            this.RaisePropertyChanged("TotalKM");
            this.RaisePropertyChanged("KM");

        }
    }
}
