using LiteDB;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data.Player;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                this.GooglePassword = item.GooglePassword;
                this.GoogleUsername = item.GoogleUsername;
                this.PtcPassword = item.PtcPassword;
                this.PtcUsername = item.PtcUsername;
            }

            public string AppliedUsername => $"{PtcUsername}{GoogleUsername}";

            [BsonId]
            public int Id { get; set; }
            public DateTime LoggedTime { get; set; }
            public int Level { get; set; }

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
            LoadDataFromDB();
            SyncDatabase(accounts);

        }

        private void LoadDataFromDB()
        {
            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                this.Accounts = accountdb.FindAll().ToList();
                this.Accounts.RemoveAll(x => string.IsNullOrEmpty(x.PtcUsername) && string.IsNullOrEmpty(x.GoogleUsername));
                foreach (var item in this.Accounts)
                {
                    item.IsRunning = false;

                }
            }
        }

        private void SyncDatabase(List<AuthConfig> accounts)
        {
            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                foreach (var item in accounts)
                {
                    var existing = this.Accounts.FirstOrDefault(x => x.GoogleUsername == item.GoogleUsername && x.PtcUsername == item.PtcUsername);

                    if (existing == null)
                    {
                        BotAccount newAcc = new BotAccount(item);
                        newAcc.Id = this.Accounts.Count == 0 ? 1 : this.Accounts.Max(x => x.Id) + 1;
                        accountdb.Insert(newAcc);
                        this.Accounts.Add(newAcc);
                    }
                }

            }
        }

        internal BotAccount GetMinRuntime()
        {
            return this.Accounts.OrderBy(x => x.RuntimeTotal).Where(x => x.ReleaseBlockTime < DateTime.Now).FirstOrDefault();
        }

        public BotAccount Add(AuthConfig authConfig)
        {
            SyncDatabase(new List<AuthConfig>() { authConfig });

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
            runningAccount = GetMinRuntime();

            if (session.LogicSettings.AllowMultipleBot
              && session.LogicSettings.MultipleBotConfig.SelectAccountOnStartUp)
            {
                SelectAccountForm f = new SelectAccountForm();
                f.ShowDialog();
                runningAccount  = f.SelectedAccount;
            }
            runningAccount.LoggedTime = DateTime.Now;
            return runningAccount;

            //if (session.LogicSettings.AllowMultipleBot
            //   && session.LogicSettings.MultipleBotConfig.SelectAccountOnStartUp)
            //{
            //    byte index = 0;
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    Logic.Logging.Logger.Write("PLEASE SELECT AN ACCOUNT TO START. AUTO START AFTER 30 SEC");
            //    List<Char> availableOption = new List<char>();
            //    foreach (var item in this.Accounts)
            //    {
            //        var ch = (char)(index + 65);
            //        availableOption.Add(ch);
            //        int day = (int)item.RuntimeTotal / 1440;
            //        int hour = (int)(item.RuntimeTotal - (day * 1400)) / 60;
            //        int min = (int)(item.RuntimeTotal - (day * 1400) - hour * 60);

            //        var runtime = $"{day:00}:{hour:00}:{min:00}:00";

            //        Logic.Logging.Logger.Write($"{ch}. {item.GoogleUsername}{item.PtcUsername} \t\t{runtime}");
            //        index++;
            //    }

            //    char select = ' ';
            //    DateTime timeoutvalue = DateTime.Now.AddSeconds(30);

            //    while (DateTime.Now < timeoutvalue && !availableOption.Contains(select))
            //    {
            //        if (Console.KeyAvailable)
            //        {
            //            ConsoleKeyInfo cki = Console.ReadKey();
            //            select = cki.KeyChar;
            //            select = Char.ToUpper(select);
            //            if (!availableOption.Contains(select))
            //            {
            //                Console.Out.WriteLine("Please select an account from list");
            //            }
            //        }
            //        else
            //        {
            //            Thread.Sleep(100);
            //        }
            //    }

            //    if (availableOption.Contains(select))
            //    {
            //        bot = this.Accounts[select - 65];
            //    }
            //    else
            //    {
            //        bot = this.Accounts.OrderBy(p => p.RuntimeTotal).First();
            //    }
            //}
            //if(bot == null)
            //    this.Accounts.OrderBy(p => p.RuntimeTotal).First();

            //return bot;
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
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var currentAccount = this.Accounts.FirstOrDefault(
                x => (x.AuthType == AuthType.Ptc && x.PtcUsername == session.Settings.PtcUsername) ||
                     (x.AuthType == AuthType.Google && x.GoogleUsername == session.Settings.GoogleUsername));


            var current = this.Accounts.FirstOrDefault(x => x.IsRunning);
            if (current != null)
            {
                current.RuntimeTotal += (DateTime.Now - current.LoggedTime).TotalMinutes;
                current.IsRunning = false;

                var playerStats = (session.Inventory.GetPlayerStats()).FirstOrDefault();
                current.Level = playerStats.Level;

                UpdateDatabase(current);
            }

            if (bot != null)
            {
                
                runningAccount = bot;
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
                    Logging.Logger.Write($"Switching to {runningAccount.GoogleUsername}{runningAccount.PtcUsername}...");

                    string body = "";
                    foreach (var item in this.Accounts)
                    {
                        int day = (int)item.RuntimeTotal / 1440;
                        int hour = (int)(item.RuntimeTotal - (day * 1400)) / 60;
                        int min = (int)(item.RuntimeTotal - (day * 1400) - hour * 60);
                        body = body + $"{item.GoogleUsername}{item.PtcUsername}     {item.GetRuntime()}\r\n";
                    }

#pragma warning disable 4014 // added to get rid of compiler warning. Remove this if async code is used below.
                    SendNotification(session, $"Account changed to {runningAccount.GoogleUsername}{runningAccount.PtcUsername}", body);
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
            runningAccount.IsRunning = true;
            runningAccount.LoggedTime = DateTime.Now;
            
            UpdateDatabase(runningAccount);

            return runningAccount;
        }

        public void SwitchAccountTo(BotAccount account)
        {
            this.requestedAccount = account;
        }

        public void ThrowIfSwitchAccountRequested()
        {
            if (this.requestedAccount != null && !this.requestedAccount.IsRunning)
                throw new ActiveSwitchAccountManualException(this.requestedAccount);
        }

        private BotAccount requestedAccount = null;
        private BotAccount runningAccount;
        private void UpdateDatabase(BotAccount current)
        {
            current.RaisePropertyChanged("RuntimeTotal");
                current.RaisePropertyChanged("IsRunning");

            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");
                accountdb.Update(current);
            }
        }

        public void DumpAccountList()
        {
            foreach (var item in this.Accounts)
            {
                if (item.Level > 0)
                    Logging.Logger.Write($"{item.PtcUsername}{item.GoogleUsername}(Level: {item.Level})\t\t\tRuntime : {item.GetRuntime()}");
                else
                    Logging.Logger.Write($"{item.PtcUsername}{item.GoogleUsername}(Level: ??)\t\t\tRuntime : {item.GetRuntime()}");
            }
        }

        public BotAccount FindAvailableAccountForPokemonSwitch(string encounterId)
        {
            ISession session = TinyIoCContainer.Current.Resolve<ISession>();

            //set current 
            foreach (var bot in this.Accounts.OrderByDescending(p => p.RuntimeTotal))
            {
                if (bot.ReleaseBlockTime > DateTime.Now) continue;
                var key = bot.AuthType == AuthType.Google ? bot.GoogleUsername : bot.PtcUsername;
                key += encounterId;
                if (session.Cache.GetCacheItem(key) == null)
                {
                    runningAccount.RuntimeTotal += (DateTime.Now - runningAccount.LoggedTime).TotalMinutes;
                    runningAccount.IsRunning = false;

                    var playerStats = (session.Inventory.GetPlayerStats()).FirstOrDefault();
                    runningAccount.Level = playerStats.Level;

                    UpdateDatabase(runningAccount);

                    runningAccount = bot;
                    runningAccount.LoggedTime = DateTime.Now;
                    return runningAccount;
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
