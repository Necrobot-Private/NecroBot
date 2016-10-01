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
using PoGo.NecroBot.Logic.Exceptions;

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

        public async Task Start(IState initialState, ISession session, string subPath)
        {
            GlobalSettings globalSettings = null;

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
                    globalSettings = GlobalSettings.Load(subPath);
                    session.LogicSettings = new LogicSettings(globalSettings);
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    Logger.Write(" ##### config.json ##### ", LogLevel.Info);
                }
            };

            do
            {
                try
                {
                    state = await state.Execute(session, session.CancellationTokenSource.Token);

                    // Exit the bot if both catching and looting has reached its limits
                    if ((UseNearbyPokestopsTask._pokestopLimitReached || UseNearbyPokestopsTask._pokestopTimerReached) &&
                        (CatchPokemonTask._catchPokemonLimitReached || CatchPokemonTask._catchPokemonTimerReached))
                    {
                        session.EventDispatcher.Send(new ErrorEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.ExitDueToLimitsReached)
                        });

                        session.CancellationTokenSource.Cancel();

                        // A bit rough here; works but can be improved
                        Thread.Sleep(10000);
                        state = null;
                        session.CancellationTokenSource.Dispose();
                        Environment.Exit(0);
                    }

                }
                catch (ActiveSwitchByPokemonException rsae)
                {
                    session.EventDispatcher.Send(new WarnEvent { Message = "Encountered a good pokemon , switch another bot to catch him too." });
                    session.ResetSessionToWithNextBot(session.Client.CurrentLatitude, session.Client.CurrentLongitude, session.Client.CurrentAltitude);
                    state = new LoginState(rsae.LastEncounterPokemonId);
                }
                catch (ActiveSwitchByRuleException se)
                {
                    session.EventDispatcher.Send(new WarnEvent { Message = $"Switch bot account activated by : {se.MatchedRule.ToString()}  - {se.ReachedValue} " });
                    if(session.LogicSettings.MultipleBotConfig.StartFromDefaultLocation)
                    {
                        session.ResetSessionToWithNextBot(globalSettings.LocationConfig.DefaultLatitude, globalSettings.LocationConfig.DefaultLongitude, session.Client.CurrentAltitude);

                    }
                    else
                    {
                        session.ResetSessionToWithNextBot(); //current location
                    }
                    //return to login state
                    state = new LoginState();
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
                    if (state is LoginState)
                    {
                    }
                    else
                    state = _initialState;
                }
            } while (state != null);
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Dispose();
        }
    }
}