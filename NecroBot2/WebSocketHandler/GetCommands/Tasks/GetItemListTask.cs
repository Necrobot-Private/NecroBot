using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using NecroBot2.WebSocketHandler.GetCommands.Events;
using SuperSocket.WebSocket;

namespace NecroBot2.WebSocketHandler.GetCommands.Tasks
{
    internal class GetItemListTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            var allItems = await session.Inventory.GetItems();
            webSocketSession.Send(EncodingHelper.Serialize(new ItemListResponce(allItems, requestID)));
        }
    }
}