#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Events;
using PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Helpers;
using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;
using PoGo.NecroBot.Logic.Model;

#endregion

namespace PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Tasks
{
    internal class GetPokemonListTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            using (var blocker = new BlockableScope(session, BotActions.ListItems))
            {
                if (!await blocker.WaitToRun()) return;

                var allPokemonInBag = await session.Inventory.GetHighestsCp(1000);
                var list = new List<PokemonListWeb>();
                allPokemonInBag.ToList().ForEach(o => list.Add(new PokemonListWeb(o)));
                webSocketSession.Send(EncodingHelper.Serialize(new PokemonListResponce(list, requestID)));
            }
        }
    }
}