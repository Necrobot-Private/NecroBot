using LiteDB;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinyIoC;
using static PoGo.NecroBot.Logic.Utils.PushNotificationClient;

namespace PoGo.NecroBot.Logic
{
    public class MultiAccountManager
    {
        private AccountConfigContext _context = new AccountConfigContext();
        private const string ACCOUNT_DB_NAME = "accounts.db";

        public object Settings { get; private set; }
        private GlobalSettings _globalSettings { get; set; }
        public MultiAccountManager(GlobalSettings globalSettings, List<AuthConfig> accounts)
        {
            _globalSettings = globalSettings;
            MigrateDatabase();
            SyncDatabase(accounts);
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

            string body = "";
            foreach (var item in Accounts)
            {
                body = body + $"{item.Username}     {item.GetRuntime()}\r\n";
            }

            var session = TinyIoCContainer.Current.Resolve<ISession>();

            Logging.Logger.Write($"Account changed to {newAccount.Username}.");

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
            SendNotification(session, $"Account changed to {newAccount.Username}", body);
#pragma warning restore 4014
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

            // Backup old config file.
            long ts = DateTime.UtcNow.ToUnixTime(); // Add timestamp to avoid file conflicts
            if (File.Exists(ACCOUNT_DB_NAME))
            {
                string backupPath = $"accounts-{schemaVersion}-{ts}.backup.db";
                Logging.Logger.Write($"Backing up {ACCOUNT_DB_NAME} to: {backupPath}", LogLevel.Info);
            
                File.Copy(ACCOUNT_DB_NAME, backupPath);
            }
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

                    case 24:
                        MigrateLiteDbToSqLite();
                        //File.Delete(ACCOUNT_DB_NAME);
                        break;
                }
            }
        }

        private void MigrateLiteDbToSqLite()
        {
            // Delete all accounts
            _context.Account.RemoveRange(_context.Account);
            _context.SaveChanges();

            using (var liteDb = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var liteDbAccounts = liteDb.GetCollection<BotAccount>("accounts");
                foreach (var liteDbAccount in liteDbAccounts.FindAll())
                {
                    if (string.IsNullOrEmpty(liteDbAccount.Username) || string.IsNullOrEmpty(liteDbAccount.Password))
                        continue;

                    Account newAccount = new Account();
                    newAccount.AuthType = liteDbAccount.AuthType;
                    newAccount.Username = liteDbAccount.Username;
                    newAccount.Password = liteDbAccount.Password;
                    newAccount.RuntimeTotal = liteDbAccount.RuntimeTotal;
                    newAccount.LastRuntimeUpdatedAt = liteDbAccount.LastRuntimeUpdatedAt.ToUnixTime();
                    if (liteDbAccount.ReleaseBlockTime > DateTime.Now)
                        newAccount.ReleaseBlockTime = liteDbAccount.ReleaseBlockTime.ToUnixTime();
                    newAccount.Nickname = liteDbAccount.Nickname;
                    newAccount.LoggedTime = liteDbAccount.LoggedTime.ToUnixTime();
                    newAccount.Level = liteDbAccount.Level;
                    newAccount.LastLogin = liteDbAccount.LastLogin;
                    newAccount.LastLoginTimestamp = liteDbAccount.LastLoginTimestamp;
                    newAccount.Stardust = liteDbAccount.Stardust;
                    newAccount.CurrentXp = liteDbAccount.CurrentXp;
                    newAccount.NextLevelXp = liteDbAccount.NextLevelXp;
                    newAccount.PrevLevelXp = liteDbAccount.PrevLevelXp;
                    _context.Account.Add(newAccount);
                    _context.SaveChanges();
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
                return _context.Account.OrderBy(x => x.RuntimeTotal.HasValue ? x.RuntimeTotal.Value : 0).ThenBy(x => x != null ? x.Id : 0).FirstOrDefault();
            else
                return _context.Account.OrderBy(x => x.RuntimeTotal.HasValue ? x.RuntimeTotal.Value : 0).ThenBy(x => x != null ? x.Id : 0).Where(x => x != null && x.ReleaseBlockTime.HasValue && x.ReleaseBlockTime.Value < DateTime.Now.ToUnixTime()).FirstOrDefault();
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
                var runnableAccount = _context.Account.OrderByDescending(p => p.RuntimeTotal).ThenBy(p => p.Id).LastOrDefault(p => p != currentAccount && (!p.ReleaseBlockTime.HasValue || p.ReleaseBlockTime.HasValue && p.ReleaseBlockTime.Value < DateTime.Now.ToUnixTime()));

                if (runnableAccount != null)
                    return runnableAccount;
            }

            if (!pauseIfNoSwitchableAccount)
                return null;

            // If we got here all accounts blocked so pause and retry.
            var pauseTime = session.LogicSettings.MultipleBotConfig.OnLimitPauseTimes;
#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
            SendNotification(session, "All accounts are blocked.", $"None of your accounts are available to switch to, so bot will sleep for {pauseTime} minutes until next account is available to run.", true);
#pragma warning restore 4014

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

                if (save)
                    _context.SaveChanges();
            }
        }
        
        public void DumpAccountList()
        {
            foreach (var item in Accounts)
            {
                if (item.Level > 0)
                    Logging.Logger.Write($"{item.Username} (Level: {item.Level})\t\t\tRuntime : {item.GetRuntime()}");
                else
                    Logging.Logger.Write($"{item.Username} (Level: ??)\t\t\tRuntime : {item.GetRuntime()}");
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
            account.CurrentXp = stat.StatsExport.CurrentXp;
            account.NextLevelXp = stat.StatsExport.LevelupXp;
            account.PrevLevelXp = stat.StatsExport.PreviousXp;
            var now = DateTime.Now;
            if (account.LastRuntimeUpdatedAt.HasValue)
                account.RuntimeTotal += (now - TimeUtil.GetDateTimeFromMilliseconds(account.LastRuntimeUpdatedAt.Value)).TotalMinutes;
            account.LastRuntimeUpdatedAt = now.ToUnixTime();

            UpdateLocalAccount(account);
        }
    }
}
