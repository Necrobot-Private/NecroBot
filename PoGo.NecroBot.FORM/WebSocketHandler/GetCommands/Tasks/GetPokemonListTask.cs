using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.LIB.State;
using PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Events;
using PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Helpers;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.FORM.WebSocketHandler.GetCommands.Tasks
{
    internal class GetPokemonListTask
    {
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            var allPokemonInBag = await session.Inventory.GetHighestsCp(1000);
            var list = new List<PokemonListWeb>();
            allPokemonInBag.ToList().ForEach(o => list.Add(new PokemonListWeb(o)));
            webSocketSession.Send(EncodingHelper.Serialize(new PokemonListResponce(list, requestID)));
        }
    }
}