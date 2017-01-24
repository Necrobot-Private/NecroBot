using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Model
{
    public class ViewModelBase    : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ISession Session { get; set; }
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
