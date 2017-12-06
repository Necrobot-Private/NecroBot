#region using directives

using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Service.WebSocketHandler.GetCommands.Events;
using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;

#endregion

namespace PoGo.NecroBot.Logic.Service.WebSocketHandler.GetCommands.Tasks
{
    internal class GetItemListTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            // using (var blocker = new BlockableScope(session, BotActions.ListItems))
            {
                //if (!await blocker.WaitToRun()) return;

                var allItems = await session.Inventory.GetItems();
                webSocketSession.Send(EncodingHelper.Serialize(new ItemListResponce(allItems, requestID)));
            }
        }
    }
}