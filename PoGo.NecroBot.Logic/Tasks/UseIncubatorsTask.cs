#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Utils;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class UseIncubatorsTask
    {   // Still working on this-for NecroBot window console to update egg KM for egg with least KM remaining TheWizard1328
        public static double KmToWalk { get; set; }
        public static long Exp { get; set; }
        public static long Stardust { get; set; }
        public static object eggIncubatorStatusEvent { get; private set; }

        public static async Task Execute(ISession session, CancellationToken cancellationToken,
            ulong eggId, string incubatorId)
        {
            var incubators = (await session.Inventory.GetEggIncubators().ConfigureAwait(false))
                .Where(x => x.UsesRemaining > 0 || x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                .FirstOrDefault(x => x.Id == incubatorId);

            var unusedEggs = (await session.Inventory.GetEggs().ConfigureAwait(false))
                .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                .FirstOrDefault(x => x.Id == eggId);

            if (incubators == null || unusedEggs == null) return;

            var rememberedIncubatorsFilePath = Path.Combine(session.LogicSettings.ProfilePath, "temp", "incubators.json");
            var rememberedIncubators = GetRememberedIncubators(rememberedIncubatorsFilePath);
            var response = await session.Client.Inventory.UseItemEggIncubator(incubators.Id, unusedEggs.Id).ConfigureAwait(false);
            var newRememberedIncubators = new List<IncubatorUsage>();

            if (response.Result == UseItemEggIncubatorResponse.Types.Result.Success)
            {
                newRememberedIncubators.Add(new IncubatorUsage { IncubatorId = incubators.Id, PokemonId = unusedEggs.Id });

                session.EventDispatcher.Send(new EggIncubatorStatusEvent
                {
                    IncubatorId = incubators.Id,
                    WasAddedNow = true,
                    PokemonId = unusedEggs.Id,
                    KmToWalk = unusedEggs.EggKmWalkedTarget,
                    KmRemaining = response.EggIncubator.TargetKmWalked
                });

                if (!newRememberedIncubators.SequenceEqual(rememberedIncubators))
                    SaveRememberedIncubators(newRememberedIncubators, rememberedIncubatorsFilePath);
            }
            else
            {
                //error output
            }
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            try
            {
                var playerStats = (await session.Inventory.GetPlayerStats().ConfigureAwait(false)).FirstOrDefault();
                if (playerStats == null)
                    return;

                var kmWalked = playerStats.KmWalked;

                var incubators = (await session.Inventory.GetEggIncubators().ConfigureAwait(false))
                    .Where(x => x.UsesRemaining > 0 || x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                    .OrderByDescending(x => x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                    .ToList();

                var unusedEggs = (await session.Inventory.GetEggs().ConfigureAwait(false))
                    .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                    .OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart)
                    .ToList();

                var rememberedIncubatorsFilePath = Path.Combine(session.LogicSettings.ProfilePath, "temp", "incubators.json");
                var rememberedIncubators = GetRememberedIncubators(rememberedIncubatorsFilePath);
                var pokemons = (await session.Inventory.GetPokemons().ConfigureAwait(false)).ToList();
                var eggIncubatorStatusEvent = new EggIncubatorStatusEvent();

                // Check if eggs in remembered incubator usages have since hatched
                // (instead of calling session.Client.Inventory.GetHatchedEgg(), which doesn't seem to work properly)
                foreach (var incubator in rememberedIncubators)
                {
                    var hatched = pokemons.FirstOrDefault(x => !x.IsEgg && x.Id == incubator.PokemonId);
                    if (hatched == null) continue;

                    //Still Needs some work - TheWizard1328
                    var stats = session.RuntimeStatistics;           // Total Km walked
                    var KMs = eggIncubatorStatusEvent.KmToWalk; //playerStats.KmWalked - hatched.EggKmWalkedStart; // Total Km Walked(hatched.EggKmWalkedStart=0)
                    var stardust1 = session.Inventory.GetStarDust(); // Total trainer Stardust
                    var stardust2 = stats.TotalStardust;             // Total trainer Stardust
                    var ExpAwarded1 = playerStats.Experience;        // Total Player Exp - TheWizard1328
                    var ExpAwarded2 = stats.TotalExperience;         // Session Exp - TheWizard1328
                    var TotCandy = session.Inventory.GetCandyCount(hatched.PokemonId);
                    //Temp logger line personal testing info - TheWizard1328
                    Logger.Write($"Hatch: eISE.KmWalked: {eggIncubatorStatusEvent.KmWalked:0.00} | eISE.KmToWalk: {eggIncubatorStatusEvent.KmToWalk:0.00} | " +
                                 $"XP1: {ExpAwarded1} | XP2: {ExpAwarded2} | SD1: {stardust1} | SD2: {stardust2}", LogLevel.Egg, ConsoleColor.DarkYellow);

                    if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification)
                        await PushNotificationClient.SendNotification(session, $"Egg has hatched.", $"Pokemon: {hatched.PokemonId}\n" +
                                                                                                    $"Lvl: {PokemonInfo.GetLevel(hatched)}\n" +
                                                                                                    $"CP:  {hatched.Cp}\n" +
                                                                                                    $"IV:  {Math.Round(PokemonInfo.CalculatePokemonPerfection(hatched), 2)}\n", true).ConfigureAwait(false);

                    session.EventDispatcher.Send(new EggHatchedEvent
                    {
                        Dist = KMs, //Still working on this - TheWizard1328
                        Id = hatched.Id,
                        PokemonId = hatched.PokemonId,
                        Level = PokemonInfo.GetLevel(hatched),
                        Cp = hatched.Cp,
                        MaxCp = PokemonInfo.CalculateMaxCp(hatched.PokemonId),
                        Perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(hatched), 2),
                        HXP = ExpAwarded1,
                        HSD = stardust2, // This still needs work too to display the total SD after hatching - TheWizard1328
                        HCandy = await TotCandy,
                    });
                }

                var newRememberedIncubators = new List<IncubatorUsage>();

                foreach (var incubator in incubators)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                    if (incubator.PokemonId == 0)
                    {
                        // Unlimited incubators prefer short eggs, limited incubators prefer long eggs
                        // Special case: If only one incubator is available at all, it will prefer long eggs
                        var egg = (incubator.ItemId == ItemId.ItemIncubatorBasicUnlimited && incubators.Count > 1)
                            ? unusedEggs.FirstOrDefault()
                            : unusedEggs.LastOrDefault();

                        if (egg == null)
                            continue;

                        // Skip (save) limited incubators depending on user choice in config
                        if (!session.LogicSettings.UseLimitedEggIncubators
                            && incubator.ItemId != ItemId.ItemIncubatorBasicUnlimited)
                            continue;

                        var response = await session.Client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id).ConfigureAwait(false);
                        unusedEggs.Remove(egg);

                        newRememberedIncubators.Add(new IncubatorUsage { IncubatorId = incubator.Id, PokemonId = egg.Id });

                        session.EventDispatcher.Send(new EggIncubatorStatusEvent
                        {
                            IncubatorId = incubator.Id,
                            WasAddedNow = true,
                            PokemonId = egg.Id,
                            KmToWalk = egg.EggKmWalkedTarget,
                            KmRemaining = response.EggIncubator.TargetKmWalked - kmWalked
                        });
                    }
                    else
                    {
                        newRememberedIncubators.Add(new IncubatorUsage
                        {
                            IncubatorId = incubator.Id,
                            PokemonId = incubator.PokemonId
                        });

                        session.EventDispatcher.Send(new EggIncubatorStatusEvent
                        {
                            IncubatorId = incubator.Id,
                            PokemonId = incubator.PokemonId,
                            KmToWalk = incubator.TargetKmWalked - incubator.StartKmWalked,
                            KmRemaining = incubator.TargetKmWalked - kmWalked
                        });
                    }
                }

                if (!newRememberedIncubators.SequenceEqual(rememberedIncubators))
                    SaveRememberedIncubators(newRememberedIncubators, rememberedIncubatorsFilePath);
            }
            catch (Exception)
            {
            }
        }

        private static List<IncubatorUsage> GetRememberedIncubators(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<List<IncubatorUsage>>(File.ReadAllText(filePath, Encoding.UTF8));

            return new List<IncubatorUsage>(0);
        }

        private static void SaveRememberedIncubators(List<IncubatorUsage> incubators, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, JsonConvert.SerializeObject(incubators), Encoding.UTF8);
        }

        private class IncubatorUsage : IEquatable<IncubatorUsage>
        {
            public string IncubatorId;
            public ulong PokemonId;

            public bool Equals(IncubatorUsage other)
            {
                return other != null && other.IncubatorId == IncubatorId && other.PokemonId == PokemonId;
            }
        }
    }
}
