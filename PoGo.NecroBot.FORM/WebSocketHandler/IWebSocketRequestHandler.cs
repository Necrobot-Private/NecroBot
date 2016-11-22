using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler
{
    internal interface IWebSocketRequestHandler
    {
        string Command { get; }
        Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message);
    }
}