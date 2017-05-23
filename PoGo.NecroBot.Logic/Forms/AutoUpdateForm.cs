using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.UI;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Markdig;
using System.Text.RegularExpressions;
using System.Web;

namespace PoGo.NecroBot.Logic.Forms
{

    public partial class AutoUpdateForm : Form
    {
        public string LatestVersion { get; set; }
        public string CurrentVersion { get; set; }
        public bool AutoUpdate { get; set; }
        public string DownloadLink { get; set; }
        public string ChangelogLink { get; set; }
        public string Destination { get; set; }
        public ISession Session { get; set; }

        public AutoUpdateForm()
        {
            InitializeComponent();
        }

        public static string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }

        private void AutoUpdateForm_Load(object sender, EventArgs e)
        {
            richTextBox1.SetInnerMargins(25, 25, 25, 25);
            lblCurrent.Text = $"v{CurrentVersion}";
            lblLatest.Text = $"v{LatestVersion}";
            var Client = new WebClient();
            var ChangelogRaw = Client.DownloadString(ChangelogLink);
            var ChangelogFormatted = StripHTML(Markdown.ToHtml(ChangelogRaw)).Replace("Full Changelog", "").Replace("Change Log", "");
            if (ChangelogFormatted.Length > 0)
            {
                richTextBox1.Text = ChangelogFormatted;
            }
            else
            {
                richTextBox1.Text = "No Changelog Detected...";
            }
            if (AutoUpdate)
            {
                btnUpdate.Enabled = false;
                lblMessage.Enabled = true;
                btnUpdate.Text = "Downloading...";
                StartDownload();
            }
        }

        public bool DownloadFile(string url, string dest)
        {
            Session.EventDispatcher.Send(new UpdateEvent
            {
                Message = Session.Translation.GetTranslation(TranslationString.DownloadingUpdate)
            });

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;

                    client.DownloadFileAsync(new Uri(url), dest);
                    Logger.Write(dest, LogLevel.Info);
                }
                catch
                {
                    Close();
                }
                return true;
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Session.EventDispatcher.Send(new UpdateEvent
            {
                Message = Session.Translation.GetTranslation(TranslationString.FinishedDownloadingRelease)
            });

            Invoke(new Action(() =>
            {
                DialogResult = DialogResult.OK;
                Close();
            }));
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                lblMessage.Text = $"Updating {Application.ProductName} from v{CurrentVersion} to v{LatestVersion} ({e.ProgressPercentage}% Completed)";
            }));
        }


        public void StartDownload()
        {
            Session.EventDispatcher.Send(new StatusBarEvent($"Updating to v{LatestVersion}, Downloading from {DownloadLink}"));
            Logger.Write(DownloadLink, LogLevel.Info);
            DownloadFile(DownloadLink, Destination);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Text = "Downloading...";
            btnUpdate.Enabled = false;
            StartDownload();
        }

        private void Btncancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
