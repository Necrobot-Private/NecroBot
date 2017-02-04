using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchAccountManualException : Exception
    {
        public MultiAccountManager.BotAccount RequestedAccount;

        public ActiveSwitchAccountManualException(MultiAccountManager.BotAccount requestedAccount)
        {
            this.RequestedAccount = requestedAccount;
        }
    }
}
