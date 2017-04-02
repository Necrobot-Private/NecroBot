using PokemonGo.RocketAPI.Enums;

namespace PoGo.NecroBot.Logic.Event.Player
{
    public class LoginEvent : IEvent
    {
        public AuthType AuthType { get; set; }
        public string Username { get; set; }

        public LoginEvent(AuthType authType, string v)
        {
            AuthType = authType;
            Username = v;
        }
    }
}