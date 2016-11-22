#region using directives

using System;
using System.Windows.Forms;
using PoGo.NecroBot.FORM.Forms;

#endregion

namespace PoGo.NecroBot.FORM
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}