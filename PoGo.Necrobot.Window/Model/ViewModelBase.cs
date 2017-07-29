using PoGo.NecroBot.Logic.State;
using System.ComponentModel;

namespace PoGo.NecroBot.Window.Model
{
    public class ViewModelBase    : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        public ISession Session { get; set; }
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
