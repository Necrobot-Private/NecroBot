using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Window
{
    public partial class MainClientWindow
    {
        ISession currentSession;
        Model.DataContext datacontext;
        StatisticsAggregator playerStats;
    }
}
