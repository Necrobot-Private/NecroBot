using System;
using System.Diagnostics;
using System.Windows.Forms;
using PoGo.NecroBot.Logic.Model.Settings;

namespace RocketBot2.Forms
{
    public partial class AuthAPIForm : Form
    {
        public APIConfig Config
        {
            get
            {
                return new APIConfig()
                {
                    UsePogoDevAPI = radHashServer.Checked,
                    UseCustomAPI = radCustomHash.Checked,
                    AuthAPIKey = txtAPIKey.Text.Trim(),
                    UrlHashServices = txtCustomHash.Text.Trim()
                };
            }
            set
            {
                radHashServer.Checked = value.UsePogoDevAPI;
                radCustomHash.Checked = value.UseCustomAPI;
                txtCustomHash.Enabled = value.UseCustomAPI;
            }
        }

        private bool forceInput;

        public AuthAPIForm(bool forceInput)
        {
            InitializeComponent();

            if (forceInput)
            {
                this.forceInput = forceInput;
                ControlBox = false;
                btnCancel.Visible = false;
            }
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                if (forceInput)
                {
                    myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                }

                return myCp;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (radHashServer.Checked && string.IsNullOrEmpty(txtAPIKey.Text))
            {
                MessageBox.Show("Please enter a valid API Key", "Missing API Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (radCustomHash.Checked && string.IsNullOrEmpty(txtAPIKey.Text))
            {
                MessageBox.Show("Please enter a valid API Key", "Missing API Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (radCustomHash.Checked && string.IsNullOrEmpty(txtCustomHash.Text))
            {
                MessageBox.Show("Please enter a valid Hash URL", "Missing Hash URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!radHashServer.Checked && !radCustomHash.Checked)
            {
                MessageBox.Show("Please select an API method", "Config error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void AuthAPIForm_Load(object sender, EventArgs e)
        {
        }
    }
}