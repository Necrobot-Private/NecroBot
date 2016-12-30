#region using directives

using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;

#endregion

namespace NecroBot2.WebSocketHandler.ActionCommands
{
    public class LevelUpPokemonHandler : IWebSocketRequestHandler
    {
        public string Command { get; private set;}

        public LevelUpPokemonHandler()
        {
            Command = "LevelUpPokemon";
        }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await Logic.Tasks.LevelUpSpecificPokemonTask.Execute(session, (ulong)message.PokemonId);
        }
    }
}