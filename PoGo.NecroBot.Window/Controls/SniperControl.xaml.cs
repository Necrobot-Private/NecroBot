using PoGo.NecroBot.Window.Model;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Snipe;
using PoGo.NecroBot.Logic.Service;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Enums;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PoGo.NecroBot.Window.Controls
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

        private void SnipeGrid_OnSnipePokemon(SnipePokemonViewModel selected, bool all)
        {
            var data = selected.Ref as EncounteredEvent;
            Task.Run(async () =>
            {
                if(all)
                {
                    Session.EventDispatcher.Send(new AllBotSnipeEvent(data.EncounterId));
                };

                var move1 = PokemonMove.MoveUnset;
                var move2 = PokemonMove.MoveUnset;
                Enum.TryParse(data.Move1, true, out move1);
                Enum.TryParse(data.Move2, true, out move2);
                ulong encounterid = 0;
                ulong.TryParse(data.EncounterId, out encounterid);
                bool caught = BotDataSocketClient.CheckIfPokemonBeenCaught(data.Latitude, data.Longitude, data.PokemonId, encounterid, Session);
                if (!caught)
                {
                    
                    await MSniperServiceTask.AddSnipeItem(Session, new MSniperServiceTask.MSniperInfo2()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cobPokemonId.ItemsSource = Enum.GetValues(typeof(PokemonId));
        }

        private void BtnAddCoord_Click(object sender, RoutedEventArgs e)
        {
            var model = (SnipeListViewModel)DataContext;
            var current = model.ManualSnipe;
            Task.Run(async () =>
            {
                await MSniperServiceTask.AddSnipeItem(Session, new MSniperServiceTask.MSniperInfo2()
                {
                    PokemonId = (short)current.PokemonId,
                    Latitude = current.Latitude,
                    Longitude = current.Longitude
                }, true);
                current.Clear();
                Dispatcher.Invoke(() =>
                {
                    rtbFreeText.Document.Blocks.Clear();
                });
            });
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var model = (SnipeListViewModel)DataContext;
            if (model == null) return;

            var current = model.ManualSnipe;

            string richText = new TextRange(rtbFreeText.Document.ContentStart, rtbFreeText.Document.ContentEnd).Text;

            current.Parse(richText);
        }
    }
}
