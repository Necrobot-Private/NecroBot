using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows;

namespace PoGo.NecroBot.Window
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

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
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
                Hide();
                settingWindow.Show();
            }
        }
    }
}
