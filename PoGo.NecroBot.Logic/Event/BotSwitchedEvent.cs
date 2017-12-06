using PoGo.NecroBot.Logic.Model;

namespace PoGo.NecroBot.Logic.Event
{
    public class BotSwitchedEvent : IEvent
    {
        private Account Account;

        public BotSwitchedEvent(Account nextBot)
        {
            Account = nextBot;
        }
    }
}