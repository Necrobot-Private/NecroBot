using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Mini.State;
using NecroBot2.WebSocketHandler.GetCommands.Events;
using NecroBot2.WebSocketHandler.GetCommands.Helpers;
using SuperSocket.WebSocket;

namespace NecroBot2.WebSocketHandler.GetCommands.Tasks
{
    internal class GetTrainerProfileTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            var playerStats = (await session.Inventory.GetPlayerStats()).FirstOrDefault();
            if (playerStats == null)
                return;
            var tmpData = new TrainerProfileWeb(session.Profile.PlayerData, playerStats);
            webSocketSession.Send(EncodingHelper.Serialize(new TrainerProfileResponce(tmpData, requestID)));
        }
    }
}