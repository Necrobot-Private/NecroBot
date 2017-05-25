using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseLuckyEggTask
    {
        public static async Task Execute(ISession session)
        {
            await session.Inventory.UseLuckyEgg().ConfigureAwait(false);
        }
    }
}