using System.Threading.Tasks;
using PoGo.NecroBot.LIB.State;
using PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Tasks;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.GetCommands
{
    public class GetPokemonListHandler : IWebSocketRequestHandler
    {
        public GetPokemonListHandler()
        {
            Command = "GetPokemonList";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await GetPokemonListTask.Execute(session, webSocketSession, (string) message.RequestID);
        }
    }
}