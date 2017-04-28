#region using directives

using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using SuperSocket.WebSocket;

#endregion

namespace PoGo.NecroBot.Logic.Service.WebSocketHandler.ActionCommands
{
    public class LevelUpPokemonHandler : IWebSocketRequestHandler
    {
        public string Command { get; private set; }

        public LevelUpPokemonHandler()
        {
            Command = "LevelUpPokemon";
        }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await LevelUpSpecificPokemonTask.Execute(session, (ulong) message.PokemonId);
        }
    }
}