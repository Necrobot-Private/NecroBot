namespace PoGo.NecroBot.Window.Model
{
    public class UIViewModel : ViewModelBase
    {
        private string playerStatus;
        private string playerName;
        public string PlayerStatus
        {
            get { return playerStatus; }
            set
            {
                playerStatus = value;
                RaisePropertyChanged("PlayerStatus");
            }
        }
        public string PlayerName
        {
            get { return playerName; }
            set
            {
                playerName = value;
                RaisePropertyChanged("PlayerName");
            }
        }

    }
}
