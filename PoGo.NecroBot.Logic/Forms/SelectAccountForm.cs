using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Model;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Windows.Forms;

namespace PoGo.NecroBot.Logic.Forms
{
    public partial class SelectAccountForm : Form
    {
        public Account SelectedAccount { get; set; }
        public SelectAccountForm()
        {
            InitializeComponent();
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SelectAccountForm_Load(object sender, EventArgs e)
        {
            BringToFront();

            WindowState = FormWindowState.Minimized;
            Show();
            WindowState = FormWindowState.Normal;

            lvAcc.BeginUpdate();
            using (var db = new AccountConfigContext())
            {
                foreach (var item in db.Account)
                {
                    EXListViewItem lvItem = new EXListViewItem(item.AuthType.ToString());
                    lvItem.SubItems.Add(new EXControlListViewSubItem() { Text = item.Username });
                    lvItem.SubItems.Add(new EXControlListViewSubItem() { Text = item.GetRuntime() });
                    lvItem.SubItems.Add(new EXControlListViewSubItem() { Text = "" });

                    EXControlListViewSubItem cs = new EXControlListViewSubItem()
                    {
                    };
                    Button b = new Button()
                    {
                        Text = "START",
                        Height = 55
                    };
                    b.Click += SelectBot_Click;
                    b.Tag = item;
                    lvItem.SubItems.Add(cs);
                    lvAcc.AddControlToSubItem(b, cs);
                    lvAcc.Items.Add(lvItem);

                }
            }
            lvAcc.EndUpdate();
        }

        private void SelectBot_Click(object sender, EventArgs e)
        {
            SelectedAccount  = (Account)((Button)sender).Tag;
            Close();
        }

        private void SelectAccountForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SelectedAccount == null)
            {
                var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
                SelectedAccount = manager.GetMinRuntime();
                SelectedAccount.LoggedTime = DateTime.Now.ToUnixTime();
            }
        }

        int countdown = 31;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            countdown--;
            var translator = TinyIoC.TinyIoCContainer.Current.Resolve<ITranslation>();
            label1.Text = translator.GetTranslation(TranslationString.MultiAccountAutoSelect, countdown);
            if (countdown <= 0) Close();
        }

        private void Btnclose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
