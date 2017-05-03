using PoGo.NecroBot.Logic.Model;

namespace PoGo.NecroBot.Logic.Event
{
    public class BotSwitchedEvent : IEvent
    {
        private BotAccount Account;

        public BotSwitchedEvent(BotAccount nextBot)
        {
            Account = nextBot;
        }
    }
}