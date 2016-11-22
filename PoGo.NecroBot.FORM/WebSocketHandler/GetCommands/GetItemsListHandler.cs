using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Tasks;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.GetCommands
{
    internal class GetItemsListHandler : IWebSocketRequestHandler
    {
        public GetItemsListHandler()
        {
            Command = "GetItemsList";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await GetItemListTask.Execute(session, webSocketSession, (string) message.RequestID);
        }
    }
}