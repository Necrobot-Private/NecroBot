#region using directives

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public class StateMachine
    {
        private IState _initialState;

        public Task AsyncStart(IState initialState, Session session, string subPath)
        {
            return Task.Run(() => Start(initialState, session, subPath));
        }

        public void SetFailureState(IState state)
        {
            _initialState = state;
        }

        public async Task Start(IState initialState, Session session, string subPath)
        {
            var state = initialState;
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), subPath);
            var profileConfigPath = Path.Combine(profilePath, "config");

            FileSystemWatcher configWatcher = new FileSystemWatcher();
            configWatcher.Path = profileConfigPath;
            configWatcher.Filter = "config.json";
            configWatcher.NotifyFilter = NotifyFilters.LastWrite;
            configWatcher.EnableRaisingEvents = true;
            configWatcher.Changed += (sender, e) =>
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    session.LogicSettings = new LogicSettings(GlobalSettings.Load(subPath));
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    Logger.Write(" ##### config.json is updated ##### ", LogLevel.Info);
                }
            };

            // We need a CTS to be able to cancel taks at all
            // All cancelling through the tasks originates from here
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            do
            {
                try
                {
                    state = await state.Execute(session, cancellationToken);

                    // Exit the bot if both catching and looting has reached its limits
                    if ((UseNearbyPokestopsTask._pokestopLimitReached || UseNearbyPokestopsTask._pokestopTimerReached) &&
                        (CatchPokemonTask._catchPokemonLimitReached || CatchPokemonTask._catchPokemonTimerReached))
                    {
                        session.EventDispatcher.Send(new ErrorEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.ExitDueToLimitsReached)
                        });

                        cts.Cancel();

                        // A bit rough here; works but can be improved
                        await Task.Delay(10000);
                        state = null;
                        cts.Dispose();
                        Environment.Exit(0);
                    }
                }
                catch (InvalidResponseException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = "Niantic Servers unstable, throttling API Calls." });
                    await Task.Delay(1000);
                }
                catch (OperationCanceledException)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Current Operation was canceled."});
                    state = _initialState;
                }
                catch (MinimumClientVersionException ex)
                {
                    // We need to terminate the client.
                    session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.MinimumClientVersionException, ex.CurrentApiVersion.ToString(), ex.MinimumClientVersion.ToString())
                    });

                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                    Console.ReadKey();
                    System.Environment.Exit(1);
                }
                catch (LoginFailedException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.AccountBanned) });
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                    Console.ReadKey();
                    System.Environment.Exit(1);
                }
                catch (PtcOfflineException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.PtcOffline) });
                    session.EventDispatcher.Send(new NoticeEvent { Message = session.Translation.GetTranslation(TranslationString.TryingAgainIn, 15) });

                    await Task.Delay(15000);
                    state = _initialState;
                }
                catch (GoogleOfflineException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.GoogleOffline) });
                    session.EventDispatcher.Send(new NoticeEvent { Message = session.Translation.GetTranslation(TranslationString.TryingAgainIn, 15) });

                    await Task.Delay(15000);
                    state = _initialState;
                }
                catch (AccessTokenExpiredException)
                {
                    session.EventDispatcher.Send(new NoticeEvent { Message = "Access Token Expired. Logging in again..." });
                    state = _initialState;
                }
                catch (CaptchaException)
                {
                    // TODO Show the captcha.
                    session.EventDispatcher.Send(new WarnEvent { Message = session.Translation.GetTranslation(TranslationString.CaptchaShown) });
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Pokemon Servers might be offline / unstable. Trying again..."});
                    session.EventDispatcher.Send(new ErrorEvent { Message = "Error: " + ex });
                    await Task.Delay(1000);
                    state = _initialState;
                }
            } while (state != null);
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Dispose();
        }
    }
}