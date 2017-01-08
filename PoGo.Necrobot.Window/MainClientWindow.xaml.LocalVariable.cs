using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window
{
    public partial class MainClientWindow
    {
        ISession currentSession;
        Model.DataContext datacontext;
        StatisticsAggregator playerStats;
        GetPlayerResponse playerProfile;
    }
}
