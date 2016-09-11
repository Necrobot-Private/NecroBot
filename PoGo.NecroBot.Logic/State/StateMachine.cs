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
                    Logger.Write(" ##### config.json ##### ", LogLevel.Info);
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
                        Thread.Sleep(10000);
                        state = null;
                        cts.Dispose();
                        Environment.Exit(0);
                    }

                }
                catch (InvalidResponseException)
                {
                    session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = "Niantic Servers unstable, throttling API Calls."
                    });
                }
                catch (OperationCanceledException)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Current Operation was canceled."});
                    state = _initialState;
                }
                catch (Exception ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Pokemon Servers might be offline / unstable. Trying again..."});
                    Thread.Sleep(1000);
                    session.EventDispatcher.Send(new ErrorEvent { Message = "Error: " + ex });
                    state = _initialState;
                }
            } while (state != null);
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Dispose();
        }
    }
}