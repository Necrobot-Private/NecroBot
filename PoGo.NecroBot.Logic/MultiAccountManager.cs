using LiteDB;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
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
                AuthType = item.AuthType;
                Password = item.Password;
                Username = item.Username;
            }
                        
            // AutoId will be automatically incremented.
            public int Id { get; set; }
            public string Nickname { get; set; }
            public DateTime LoggedTime { get; set; }
            public int Level { get; set; }
            public string LastLogin { get; set; }
            public long LastLoginTimestamp { get; set; }
            public int Stardust { get; set; }
            public long CurrentXp { get; set; }
            public long PrevLevelXp { get; set; }
            public long NextLevelXp { get; set; }

            [BsonIgnore]
            public string ExperienceInfo
            {
                get
                {
                    int percentComplete = 0;
                    double xp = CurrentXp - PrevLevelXp;
                    double levelXp = NextLevelXp - PrevLevelXp;

                    if (levelXp > 0)
                        percentComplete = (int)Math.Floor(xp / levelXp * 100);
                    return $"{xp}/{levelXp} ({percentComplete}%)";
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public string GetRuntime()
            {
                var seconds = (int)((double)RuntimeTotal * 60);
                var duration = new TimeSpan(0, 0, seconds);

                return duration.ToString(@"dd\:hh\:mm\:ss");
            }
        }
		public List<BotAccount> Accounts { get; set; }
        public object Settings { get; private set; }
        private GlobalSettings _globalSettings { get; set; }
        public MultiAccountManager(GlobalSettings globalSettings, List<AuthConfig> accounts)
        {
            _globalSettings = globalSettings;
            MigrateDatabase();
            LoadDataFromDB();
            SyncDatabase(accounts);
        }

        public BotAccount GetCurrentAccount()
        {
            return Accounts.FirstOrDefault(x => x.IsRunning == true);
        }

        public void SwitchAccounts(BotAccount newAccount)
        {
            if (newAccount == null)
                return;
            
            var runningAccount = GetCurrentAccount();
            if (runningAccount != null)
            {
                runningAccount.IsRunning = false;
                var now = DateTime.Now;
                runningAccount.RuntimeTotal += (now - runningAccount.LastRuntimeUpdatedAt).TotalMinutes;
                runningAccount.LastRuntimeUpdatedAt = now;
                UpdateDatabase(runningAccount);
            }

            newAccount.IsRunning = true;
            newAccount.LoggedTime = DateTime.Now;
            newAccount.LastRuntimeUpdatedAt = newAccount.LoggedTime;
            UpdateDatabase(newAccount);

            // Update current auth config with new account.
            _globalSettings.Auth.CurrentAuthConfig = newAccount;

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
                currentAccount.ReleaseBlockTime = DateTime.Now.AddMinutes(expired);
                UpdateDatabase(currentAccount);
            }
        }

        private void LoadDataFromDB()
        {
            LiteEngine.Upgrade(ACCOUNT_DB_NAME, null, true);
            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");
                accountdb.EnsureIndex(x => x.Username, true);
                accountdb.EnsureIndex(x => x.IsRunning, false);

				Accounts = accountdb.FindAll().OrderBy(p => p.Id).ToList();
				
                foreach (var item in Accounts)
                {
                    item.IsRunning = false;
                    item.RuntimeTotal = 0;
                    UpdateDatabase(item);
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
                }
            }
        }

        private void SyncDatabase(List<AuthConfig> accounts)
        {
            if (accounts.Count() == 0)
                return;

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                // Add new accounts and update existing accounts.
                foreach (var item in accounts)
                {
                    var existing = Accounts.FirstOrDefault(x => x.Username == item.Username && x.AuthType == item.AuthType);

                    if (existing == null)
                    {
                        try
                        {
                            BotAccount newAcc = new BotAccount(item);
                            accountdb.Insert(newAcc);
                            Accounts.Add(newAcc);
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
                        UpdateDatabase(existing);
                    }
                }

                // Remove accounts that are not in the auth.json but in the database.
                List<BotAccount> accountsToRemove = new List<BotAccount>();
                foreach (var item in Accounts)
                {
                    var existing = accounts.FirstOrDefault(x => x.Username == item.Username && x.AuthType == item.AuthType);
                    if (existing == null)
                    {
                        accountsToRemove.Add(item);
                    }
                }

                foreach (var item in accountsToRemove)
                {
                    Accounts.Remove(item);
                    accountdb.Delete(item.Id);
                }
            }
        }

        internal BotAccount GetMinRuntime(bool ignoreBlockCheck = false)
        {
            return Accounts.OrderBy(x => x.RuntimeTotal).Where(x => !ignoreBlockCheck || x.ReleaseBlockTime < DateTime.Now).FirstOrDefault();
        }

        public bool AllowMultipleBot()
        {
            return Accounts.Count > 1;
        }

        public BotAccount GetStartUpAccount()
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            BotAccount startupAccount;

            if (!AllowMultipleBot())
            {
                startupAccount = Accounts.Last();
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

        public BotAccount GetSwitchableAccount(BotAccount bot = null, bool pauseIfNoSwitchableAccount = true)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();
            var currentAccount = GetCurrentAccount();

            // If the bot to switch to is the same as the current bot then just return.
            if (bot == currentAccount)
                return bot;
            
            if (bot != null)
                return bot;

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");
                
                var runnableAccount = Accounts.OrderByDescending(p => p.RuntimeTotal).ThenBy(p => p.Id).LastOrDefault(p => p != currentAccount && p.ReleaseBlockTime < DateTime.Now);

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
        public void SwitchAccountTo(BotAccount account)
        {
            requestedAccount = account;
            switchAccountRequest = true;
        }

        public void ThrowIfSwitchAccountRequested()
        {
            if (switchAccountRequest && requestedAccount != null && !requestedAccount.IsRunning)
            {
                switchAccountRequest = false;
                throw new ActiveSwitchAccountManualException(requestedAccount);
            }
        }

        private BotAccount requestedAccount = null;
        public void UpdateDatabase(BotAccount current)
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

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");
                accountdb.Update(current);
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

        public BotAccount FindAvailableAccountForPokemonSwitch(string encounterId)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();

            //set current 
            BotAccount switchableAccount = GetSwitchableAccount(null, false); // Don't pause if no switchable account is available.
            if (switchableAccount != null)
            {
                var key = switchableAccount.Username + encounterId;
                if (session.Cache.GetCacheItem(key) == null)
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
            account.RuntimeTotal += (now - account.LastRuntimeUpdatedAt).TotalMinutes;
            account.LastRuntimeUpdatedAt = now;

            UpdateDatabase(account);
        }
    }
}
