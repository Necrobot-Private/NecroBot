using PoGo.NecroBot.Logic.Model;
using System;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchAccountManualException : Exception
    {
        public BotAccount RequestedAccount;

        public ActiveSwitchAccountManualException(BotAccount requestedAccount)
        {
            RequestedAccount = requestedAccount;
        }
    }
}
