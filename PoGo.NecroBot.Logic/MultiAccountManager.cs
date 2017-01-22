using LiteDB;
using PoGo.NecroBot.Logic.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic
{
    public class MultiAccountManager
    {
        private const string ACCOUNT_DB_NAME = "accounts.db";

        public class BotAccount: AuthConfig
        {
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
        }
        }

        private void SyncDatabase(List<AuthConfig> accounts)
        {
            using (var db = new LiteDatabase(ACCOUNT_DB_NAME))
            {
                var accountdb = db.GetCollection<BotAccount>("accounts");

                foreach (var item in accounts)
                {
                    var existing = this.Accounts.FirstOrDefault(x => x.GoogleUsername == item.GoogleUsername || x.PtcUsername == item.PtcUsername);

                    if(existing == null) {
                        BotAccount newAcc = (BotAccount)item;
                        newAcc.Id = this.Accounts.Max(x => x.Id) + 1;
                        accountdb.Insert(newAcc);
                        this.Accounts.Add(newAcc); 
                    }
                }

            }
        }
    }
}
