#region using directives

using PoGo.NecroBot.Logic.Mini.State;
using PoGo.NecroBot.Logic.Mini.Tasks;

#endregion

namespace PoGo.NecroBot.Logic.Mini.Service
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