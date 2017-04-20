#region using directives

using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Service.WebSocketHandler.GetCommands.Events;
using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;

#endregion

namespace PoGo.NecroBot.Logic.Service.WebSocketHandler.GetCommands.Tasks
{
    internal class GetPokemonSettingsTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            //using (var blocker = new BlockableScope(session, BotActions.PokemonSettings))
            {
                // if (!await blocker.WaitToRun()) return;

                var settings = await session.Inventory.GetPokemonSettings();
                webSocketSession.Send(EncodingHelper.Serialize(new WebResponce
                {
                    Command = "PokemonSettings",
                    Data = settings,
                    RequestID = requestID
                }));
            }
        }
    }
}