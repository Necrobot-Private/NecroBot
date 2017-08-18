using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace PoGo.NecroBot.CLI.Forms
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();
        }

        private void InfoForm_Load(object sender, EventArgs e)
        {
            Text = $"NecroBot 2 - v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        }

        private void DonateLogo_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.mypogosnipers.com/?donate");
        }

        private void Link_click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(((LinkLabel) sender).Text);
        }

        private int count = 60;

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (count-- <= 0)
            {
                Close();
            }
        }

        private void NecroLogo_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NecroBot-Private/NecroBot");
        }
    }
}