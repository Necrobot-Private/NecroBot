using System;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchAccountManualException : Exception
    {
        public MultiAccountManager.BotAccount RequestedAccount;

        public ActiveSwitchAccountManualException(MultiAccountManager.BotAccount requestedAccount)
        {
            RequestedAccount = requestedAccount;
        }
    }
}
