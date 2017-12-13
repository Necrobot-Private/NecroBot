using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseLuckyEggConstantlyTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            
            var currentAmountOfLuckyEggs = await session.Inventory.GetItemAmountByType(ItemId.ItemLuckyEgg).ConfigureAwait(false);
            if (currentAmountOfLuckyEggs == 0)
            {
                Logger.Write(session.Translation.GetTranslation(TranslationString.NoEggsAvailable));
                return;
            }
            else
            {
                Logger.Write(session.Translation.GetTranslation(TranslationString.UseLuckyEggAmount, currentAmountOfLuckyEggs));
            }

            await session.Inventory.UseLuckyEgg().ConfigureAwait(false);
        }
    }
}