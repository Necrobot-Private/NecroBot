using LiteDB;
using Microsoft.EntityFrameworkCore;
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
        private DatabaseConfigContext _context = new DatabaseConfigContext();
        private const string ACCOUNT_DB_NAME = "accounts.db";

        public object Settings { get; private set; }
        private GlobalSettings _globalSettings { get; set; }
        public MultiAccountManager(GlobalSettings globalSettings, List<AuthConfig> accounts)
        {
            _globalSettings = globalSettings;
            MigrateDatabase();
            LoadDataFromDB();
            SyncDatabase(accounts);
        }

        public Account GetCurrentAccount(DatabaseConfigContext db)
        {
            var runningAccounts = from a in db.Account
                                  where a.IsRunning == 1
                                  select a;
            return runningAccounts.FirstOrDefault();
        }

        public LocalView<Account> GetBindableAccounts()
        {
            _context.Account.Load();
            return _context.Account.Local;
        }

        public List<Account> GetAccountsReadOnly()
        {
            using (var db = new DatabaseConfigContext())
            {
                return db.Account.ToList();
            }
        }

        public void SwitchAccounts(Account newAccount)
        {
            if (newAccount == null)
                return;

            using (var db = new DatabaseConfigContext())
            {
                var runningAccount = GetCurrentAccount(db);
                if (runningAccount != null)
                {
                    runningAccount.IsRunning = 0;
                    var now = DateTime.Now;

                    if (runningAccount.LastRuntimeUpdatedAt.HasValue)
                        runningAccount.RuntimeTotal += (now - TimeUtil.GetDateTimeFromMilliseconds(runningAccount.LastRuntimeUpdatedAt.Value)).TotalMinutes;
                    runningAccount.LastRuntimeUpdatedAt = now.ToUnixTime();
                    UpdateDatabase(db, runningAccount);
                }

                newAccount.IsRunning = 1;
                newAccount.LoggedTime = DateTime.Now.ToUnixTime();
                newAccount.LastRuntimeUpdatedAt = newAccount.LoggedTime;
                UpdateDatabase(db, newAccount);

                // Update current auth config with new account.
                _globalSettings.Auth.CurrentAuthConfig.AuthType = (AuthType)newAccount.AuthType;
                _globalSettings.Auth.CurrentAuthConfig.Username = newAccount.Username;
                _globalSettings.Auth.CurrentAuthConfig.Password = newAccount.Password;

                string body = "";
                foreach (var item in db.Account)
                {
                    body = body + $"{item.Username}     {item.GetRuntime()}\r\n";
                }

                var session = TinyIoCContainer.Current.Resolve<ISession>();

                Logging.Logger.Write($"Account changed to {newAccount.Username}.");

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
                SendNotification(session, $"Account changed to {newAccount.Username}", body);
#pragma warning restore 4014
            }
        }

        public void BlockCurrentBot(int expired = 60)
        {
            using (var db = new DatabaseConfigContext())
            {
                var currentAccount = GetCurrentAccount(db);

                if (currentAccount != null)
                {
                    currentAccount.ReleaseBlockTime = DateTime.Now.AddMinutes(expired).ToUnixTime();
                    UpdateDatabase(db, currentAccount);
                }
            }
        }

        private void LoadDataFromDB()
        {
            using (var db = new DatabaseConfigContext())
            {
                foreach (var item in db.Account.OrderBy(p => p.Id))
                {
                    item.IsRunning = 0;
                    item.RuntimeTotal = 0;
                    UpdateDatabase(db, item);
                }
            }
        }

        private void MigrateDatabase()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();

            if (AuthSettings.SchemaVersionBeforeMigration == UpdateConfig.CURRENT_SCHEMA_VERSION)
                return;

            int schemaVersion = AuthSettings.SchemaVersionBeforeMigration;

            if (!File.Exists("accounts.db"))
                return;
            
            // Backup old config file.
            long ts = DateTime.UtcNow.ToUnixTime(); // Add timestamp to avoid file conflicts
            if (File.Exists("accounts.db"))
            {
                string backupPath = $"accounts-{schemaVersion}-{ts}.backup.db";
                Logging.Logger.Write($"Backing up accounts.db to: {backupPath}", LogLevel.Info);
            
                File.Copy("accounts.db", backupPath);
            }
            // Add future schema migrations below.
            int version;
            for (version = schemaVersion; version < UpdateConfig.CURRENT_SCHEMA_VERSION; version++) 
            {
                Logging.Logger.Write($"Migrating accounts.db from schema version {version} to {version + 1}", LogLevel.Info);
                switch (version)
                {
                    case 19:
                        // Just delete the accounts.db so it gets regenerated from scratch.
                        File.Delete("accounts.db");
                        
                        break;

                    case 24:
                        //File.Delete("accounts.db");
                        MigrateLiteDbToSqLite();
                        break;
                }
            }
        }

        private void MigrateLiteDbToSqLite()
        {
            using (var db = new DatabaseConfigContext())
            {
                // Delete all accounts
                db.Account.RemoveRange(db.Account);
                db.SaveChanges();

                using (var liteDb = new LiteDatabase(ACCOUNT_DB_NAME))
                {
                    var liteDbAccounts = liteDb.GetCollection<BotAccount>("accounts");
                    foreach (var liteDbAccount in liteDbAccounts.FindAll())
                    {
                        Account newAccount = new Account();
                        newAccount.AuthType = (long)liteDbAccount.AuthType;
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
                        db.Account.Add(newAccount);
                        db.SaveChanges();
                    }
                }
            }
        }

        private void SyncDatabase(List<AuthConfig> accounts)
        {
            if (accounts.Count() == 0)
                return;

            using (var db = new DatabaseConfigContext())
            {
                // Add new accounts and update existing accounts.
                foreach (var item in accounts)
                {
                    var existing = db.Account.FirstOrDefault(x => x.Username == item.Username && x.AuthType == (long)item.AuthType);

                    if (existing == null)
                    {
                        try
                        {
                            Account newAcc = new Account(item);
                            db.Account.Add(newAcc);
                            db.SaveChanges();
                        }
                        catch(Exception)
                        {
                            Logic.Logging.Logger.Write("Error while saving data into accounts.db, please delete account.db and restart bot to have it fully work in order");
                        }
                    }
                    else
                    {
                        // Update credentials in database using values from json.
                        existing.Username = item.Username;
                        existing.Password = item.Password;
                        UpdateDatabase(db, existing);
                    }
                }

                // Remove accounts that are not in the auth.json but in the database.
                List<Account> accountsToRemove = new List<Account>();
                foreach (var item in db.Account)
                {
                    var existing = accounts.FirstOrDefault(x => x.Username == item.Username && (long)x.AuthType == item.AuthType);
                    if (existing == null)
                    {
                        accountsToRemove.Add(item);
                    }
                }

                foreach (var item in accountsToRemove)
                {
                    db.Account.Remove(item);
                    db.SaveChanges();
                }
            }
        }

        internal Account GetMinRuntime(DatabaseConfigContext db, bool ignoreBlockCheck = false)
        {
            return db.Account.OrderBy(x => x.RuntimeTotal).Where(x => !ignoreBlockCheck || (x.ReleaseBlockTime.HasValue && x.ReleaseBlockTime.Value < DateTime.Now.ToUnixTime())).FirstOrDefault();
        }

        public bool AllowMultipleBot()
        {
            using (var db = new DatabaseConfigContext())
            {
                return db.Account.Count() > 1;
            }
        }

        public Account GetStartUpAccount()
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            Account startupAccount;

            using (var db = new DatabaseConfigContext())
            {
                if (!AllowMultipleBot())
                {
                    startupAccount = db.Account.Last();
                }
                else
                {
                    startupAccount = GetMinRuntime(db, true);
                }
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
            using (var db = new DatabaseConfigContext())
            {
                var currentAccount = GetCurrentAccount(db);

                // If the bot to switch to is the same as the current bot then just return.
                if (bot == currentAccount)
                    return bot;

                if (bot != null)
                    return bot;

                var runnableAccount = db.Account.OrderByDescending(p => p.RuntimeTotal).ThenBy(p => p.Id).LastOrDefault(p => p != currentAccount && (!p.ReleaseBlockTime.HasValue || p.ReleaseBlockTime.HasValue && p.ReleaseBlockTime.Value < DateTime.Now.ToUnixTime()));

                if (runnableAccount != null)
                    return runnableAccount;

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
        public void UpdateDatabase(DatabaseConfigContext db, Account current)
        {
            current.RaisePropertyChanged("Nickname");
            current.RaisePropertyChanged("RuntimeTotal");
            current.RaisePropertyChanged("IsRunning");
            current.RaisePropertyChanged("Level");
            current.RaisePropertyChanged("LastLogin");
            current.RaisePropertyChanged("LastLoginTimestamp");
            current.RaisePropertyChanged("Level");
            current.RaisePropertyChanged("Stardust");
            current.RaisePropertyChanged("CurrentXp");
            current.RaisePropertyChanged("NextLevelXp");
            current.RaisePropertyChanged("PrevLevelXp");
            current.RaisePropertyChanged("ExperienceInfo");

            db.SaveChanges();
        }
        
        public void DumpAccountList()
        {
            using (var db = new DatabaseConfigContext())
            {
                foreach (var item in db.Account)
                {
                    if (item.Level > 0)
                        Logging.Logger.Write($"{item.Username} (Level: {item.Level})\t\t\tRuntime : {item.GetRuntime()}");
                    else
                        Logging.Logger.Write($"{item.Username} (Level: ??)\t\t\tRuntime : {item.GetRuntime()}");
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
            using (var db = new DatabaseConfigContext())
            {
                var account = GetCurrentAccount(db);
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

                UpdateDatabase(db, account);
            }
        }
    }
}
