using PoGo.NecroBot.Logic.Event;
using POGOProtos.Enums;
using POGOProtos.Inventory;

namespace PoGo.Necrobot.Window.Model
{
    public class CatchPokemonViewModel : SidebarItemViewModel
    {
        public string PokemonName { get; set; }
        public double IV { get;  set; }
        public ulong EncounterId { get;  set; }
        public string CatchType { get; set; }
        public string CatchStatus { get; set; }

        public string Level { get; set; }

        public int CP { get; set; }

        public int PokeBalls { get; set; }

        public int GreatBalls { get; set; }
        public int UltraBalls { get; set; }
        public double Probability { get;  set; }
        public string Move1 { get;  set; }
        public string Move2 { get;  set; }
        public PokemonId PokemonId { get; set; }
        public Candy Candy { get; private set; }

        public int Exp { get; set; }

        public CatchPokemonViewModel(PokemonCaptureEvent ev)
        {
            this.UUID = ev.EncounterId.ToString();
            this.PokemonId = ev.Id;
            this.PokemonName = ev.Id.ToString();
            this.IV = ev.Perfection;
            this.CP = ev.Cp;
            this.Candy = ev.Candy;
            this.Probability = ev.Probability;
            this.EncounterId = ev.EncounterId;
            this.CatchType = ev.CatchType;
            this.Move1 = ev.Move1.ToString();
            this.Move2 = ev.Move2.ToString();
            this.CatchStatus = ev.Status.ToString();
            this.PokeBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemPokeBall? 1: 0;
            this.UltraBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemUltraBall ? 1 : 0;
            this.GreatBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemGreatBall ? 1 : 0;
            this.Exp = ev.Exp;
        }
    }
}
