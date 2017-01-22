using LiteDB;
using PoGo.NecroBot.Logic.Forms;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoGo.NecroBot.Logic
{
    public class MultiAccountManager
    {
        private const string ACCOUNT_DB_NAME = "accounts.db";

        public class BotAccount: AuthConfig
        {

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

        }
        public List<BotAccount> Accounts { get; set; }

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

                    if(existing == null) {
                        BotAccount newAcc = new BotAccount(item);
                        newAcc.Id = this.Accounts.Count ==0? 1:this.Accounts.Max(x => x.Id) + 1;
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

        public void Add(AuthConfig authConfig)
        {
            SyncDatabase(new List<AuthConfig>() { authConfig });
        }

        public BotAccount GetStartUpAccount()
        {
            var bot = GetMinRuntime();
            ISession session = TinyIoC.TinyIoCContainer.Current.Resolve<ISession>();

            if (session.LogicSettings.AllowMultipleBot
              && session.LogicSettings.MultipleBotConfig.SelectAccountOnStartUp)
            {
                SelectAccountForm f = new SelectAccountForm();
                f.ShowDialog();
                bot = f.SelectedAccount;
            }

            return bot;

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
        
    }
}
