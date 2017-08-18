#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RecycleItemsTask
    {
        private static int _diff;
        private static Random rnd = new Random();

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {

            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            var currentTotalItems = await session.Inventory.GetTotalItemCount().ConfigureAwait(false);
            if ((session.Profile.PlayerData.MaxItemStorage * session.LogicSettings.RecycleInventoryAtUsagePercentage / 100.0f) > currentTotalItems)
                return;

            var currentAmountOfPokeballs = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall).ConfigureAwait(false);
            var currentAmountOfGreatballs = await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall).ConfigureAwait(false);
            var currentAmountOfUltraballs = await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall).ConfigureAwait(false);
            var currentAmountOfMasterballs = await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall).ConfigureAwait(false);

            if (session.LogicSettings.DetailedCountsBeforeRecycling)
                Logger.Write(session.Translation.GetTranslation(TranslationString.CurrentPokeballInv,
                    currentAmountOfPokeballs.ToString("0").PadLeft(3, ' '), currentAmountOfGreatballs.ToString("0").PadLeft(3, ' '), currentAmountOfUltraballs.ToString("0").PadLeft(3, ' '),
                    currentAmountOfMasterballs.ToString("0").PadLeft(3, ' ')));

            var currentPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
            var currentSuperPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
            var currentHyperPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);
            var currentMaxPotions = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false);

            var currentAmountOfPotions = currentPotions + currentSuperPotions + currentHyperPotions + currentMaxPotions;

            if (session.LogicSettings.DetailedCountsBeforeRecycling)
                Logger.Write(session.Translation.GetTranslation(TranslationString.CurrentPotionInv,
                    currentPotions.ToString("0").PadLeft(3, ' '), currentSuperPotions.ToString("0").PadLeft(3, ' '), currentHyperPotions.ToString("0").PadLeft(3, ' '), currentMaxPotions.ToString("0").PadLeft(3, ' ')));

            var currentRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemRevive).ConfigureAwait(false);
            var currentMaxRevives = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive).ConfigureAwait(false);

            var currentAmountOfRevives = currentRevives + currentMaxRevives;

            var currentAmountOfEvoItems = await session.Inventory.GetItemAmountByType(ItemId.ItemUpGrade).ConfigureAwait(false) +
                                          await session.Inventory.GetItemAmountByType(ItemId.ItemDragonScale).ConfigureAwait(false) +
                                          await session.Inventory.GetItemAmountByType(ItemId.ItemKingsRock).ConfigureAwait(false) +
                                          await session.Inventory.GetItemAmountByType(ItemId.ItemMetalCoat).ConfigureAwait(false) +
                                          await session.Inventory.GetItemAmountByType(ItemId.ItemSunStone).ConfigureAwait(false);

            var currentAmountOfRAIDPasses = await session.Inventory.GetItemAmountByType(ItemId.ItemFreeRaidTicket).ConfigureAwait(false) +
                                            await session.Inventory.GetItemAmountByType(ItemId.ItemPaidRaidTicket).ConfigureAwait(false);

            if (session.LogicSettings.DetailedCountsBeforeRecycling)
                Logger.Write(session.Translation.GetTranslation(TranslationString.CurrentReviveInv,
                    currentRevives.ToString("0").PadLeft(3, ' '), currentMaxRevives.ToString("0").PadLeft(3, ' '), currentAmountOfEvoItems.ToString("0").PadLeft(3, ' '), currentAmountOfRAIDPasses.ToString("0").PadLeft(3, ' ')));

            var currentAmountOfBerries = await session.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenPinapBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenRazzBerry).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenNanabBerry).ConfigureAwait(false);
            var currentAmountOfIncense = await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseOrdinary).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseSpicy).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseCool).ConfigureAwait(false) +
                                         await session.Inventory.GetItemAmountByType(ItemId.ItemIncenseFloral).ConfigureAwait(false);
            var currentAmountOfLuckyEggs = await session.Inventory.GetItemAmountByType(ItemId.ItemLuckyEgg).ConfigureAwait(false);
            var currentAmountOfLures = await session.Inventory.GetItemAmountByType(ItemId.ItemTroyDisk).ConfigureAwait(false);

            if (session.LogicSettings.DetailedCountsBeforeRecycling)
                Logger.Write(session.Translation.GetTranslation(TranslationString.CurrentMiscItemInv,
                    currentAmountOfBerries.ToString("0").PadLeft(3,' '), currentAmountOfIncense.ToString("0").PadLeft(3, ' '), currentAmountOfLuckyEggs.ToString("0").PadLeft(3, ' '), currentAmountOfLures.ToString("0").PadLeft(3, ' ')));

            if (!session.LogicSettings.VerboseRecycling)
                Logger.Write(session.Translation.GetTranslation(TranslationString.RecyclingQuietly), LogLevel.Recycling);

            await OptimizedRecycleBalls(session, cancellationToken).ConfigureAwait(false);
            await OptimizedRecyclePotions(session, cancellationToken).ConfigureAwait(false);
            await OptimizedRecycleRevives(session, cancellationToken).ConfigureAwait(false);
            await OptimizedRecycleBerries(session, cancellationToken).ConfigureAwait(false);
            await OptimizedRecycleEvoItems(session, cancellationToken).ConfigureAwait(false);

            //await session.Inventory.RefreshCachedInventory().ConfigureAwait(false);
            currentTotalItems = await session.Inventory.GetTotalItemCount().ConfigureAwait(false);
            if ((session.Profile.PlayerData.MaxItemStorage * session.LogicSettings.RecycleInventoryAtUsagePercentage / 100.0f) > currentTotalItems)
                return;

            var items = await session.Inventory.GetItemsToRecycle(session).ConfigureAwait(false);

            foreach (var item in items)
            {
                if (item.Count <= 1 ||
                    (session.SaveBallForByPassCatchFlee &&
                        (item.ItemId == ItemId.ItemPokeBall ||
                        item.ItemId == ItemId.ItemGreatBall ||
                        item.ItemId == ItemId.ItemUltraBall))

                    ) continue;

                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                await session.Client.Inventory.RecycleItem(item.ItemId, item.Count).ConfigureAwait(false);
                await session.Inventory.UpdateInventoryItem(item.ItemId).ConfigureAwait(false);

                if (session.LogicSettings.VerboseRecycling)
                    session.EventDispatcher.Send(new ItemRecycledEvent { Id = item.ItemId, Count = item.Count });

                await DelayingUtils.DelayAsync(session.LogicSettings.RecycleActionDelay, 500, session.CancellationTokenSource.Token).ConfigureAwait(false);
            }
            //await session.Inventory.RefreshCachedInventory();
        }

        private static async Task RecycleItems(ISession session, CancellationToken cancellationToken, int itemCount, ItemId item, int maxItemToKeep = 1000)
        {
            int itemsToRecycle = 0;
            int itemsToKeep = itemCount - _diff;
            if (itemsToKeep < 0)
                itemsToKeep = 0;

            if (maxItemToKeep > 0)
            {
                itemsToKeep = Math.Min(itemsToKeep, maxItemToKeep);
            }
            itemsToRecycle = itemCount - itemsToKeep;
            if (itemsToRecycle > 0)
            {
                _diff -= itemsToRecycle;
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                await session.Client.Inventory.RecycleItem(item, itemsToRecycle).ConfigureAwait(false);
                await session.Inventory.UpdateInventoryItem(item).ConfigureAwait(false);
                if (session.LogicSettings.VerboseRecycling)
                    session.EventDispatcher.Send(new ItemRecycledEvent { Id = item, Count = itemsToRecycle });

                await DelayingUtils.DelayAsync(session.LogicSettings.RecycleActionDelay, 500, session.CancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        private static async Task OptimizedRecycleBalls(ISession session, CancellationToken cancellationToken)
        {
            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall).ConfigureAwait(false);
            var greatBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall).ConfigureAwait(false);
            var ultraBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall).ConfigureAwait(false);
            var masterBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall).ConfigureAwait(false);

            int totalBallsCount = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;

            if (session.SaveBallForByPassCatchFlee) return;

            int random = rnd.Next(-1 * session.LogicSettings.RandomRecycleValue, session.LogicSettings.RandomRecycleValue + 1);

            int totalPokeballsToKeep;

            if (session.LogicSettings.UseRecyclePercentsInsteadOfTotals)
            {
                totalPokeballsToKeep = (int)Math.Floor(session.LogicSettings.PercentOfInventoryPokeballsToKeep / 100.0 * session.Profile.PlayerData.MaxItemStorage);
            }
            else
            {
                totalPokeballsToKeep = session.LogicSettings.TotalAmountOfPokeballsToKeep;
            }

            if (totalBallsCount > totalPokeballsToKeep)
            {
                if (session.LogicSettings.RandomizeRecycle)
                {
                    _diff = totalBallsCount - totalPokeballsToKeep + random;
                }
                else
                {
                    _diff = totalBallsCount - totalPokeballsToKeep;
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, pokeBallsCount, ItemId.ItemPokeBall).ConfigureAwait(false);
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, greatBallsCount, ItemId.ItemGreatBall).ConfigureAwait(false);
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, ultraBallsCount, ItemId.ItemUltraBall).ConfigureAwait(false);
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, masterBallsCount, ItemId.ItemMasterBall).ConfigureAwait(false);
                }
            }
        }

        private static async Task OptimizedRecyclePotions(ISession session, CancellationToken cancellationToken)
        {
            var potionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion).ConfigureAwait(false);
            var superPotionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion).ConfigureAwait(false);
            var hyperPotionsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion).ConfigureAwait(false);
            var maxPotionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion).ConfigureAwait(false);

            int totalPotionsCount = potionCount + superPotionCount + hyperPotionsCount + maxPotionCount;
            int random = rnd.Next(-1 * session.LogicSettings.RandomRecycleValue, session.LogicSettings.RandomRecycleValue + 1);

            int totalPotionsToKeep;
            if (session.LogicSettings.UseRecyclePercentsInsteadOfTotals)
            {
                totalPotionsToKeep = (int)Math.Floor(session.LogicSettings.PercentOfInventoryPotionsToKeep / 100.0 * session.Profile.PlayerData.MaxItemStorage);
            }
            else
            {
                totalPotionsToKeep = session.LogicSettings.TotalAmountOfPotionsToKeep;
            }

            if (totalPotionsCount > totalPotionsToKeep)
            {
                if (session.LogicSettings.RandomizeRecycle)
                {
                    _diff = totalPotionsCount - totalPotionsToKeep + random;
                }
                else
                {
                    _diff = totalPotionsCount - totalPotionsToKeep;
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, potionCount, ItemId.ItemPotion).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, superPotionCount, ItemId.ItemSuperPotion).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, hyperPotionsCount, ItemId.ItemHyperPotion).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, maxPotionCount, ItemId.ItemMaxPotion).ConfigureAwait(false);
                }
            }
        }

        private static async Task OptimizedRecycleRevives(ISession session, CancellationToken cancellationToken)
        {
            var reviveCount = await session.Inventory.GetItemAmountByType(ItemId.ItemRevive).ConfigureAwait(false);
            var maxReviveCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive).ConfigureAwait(false);

            int totalRevivesCount = reviveCount + maxReviveCount;
            int random = rnd.Next(-1 * session.LogicSettings.RandomRecycleValue, session.LogicSettings.RandomRecycleValue + 1);

            int totalRevivesToKeep;
            if (session.LogicSettings.UseRecyclePercentsInsteadOfTotals)
            {
                totalRevivesToKeep = (int)Math.Floor(session.LogicSettings.PercentOfInventoryRevivesToKeep / 100.0 * session.Profile.PlayerData.MaxItemStorage);
            }
            else
            {
                totalRevivesToKeep = session.LogicSettings.TotalAmountOfRevivesToKeep;
            }

            if (totalRevivesCount > totalRevivesToKeep)
            {
                if (session.LogicSettings.RandomizeRecycle)
                {
                    _diff = totalRevivesCount - totalRevivesToKeep + random;
                }
                else
                {
                    _diff = totalRevivesCount - totalRevivesToKeep;
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, reviveCount, ItemId.ItemRevive).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, maxReviveCount, ItemId.ItemMaxRevive).ConfigureAwait(false);
                }
            }
        }

        private static async Task OptimizedRecycleBerries(ISession session, CancellationToken cancellationToken)
        {
            var razz = await session.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry).ConfigureAwait(false);
            var bluk = await session.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry).ConfigureAwait(false);
            var nanab = await session.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry).ConfigureAwait(false);
            var pinap = await session.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry).ConfigureAwait(false);
            var wepar = await session.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry).ConfigureAwait(false);
            var goldnana = await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenNanabBerry).ConfigureAwait(false);
            var goldpinap = await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenPinapBerry).ConfigureAwait(false);
            var goldrazz = await session.Inventory.GetItemAmountByType(ItemId.ItemGoldenRazzBerry).ConfigureAwait(false);

            int totalBerryCount = razz + bluk + nanab + pinap + wepar + goldnana + goldpinap + goldrazz;
            int random = rnd.Next(-1 * session.LogicSettings.RandomRecycleValue, session.LogicSettings.RandomRecycleValue + 1);

            int totalBerriesToKeep;
            if (session.LogicSettings.UseRecyclePercentsInsteadOfTotals)
            {
                totalBerriesToKeep = (int)Math.Floor(session.LogicSettings.PercentOfInventoryBerriesToKeep / 100.0 * session.Profile.PlayerData.MaxItemStorage);
            }
            else
            {
                totalBerriesToKeep = session.LogicSettings.TotalAmountOfBerriesToKeep;
            }

            if (totalBerryCount > totalBerriesToKeep)
            {
                if (session.LogicSettings.RandomizeRecycle)
                {
                    _diff = totalBerryCount - totalBerriesToKeep + random;
                }
                else
                {
                    _diff = totalBerryCount - totalBerriesToKeep;
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, razz, ItemId.ItemRazzBerry).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, bluk, ItemId.ItemBlukBerry).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, nanab, ItemId.ItemNanabBerry).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, pinap, ItemId.ItemPinapBerry).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, wepar, ItemId.ItemWeparBerry).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, goldnana, ItemId.ItemGoldenNanabBerry).ConfigureAwait(false);
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, goldpinap, ItemId.ItemGoldenPinapBerry).ConfigureAwait(false);
                }
                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, goldrazz, ItemId.ItemGoldenRazzBerry).ConfigureAwait(false);
                }
            }
        }

        private static async Task OptimizedRecycleEvoItems(ISession session, CancellationToken cancellationToken)
        {
            var upgrade = await session.Inventory.GetItemAmountByType(ItemId.ItemUpGrade).ConfigureAwait(false);
            var dragon = await session.Inventory.GetItemAmountByType(ItemId.ItemDragonScale).ConfigureAwait(false);
            var kings = await session.Inventory.GetItemAmountByType(ItemId.ItemKingsRock).ConfigureAwait(false);
            var metal = await session.Inventory.GetItemAmountByType(ItemId.ItemMetalCoat).ConfigureAwait(false);
            var sun = await session.Inventory.GetItemAmountByType(ItemId.ItemSunStone).ConfigureAwait(false);

            int totalEvolutionCount = upgrade + dragon + kings + metal + sun;
            int random = rnd.Next(-1 * session.LogicSettings.RandomRecycleValue, session.LogicSettings.RandomRecycleValue + 1);

            int totalEvolutionToKeep;
            if (session.LogicSettings.UseRecyclePercentsInsteadOfTotals)
            {
                totalEvolutionToKeep = (int)Math.Floor(session.LogicSettings.PercentOfInventoryEvolutionToKeep / 100.0 * session.Profile.PlayerData.MaxItemStorage);
            }
            else
            {
                totalEvolutionToKeep = session.LogicSettings.TotalAmountOfEvolutionToKeep;
            }

            if (totalEvolutionCount > totalEvolutionToKeep)
            {
                if (session.LogicSettings.RandomizeRecycle)
                {
                    _diff = totalEvolutionCount - totalEvolutionToKeep + random;
                }
                else
                {
                    _diff = totalEvolutionCount - totalEvolutionToKeep;
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, upgrade, ItemId.ItemUpGrade).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, dragon, ItemId.ItemDragonScale).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, kings, ItemId.ItemKingsRock).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, metal, ItemId.ItemMetalCoat).ConfigureAwait(false);
                }

                if (_diff > 0)
                {
                    await RecycleItems(session, cancellationToken, sun, ItemId.ItemSunStone).ConfigureAwait(false);
                }
            }
        }

        public static async Task DropItem(ISession session, ItemId item, int count)
        {
            using (var blocker = new BlockableScope(session, BotActions.RecycleItem))
            {
                if (!await blocker.WaitToRun().ConfigureAwait(false)) return;

                if (count > 0)
                {
                    await session.Client.Inventory.RecycleItem(item, count).ConfigureAwait(false);
                    await session.Inventory.UpdateInventoryItem(item).ConfigureAwait(false);

                    if (session.LogicSettings.VerboseRecycling)
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = item, Count = count });

                    await DelayingUtils.DelayAsync(session.LogicSettings.RecycleActionDelay, 500, session.CancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
