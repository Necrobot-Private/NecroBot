using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.Event;
using PoGo.NecroBot.Logic.Mini.State;
using PoGo.NecroBot.Logic.Mini.Utils;

namespace PoGo.NecroBot.Logic.Mini.Tasks
{
    public class InventoryListTask
    {
        public static async Task Execute(ISession session)
        {
            // Refresh inventory so that the player stats are fresh
            await session.Inventory.RefreshCachedInventory();

            var inventory = await session.Inventory.GetItems();

            session.EventDispatcher.Send(
                new InventoryListEvent
                {
                    Items = inventory.ToList()
                });

            DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
        }
    }
}