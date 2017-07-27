using PoGo.NecroBot.Logic.State;
using POGOProtos.Map.Fort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class GetGymBadgeDetailsTask
    {
        public static async Task Execute(ISession session, FortData fort)
        {
            var response = await session.Client.Inventory.GetGymBadgeDetails(fort.Id, fort.Latitude, fort.Longitude).ConfigureAwait(false);
        }
    }
}
