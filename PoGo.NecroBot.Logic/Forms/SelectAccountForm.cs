using PoGo.NecroBot.Logic.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoGo.NecroBot.Logic.Forms
{
    public partial class SelectAccountForm : Form
    {
        public MultiAccountManager.BotAccount SelectedAccount { get; set; }
        public SelectAccountForm()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SelectAccountForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();

            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;


            lvAcc.BeginUpdate();
            var accManager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            foreach (var item in accManager.Accounts)
            {
                EXListViewItem lvItem = new EXListViewItem(item.AuthType.ToString());
                lvItem.SubItems.Add( new EXControlListViewSubItem() { Text = item.AppliedUsername });
                lvItem.SubItems.Add(new EXControlListViewSubItem() { Text = item.GetRuntime() });
                lvItem.SubItems.Add(new EXControlListViewSubItem() { Text = "" });

                EXControlListViewSubItem cs = new EXControlListViewSubItem()
                {
                };
                Button b = new Button();
                b.Text = "START";
                b.Height = 55;
                b.Click += selectBot_Click;
                b.Tag = item;
                lvItem.SubItems.Add(cs);
                lvAcc.AddControlToSubItem(b, cs);
                lvAcc.Items.Add(lvItem);

            }
            lvAcc.EndUpdate();
        }

        private void selectBot_Click(object sender, EventArgs e)
        {
            SelectedAccount  = (MultiAccountManager.BotAccount)((Button)sender).Tag;
            this.Close();
        }

        private void SelectAccountForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if(this.SelectedAccount == null)
            {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();

                SelectedAccount = manager.GetMinRuntime();
                SelectedAccount.LoggedTime = DateTime.Now;
            }
        }

        int countdown = 31;
        private void timer1_Tick(object sender, EventArgs e)
        {
            countdown--;
            var translator = TinyIoC.TinyIoCContainer.Current.Resolve<ITranslation>();
            this.label1.Text = translator.GetTranslation(TranslationString.MultiAccountAutoSelect, countdown);
            if (countdown <= 0) this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
