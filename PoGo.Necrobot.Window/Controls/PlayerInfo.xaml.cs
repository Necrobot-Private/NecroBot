using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.State;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for PlayerInfo.xaml
    /// </summary>
    /// 

    public partial class PlayerInfo : UserControl
    {
        public ISession Session { get; set; }

        public String Label
        {
            get { return (String)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(PlayerInfo), new PropertyMetadata(""));

        public PlayerInfoModel PlayerData
        {
            get { return (PlayerInfoModel)GetValue(PlayerDataProperty); }
            set { SetValue(PlayerDataProperty, value); }
        }

        public static readonly DependencyProperty PlayerDataProperty =
           DependencyProperty.Register("PlayerData", typeof(PlayerInfoModel), typeof(PlayerInfo), new PropertyMetadata(null));

        public PlayerInfo()
        {
            InitializeComponent();
        }
        
        private async void GetPokeCoin_Click(object sender, RoutedEventArgs e)
        {
            var deployed = await Session.Inventory.GetDeployedPokemons();
            if (deployed.Count() > 0)
            {
                await Session.Client.Player.CollectDailyDefenderBonus();
            }
            else
            {
                return;
            }
        }
    }
}
