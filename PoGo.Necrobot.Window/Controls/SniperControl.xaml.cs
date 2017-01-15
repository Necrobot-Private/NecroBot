using PoGo.NecroBot.CLI;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for SniperControl.xaml
    /// </summary>
    public partial class SniperControl : UserControl
    {
        public ISession Session { get; set; }
        public SniperControl()
        {
            InitializeComponent();
        }

        private void SnipeGrid_OnSnipePokemon(Model.SnipePokemonViewModel selected)
        {
            var data = selected.Ref as EncounteredEvent;
            Task.Run(async () =>
            {
                var move1 = PokemonMove.MoveUnset;
                var move2 = PokemonMove.MoveUnset;
                Enum.TryParse<PokemonMove>(data.Move1, true, out move1);
                Enum.TryParse<PokemonMove>(data.Move2, true, out move2);
                ulong encounterid = 0;
                ulong.TryParse(data.EncounterId, out encounterid);
                bool caught = BotDataSocketClient.CheckIfPokemonBeenCaught(data.Latitude, data.Longitude, data.PokemonId, encounterid, Session);
                if (!caught)
                {
                    await MSniperServiceTask.AddSnipeItem(this.Session, new MSniperServiceTask.MSniperInfo2()
                    {
                        Latitude = data.Latitude,
                        Longitude = data.Longitude,
                        EncounterId = encounterid,
                        SpawnPointId = data.SpawnPointId,
                        PokemonId = (short)data.PokemonId,
                        Iv = data.IV,
                        Move1 = move1,
                        Move2 = move2,
                        ExpiredTime = data.ExpireTimestamp
                    }, true);
                }
            });
        }
    }
}
