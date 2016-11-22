using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Tasks;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.GetCommands
{
    internal class GetEggListHandler : IWebSocketRequestHandler
    {
        public GetEggListHandler()
        {
            Command = "GetEggList";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await GetEggListTask.Execute(session, webSocketSession, (string) message.RequestID);
        }
    }
}