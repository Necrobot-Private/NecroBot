using PoGo.Necrobot.Window.Model;
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
