using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using PoGo.NecroBot.Logic.State;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        public App()
        {
            //ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            
            base.OnStartup(e);
           
        }

       
        

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            
            base.OnLoadCompleted(e);
        }
    }
}
