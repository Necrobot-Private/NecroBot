using System;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using POGOProtos.Map.Fort;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseItemCaptureTask
    {
        public static async Task Execute(ISession session, ItemId itemid, ulong encounterid, FortData fort)
        {
            string UsedTo = encounterid > 0 ? "Pokemon" : "Fort";
            Logger.Write($"Use {itemid.ToString()} as {UsedTo}", LogLevel.Recycling);
            await session.Client.Inventory.UseItemCapture(itemid, encounterid, fort.Id).ConfigureAwait(false);
        }
    }
}