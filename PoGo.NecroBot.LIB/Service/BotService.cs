#region using directives

using PoGo.NecroBot.LIB.State;
using PoGo.NecroBot.LIB.Tasks;

#endregion

namespace PoGo.NecroBot.LIB.Service
{
    public class BotService
    {
        public ILogin LoginTask;
        public ISession Session;

        public void Run()
        {
            LoginTask.DoLogin();
        }
    }
}