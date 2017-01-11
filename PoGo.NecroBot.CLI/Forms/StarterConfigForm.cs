using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Common;

namespace PoGo.NecroBot.CLI.Forms
{
    public partial class StarterConfigForm : Form
    {
        private GlobalSettings settings;
        private IElevationService elevationService;
        private string configFile;

        public ISession Session { get; set; }

        public StarterConfigForm()
        {
            InitializeComponent();

        }

        public StarterConfigForm(Session _session)
        {
           
            InitializeComponent();
        }

        public StarterConfigForm(Session _session, GlobalSettings settings, IElevationService elevationService, string configFile) : this(_session)
        {
            this.Session = _session;
            this.txtLanguage.Text = this.Session.LogicSettings.TranslationLanguageCode;
            this.txtLat.Text = settings.LocationConfig.DefaultLatitude.ToString();
            this.txtLng.Text = settings.LocationConfig.DefaultLongitude.ToString();
            this.settings = settings;
            this.txtWebsocketPort.Text = settings.WebsocketsConfig.WebSocketPort.ToString();
            this.chkAllowWebsocket.Checked = settings.WebsocketsConfig.UseWebsocket;

            this.elevationService = elevationService;
            this.configFile = configFile;
        }

        private void wizardPage2_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            this.settings.Auth.AuthConfig.AuthType = comboBox1.Text == "ptc" ? PokemonGo.RocketAPI.Enums.AuthType.Ptc : PokemonGo.RocketAPI.Enums.AuthType.Google;
            if (this.settings.Auth.AuthConfig.AuthType == PokemonGo.RocketAPI.Enums.AuthType.Ptc)
            {
                this.settings.Auth.AuthConfig.PtcUsername = txtUsername.Text;
                this.settings.Auth.AuthConfig.PtcPassword = txtPassword.Text;
            }
            else {
                this.settings.Auth.AuthConfig.GoogleUsername = txtUsername.Text;
                this.settings.Auth.AuthConfig.GooglePassword = txtPassword.Text;
            }
        }

        private void SelectLanguagePage_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
        }

        private void wizardPage4_Click(object sender, EventArgs e)
        {
            this.settings.LocationConfig.DefaultLatitude = Convert.ToDouble(txtLat.Text);
            this.settings.LocationConfig.DefaultLongitude= Convert.ToDouble(txtLng.Text);
        }

        private void WalkinSpeedPage_Click(object sender, EventArgs e)
        {
            this.settings.LocationConfig.WalkingSpeedInKilometerPerHour = Convert.ToDouble(txtSpeed.Text);
            this.settings.LocationConfig.WalkingSpeedVariant = Convert.ToDouble(txtSpeed.Text);
            this.settings.LocationConfig.UseWalkingSpeedVariant = chkAllowVariant.Checked;
        }

        private void chkAllowVariant_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_Click(object sender, EventArgs e)
        {

        }

        private void WebSocketPage_Click(object sender, EventArgs e)
        {
            this.settings.WebsocketsConfig.UseWebsocket = chkAllowWebsocket.Checked;
            this.settings.WebsocketsConfig.WebSocketPort = Convert.ToInt32(txtWebsocketPort.Text);

        }

        private void wizardControl1_Finished(object sender, EventArgs e)
        {
            GlobalSettings.SaveFiles(settings, this.configFile);
            new Session(new ClientSettings(settings, elevationService), new LogicSettings(settings), elevationService);
            Logger.Write(Session.Translation.GetTranslation(TranslationString.FirstStartSetupCompleted), LogLevel.Info);

        }

        private void SelectLanguagePage_Click(object sender, EventArgs e)
        {
            this.Session = new Session(new ClientSettings(settings, elevationService), new LogicSettings(settings), elevationService);

            this.settings.ConsoleConfig.TranslationLanguageCode = this.txtLanguage.Text;

        }

        private void wizardControl1_Cancelling(object sender, CancelEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            GlobalSettings.SaveFiles(settings, this.configFile);
            new Session(new ClientSettings(settings, elevationService), new LogicSettings(settings), elevationService);
            Logger.Write(Session.Translation.GetTranslation(TranslationString.FirstStartSetupCompleted), LogLevel.Info);


            this.Close();
        }
    }
}
