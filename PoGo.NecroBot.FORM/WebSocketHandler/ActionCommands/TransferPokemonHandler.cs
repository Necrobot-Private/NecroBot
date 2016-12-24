using System.Threading.Tasks;
using PoGo.NecroBot.LIB.State;
using PoGo.NecroBot.LIB.Tasks;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.ActionCommands
{
    public class TransferPokemonHandler : IWebSocketRequestHandler
    {
        public TransferPokemonHandler()
        {
            Command = "TransferPokemon";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await TransferSpecificPokemonTask.Execute(session, (ulong) message.PokemonId);
        }
    }
}