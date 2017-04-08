using POGOProtos.Enums;
using System;
using PoGo.NecroBot.Logic.Event;
using TinyIoC;
using PoGo.NecroBot.Logic.State;

namespace PoGo.Necrobot.Window.Model
{
    public class SnipePokemonViewModel : ViewModelBase
    {

        public SnipePokemonViewModel(EncounteredEvent e)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            UniqueId = e.EncounterId;
            ulong.TryParse(e.EncounterId, out ulong encounterid);
            Ref = e;
            AllowSnipe = true;
            PokemonId = e.PokemonId;
            IV = e.IV;
            Latitude = e.Latitude;
            Longitude = e.Longitude;
            Move1 = e.Move1;
            Move2 = e.Move2;
            Expired = DateTime.Now.AddSeconds(session.LogicSettings.UIConfig.SnipeItemListDisplayTime);
            EncounterId = encounterid;
            Level = e.Level;
            SpawnPointId = e.SpawnPointId;
            Verified = (EncounterId > 0 && !SpawnPointId.Contains("-") ? "Verified":"");
            
        }
        public string PokemonName => PokemonId.ToString();

        public int Id => (int)PokemonId;

        public PokemonId PokemonId { get; set; }
        public DateTime Added { get; set; }
        public double IV { get; set; }
        public string Move1 { get; set; }
        public string Move2 { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ulong EncounterId { get;  set; }
        public string SpawnPointId { get;  set; }
        public string Verified { get;  set; }
        public int Level { get;  set; }
        public DateTime Expired { get;  set; }
        public int RemainTimes =>  (int)(Expired - DateTime.Now).TotalSeconds;
        public Object Ref { get; set; }
        public bool AllowSnipe { get;  set; }
        public string UniqueId { get;  set; }
        public bool Recommend { get;  set; }
    }
}
