using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.Necrobot.Window.Model
{
    public class SnipePokemonViewModel : ViewModelBase
    {

        public SnipePokemonViewModel(EncounteredEvent e)
        {
            this.UniqueId = e.EncounterId;
            //var move1 = PokemonMove.MoveUnset;
            //var move2 = PokemonMove.MoveUnset;
            //Enum.TryParse<PokemonMove>(e.Move1, true, out move1);
            //Enum.TryParse<PokemonMove>(e.Move2, true, out move2);
            ulong encounterid = 0;
            ulong.TryParse(e.EncounterId, out encounterid);
            this.Ref = e;
            this.AllowSnipe = true;
            PokemonId = e.PokemonId;
            this.IV = e.IV;
            this.Latitude = e.Latitude;
            this.Longitude = e.Longitude;
            this.Move1 = e.Move1;
            this.Move2 = e.Move2;
            this.Expired = DateTime.Now.AddMinutes(2);
            this.EncounterId = encounterid;
            this.Level = e.Level;
            this.SpawnPointId = e.SpawnPointId;
            this.Verified = (this.EncounterId>0 ? "Verified":"");
            
        }

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
        public int RemainTimes =>  (int)(this.Expired - DateTime.Now).TotalSeconds;
        public Object Ref { get; set; }
        public bool AllowSnipe { get;  set; }
        public string UniqueId { get;  set; }
        public bool Recommend { get;  set; }
    }
}
