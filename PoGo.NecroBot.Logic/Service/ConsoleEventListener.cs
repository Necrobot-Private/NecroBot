#region using directives

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Gym;
using PoGo.NecroBot.Logic.Event.Player;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Event.Snipe;
using System.Linq;
using PoGo.NecroBot.Logic.Utils;
using PoGo.NecroBot.Logic.Model.Settings;
using System.IO;

#endregion

namespace PoGo.NecroBot.Logic.Service
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class ConsoleEventListener
    {
        public delegate void HumanWalkEventDelegate(HumanWalkingEvent e);
        public static event HumanWalkEventDelegate HumanWalkEvent;

        private static void HandleEvent(ProfileEvent profileEvent, ISession session)
        {
            Logger.Write(session.Translation.GetTranslation(TranslationString.EventProfileLogin,
                profileEvent.Profile.PlayerData.Username ?? ""));
        }

        private static void HandleEvent(ErrorEvent errorEvent, ISession session)
        {
            Logger.Write(errorEvent.ToString(), LogLevel.Error, force: true);
        }

        private static void HandleEvent(SnipePokemonUpdateEvent e, ISession session)
        {
            //move to resource later
            if (e.IsRemoteEvent)
                Logger.Write($"Expired snipe pokemon has been removed from queue : {e.Data.PokemonId} ");
        }

        private static void HandleEvent(NoticeEvent noticeEvent, ISession session)
        {
            Logger.Write(noticeEvent.ToString());
        }

        public static void HandleEvent(PokestopLimitUpdate ev, ISession session)
        {
            Logger.Write($"(POKESTOP LIMIT) {ev.Value}/{ev.Limit}", LogLevel.Info, ConsoleColor.Yellow);
        }

        public static void HandleEvent(CatchLimitUpdate ev, ISession session)
        {
            Logger.Write($"(CATCH LIMIT) {ev.Value}/{ev.Limit}", LogLevel.Info, ConsoleColor.Yellow);
        }

        private static void HandleEvent(TargetLocationEvent ev, ISession session)
        {
            //Logger.Write(session.Translation.GetTranslation(TranslationString.TargetLocationSet, ev.Latitude, ev.Longitude), LogLevel.Info);
        }

        private static void HandleEvent(BuddyUpdateEvent ev, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(
                    TranslationString.BuddyPokemonUpdate, ev.Pokemon.PokemonId.ToString()
                ),
                LogLevel.Info
            );
        }

        private static void HandleEvent(WarnEvent warnEvent, ISession session)
        {
            Logger.Write(warnEvent.ToString(), LogLevel.Warning);

            if (!warnEvent.RequireInput) return;
            Logger.Write(session.Translation.GetTranslation(TranslationString.RequireInputText), LogLevel.Warning);
        }

        private static void HandleEvent(UseLuckyEggEvent useLuckyEggEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(TranslationString.EventUsedLuckyEgg, useLuckyEggEvent.Count),
                LogLevel.Egg
            );
        }

        private static void HandleEvent(PokemonEvolveEvent pokemonEvolveEvent, ISession session)
        {
            string strPokemon = session.Translation.GetPokemonTranslation(pokemonEvolveEvent.Id);
            string logMessage = pokemonEvolveEvent.Result == EvolvePokemonResponse.Types.Result.Success
                ? session.Translation.GetTranslation(TranslationString.EventPokemonEvolvedSuccess,
                    strPokemon.PadRight(12, ' '),
                    session.Translation.GetPokemonTranslation(pokemonEvolveEvent.EvolvedPokemon.PokemonId).PadRight(12, ' '),
                    pokemonEvolveEvent.EvolvedPokemon.Cp.ToString("0").PadLeft(4, ' '),
                    pokemonEvolveEvent.EvolvedPokemon.Perfection().ToString("0.00").PadLeft(6, ' '),
                    pokemonEvolveEvent.Exp.ToString("0").PadLeft(4, ' '),
                    pokemonEvolveEvent.Candy.ToString("0").PadLeft(3, ' '),
                    pokemonEvolveEvent.EvolvedPokemon.Level().ToString("0.0").PadLeft(4, ' '))
                : session.Translation.GetTranslation(TranslationString.EventPokemonEvolvedFailed,
                    session.Translation.GetPokemonTranslation(pokemonEvolveEvent.Id).PadRight(12, ' '),
                    pokemonEvolveEvent.Result,
                    strPokemon);
            logMessage = (pokemonEvolveEvent.Sequence > 0 ? $"{pokemonEvolveEvent.Sequence}. " : "") + logMessage;
            Logger.Write(logMessage, LogLevel.Evolve);
        }

        private static void HandleEvent(TransferPokemonEvent transferPokemonEvent, ISession session)
        {
            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification && transferPokemonEvent.Slashed)
                PushNotificationClient.SendNotification(session, $"Transferred Slashed Pokemon", $"{session.Translation.GetPokemonTranslation(transferPokemonEvent.PokemonId)}\n" +
                                                                                                 $"Lvl: {transferPokemonEvent.Level}\n" +
                                                                                                 $"CP:  {transferPokemonEvent.Cp}/{transferPokemonEvent.BestCp}\n" +
                                                                                                 $"IV:  {transferPokemonEvent.Perfection.ToString("0.00")}\n", true).ConfigureAwait(false);
            Logger.Write(
                session.Translation.GetTranslation(TranslationString.EventPokemonTransferred,
                    session.Translation.GetPokemonTranslation(transferPokemonEvent.PokemonId).PadRight(12, ' '),
                    transferPokemonEvent.Cp.ToString("0").PadLeft(4, ' '),
                    transferPokemonEvent.Perfection.ToString("0.00").PadLeft(6, ' '),
                    transferPokemonEvent.BestCp.ToString("0").PadLeft(4, ' '),
                    transferPokemonEvent.BestPerfection.ToString("0.00").PadLeft(6, ' '),
                    transferPokemonEvent.Candy.ToString("0").PadLeft(4, ' '),
                    transferPokemonEvent.Level.ToString("0.0").PadLeft(4, ' '),
                    transferPokemonEvent.Slashed.ToString().PadLeft(5, ' ')
                ),
                LogLevel.Transfer
            );
        }

        private static void HandleEvent(UpgradePokemonEvent upgradePokemonEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(TranslationString.EventPokemonUpgraded,
                    session.Translation.GetPokemonTranslation(upgradePokemonEvent.PokemonId).PadRight(12, ' '),
                    upgradePokemonEvent.Lvl.ToString("0.0").PadLeft(4, ' '),
                    upgradePokemonEvent.Cp.ToString("0").PadLeft(4, ' '),
                    upgradePokemonEvent.Perfection.ToString("0.00").PadLeft(6, ' '),
                    upgradePokemonEvent.BestCp.ToString("0").PadLeft(4, ' '),
                    upgradePokemonEvent.BestPerfection.ToString("0.00").PadLeft(6, ' '),
                    upgradePokemonEvent.USD.ToString("0").PadLeft(5, ' '),
                    upgradePokemonEvent.Candy.ToString("0").PadLeft(4, ' ')),
                LogLevel.LevelUp);
        }

        private static void HandleEvent(RenamePokemonEvent renamePokemonEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(
                    TranslationString.PokemonRename,
                    session.Translation.GetPokemonTranslation(renamePokemonEvent.PokemonId).PadRight(13), 
                    renamePokemonEvent.OldNickname.PadRight(13),
                    renamePokemonEvent.NewNickname
                ),
                LogLevel.Info);
        }

        private static void HandleEvent(ItemRecycledEvent itemRecycledEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(
                    TranslationString.EventItemRecycled, itemRecycledEvent.Count.ToString("0").PadLeft(3), itemRecycledEvent.Id
                ),
                LogLevel.Recycling
            );
        }

        private static void HandleEvent(EggIncubatorStatusEvent eggIncubatorStatusEvent, ISession session)
        {
            Logger.Write(eggIncubatorStatusEvent.WasAddedNow
                    ? session.Translation.GetTranslation(TranslationString.IncubatorPuttingEgg,
                        eggIncubatorStatusEvent.KmToWalk.ToString("0.00").PadLeft(5))
                    : session.Translation.GetTranslation(TranslationString.IncubatorStatusUpdate,
                        eggIncubatorStatusEvent.KmRemaining.ToString("0.00").PadLeft(5),
                        eggIncubatorStatusEvent.KmToWalk.ToString("0.00").PadLeft(5)),
                LogLevel.Egg);
        }

        private static void HandleEvent(EggHatchedEvent eggHatchedEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(
                    TranslationString.IncubatorEggHatched,
                    eggHatchedEvent.Dist.ToString("0.00").PadLeft(5),
                    session.Translation.GetPokemonTranslation(eggHatchedEvent.PokemonId).PadRight(13),
                    eggHatchedEvent.Level.ToString("0.0").PadLeft(4),
                    eggHatchedEvent.Cp.ToString("0").PadLeft(4),
                    eggHatchedEvent.MaxCp.ToString("0").PadLeft(4),
                    eggHatchedEvent.Perfection.ToString("0.00").PadLeft(6),
                    eggHatchedEvent.HXP.ToString("0").PadLeft(4),
                    eggHatchedEvent.HSD.ToString("0").PadLeft(4),
                    eggHatchedEvent.HCandy.ToString("0").PadLeft(4)
                ),
                LogLevel.Egg);
        }

        public static GlobalSettings _settings;
        public static Session _session;

        private static void HandleEvent(FortUsedEvent fortUsedEvent, ISession session)
        {
            if (fortUsedEvent.InventoryFull)
            {
                Logger.Write(session.Translation.GetTranslation(TranslationString.InvFullPokestopLooting), fortUsedEvent.Fort.Type == FortType.Checkpoint ? LogLevel.Pokestop : LogLevel.Gym, ConsoleColor.Cyan);  //LogLevel.Pokestop);
                return;
            }

            string PokemonDataEgg = "No";

            if (fortUsedEvent.PokemonDataEgg != null && fortUsedEvent.PokemonDataEgg.IsEgg)
            {
                PokemonDataEgg = $"Yes {fortUsedEvent.PokemonDataEgg.EggKmWalkedTarget:0.0} Km";
            }

            string eventMessage = session.Translation.GetTranslation(TranslationString.EventFortUsed, fortUsedEvent.Name,
                    fortUsedEvent.Exp, fortUsedEvent.Gems,
                     fortUsedEvent.Items, fortUsedEvent.Badges, fortUsedEvent.BonusLoot, fortUsedEvent.RaidTickets, fortUsedEvent.TeamBonusLoot, PokemonDataEgg, session.Inventory.GetEggs().Result.Count(), fortUsedEvent.Latitude, fortUsedEvent.Longitude, fortUsedEvent.Altitude);

            if (fortUsedEvent.Fort.Type == FortType.Checkpoint)
                Logger.Write(eventMessage, LogLevel.Pokestop);
            else
                Logger.Write(eventMessage, LogLevel.GymDisk, ConsoleColor.Cyan); //LogLevel.Pokestop);

            //_session.Client.Player.SetCoordinates(fortUsedEvent.Latitude, fortUsedEvent.Longitude, fortUsedEvent.Altitude);

            //_settings.LocationConfig.DefaultLatitude = fortUsedEvent.Latitude;
            //_settings.LocationConfig.DefaultLongitude = fortUsedEvent.Longitude;

            //_session.Client.Settings.DefaultLatitude = fortUsedEvent.Latitude;
            //_session.Client.Settings.DefaultLongitude = fortUsedEvent.Longitude;

            //_settings.Save(Path.Combine(_settings.ProfileConfigPath, "config.json"));
        }

        private static void HandleEvent(FortFailedEvent fortFailedEvent, ISession session)
        {
            if (fortFailedEvent.Try != 1 && fortFailedEvent.Looted == false)
            {
                Logger.LineSelect(); // Replaces the last line to prevent spam.
            }

            if (fortFailedEvent.Looted)
            {
                Logger.Write(
                    session.Translation.GetTranslation(TranslationString.SoftBanBypassed),
                    LogLevel.SoftBan, ConsoleColor.Green);
            }
            else
            {
                Logger.Write(
                    session.Translation.GetTranslation(TranslationString.EventFortFailed, fortFailedEvent.Name,
                        fortFailedEvent.Try, fortFailedEvent.Max),
                    LogLevel.SoftBan);
            }
        }

        private static void HandleEvent(FortTargetEvent fortTargetEvent, ISession session)
        {
            int intTimeForArrival = (int) (fortTargetEvent.Distance /
                                           (session.LogicSettings.WalkingSpeedInKilometerPerHour * 0.5));

            string targetType = "";
            if (fortTargetEvent.Type == FortType.Gym)
                targetType = session.Translation.GetTranslation(TranslationString.Gym); // "Gym";
            else if (fortTargetEvent.Type == FortType.Checkpoint)
            {
                if (fortTargetEvent.Name != "User selected")
                    targetType = session.Translation.GetTranslation(TranslationString.Pokestop); // "Pokestop";
                else
                    targetType = "POI";
            }

            if (fortTargetEvent.Distance > 15)
                Logger.Write(
                    session.Translation.GetTranslation(TranslationString.EventFortTargeted, Math.Round(fortTargetEvent.Distance).ToString("0").PadLeft(3, ' '),
                        intTimeForArrival.ToString("0").PadLeft(3,' '), fortTargetEvent.Route,
                        targetType, fortTargetEvent.Name),
                    LogLevel.Info, ConsoleColor.Gray);
        }

        private static void HandleEvent(PokemonCaptureEvent pokemonCaptureEvent, ISession session)
        {
            Func<ItemId, string> returnRealBallName = a =>
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (a)
                {
                    case ItemId.ItemPokeBall:
                        return session.Translation.GetTranslation(TranslationString.Pokeball);
                    case ItemId.ItemGreatBall:
                        return session.Translation.GetTranslation(TranslationString.GreatPokeball);
                    case ItemId.ItemUltraBall:
                        return session.Translation.GetTranslation(TranslationString.UltraPokeball);
                    case ItemId.ItemMasterBall:
                        return session.Translation.GetTranslation(TranslationString.MasterPokeball);
                    default:
                        return session.Translation.GetTranslation(TranslationString.CommonWordUnknown);
                }
            };

            var catchType = pokemonCaptureEvent.CatchType;

            string strStatus;
            switch (pokemonCaptureEvent.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusError);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusEscape);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusFlee);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusMissed);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusSuccess);
                    break;
                default:
                    strStatus = pokemonCaptureEvent.Status.ToString();
                    break;
            }

            var catchStatus = pokemonCaptureEvent.Attempt > 1
                ? session.Translation.GetTranslation(TranslationString.CatchStatusAttempt, strStatus,
                    pokemonCaptureEvent.Attempt)
                : session.Translation.GetTranslation(TranslationString.CatchStatus, strStatus);

            var familyCandies = pokemonCaptureEvent.Candy?.Candy_ > 0
                ? session.Translation.GetTranslation(TranslationString.Candies, pokemonCaptureEvent.Candy.Candy_)
                : "";

            string message;

            if (pokemonCaptureEvent.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
            {
                if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification && pokemonCaptureEvent.Shiny == "Yes")
                    PushNotificationClient.SendNotification(session, $"Shiny Pokemon Captured", $"{session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id)}\n" +
                                                                                                $"Lvl: {pokemonCaptureEvent.Level}\n" +
                                                                                                $"CP:  {pokemonCaptureEvent.Cp}/{pokemonCaptureEvent.MaxCp}\n" +
                                                                                                $"IV:  {pokemonCaptureEvent.Perfection.ToString("0.00")}\n" +
                                                                                                $"Lat: {pokemonCaptureEvent.Latitude.ToString("0.000000")}\n" +
                                                                                                $"Lon: {pokemonCaptureEvent.Longitude.ToString("0.000000")}", true).ConfigureAwait(false);

                if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification && pokemonCaptureEvent.Perfection >= session.LogicSettings.FavoriteMinIvPercentage)
                    PushNotificationClient.SendNotification(session, $"High IV Pokemon Captured", $"{session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id)}\n" +
                                                                                                  $"Lvl: {pokemonCaptureEvent.Level}\n" +
                                                                                                  $"CP:  {pokemonCaptureEvent.Cp}/{pokemonCaptureEvent.MaxCp}\n" +
                                                                                                  $"IV:  {pokemonCaptureEvent.Perfection.ToString("0.00")}\n" +
                                                                                                  $"Lat: {pokemonCaptureEvent.Latitude.ToString("0.000000")}\n" +
                                                                                                  $"Lon: {pokemonCaptureEvent.Longitude.ToString("0.000000")}", true).ConfigureAwait(false);

                message = session.Translation.GetTranslation(TranslationString.EventPokemonCaptureSuccess, catchStatus,
                    catchType, 
                    session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id).PadRight(12, ' '),
                    pokemonCaptureEvent.Level.ToString("0.0").PadLeft(4, ' '), 
                    pokemonCaptureEvent.Cp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.MaxCp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.Perfection.ToString("0.00").PadLeft(6, ' '), 
                    pokemonCaptureEvent.Probability.ToString("0.00").PadLeft(6, ' '),
                    pokemonCaptureEvent.Distance.ToString("F2"),
                    returnRealBallName(pokemonCaptureEvent.Pokeball).PadRight(10, ' '), 
                    pokemonCaptureEvent.BallAmount.ToString("0").PadLeft(3, ' '),
                    pokemonCaptureEvent.Exp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.Stardust.ToString("0").PadLeft(4, ' '),
                    familyCandies, 
                    pokemonCaptureEvent.Latitude.ToString("0.000000"),
                    pokemonCaptureEvent.Longitude.ToString("0.000000"),
                    pokemonCaptureEvent.Move1.ToString().PadRight(16, ' '), 
                    pokemonCaptureEvent.Move2.ToString().PadRight(16, ' '),
                    pokemonCaptureEvent.Rarity.ToString().PadRight(10, ' '),
                    pokemonCaptureEvent.CaptureReason,
                    pokemonCaptureEvent.Shiny.ToString().PadRight(3, ' '),
                    pokemonCaptureEvent.Form,
                    pokemonCaptureEvent.Costume,
                    pokemonCaptureEvent.Gender
                );
                Logger.Write(message, LogLevel.Caught);
            }
            else
            {
                if (pokemonCaptureEvent.Status == CatchPokemonResponse.Types.CatchStatus.CatchFlee)
                {
                    if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification && pokemonCaptureEvent.Shiny == "Yes")
                        PushNotificationClient.SendNotification(session, $"Shiny Pokemon Ran Away", $"{session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id)}\n" +
                                                                                                    $"Lvl: {pokemonCaptureEvent.Level}\n" +
                                                                                                    $"CP:  {pokemonCaptureEvent.Cp}/{pokemonCaptureEvent.MaxCp}\n" +
                                                                                                    $"IV:  {pokemonCaptureEvent.Perfection.ToString("0.00")}\n" +
                                                                                                    $"Lat: {pokemonCaptureEvent.Latitude.ToString("0.000000")}\n" +
                                                                                                    $"Lon: {pokemonCaptureEvent.Longitude.ToString("0.000000")}", true).ConfigureAwait(false);

                    if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification && pokemonCaptureEvent.Perfection >= session.LogicSettings.FavoriteMinIvPercentage)
                        PushNotificationClient.SendNotification(session, $"High IV Pokemon Ran Away", $"{session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id)}\n" +
                                                                                                      $"Lvl: {pokemonCaptureEvent.Level}\n" +
                                                                                                      $"CP:  {pokemonCaptureEvent.Cp}/{pokemonCaptureEvent.MaxCp}\n" +
                                                                                                      $"IV:  {pokemonCaptureEvent.Perfection.ToString("0.00")}\n" +
                                                                                                      $"Lat: {pokemonCaptureEvent.Latitude.ToString("0.000000")}\n" +
                                                                                                      $"Lon: {pokemonCaptureEvent.Longitude.ToString("0.000000")}", true).ConfigureAwait(false);
                }
                message = session.Translation.GetTranslation(TranslationString.EventPokemonCaptureFailed, catchStatus,
                    catchType,
                    session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id).PadRight(12, ' '),
                    pokemonCaptureEvent.Level.ToString("0.0").PadLeft(4, ' '),
                    pokemonCaptureEvent.Cp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.MaxCp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.Perfection.ToString("0.00").PadLeft(6, ' '),
                    pokemonCaptureEvent.Probability.ToString("0.00").PadLeft(6, ' '),
                    pokemonCaptureEvent.Distance.ToString("F2"),
                    returnRealBallName(pokemonCaptureEvent.Pokeball).PadRight(10, ' '),
                    pokemonCaptureEvent.BallAmount.ToString("0").PadLeft(3, ' '),
                    pokemonCaptureEvent.Exp.ToString("0").PadLeft(4, ' '),
                    pokemonCaptureEvent.Latitude.ToString("0.000000"),
                    pokemonCaptureEvent.Longitude.ToString("0.000000"),
                    pokemonCaptureEvent.Move1.ToString().PadRight(15, ' '),
                    pokemonCaptureEvent.Move2.ToString().PadRight(15, ' '),
                    pokemonCaptureEvent.Rarity.ToString().PadRight(10, ' ')
                );
                Logger.Write(message, LogLevel.Flee);
            }
        }

        private static void HandleEvent(NoPokeballEvent noPokeballEvent, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(TranslationString.EventNoPokeballs,
                    noPokeballEvent.Id, noPokeballEvent.Cp),
                LogLevel.Caught);
        }

        private static void HandleEvent(UseBerryEvent useBerryEvent, ISession session)
        {
            string strBerry;
            switch (useBerryEvent.BerryType)
            {
                case ItemId.ItemRazzBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemRazzBerry);
                    break;
                case ItemId.ItemNanabBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemNanabBerry);
                    break;
                case ItemId.ItemPinapBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemPinapBerry);
                    break;
                case ItemId.ItemWeparBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemWeparBerry);
                    break;
                case ItemId.ItemBlukBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemBlukBerry);
                    break;
                default:
                    strBerry = useBerryEvent.BerryType.ToString();
                    break;
            }

            Logger.Write(
                session.Translation.GetTranslation(TranslationString.EventUseBerry, strBerry, useBerryEvent.Count),
                LogLevel.Berry
            );
        }

        private static void HandleEvent(SnipeEvent snipeEvent, ISession session)
        {
            Logger.Write(snipeEvent.ToString(), LogLevel.Sniper);
        }

        private static void HandleEvent(SnipeScanEvent snipeScanEvent, ISession session)
        {
            Logger.Write(snipeScanEvent.PokemonId == PokemonId.Missingno
                ? ((snipeScanEvent.Source != null) ? "(" + snipeScanEvent.Source + ") " : null) +
                  session.Translation.GetTranslation(TranslationString.SnipeScan,
                      $"{snipeScanEvent.Bounds.Latitude},{snipeScanEvent.Bounds.Longitude}")
                : ((snipeScanEvent.Source != null) ? "(" + snipeScanEvent.Source + ") " : null) +
                  session.Translation.GetTranslation(TranslationString.SnipeScanEx,
                      session.Translation.GetPokemonTranslation(snipeScanEvent.PokemonId),
                      snipeScanEvent.Iv > 0
                          ? snipeScanEvent.Iv.ToString(CultureInfo.InvariantCulture)
                          : session.Translation.GetTranslation(TranslationString.CommonWordUnknown),
                      $"{snipeScanEvent.Bounds.Latitude},{snipeScanEvent.Bounds.Longitude}"), LogLevel.Sniper);
        }

        private static void HandleEvent(DisplayHighestsPokemonEvent displayHighestsPokemonEvent, ISession session)
        {
            if (session.LogicSettings.AmountOfPokemonToDisplayOnStart <= 0)
            {
                return;
            }

            string strHeader;
            //PokemonData | CP | IV | Level | MOVE1 | MOVE2 | Candy
            switch (displayHighestsPokemonEvent.SortedBy)
            {
                case "Level":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsLevelHeader);
                    break;
                case "IV":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsPerfectHeader);
                    break;
                case "CP":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsCpHeader);
                    break;
                case "MOVE1":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestMove1Header);
                    break;
                case "MOVE2":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestMove2Header);
                    break;
                case "Candy":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestCandy);
                    break;
                default:
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsHeader);
                    break;
            }
            var strPerfect = session.Translation.GetTranslation(TranslationString.CommonWordPerfect);
            var strName = session.Translation.GetTranslation(TranslationString.CommonWordName).ToUpper();
            var move1 = session.Translation.GetTranslation(TranslationString.DisplayHighestMove1Header);
            var move2 = session.Translation.GetTranslation(TranslationString.DisplayHighestMove2Header);
            var candy = session.Translation.GetTranslation(TranslationString.DisplayHighestCandy);

            Logger.Write(
                session.Translation.GetTranslation(TranslationString.HighestsPokemoHeader, strHeader),
                LogLevel.Info,
                ConsoleColor.Yellow
            );
            foreach (var pokemon in displayHighestsPokemonEvent.PokemonList)
            {
                string strMove1 = session.Translation.GetPokemonMovesetTranslation(pokemon.Item5);
                string strMove2 = session.Translation.GetPokemonMovesetTranslation(pokemon.Item6);

                Logger.Write(
                    session.Translation.GetTranslation(
                        TranslationString.HighestsPokemoCell,
                        pokemon.Item1.Cp.ToString().PadLeft(4, ' '),
                        pokemon.Item2.ToString().PadLeft(4, ' '),
                        pokemon.Item3.ToString("0.00").PadRight(6, ' '),
                        strPerfect,
                        pokemon.Item4.ToString("00").PadRight(2, ' '),
                        strName,
                        session.Translation.GetPokemonTranslation(pokemon.Item1.PokemonId).PadRight(12, ' '),
                        move1,
                        strMove1.PadRight(16, ' '),
                        move2,
                        strMove2.PadRight(16, ' '),
                        candy,
                        pokemon.Item7.ToString("00").PadRight(2, ' ')
                    ),
                    LogLevel.Info,
                    ConsoleColor.Yellow
                );
            }
        }

        private static void HandleEvent(EvolveCountEvent evolveCountEvent, ISession session)
        {
            if (evolveCountEvent.Evolves>0)
                Logger.Write(
                    session.Translation.GetTranslation(TranslationString.PkmPotentialEvolveCount, evolveCountEvent.Evolves),
                    LogLevel.Evolve
            );
        }

        private static void HandleEvent(UpdateEvent updateEvent, ISession session)
        {
            Logger.Write(updateEvent.ToString(), LogLevel.Update);
        }

        private static void HandleEvent(SnipeModeEvent event1, ISession session)
        {
        }

        private static void HandleEvent(PokeStopListEvent event1, ISession session)
        {
        }

        private static void HandleEvent(EggsListEvent event1, ISession session)
        {
        }

        private static void HandleEvent(InventoryListEvent event1, ISession session)
        {
        }

        private static void HandleEvent(PokemonListEvent event1, ISession session)
        {
        }

        private static void HandleEvent(LoginEvent e, ISession session)
        {
            Logger.Write(
                session.Translation.GetTranslation(TranslationString.LoggingIn, e.AuthType, e.Username),
                LogLevel.Info, ConsoleColor.DarkYellow
            );
        }

        private static void HandleEvent(UpdatePositionEvent event1, ISession session)
        {
            //uncomment for more info about locations
            //Logger.Write(event1.Latitude.ToString("0.0000000000") + "," + event1.Longitude.ToString("0.0000000000"), LogLevel.Debug, force: true);
        }

        private static void HandleEvent(HumanWalkingEvent humanWalkingEvent, ISession session)
        {
            if (session.LogicSettings.ShowVariantWalking)
                Logger.Write(
                    session.Translation.GetTranslation(
                        TranslationString.HumanWalkingVariant,
                        humanWalkingEvent.OldWalkingSpeed,
                        humanWalkingEvent.CurrentWalkingSpeed
                    ),
                    LogLevel.Info,
                    ConsoleColor.DarkCyan
                );

            HumanWalkEvent?.Invoke(humanWalkingEvent);
        }

        private static void HandleEvent(KillSwitchEvent killSwitchEvent, ISession session)
        {
            if (killSwitchEvent.RequireStop)
            {
                Logger.Write(killSwitchEvent.Message, LogLevel.Warning);
                Logger.Write(session.Translation.GetTranslation(TranslationString.RequireInputText), LogLevel.Warning);
            }
            else
                Logger.Write(killSwitchEvent.Message, LogLevel.Info, ConsoleColor.White);
        }

        private static void HandleEvent(HumanWalkSnipeEvent ev, ISession session)
        {
            switch (ev.Type)
            {
                case HumanWalkSnipeEventTypes.StartWalking:
                    var strPokemon = session.Translation.GetPokemonTranslation(ev.PokemonId);
                    Logger.Write(session.Translation.GetTranslation(TranslationString.HumanWalkSnipe,
                            strPokemon,
                            ev.Latitude,
                            ev.Longitude,
                            ev.Distance,
                            ev.Expires / 60,
                            ev.Expires % 60,
                            ev.Estimate / 60,
                            ev.Estimate % 60,
                            ev.SpinPokeStop ? "Yes" : "No",
                            ev.CatchPokemon ? "Yes" : "No",
                            ev.WalkSpeedApplied),
                        LogLevel.Sniper,
                        ConsoleColor.Yellow);
                    break;
                case HumanWalkSnipeEventTypes.DestinationReached:
                    Logger.Write(
                        session.Translation.GetTranslation(
                            TranslationString.HumanWalkSnipeDestinationReached,
                            ev.Latitude, ev.Longitude, ev.PauseDuration
                        ),
                        LogLevel.Sniper
                    );
                    break;
                case HumanWalkSnipeEventTypes.PokemonScanned:
                    if (ev.Pokemons != null && ev.Pokemons.Count > 0 && ev.DisplayMessage)
                        Logger.Write(
                            session.Translation.GetTranslation(TranslationString.HumanWalkSnipeUpdate,
                                ev.Pokemons.Count, 2, 3),
                            LogLevel.Sniper,
                            ConsoleColor.DarkMagenta
                        );
                    break;
                case HumanWalkSnipeEventTypes.PokestopUpdated:
                    Logger.Write(
                        session.Translation.GetTranslation(
                            TranslationString.HumanWalkSnipeAddedPokestop,
                            ev.NearestDistance,
                            ev.Pokestops.Count
                        ),
                        LogLevel.Sniper,
                        ConsoleColor.Yellow
                    );
                    break;
                case HumanWalkSnipeEventTypes.NotEnoughtPalls:
                    Logger.Write(
                        session.Translation.GetTranslation(
                            TranslationString.HumanWalkSnipeNotEnoughtBalls,
                            ev.CurrentBalls,
                            ev.MinBallsToSnipe
                        ),
                        LogLevel.Sniper,
                        ConsoleColor.Yellow
                    );
                    break;
                case HumanWalkSnipeEventTypes.EncounterSnipePokemon:
                    Logger.Write(session.Translation.GetTranslation(TranslationString.HumanWalkSnipePokemonEncountered,
                        session.Translation.GetPokemonTranslation(ev.PokemonId),
                        ev.Latitude,
                        ev.Longitude));
                    break;
                case HumanWalkSnipeEventTypes.AddedSnipePokemon:
                    break;
                case HumanWalkSnipeEventTypes.TargetedPokemon:
                    break;
                case HumanWalkSnipeEventTypes.ClientRequestUpdate:
                    break;
                case HumanWalkSnipeEventTypes.QueueUpdated:
                    break;
                default:
                    break;
            }
        }

        private static void HandleEvent(GymDetailInfoEvent ev, ISession session)
        {
            var GymDeployed = new GymDeployResponse();
            var Deployed = GymDeployed.GymStatusAndDefenders.GymDefender.ToList();
            Logger.Write($"Visited Gym: {ev.Name} | Team: {ev.Team} | Gym points: {ev.Point} | Free Spots: {6 - Deployed.Count}", LogLevel.Gym,
                (ev.Team == TeamColor.Red)
                    ? ConsoleColor.Red
                    : (ev.Team == TeamColor.Yellow ? ConsoleColor.Yellow : ConsoleColor.Blue));
        }

        //TODO - move to string translation later.
        private static void HandleEvent(GymDeployEvent ev, ISession session)
        {
            var Info = new GymDetailInfoEvent();
            var GymDeployed = new GymDeployResponse();
            var Deployed = GymDeployed.GymStatusAndDefenders.GymDefender.ToList();

            Logger.Write($"Great!!! Your {ev.PokemonId.ToString()} is now defending {ev.Name} GYM. | Free Spots: {6 - Deployed.Count}",
                LogLevel.Gym, ConsoleColor.Green);

            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification)
                PushNotificationClient.SendNotification(session, $"Gym Post", $"Great!!! Your {ev.PokemonId.ToString()} is now defending {ev.Name} GYM.\nFree Spots: {6 - Deployed.Count}", true).ConfigureAwait(false);
        }

        private static void HandleEvent(GymBattleStarted ev, ISession session)
        {
            Logger.Write($"Battle started at gym: {ev.GymName}...", LogLevel.Gym, ConsoleColor.Blue);
        }

        private static void HandleEvent(GymErrorUnset ev, ISession session)
        {
            Logger.Write($"Error starting battle with gym: {ev.GymName}. Skipping...",
                LogLevel.Error, ConsoleColor.Red);
        }

        private static void HandleEvent(GymListEvent ev, ISession session)
        {
            Logger.Write($"{ev.Gyms.Count} gyms has been added to farming area.", LogLevel.Gym, ConsoleColor.Cyan);
        }

        private static void HandleEvent(GymWalkToTargetEvent ev, ISession session)
        {
            //Logger.Write(
            //    $"Traveling to gym: {ev.Name} | Lat: {ev.Latitude}, Lng: {ev.Longitude} | ({ev.Distance:0.00}m)",
            //    LogLevel.Gym, ConsoleColor.Cyan
            //);
        }

        private static void HandleEvent(GymTeamJoinEvent ev, ISession session)
        {
            switch (ev.Status)
            {
                case SetPlayerTeamResponse.Types.Status.Unset:
                    break;
                case SetPlayerTeamResponse.Types.Status.Success:
                    Logger.Write($"(TEAM) Joined the {ev.Team} Team!", LogLevel.Gym,
                        (ev.Team == TeamColor.Red)
                            ? ConsoleColor.Red
                            : (ev.Team == TeamColor.Yellow ? ConsoleColor.Yellow : ConsoleColor.Blue));
                    break;
                case SetPlayerTeamResponse.Types.Status.TeamAlreadySet:
                    Logger.Write($"You have joined this team already! ", LogLevel.Gym, color: ConsoleColor.Red);
                    break;
                case SetPlayerTeamResponse.Types.Status.Failure:
                    Logger.Write($"Unable to join team : {ev.Team.ToString()}", color: ConsoleColor.Red);
                    break;
                default:
                    break;
            }
        }

        private static void HandleEvent(EventUsedPotion ev, ISession session)
        {
            Logger.Write(
                $"Used {ev.Type,-8} Potion on {ev.PokemonId,-12} with CP: {ev.PokemonCp,4:###0}. Remaining: {ev.Remaining,3:##0}"
            );
        }

        private static void HandleEvent(EventUsedRevive ev, ISession session)
        {
            Logger.Write(
                $"Used {ev.Type,-8} Revive on {ev.PokemonId,-12} with CP: {ev.PokemonCp,4:###0}. Remaining: {ev.Remaining,3:##0}"
            );
        }

        public static void HandleEvent(IEvent evt, ISession session)
        {
        }

        public void Listen(IEvent evt, ISession session)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve, session);
            }
            catch
            {
            }
        }
    }
}
