using PoGo.NecroBot.Logic.Common;

namespace PoGo.Necrobot.Window.Model
{
    public class AddAccountViewModel : ViewModelBase
    {
        public string AuthType { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int AccountStart { get; set; }

        public int AccountEnd { get; set; }

        public string UsernameTemplate { get; set; }

        public AddAccountViewModel()
        {
            UsernameTemplate = "username_xy{0}";
            AccountStart = 1;
            AccountEnd = 10;
            AuthType = "Ptc";
        }
    }
}