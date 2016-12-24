using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using SuperSocket.WebSocket;

namespace NecroBot2.WebSocketHandler
{
    internal interface IWebSocketRequestHandler
    {
        string Command { get; }
        Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message);
    }
}