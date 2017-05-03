using PoGo.NecroBot.Logic.Model;
using System;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchAccountManualException : Exception
    {
        public Account RequestedAccount;

        public ActiveSwitchAccountManualException(Account requestedAccount)
        {
            RequestedAccount = requestedAccount;
        }
    }
}
