using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using PoGo.NecroBot.Logic.Mini.Tasks;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.ActionCommands
{
    public class EvolvePokemonHandler : IWebSocketRequestHandler
    {
        public EvolvePokemonHandler()
        {
            Command = "EvolvePokemon";
        }

        public string Command { get; }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await EvolveSpecificPokemonTask.Execute(session, (ulong) message.PokemonId);
        }
    }
}