namespace PoGo.NecroBot.Logic.Event
{
    public class BotSwitchedEvent : IEvent
    {
        private MultiAccountManager.BotAccount Account;

        public BotSwitchedEvent(MultiAccountManager.BotAccount nextBot)
        {
            this.Account = nextBot;
        }
    }
}