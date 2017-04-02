using POGOProtos.Inventory;

namespace PoGo.Necrobot.Window.Model
{
    public class IncubatorViewModel : ViewModelBase
    {

        public IncubatorViewModel(EggIncubator incu)
        {

            Id = incu.Id;
            InUse = incu.PokemonId > 0;
            KM = incu.StartKmWalked;
            TotalKM = incu.TargetKmWalked;
            PokemonId = incu.PokemonId;
            UsesRemaining = incu.UsesRemaining;
            IsUnlimited = incu.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncubatorBasicUnlimited;

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
            InUse = incuModel.PokemonId > 0;
            PokemonId = incuModel.PokemonId;
            UsesRemaining = incuModel.UsesRemaining;
            KM = incuModel.KM;
            TotalKM = incuModel.TotalKM;

            RaisePropertyChanged("InUse");
            RaisePropertyChanged("PokemonId");
            RaisePropertyChanged("UsesRemaining");
            RaisePropertyChanged("TotalKM");
            RaisePropertyChanged("KM");

        }
    }
}
