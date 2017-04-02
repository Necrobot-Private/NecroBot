using PoGo.Necrobot.Window.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for PlayerInfo.xaml
    /// </summary>
    /// 

    public partial class PlayerInfo : UserControl
    {

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
            //this.DataContext = this.PlayerData;
            InitializeComponent();
        }
                           
    }
}
