using Microsoft.EntityFrameworkCore.ChangeTracking;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinyIoC;

namespace PoGo.NecroBot.Logic
{
    public class MultiAccountManager
    {
        private AccountConfigContext _context = new AccountConfigContext();
        private const string ACCOUNT_DB_NAME = "accounts.db";
        public object Settings { get; private set; }
        private GlobalSettings _globalSettings { get; set; }
        Client Client { get; set; }
        Account account { get; set; }

        public MultiAccountManager(GlobalSettings globalSettings, List<AuthConfig> accounts)
        {
            _globalSettings = globalSettings;
            MigrateDatabase();
            SyncDatabase(accounts);
        }

        public MultiAccountManager()
        {
        }

        private LocalView<Account> _localAccounts;

        public LocalView<Account> Accounts
        {
            get
            {
                if (_localAccounts != null)
                    return _localAccounts;

                if (_context.Account.Count() > 0)
                {
                    foreach (var item in _context.Account.OrderBy(p => p.Id))
                    {
                        item.IsRunning = 0;
                        item.LastRuntimeUpdatedAt = null;
                        item.RuntimeTotal = 0;
                    }
                    _context.SaveChanges();
                }
                _localAccounts = _context.Account.Local;
                return _localAccounts;
            }
        }

        public AccountConfigContext GetDbContext()
        {
            return _context;
        }

        public List<Account> AccountsReadOnly
        {
            get
            {
                return _context.Account.ToList();
            }
        }

        public Account GetCurrentAccount()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            return _context.Account.FirstOrDefault(a => session.Settings.Username == a.Username && session.Settings.AuthType == a.AuthType);
        }

        public void SwitchAccounts(Account newAccount)
        {
            if (newAccount == null)
                return;

            var runningAccount = GetCurrentAccount();
            if (runningAccount != null)
            {
                runningAccount.IsRunning = 0;
                var now = DateTime.Now;

                if (runningAccount.LastRuntimeUpdatedAt.HasValue)
                    runningAccount.RuntimeTotal += (now - TimeUtil.GetDateTimeFromMilliseconds(runningAccount.LastRuntimeUpdatedAt.Value)).TotalMinutes;
                runningAccount.LastRuntimeUpdatedAt = now.ToUnixTime();
                UpdateLocalAccount(runningAccount);
            }

            newAccount.IsRunning = 1;
            newAccount.LoggedTime = DateTime.Now.ToUnixTime();
            newAccount.LastRuntimeUpdatedAt = newAccount.LoggedTime;
            UpdateLocalAccount(newAccount);

            // Update current auth config with new account.
            _globalSettings.Auth.CurrentAuthConfig.AuthType = (AuthType)newAccount.AuthType;
            _globalSettings.Auth.CurrentAuthConfig.Username = newAccount.Username;
            _globalSettings.Auth.CurrentAuthConfig.Password = newAccount.Password;
            _globalSettings.Auth.CurrentAuthConfig.AutoExitBotIfAccountFlagged = newAccount.AutoExitBotIfAccountFlagged;
            _globalSettings.Auth.CurrentAuthConfig.AccountLatitude = newAccount.AccountLatitude;
            _globalSettings.Auth.CurrentAuthConfig.AccountLongitude = newAccount.AccountLongitude;
            _globalSettings.Auth.CurrentAuthConfig.AccountActive = newAccount.AccountActive;

            string body = "";
            foreach (var item in Accounts)
            {
                body = body + $"{item.Username} - {item.GetRuntime()}\r\n";
            }
        }

        public void BlockCurrentBot(int expired = 60)
        {
            var currentAccount = GetCurrentAccount();

            if (currentAccount != null)
            {
                currentAccount.ReleaseBlockTime = DateTime.Now.AddMinutes(expired).ToUnixTime();
                UpdateLocalAccount(currentAccount);
            }
        }

        private void LoadDataFromDB()
        {
            if (_context.Account.Count() > 0)
            {
                foreach (var item in _context.Account.OrderBy(p => p.Id))
                {
                    item.IsRunning = 0;
                    item.RuntimeTotal = 0;
                    UpdateLocalAccount(item);
                }
            }
        }

        private void MigrateDatabase()
        {
            if (AuthSettings.SchemaVersionBeforeMigration == UpdateConfig.CURRENT_SCHEMA_VERSION)
                return;

            int schemaVersion = AuthSettings.SchemaVersionBeforeMigration;

            // Add future schema migrations below.
            int version;
            for (version = schemaVersion; version < UpdateConfig.CURRENT_SCHEMA_VERSION; version++) 
            {
                Logging.Logger.Write($"Migrating {ACCOUNT_DB_NAME} from schema version {version} to {version + 1}", LogLevel.Info);
                switch (version)
                {
                    case 19:
                        // Just delete the accounts.db so it gets regenerated from scratch.
                        File.Delete(ACCOUNT_DB_NAME);
                        break;
                    case 25:
                        File.Delete(ACCOUNT_DB_NAME);
                        break;
                }
            }
        }

        private void SyncDatabase(List<AuthConfig> authConfigs)
        {
            if (authConfigs.Count() == 0)
                return;
            
            // Add new accounts and update existing accounts.
            foreach (var authConfig in authConfigs)
            {
                if (string.IsNullOrEmpty(authConfig.Username) || string.IsNullOrEmpty(authConfig.Password))
                    continue;

                var existing = _context.Account.FirstOrDefault(x => x.Username == authConfig.Username && x.AuthType == authConfig.AuthType);

                if (existing == null)
                {
                    try
                    {
                        Account newAcc = new Account(authConfig);
                        _context.Account.Add(newAcc);
                        _context.SaveChanges();
                    }
                    catch(Exception)
                    {
                        Logic.Logging.Logger.Write($"Error while saving data into {ACCOUNT_DB_NAME}, please delete {ACCOUNT_DB_NAME} and restart bot to have it fully work in order");
                    }
                }
                else
                {
                    // Update credentials in database using values from json.
                    existing.Username = authConfig.Username;
                    existing.Password = authConfig.Password;
                    existing.AutoExitBotIfAccountFlagged = authConfig.AutoExitBotIfAccountFlagged;
                    existing.AccountLatitude = authConfig.AccountLatitude;
                    existing.AccountLongitude = authConfig.AccountLongitude;
                    existing.AccountActive = authConfig.AccountActive;
                    _context.SaveChanges();
                }
            }
            
            // Remove accounts that are not in the auth.json but in the database.
            List<Account> accountsToRemove = new List<Account>();
            foreach (var item in _context.Account)
            {
                var existing = authConfigs.FirstOrDefault(x => x.Username == item.Username && x.AuthType == item.AuthType);
                if (existing == null)
                {
                    accountsToRemove.Add(item);
                }
            }

            foreach (var item in accountsToRemove)
            {
                _context.Account.Remove(item);
            }
            _context.SaveChanges();
        }

        internal Account GetMinRuntime(bool ignoreBlockCheck = false)
        {
            if (_context.Account.Count() == 0)
                return null;

            if (ignoreBlockCheck)
                return _context.Account.OrderBy(x => x.Level).ThenBy(x => x.CurrentXp).Where(x => x.AccountActive == true).FirstOrDefault();
            else
                return _context.Account.OrderBy(x => x.AccountActive).ThenBy(x => x.Level).ThenBy(x => x.CurrentXp).ThenBy(x => x.RuntimeTotal).Where(x => x != null && x.ReleaseBlockTime.HasValue && x.ReleaseBlockTime < DateTime.Now.ToUnixTime()).FirstOrDefault();
        }

        public bool AllowMultipleBot()
        {
            return _context.Account.Count() > 1;
        }

        public Account GetStartUpAccount()
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            Account startupAccount;
            
            if (!AllowMultipleBot())
            {
                startupAccount = _context.Account.Last();
            }
            else
            {
                startupAccount = GetMinRuntime(true);
            }
        
            if (AllowMultipleBot()
              && session.LogicSettings.MultipleBotConfig.SelectAccountOnStartUp)
            {
                SelectAccountForm f = new SelectAccountForm();
                f.ShowDialog();
                startupAccount = f.SelectedAccount;
            }
            return startupAccount;
        }

        private DateTime disableSwitchTime = DateTime.MinValue;
        internal void DisableSwitchAccountUntil(DateTime untilTime)
        {
            if (disableSwitchTime < untilTime) disableSwitchTime = untilTime;
        }

        public bool AllowSwitch()
        {
            return disableSwitchTime < DateTime.Now;
        }

        public Account GetSwitchableAccount(Account bot = null, bool pauseIfNoSwitchableAccount = true)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();

            var currentAccount = GetCurrentAccount();

            // If the bot to switch to is the same as the current bot then just return.
            if (bot == currentAccount)
                return bot;

            if (bot != null)
                return bot;

            if (_context.Account.Count() > 0)
            {
                var runnableAccount = _context.Account.OrderBy(x => x.RuntimeTotal).ThenBy(p => p.AccountActive).ThenBy(p => p.Level).ThenBy(p => p.CurrentXp).FirstOrDefault(p => p != currentAccount);

                if (runnableAccount != null)
                    return runnableAccount;
            }

            if (!pauseIfNoSwitchableAccount)
                return null;

            // If we got here all accounts blocked so pause and retry.
            var pauseTime = session.LogicSettings.MultipleBotConfig.OnLimitPauseTimes;

            Logic.Logging.Logger.Write($"All accounts are blocked. None of your accounts are available to switch to, so bot will sleep for {pauseTime} minutes until next account is available to run.");
            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification)
                PushNotificationClient.SendNotification(session, "All accounts are blocked.", $"None of your accounts are available to switch to, so bot will sleep for {pauseTime} minutes until next account is available to run.", true).ConfigureAwait(false);

            Task.Delay(pauseTime * 60 * 1000).Wait();
            return GetSwitchableAccount();
        }

        private bool switchAccountRequest = false;
        public void SwitchAccountTo(Account account)
        {
            requestedAccount = account;
            switchAccountRequest = true;
        }

        public void ThrowIfSwitchAccountRequested()
        {
            if (switchAccountRequest && requestedAccount != null && (!requestedAccount.IsRunning.HasValue || requestedAccount.IsRunning.Value == 0))
            {
                switchAccountRequest = false;
                throw new ActiveSwitchAccountManualException(requestedAccount);
            }
        }

        private Account requestedAccount = null;
        public void UpdateLocalAccount(Account current, bool save = true)
        {
            //var localAccount = Accounts.OrderBy(x => x.AccountActive).ThenBy(x => x.RuntimeTotal).ThenBy(p => p.Level).ThenBy(p => p.CurrentXp).Where(x => x.Id == current.Id).FirstOrDefault();
            var localAccount = Accounts.Where(a => a.Id == current.Id).FirstOrDefault();
            if (localAccount != null)
            {
                localAccount.Nickname = current.Nickname;
                localAccount.RaisePropertyChanged("Nickname");
                localAccount.RuntimeTotal = current.RuntimeTotal;
                localAccount.RaisePropertyChanged("RuntimeTotal");
                localAccount.IsRunning = current.IsRunning;
                localAccount.RaisePropertyChanged("IsRunning");
                localAccount.Level = current.Level;
                localAccount.RaisePropertyChanged("Level");
                localAccount.LastLogin = current.LastLogin;
                localAccount.RaisePropertyChanged("LastLogin");
                localAccount.LastLoginTimestamp = current.LastLoginTimestamp;
                localAccount.RaisePropertyChanged("LastLoginTimestamp");
                localAccount.Level = current.Level;
                localAccount.RaisePropertyChanged("Level");
                localAccount.Stardust = current.Stardust;
                localAccount.RaisePropertyChanged("Stardust");
                localAccount.CurrentXp = current.CurrentXp;
                localAccount.RaisePropertyChanged("CurrentXp");
                localAccount.NextLevelXp = current.NextLevelXp;
                localAccount.RaisePropertyChanged("NextLevelXp");
                localAccount.PrevLevelXp = current.PrevLevelXp;
                localAccount.RaisePropertyChanged("PrevLevelXp");
                localAccount.RaisePropertyChanged("ExperienceInfo");

                localAccount.AccountLatitude = string.IsNullOrEmpty(current.AccountLatitude.ToString()) ? Client.CurrentLatitude : current.AccountLatitude; //current.AccountLatitude;
                localAccount.AccountLongitude = string.IsNullOrEmpty(current.AccountLongitude.ToString()) ? Client.CurrentLongitude : current.AccountLongitude;
                localAccount.AccountActive = current.AccountActive;

                if (save)
                    _context.SaveChanges();
            }
        }

        public void DumpAccountList()
        {
            var userL = 0;
            var maxL = 0;
            var user = "";

            foreach (var item in Accounts)
            {
                user = string.IsNullOrEmpty(item.Nickname) ? item.Username : item.Nickname;
                userL = user.Length;
                if (userL > maxL)
                {
                    maxL = userL;
                }
            }

            //Accounts.OrderBy(x => x.RuntimeTotal).ThenBy(p => p.Level).ThenBy(p => p.CurrentXp).FirstOrDefault();
            //Accounts.OrderBy(x => x.RuntimeTotal).ThenBy(p => p.Level).ThenBy(p => p.CurrentXp).Where(p => p.AccountActive == true).FirstOrDefault();
            foreach (var item in Accounts)
            {
                user = string.IsNullOrEmpty(item.Nickname) ? item.Username : item.Nickname;

                if (item.Level > 0)
                {
                    if (item.AccountActive)
                        Logger.Write($"{user.PadRight(maxL)} | Lvl: {item.Level,2:#0} | XP: {item.CurrentXp,8:0}({(double)item.CurrentXp.Value / (double)item.NextLevelXp.Value * 100,2:#0}%) | SD: {item.Stardust,8:0} | Runtime: {item.RuntimeTotal:00:00}", LogLevel.BotStats);
                    else
                        Logger.Write($"{user.PadRight(maxL)} | Lvl: {item.Level,2:#0} | XP: {item.CurrentXp,8:0}({(double)item.CurrentXp.Value / (double)item.NextLevelXp.Value * 100,2:#0}%) | SD: {item.Stardust,8:0} | Runtime: {item.RuntimeTotal:00:00}", LogLevel.BotStats, ConsoleColor.Red);
                }
                else
                {
                    if (item.AccountActive)
                        Logger.Write($"{user.PadRight(maxL)} | Lvl: ?? | XP:        0( 0%) | SD:        0 | Runtime: {item.RuntimeTotal:00:00}", LogLevel.BotStats, ConsoleColor.Yellow);
                    else
                        Logger.Write($"{user.PadRight(maxL)} | Lvl: ?? | XP:        0( 0%) | SD:        0 | Runtime: {item.RuntimeTotal:00:00}", LogLevel.BotStats, ConsoleColor.Red);
                }
            }
        }

        public Account FindAvailableAccountForPokemonSwitch(string encounterId)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();

            //set current 
            Account switchableAccount = GetSwitchableAccount(null, false); // Don't pause if no switchable account is available.
            if (switchableAccount != null)
            {
                if (session.Cache.GetCacheItem(CatchPokemonTask.GetUsernameEncounterCacheKey(switchableAccount.Username, encounterId)) == null)
                {
                    // Don't edit the running account until we actually switch.  Just return the pending account.
                    return switchableAccount;
                }
            }
            return null;
        }
        
        internal void DirtyEventHandle(Statistics stat)
        {
            var account = GetCurrentAccount();
            if (account == null)
                return;

            account.Level = stat.StatsExport.Level;
            account.Stardust = stat.TotalStardust;
            account.CurrentXp = stat.StatsExport.CurrentXp - stat.StatsExport.LevelXp;
            account.NextLevelXp = stat.StatsExport.LevelupXp - stat.StatsExport.LevelXp;
            account.PrevLevelXp = stat.StatsExport.PreviousXp - stat.StatsExport.LevelXp;
            var now = DateTime.Now;
            if (account.LastRuntimeUpdatedAt.HasValue)
                account.RuntimeTotal += (now - TimeUtil.GetDateTimeFromMilliseconds(account.LastRuntimeUpdatedAt.Value)).TotalMinutes;
            account.LastRuntimeUpdatedAt = now.ToUnixTime();

            UpdateLocalAccount(account);
        }
    }
}
