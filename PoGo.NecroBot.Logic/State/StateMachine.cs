#region using directives

using System;
using System.IO;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Captcha;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using static System.Threading.Tasks.Task;
using static PoGo.NecroBot.Logic.Utils.PushNotificationClient;
using TinyIoC;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public class StateMachine
    {
        private IState _initialState;

        public Task AsyncStart(IState initialState, Session session, string subPath, bool excelConfigAllowed = false)
        {
            return Run(() => Start(initialState, session, subPath, excelConfigAllowed));
        }

        public void SetFailureState(IState state)
        {
            _initialState = state;
        }

        public async Task Start(IState initialState, ISession session, string subPath, bool excelConfigAllowed = false)
        {
            GlobalSettings globalSettings = null;

            var state = initialState;
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), subPath);
            var profileConfigPath = Path.Combine(profilePath, "config");
            globalSettings = GlobalSettings.Load(subPath);

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
                    // BUG: duplicate boolean negation will take no effect
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    configWatcher.EnableRaisingEvents = !configWatcher.EnableRaisingEvents;
                    Logger.Write(" ##### config.json ##### ", LogLevel.Info);
                }
            };

            //watch the excel config file
            if (excelConfigAllowed)
            {
                // TODO - await is legal here! USE it or use pragma to suppress compilerwarning and write a comment why it is not used
                // TODO: Attention - do not touch (add pragma) when you do not know what you are doing ;)
                // jjskuld - Ignore CS4014 warning for now.
                #pragma warning disable 4014
                Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            FileInfo inf = new FileInfo($"{profileConfigPath}\\config.xlsm");
                            if (inf.LastWriteTime > DateTime.Now.AddSeconds(-5))
                            {
                                globalSettings = ExcelConfigHelper.ReadExcel(globalSettings, inf.FullName);
                                session.LogicSettings = new LogicSettings(globalSettings);
                                Logger.Write(" ##### config.xlsm ##### ", LogLevel.Info);
                            }
                            await Delay(5000);
                        }
                        catch (Exception)
                        {
                            // TODO Bad practice! Wanna log this?
                        }
                    }
                });
                #pragma warning restore 4014
            }

            int apiCallFailured = 0;
            do
            {
                try
                {
                    state = await state.Execute(session, session.CancellationTokenSource.Token);

                    // Exit the bot if both catching and looting has reached its limits
                    if ((UseNearbyPokestopsTask._pokestopLimitReached ||
                         UseNearbyPokestopsTask._pokestopTimerReached) &&
                        session.Stats.CatchThresholdExceeds(session))
                    {
                        session.EventDispatcher.Send(new ErrorEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.ExitDueToLimitsReached)
                        });

                        session.CancellationTokenSource.Cancel();

                        // A bit rough here; works but can be improved
                        await Delay(10000);
                        state = null;
                        session.CancellationTokenSource.Dispose();
                        Environment.Exit(0);
                    }
                }
                catch (APIBadRequestException ex)
                {
                    Logger.Write("Bad Request - If you see this message please copy error log & screenshot send back to dev asap.", level: LogLevel.Error);
                    
                    session.EventDispatcher.Send(new ErrorEvent() {Message = ex.Message});
                    Logger.Write(ex.StackTrace, level: LogLevel.Error);

                    if (session.LogicSettings.AllowMultipleBot)
                        session.ReInitSessionWithNextBot();
                    state = new LoginState();
                }
                catch (AccountNotVerifiedException)
                {
                    if (session.LogicSettings.AllowMultipleBot)
                    {
                        session.ReInitSessionWithNextBot();
                        state = new LoginState();
                    }
                    else
                    {
                        Console.Read();
                        Environment.Exit(0);
                    }
                }
                catch(ActiveSwitchAccountManualException ex)
                {
                    session.EventDispatcher.Send(new WarnEvent { Message = "Switch account requested by user" });
                    session.ReInitSessionWithNextBot(ex.RequestedAccount, session.Client.CurrentLatitude, session.Client.CurrentLongitude, session.Client.CurrentAltitude);
                    state = new LoginState();

                }
                catch (ActiveSwitchByPokemonException rsae)
                {
                    if (rsae.Snipe && rsae.EncounterData != null)
                        session.EventDispatcher.Send(new WarnEvent { Message = $"Detected a good pokemon with snipe {rsae.EncounterData.PokemonId.ToString()}   IV:{rsae.EncounterData.IV}  Move:{rsae.EncounterData.Move1}/ Move:{rsae.EncounterData.Move2}   LV: Move:{rsae.EncounterData.Level}" });
                    else
                        session.EventDispatcher.Send(new WarnEvent { Message = "Encountered a good pokemon, switch another bot to catch him too." });
                    
                    session.ReInitSessionWithNextBot(rsae.Bot, session.Client.CurrentLatitude, session.Client.CurrentLongitude, session.Client.CurrentAltitude);
                    state = new LoginState(rsae.LastEncounterPokemonId, rsae.EncounterData);
                }
                catch (ActiveSwitchByRuleException se)
                {
                    session.EventDispatcher.Send(new WarnEvent { Message = $"Switch bot account activated by : {se.MatchedRule.ToString()}  - {se.ReachedValue} " });
                    if (se.MatchedRule == SwitchRules.EmptyMap)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(90);
                        session.ReInitSessionWithNextBot();
                    }
                    else if (se.MatchedRule == SwitchRules.PokestopSoftban)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot();
                        session.ReInitSessionWithNextBot();
                    }
                    else if (se.MatchedRule == SwitchRules.CatchFlee)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(60);
                        session.ReInitSessionWithNextBot();
                    }
                    else
                    {
                        if (se.MatchedRule == SwitchRules.CatchLimitReached ||
                            se.MatchedRule == SwitchRules.SpinPokestopReached)
                        {
                            // TODO - await is legal here! USE it or use pragma to suppress compilerwarning and write a comment why it is not used
                            // TODO: Attention - do not touch (add pragma) when you do not know what you are doing ;)
                            // jjskuld - Ignore CS4014 warning for now.
                            #pragma warning disable 4014
                            SendNotification(session, $"{se.MatchedRule} - {session.Settings.Username}", "This bot has reach limit, it will be blocked for 60 mins for safety.", true);
                            #pragma warning restore 4014
                            session.EventDispatcher.Send(new WarnEvent() { Message = $"You reach limited. bot will sleep for {session.LogicSettings.MultipleBotConfig.OnLimitPauseTimes} min" });

                            TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(session.LogicSettings.MultipleBotConfig.OnLimitPauseTimes);

                            session.ReInitSessionWithNextBot();
                        }
                        else
                        {
                            if (session.LogicSettings.MultipleBotConfig.StartFromDefaultLocation)
                            {
                                session.ReInitSessionWithNextBot(null, globalSettings.LocationConfig.DefaultLatitude, globalSettings.LocationConfig.DefaultLongitude, session.Client.CurrentAltitude);
                            }
                            else
                            {
                                session.ReInitSessionWithNextBot(); //current location
                            }
                        }
                    }
                    //return to login state
                    state = new LoginState();
                }

                catch (InvalidResponseException e)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = $"Niantic Servers unstable, throttling API Calls. {e.Message}" });
                    await Delay(1000);
                    if (session.LogicSettings.AllowMultipleBot)
                    {
                        apiCallFailured++;
                        if (apiCallFailured > 20)
                        {
                            apiCallFailured = 0;
                            TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(30);
                            session.ReInitSessionWithNextBot();
                        }
                    }
                    state = new LoginState();
                }
                catch (OperationCanceledException)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = "Current Operation was canceled."});
                    if (session.LogicSettings.AllowMultipleBot)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(30);
                        session.ReInitSessionWithNextBot();
                    }
                    state = new LoginState();
                }
                catch(PtcLoginException ex)
                {
                    #pragma warning disable 4014
                    SendNotification(session, $"PTC Login failed!!!! {session.Settings.Username}", session.Translation.GetTranslation(TranslationString.PtcLoginFail), true);
                    #pragma warning restore 4014

                    if (session.LogicSettings.AllowMultipleBot)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(60); //need remove acc
                        session.ReInitSessionWithNextBot();
                        state = new LoginState();
                    }
                    else {
                        session.EventDispatcher.Send(new ErrorEvent { RequireExit = true, Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                        session.EventDispatcher.Send(new ErrorEvent { RequireExit = true, Message = session.Translation.GetTranslation(TranslationString.PtcLoginFail)  + $" ({ex.Message})"});

                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                }
                catch (LoginFailedException)
                {
                    // TODO - await is legal here! USE it or use pragma to suppress compilerwarning and write a comment why it is not used
                    // TODO: Attention - do not touch (add pragma) when you do not know what you are doing ;)
                    // jjskuld - Ignore CS4014 warning for now.
                    #pragma warning disable 4014
                    SendNotification(session, $"Banned!!!! {session.Settings.Username}", session.Translation.GetTranslation(TranslationString.AccountBanned), true);
                    #pragma warning restore 4014

                    if (session.LogicSettings.AllowMultipleBot)
                    {
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(24 * 60); //need remove acc
                        session.ReInitSessionWithNextBot();
                        state = new LoginState();
                    }
                    else {
                        session.EventDispatcher.Send(new ErrorEvent { RequireExit = true, Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                }
                catch (MinimumClientVersionException ex)
                {
                    // We need to terminate the client.
                    session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.MinimumClientVersionException, ex.CurrentApiVersion.ToString(), ex.MinimumClientVersion.ToString())
                    });

                    session.EventDispatcher.Send(new ErrorEvent { RequireExit = true, Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                catch (TokenRefreshException ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent() {Message = ex.Message});

                    if (session.LogicSettings.AllowMultipleBot)
                        session.ReInitSessionWithNextBot();
                    state = new LoginState();
                }

                catch (PtcOfflineException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.PtcOffline) });
                    session.EventDispatcher.Send(new NoticeEvent { Message = session.Translation.GetTranslation(TranslationString.TryingAgainIn, 15) });

                    await Delay(1000);
                    state = _initialState;
                }
                catch (GoogleOfflineException)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.GoogleOffline) });
                    session.EventDispatcher.Send(new NoticeEvent { Message = session.Translation.GetTranslation(TranslationString.TryingAgainIn, 15) });

                    await Delay(15000);
                    state = _initialState;
                }
                catch (AccessTokenExpiredException)
                {
                    session.EventDispatcher.Send(new NoticeEvent { Message = "Access Token Expired. Logging in again..." });
                    state = _initialState;
                }
                catch (CaptchaException captchaException)
                {
                    var resolved = await CaptchaManager.SolveCaptcha(session, captchaException.Url);
                    if (!resolved)
                    {
                        await SendNotification(session, $"Captcha required {session.Settings.Username}", session.Translation.GetTranslation(TranslationString.CaptchaShown), true);
                        session.EventDispatcher.Send(new WarnEvent { Message = session.Translation.GetTranslation(TranslationString.CaptchaShown) });
                        Logger.Debug("Captcha not resolved");
                        if (session.LogicSettings.AllowMultipleBot)
                        {
                            Logger.Debug("Change account");
                            TinyIoCContainer.Current.Resolve<MultiAccountManager>().BlockCurrentBot(15);
                            session.ReInitSessionWithNextBot();
                            state = new LoginState();
                        }
                        else
                        {
                            session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        //resolve captcha
                        state = new LoginState();
                    }
                }
                catch (HasherException ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent {Message = ex.Message});
                    //  session.EventDispatcher.Send(new ErrorEvent { Message = session.Translation.GetTranslation(TranslationString.ExitNowAfterEnterKey) });
                    state = new IdleState();
                    //Console.ReadKey();
                    //System.Environment.Exit(1);
                }
                catch (Exception ex)
                {
                    session.EventDispatcher.Send(new ErrorEvent { Message = "Pokemon Servers might be offline / unstable. Trying again..." });
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