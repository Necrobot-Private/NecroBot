using MahApps.Metro.Controls;
using Microsoft.Win32;
using PoGo.NecroBot.Logic.Model.Settings;
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
using System.Windows.Shapes;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : MetroWindow
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new OpenFileDialog()
            {
                AddExtension = false,
                DefaultExt = "*.json",
            };
            if(openDlg.ShowDialog() == true)
            {
                string filename = openDlg.FileName;

                SettingsWindow settingWindow = new SettingsWindow(this, filename);
                this.Hide();
                settingWindow.Show();
            }
        }
    }
}
