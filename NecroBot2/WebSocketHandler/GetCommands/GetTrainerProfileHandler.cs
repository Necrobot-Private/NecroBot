using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using NecroBot2.WebSocketHandler.GetCommands.Tasks;
using SuperSocket.WebSocket;

namespace NecroBot2.WebSocketHandler.GetCommands
{
    internal class GetTrainerProfileHandler : IWebSocketRequestHandler
    {
        public GetTrainerProfileHandler()
        {
            Command = "GetTrainerProfile";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await GetTrainerProfileTask.Execute(session, webSocketSession, (string) message.RequestID);
        }
    }
}