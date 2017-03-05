using LiteDB;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data.Player;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinyIoC;
using static PoGo.NecroBot.Logic.Utils.PushNotificationClient;

namespace PoGo.NecroBot.Logic
{
    public class MultiAccountManager
    {
        private const string ACCOUNT_DB_NAME = "accounts.db";

        public class BotAccount : AuthConfig , INotifyPropertyChanged
        {
            public bool IsRunning { get; set; }
            public BotAccount() { }
            public BotAccount(AuthConfig item)
            {
                this.AuthType = item.AuthType;
                this.Password = item.Password;
                this.Username = item.Username;
            }
                        
            // AutoId will be automatically incremented.
            public int Id { get; set; }
            public DateTime LoggedTime { get; set; }
            public int Level { get; set; }
            public string LastLogin { get; set; }
            public long LastLoginTimestamp { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }

            public string GetRuntime()
            {
                int day = (int)this.RuntimeTotal / 1440;
                int hour = (int)(this.RuntimeTotal - (day * 1400)) / 60;
                int min = (int)(this.RuntimeTotal - (day * 1400) - hour * 60);
                return $"{day:00}:{hour:00}:{min:00}:00";
            }
        }
        public List<BotAccount> Accounts { get; set; }
        public object Settings { get; private set; }

        public MultiAccountManager(List<AuthConfig> accounts)
        {
            MigrateDatabase();
            LoadDataFromDB();
            SyncDatabase(accounts, true /* remove missing accounts */);
        }

        public BotAccount GetCurrentAccount()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var currentAccount = this.Accounts.FirstOrDefault(
                x => x.AuthType == session.Settings.AuthType && x.Username == session.Settings.Username);
            return currentAccount;
        }

        public void BlockCurrentBot(int expired = 60)
        {
            var currentAccount = GetCurrentAccount();

            if (currentAccount != null)
            {
                currentAccount.ReleaseBlockTime = DateTime.Now.AddMinutes(expired);
                UpdateDatabase(currentAccount);
            }
        }

        private void LoadDataFromDB()
        {
            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                this.Accounts = accountdb.FindAll().ToList();

                foreach (var item in this.Accounts)
                {
                    item.IsRunning = false;
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
                        /*
                        using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
                        {
                            var accountdb = db.GetCollection<BotAccount>("accounts");
                            var accounts = accountdb.FindAll().ToList();

                            foreach (var item in accounts)
                            {
                                if (item.AuthType == PokemonGo.RocketAPI.Enums.AuthType.Google)
                                {
                                    if (!string.IsNullOrEmpty(item.GoogleUsername))
                                        item.Username = item.GoogleUsername;
                                    if (!string.IsNullOrEmpty(item.GooglePassword))
                                        item.Password = item.GooglePassword;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(item.PtcUsername))
                                        item.Username = item.PtcUsername;

                                    if (!string.IsNullOrEmpty(item.PtcPassword))
                                        item.Password = item.PtcPassword;
                                }

                                item.GoogleUsername = null;
                                item.GooglePassword = null;
                                item.PtcUsername = null;
                                item.PtcPassword = null;

                                UpdateDatabase(item);
                            }
                        }
                        */
                        break;
                }
            }
        }

        private void SyncDatabase(List<AuthConfig> accounts, bool removeMissingAccounts)
        {
            if (accounts.Count() == 0)
                return;

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                // Add new accounts and update existing accounts.
                foreach (var item in accounts)
                {
                    var existing = this.Accounts.FirstOrDefault(x => x.Username == item.Username && x.AuthType == item.AuthType);

                    if (existing == null)
                    {
                        try
                        {
                            BotAccount newAcc = new BotAccount(item);
                            accountdb.Insert(newAcc);
                            this.Accounts.Add(newAcc);
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
                        accountdb.Update(existing);
                    }
                }

                if (removeMissingAccounts)
                {
                    // Remove accounts that are not in the auth.json but in the database.
                    List<BotAccount> accountsToRemove = new List<BotAccount>();
                    foreach (var item in this.Accounts)
                    {
                        var existing = accounts.FirstOrDefault(x => x.Username == item.Username && x.AuthType == item.AuthType);
                        if (existing == null)
                        {
                            accountsToRemove.Add(item);
                        }
                    }

                    foreach (var item in accountsToRemove)
                    {
                        this.Accounts.Remove(item);
                        accountdb.Delete(item.Id);
                    }
                }
            }
        }

        internal BotAccount GetMinRuntime(bool ignoreBlockCheck= false)
        {
            return this.Accounts.OrderBy(x => x.RuntimeTotal).Where(x => !ignoreBlockCheck || x.ReleaseBlockTime < DateTime.Now).FirstOrDefault();
        }

        public BotAccount Add(AuthConfig authConfig)
        {
            SyncDatabase(new List<AuthConfig>() { authConfig }, false /* don't remove missing accounts */);

            return this.Accounts.Last();
        }

        public BotAccount GetStartUpAccount()
        {
            ISession session = TinyIoC.TinyIoCContainer.Current.Resolve<ISession>();

            if (!session.LogicSettings.AllowMultipleBot)
            {
                runningAccount = Accounts.Last();
            }
            else
            {
                runningAccount = GetMinRuntime(true);
            }

            if (session.LogicSettings.AllowMultipleBot
              && session.LogicSettings.MultipleBotConfig.SelectAccountOnStartUp)
            {
                SelectAccountForm f = new SelectAccountForm();
                f.ShowDialog();
                runningAccount  = f.SelectedAccount;
            }
            runningAccount.LoggedTime = DateTime.Now;
            return runningAccount;
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

        public BotAccount GetSwitchableAccount(BotAccount bot = null)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            var currentAccount = GetCurrentAccount();
            
            if (currentAccount != null)
            {
                Logic.Logging.Logger.Debug($"Current account {currentAccount.Username}");
                currentAccount.IsRunning = false;
                currentAccount.RuntimeTotal += (DateTime.Now - currentAccount.LoggedTime).TotalMinutes;
                
                UpdateDatabase(currentAccount);
            }

            if (bot != null)
            {
                runningAccount = bot;
                Logging.Logger.Write($"Switching to {runningAccount.Username}...");

                string body = "";
                foreach (var item in this.Accounts)
                {
                    int day = (int)item.RuntimeTotal / 1440;
                    int hour = (int)(item.RuntimeTotal - (day * 1400)) / 60;
                    int min = (int)(item.RuntimeTotal - (day * 1400) - hour * 60);
                    body = body + $"{item.GoogleUsername}{item.PtcUsername}     {item.GetRuntime()}\r\n";
                }

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
                SendNotification(session, $"Account changed to {runningAccount.Username}", body);
#pragma warning restore 4014
            }
            else {

                this.Accounts = this.Accounts.OrderByDescending(p => p.RuntimeTotal).ToList();
                var first = this.Accounts.First();
                if (first.RuntimeTotal >= 100000)
                {
                    first.RuntimeTotal = this.Accounts.Min(p => p.RuntimeTotal);
                }

                runningAccount = Accounts.LastOrDefault(p => p != currentAccount && p.ReleaseBlockTime < DateTime.Now);
                if (runningAccount != null)
                {
                    Logging.Logger.Write($"Switching to {runningAccount.Username}...");

                    string body = "";
                    foreach (var item in this.Accounts)
                    {
                        int day = (int)item.RuntimeTotal / 1440;
                        int hour = (int)(item.RuntimeTotal - (day * 1400)) / 60;
                        int min = (int)(item.RuntimeTotal - (day * 1400) - hour * 60);
                        body = body + $"{item.GoogleUsername}{item.PtcUsername}     {item.GetRuntime()}\r\n";
                    }

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
                    SendNotification(session, $"Account changed to {runningAccount.Username}", body);
#pragma warning restore 4014
                    //DumpAccountList();

                }
                else
                {

                    var pauseTime = session.LogicSettings.MultipleBotConfig.OnLimitPauseTimes;
#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
                    SendNotification(session, "All accounts are being blocked", $"None of yours account available to switch, bot will sleep for {pauseTime} mins until next acount available to run", true);
#pragma warning restore 4014

                    Task.Delay(pauseTime * 60 * 1000).Wait();
                    return GetSwitchableAccount();
                }
            }
            //overkill
            foreach (var item in this.Accounts)
            {
                item.IsRunning = false;
                UpdateDatabase(item);
            }
            runningAccount.IsRunning = true;
            runningAccount.LoggedTime = DateTime.Now;
            
            UpdateDatabase(runningAccount);

            return runningAccount;
        }

        private bool switchAccountRequest = false;
        public void SwitchAccountTo(BotAccount account)
        {
            this.requestedAccount = account;
            this.switchAccountRequest = true;
        }

        public void ThrowIfSwitchAccountRequested()
        {
            if (switchAccountRequest && this.requestedAccount != null && !this.requestedAccount.IsRunning)
            {
                switchAccountRequest = false;
                throw new ActiveSwitchAccountManualException(this.requestedAccount);
            }
        }

        private BotAccount requestedAccount = null;
        private BotAccount runningAccount;
        public void UpdateDatabase(BotAccount current)
        {
            current.RaisePropertyChanged("RuntimeTotal");
            current.RaisePropertyChanged("IsRunning");
            current.RaisePropertyChanged("Level");
            current.RaisePropertyChanged("LastLogin");
            current.RaisePropertyChanged("LastLoginTimestamp");

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");
                accountdb.Update(current);
            }
        }

        // This should be called whenever the inventory is updated (e.g. client.Inventory.OnInventoryUpdated)
        public void UpdateCurrentAccountLevel()
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            var playerStats = (session.Inventory.GetPlayerStats()).FirstOrDefault();
            if (playerStats != null)
            {
                var currentAccount = GetCurrentAccount();
                if (currentAccount != null)
                {
                    if (currentAccount.Level != playerStats.Level)
                    {
                        currentAccount.Level = playerStats.Level;
                        UpdateDatabase(currentAccount);
                    }
                }
            }
        }

        public void DumpAccountList()
        {
            foreach (var item in this.Accounts)
            {
                if (item.Level > 0)
                    Logging.Logger.Write($"{item.Username} (Level: {item.Level})\t\t\tRuntime : {item.GetRuntime()}");
                else
                    Logging.Logger.Write($"{item.Username} (Level: ??)\t\t\tRuntime : {item.GetRuntime()}");
            }
        }

        public BotAccount FindAvailableAccountForPokemonSwitch(string encounterId)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();

            //set current 
            foreach (var bot in this.Accounts.OrderByDescending(p => p.RuntimeTotal))
            {
                if (bot.ReleaseBlockTime > DateTime.Now) continue;
                var key = bot.Username;
                key += encounterId;
                if (session.Cache.GetCacheItem(key) == null)
                {
                    // Don't edit the running account until we actually switch.  Just return the pending account.
                    return bot;
                }
            }

            return null;
        }

        internal void Logged(GetPlayerResponse playerResponse, IEnumerable<PlayerStats> playerStats)
        {
            this.runningAccount.LoggedTime = DateTime.Now;
            this.runningAccount.Level = playerStats.FirstOrDefault().Level;
            UpdateDatabase(this.runningAccount);
        }
    }
}
