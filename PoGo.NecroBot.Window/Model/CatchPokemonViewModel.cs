using PoGo.NecroBot.Logic.Event;
using POGOProtos.Enums;
using POGOProtos.Inventory;

namespace PoGo.NecroBot.Window.Model
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
        public int MasterBalls { get; set; }
        public double Probability { get;  set; }
        public string Move1 { get;  set; }
        public string Move2 { get;  set; }
        public PokemonId PokemonId { get; set; }
        public Candy Candy { get; private set; }

        public int Exp { get; set; }

        public CatchPokemonViewModel(PokemonCaptureEvent ev)
        {
            UUID = ev.EncounterId.ToString();
            PokemonId = ev.Id;
            PokemonName = ev.Id.ToString();
            IV = ev.Perfection;
            CP = ev.Cp;
            Candy = ev.Candy;
            Probability = ev.Probability;
            EncounterId = ev.EncounterId;
            CatchType = ev.CatchType;
            Move1 = ev.Move1.ToString();
            Move2 = ev.Move2.ToString();
            CatchStatus = ev.Status.ToString();
            PokeBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemPokeBall? 1: 0;
            UltraBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemUltraBall ? 1 : 0;
            GreatBalls = ev.Pokeball == POGOProtos.Inventory.Item.ItemId.ItemGreatBall ? 1 : 0;
            Exp = ev.Exp;
        }
    }
}
