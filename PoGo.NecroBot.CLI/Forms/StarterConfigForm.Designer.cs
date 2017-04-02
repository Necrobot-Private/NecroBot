namespace PoGo.NecroBot.CLI.Forms
{
    partial class StarterConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StarterConfigForm));
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.SelectLanguagePage = new AeroWizard.WizardPage();
            this.txtLanguage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AccountPage = new AeroWizard.WizardPage();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.LocationPage = new AeroWizard.WizardPage();
            this.txtLng = new System.Windows.Forms.TextBox();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.WalkinSpeedPage = new AeroWizard.WizardPage();
            this.txtMapzenKey = new System.Windows.Forms.TextBox();
            this.chkMazen = new System.Windows.Forms.CheckBox();
            this.chkAllowYourwalk = new System.Windows.Forms.CheckBox();
            this.txtGoogleKey = new System.Windows.Forms.TextBox();
            this.chkEnableGoogle = new System.Windows.Forms.CheckBox();
            this.chkAllowVariant = new System.Windows.Forms.CheckBox();
            this.txtVariant = new System.Windows.Forms.TextBox();
            this.txtSpeed = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.WebSocketPage = new AeroWizard.WizardPage();
            this.txtWebsocketPort = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkAllowWebsocket = new System.Windows.Forms.CheckBox();
            this.PageFinish = new AeroWizard.WizardPage();
            this.button1 = new System.Windows.Forms.Button();
            this.SnipePage = new AeroWizard.WizardPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label11 = new System.Windows.Forms.Label();
            this.txtMinLevel = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtMinIV = new System.Windows.Forms.TextBox();
            this.chkSnipeDex = new System.Windows.Forms.CheckBox();
            this.chkEnableSnipe = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.SelectLanguagePage.SuspendLayout();
            this.AccountPage.SuspendLayout();
            this.LocationPage.SuspendLayout();
            this.WalkinSpeedPage.SuspendLayout();
            this.WebSocketPage.SuspendLayout();
            this.PageFinish.SuspendLayout();
            this.SnipePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.BackColor = System.Drawing.Color.White;
            this.wizardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Pages.Add(this.SelectLanguagePage);
            this.wizardControl1.Pages.Add(this.AccountPage);
            this.wizardControl1.Pages.Add(this.LocationPage);
            this.wizardControl1.Pages.Add(this.WalkinSpeedPage);
            this.wizardControl1.Pages.Add(this.WebSocketPage);
            this.wizardControl1.Pages.Add(this.SnipePage);
            this.wizardControl1.Pages.Add(this.PageFinish);
            this.wizardControl1.Size = new System.Drawing.Size(549, 344);
            this.wizardControl1.TabIndex = 0;
            this.wizardControl1.Text = "Initial first time config";
            this.wizardControl1.Title = "Initial first time config";
            this.wizardControl1.Cancelling += new System.ComponentModel.CancelEventHandler(this.WizardControl1_Cancelling);
            this.wizardControl1.Finished += new System.EventHandler(this.WizardControl1_Finished);
            // 
            // SelectLanguagePage
            // 
            this.SelectLanguagePage.Controls.Add(this.txtLanguage);
            this.SelectLanguagePage.Controls.Add(this.label1);
            this.SelectLanguagePage.Name = "SelectLanguagePage";
            this.SelectLanguagePage.NextPage = this.AccountPage;
            this.SelectLanguagePage.Size = new System.Drawing.Size(502, 190);
            this.SelectLanguagePage.TabIndex = 0;
            this.SelectLanguagePage.Text = "Welcome to NECROBOT2";
            this.SelectLanguagePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.SelectLanguagePage_Commit);
            this.SelectLanguagePage.Click += new System.EventHandler(this.SelectLanguagePage_Click);
            // 
            // txtLanguage
            // 
            this.txtLanguage.Location = new System.Drawing.Point(222, 13);
            this.txtLanguage.Name = "txtLanguage";
            this.txtLanguage.Size = new System.Drawing.Size(100, 23);
            this.txtLanguage.TabIndex = 1;
            this.txtLanguage.Text = "en";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select your prefer language";
            // 
            // AccountPage
            // 
            this.AccountPage.Controls.Add(this.txtPassword);
            this.AccountPage.Controls.Add(this.txtUsername);
            this.AccountPage.Controls.Add(this.label4);
            this.AccountPage.Controls.Add(this.label3);
            this.AccountPage.Controls.Add(this.label2);
            this.AccountPage.Controls.Add(this.comboBox1);
            this.AccountPage.Name = "AccountPage";
            this.AccountPage.Size = new System.Drawing.Size(502, 190);
            this.AccountPage.TabIndex = 1;
            this.AccountPage.Text = "Account setup";
            this.AccountPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.WizardPage2_Commit);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(134, 102);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(162, 23);
            this.txtPassword.TabIndex = 5;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(134, 63);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(162, 23);
            this.txtUsername.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Account Type";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "google",
            "ptc"});
            this.comboBox1.Location = new System.Drawing.Point(134, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(162, 23);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.Text = "ptc";
            // 
            // LocationPage
            // 
            this.LocationPage.Controls.Add(this.txtLng);
            this.LocationPage.Controls.Add(this.txtLat);
            this.LocationPage.Controls.Add(this.label5);
            this.LocationPage.Controls.Add(this.label6);
            this.LocationPage.Name = "LocationPage";
            this.LocationPage.Size = new System.Drawing.Size(502, 190);
            this.LocationPage.TabIndex = 3;
            this.LocationPage.Text = "Start location";
            this.LocationPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.LocationPage_Commit);
            this.LocationPage.Click += new System.EventHandler(this.WizardPage4_Click);
            // 
            // txtLng
            // 
            this.txtLng.Location = new System.Drawing.Point(125, 53);
            this.txtLng.Name = "txtLng";
            this.txtLng.Size = new System.Drawing.Size(162, 23);
            this.txtLng.TabIndex = 13;
            // 
            // txtLat
            // 
            this.txtLat.Location = new System.Drawing.Point(125, 17);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(162, 23);
            this.txtLat.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Longitude";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "Latitude";
            // 
            // WalkinSpeedPage
            // 
            this.WalkinSpeedPage.Controls.Add(this.txtMapzenKey);
            this.WalkinSpeedPage.Controls.Add(this.chkMazen);
            this.WalkinSpeedPage.Controls.Add(this.chkAllowYourwalk);
            this.WalkinSpeedPage.Controls.Add(this.txtGoogleKey);
            this.WalkinSpeedPage.Controls.Add(this.chkEnableGoogle);
            this.WalkinSpeedPage.Controls.Add(this.chkAllowVariant);
            this.WalkinSpeedPage.Controls.Add(this.txtVariant);
            this.WalkinSpeedPage.Controls.Add(this.txtSpeed);
            this.WalkinSpeedPage.Controls.Add(this.label7);
            this.WalkinSpeedPage.Controls.Add(this.label8);
            this.WalkinSpeedPage.Name = "WalkinSpeedPage";
            this.WalkinSpeedPage.Size = new System.Drawing.Size(502, 190);
            this.WalkinSpeedPage.TabIndex = 4;
            this.WalkinSpeedPage.Text = "Walking setting";
            this.WalkinSpeedPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.WalkinSpeedPage_Commit);
            this.WalkinSpeedPage.Click += new System.EventHandler(this.WalkinSpeedPage_Click);
            // 
            // txtMapzenKey
            // 
            this.txtMapzenKey.Location = new System.Drawing.Point(20, 153);
            this.txtMapzenKey.Name = "txtMapzenKey";
            this.txtMapzenKey.Size = new System.Drawing.Size(462, 23);
            this.txtMapzenKey.TabIndex = 23;
            // 
            // chkMazen
            // 
            this.chkMazen.AutoSize = true;
            this.chkMazen.Checked = true;
            this.chkMazen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMazen.Location = new System.Drawing.Point(20, 134);
            this.chkMazen.Name = "chkMazen";
            this.chkMazen.Size = new System.Drawing.Size(267, 19);
            this.chkMazen.TabIndex = 22;
            this.chkMazen.Text = "Allow Mapzen walk (enter mapzen key below)";
            this.chkMazen.UseVisualStyleBackColor = true;
            // 
            // chkAllowYourwalk
            // 
            this.chkAllowYourwalk.AutoSize = true;
            this.chkAllowYourwalk.Checked = true;
            this.chkAllowYourwalk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllowYourwalk.Location = new System.Drawing.Point(20, 111);
            this.chkAllowYourwalk.Name = "chkAllowYourwalk";
            this.chkAllowYourwalk.Size = new System.Drawing.Size(115, 19);
            this.chkAllowYourwalk.TabIndex = 21;
            this.chkAllowYourwalk.Text = "Allow yours walk";
            this.chkAllowYourwalk.UseVisualStyleBackColor = true;
            // 
            // txtGoogleKey
            // 
            this.txtGoogleKey.Location = new System.Drawing.Point(20, 82);
            this.txtGoogleKey.Name = "txtGoogleKey";
            this.txtGoogleKey.Size = new System.Drawing.Size(462, 23);
            this.txtGoogleKey.TabIndex = 20;
            // 
            // chkEnableGoogle
            // 
            this.chkEnableGoogle.AutoSize = true;
            this.chkEnableGoogle.Checked = true;
            this.chkEnableGoogle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableGoogle.Location = new System.Drawing.Point(20, 57);
            this.chkEnableGoogle.Name = "chkEnableGoogle";
            this.chkEnableGoogle.Size = new System.Drawing.Size(257, 19);
            this.chkEnableGoogle.TabIndex = 19;
            this.chkEnableGoogle.Text = "Allow google walk (Enter google key below)";
            this.chkEnableGoogle.UseVisualStyleBackColor = true;
            // 
            // chkAllowVariant
            // 
            this.chkAllowVariant.AutoSize = true;
            this.chkAllowVariant.Checked = true;
            this.chkAllowVariant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllowVariant.Location = new System.Drawing.Point(20, 32);
            this.chkAllowVariant.Name = "chkAllowVariant";
            this.chkAllowVariant.Size = new System.Drawing.Size(156, 19);
            this.chkAllowVariant.TabIndex = 18;
            this.chkAllowVariant.Text = "Allow walk speed variant";
            this.chkAllowVariant.UseVisualStyleBackColor = true;
            this.chkAllowVariant.Click += new System.EventHandler(this.ChkAllowVariant_Click);
            // 
            // txtVariant
            // 
            this.txtVariant.Location = new System.Drawing.Point(345, 6);
            this.txtVariant.Name = "txtVariant";
            this.txtVariant.Size = new System.Drawing.Size(84, 23);
            this.txtVariant.TabIndex = 17;
            this.txtVariant.Text = "0";
            // 
            // txtSpeed
            // 
            this.txtSpeed.Location = new System.Drawing.Point(91, 5);
            this.txtSpeed.Name = "txtSpeed";
            this.txtSpeed.Size = new System.Drawing.Size(162, 23);
            this.txtSpeed.TabIndex = 16;
            this.txtSpeed.Text = "30";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(280, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 15);
            this.label7.TabIndex = 15;
            this.label7.Text = "Variant";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 8);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 15);
            this.label8.TabIndex = 14;
            this.label8.Text = "Walk Speed";
            // 
            // WebSocketPage
            // 
            this.WebSocketPage.Controls.Add(this.txtWebsocketPort);
            this.WebSocketPage.Controls.Add(this.label9);
            this.WebSocketPage.Controls.Add(this.chkAllowWebsocket);
            this.WebSocketPage.Name = "WebSocketPage";
            this.WebSocketPage.NextPage = this.SnipePage;
            this.WebSocketPage.Size = new System.Drawing.Size(502, 190);
            this.WebSocketPage.TabIndex = 2;
            this.WebSocketPage.Text = "Websocket";
            this.WebSocketPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.WebSocketPage_Commit);
            this.WebSocketPage.Click += new System.EventHandler(this.WebSocketPage_Click);
            // 
            // txtWebsocketPort
            // 
            this.txtWebsocketPort.Location = new System.Drawing.Point(152, 72);
            this.txtWebsocketPort.Name = "txtWebsocketPort";
            this.txtWebsocketPort.Size = new System.Drawing.Size(162, 23);
            this.txtWebsocketPort.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(45, 76);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 15);
            this.label9.TabIndex = 17;
            this.label9.Text = "Web Socket Port";
            // 
            // chkAllowWebsocket
            // 
            this.chkAllowWebsocket.AutoSize = true;
            this.chkAllowWebsocket.Location = new System.Drawing.Point(48, 29);
            this.chkAllowWebsocket.Name = "chkAllowWebsocket";
            this.chkAllowWebsocket.Size = new System.Drawing.Size(211, 19);
            this.chkAllowWebsocket.TabIndex = 0;
            this.chkAllowWebsocket.Text = "Allow websocket (use for Web GUI)";
            this.chkAllowWebsocket.UseVisualStyleBackColor = true;
            this.chkAllowWebsocket.Click += new System.EventHandler(this.CheckBox1_Click);
            // 
            // PageFinish
            // 
            this.PageFinish.AllowBack = false;
            this.PageFinish.AllowCancel = false;
            this.PageFinish.AllowNext = false;
            this.PageFinish.Controls.Add(this.button1);
            this.PageFinish.Name = "PageFinish";
            this.PageFinish.ShowCancel = false;
            this.PageFinish.ShowNext = false;
            this.PageFinish.Size = new System.Drawing.Size(502, 190);
            this.PageFinish.TabIndex = 6;
            this.PageFinish.Text = "You are all set!";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(168, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 52);
            this.button1.TabIndex = 0;
            this.button1.Text = "START BOT";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // SnipePage
            // 
            this.SnipePage.Controls.Add(this.linkLabel1);
            this.SnipePage.Controls.Add(this.label11);
            this.SnipePage.Controls.Add(this.txtMinLevel);
            this.SnipePage.Controls.Add(this.label10);
            this.SnipePage.Controls.Add(this.txtMinIV);
            this.SnipePage.Controls.Add(this.chkSnipeDex);
            this.SnipePage.Controls.Add(this.chkEnableSnipe);
            this.SnipePage.Name = "SnipePage";
            this.SnipePage.NextPage = this.PageFinish;
            this.SnipePage.Size = new System.Drawing.Size(502, 190);
            this.SnipePage.TabIndex = 5;
            this.SnipePage.Text = "Snipe setting";
            this.SnipePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.SnipePage_Commit);
            this.SnipePage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.SnipePage_Initialize);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Tomato;
            this.linkLabel1.LinkColor = System.Drawing.Color.White;
            this.linkLabel1.Location = new System.Drawing.Point(35, 124);
            this.linkLabel1.MaximumSize = new System.Drawing.Size(450, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(426, 30);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "These are very basic initial snipe config, please make sure you read \"Auto Snipe " +
    "Config\" to fully customize your snipe setting";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(246, 83);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 15);
            this.label11.TabIndex = 5;
            this.label11.Text = "Min Level";
            // 
            // txtMinLevel
            // 
            this.txtMinLevel.Location = new System.Drawing.Point(331, 80);
            this.txtMinLevel.Name = "txtMinLevel";
            this.txtMinLevel.Size = new System.Drawing.Size(100, 23);
            this.txtMinLevel.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(35, 81);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 15);
            this.label10.TabIndex = 3;
            this.label10.Text = "Min IV% Snipe";
            // 
            // txtMinIV
            // 
            this.txtMinIV.Location = new System.Drawing.Point(126, 79);
            this.txtMinIV.Name = "txtMinIV";
            this.txtMinIV.Size = new System.Drawing.Size(100, 23);
            this.txtMinIV.TabIndex = 2;
            // 
            // chkSnipeDex
            // 
            this.chkSnipeDex.AutoSize = true;
            this.chkSnipeDex.Location = new System.Drawing.Point(35, 45);
            this.chkSnipeDex.Name = "chkSnipeDex";
            this.chkSnipeDex.Size = new System.Drawing.Size(186, 19);
            this.chkSnipeDex.TabIndex = 1;
            this.chkSnipeDex.Text = "Snipe All Pokemon Not in DEX";
            this.chkSnipeDex.UseVisualStyleBackColor = true;
            // 
            // chkEnableSnipe
            // 
            this.chkEnableSnipe.AutoSize = true;
            this.chkEnableSnipe.Location = new System.Drawing.Point(35, 18);
            this.chkEnableSnipe.Name = "chkEnableSnipe";
            this.chkEnableSnipe.Size = new System.Drawing.Size(122, 19);
            this.chkEnableSnipe.TabIndex = 0;
            this.chkEnableSnipe.Text = "Enable Auto Snipe";
            this.chkEnableSnipe.UseVisualStyleBackColor = true;
            // 
            // StarterConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 344);
            this.ControlBox = false;
            this.Controls.Add(this.wizardControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StarterConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Initial Necrobot2 Config";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.SelectLanguagePage.ResumeLayout(false);
            this.SelectLanguagePage.PerformLayout();
            this.AccountPage.ResumeLayout(false);
            this.AccountPage.PerformLayout();
            this.LocationPage.ResumeLayout(false);
            this.LocationPage.PerformLayout();
            this.WalkinSpeedPage.ResumeLayout(false);
            this.WalkinSpeedPage.PerformLayout();
            this.WebSocketPage.ResumeLayout(false);
            this.WebSocketPage.PerformLayout();
            this.PageFinish.ResumeLayout(false);
            this.SnipePage.ResumeLayout(false);
            this.SnipePage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage SelectLanguagePage;
        private System.Windows.Forms.Label label1;
        private AeroWizard.WizardPage AccountPage;
        private AeroWizard.WizardPage WebSocketPage;
        private System.Windows.Forms.TextBox txtLanguage;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private AeroWizard.WizardPage LocationPage;
        private System.Windows.Forms.TextBox txtLng;
        private System.Windows.Forms.TextBox txtLat;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private AeroWizard.WizardPage WalkinSpeedPage;
        private System.Windows.Forms.TextBox txtVariant;
        private System.Windows.Forms.TextBox txtSpeed;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtWebsocketPort;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkAllowWebsocket;
        private System.Windows.Forms.CheckBox chkAllowVariant;
        private AeroWizard.WizardPage PageFinish;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtMapzenKey;
        private System.Windows.Forms.CheckBox chkMazen;
        private System.Windows.Forms.CheckBox chkAllowYourwalk;
        private System.Windows.Forms.TextBox txtGoogleKey;
        private System.Windows.Forms.CheckBox chkEnableGoogle;
        private AeroWizard.WizardPage SnipePage;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtMinLevel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtMinIV;
        private System.Windows.Forms.CheckBox chkSnipeDex;
        private System.Windows.Forms.CheckBox chkEnableSnipe;
    }
}