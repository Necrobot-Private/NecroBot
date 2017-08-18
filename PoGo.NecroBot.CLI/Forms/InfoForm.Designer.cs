namespace PoGo.NecroBot.CLI.Forms
{
    partial class InfoForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoForm));
            this.NecroLogo = new System.Windows.Forms.PictureBox();
            this.SupportLabel = new System.Windows.Forms.Label();
            this.DownloadLabel = new System.Windows.Forms.Label();
            this.lnkDownload = new System.Windows.Forms.LinkLabel();
            this.lnkSupport = new System.Windows.Forms.LinkLabel();
            this.DonateLogo = new System.Windows.Forms.PictureBox();
            this.lnkWiki = new System.Windows.Forms.LinkLabel();
            this.WikiLabel = new System.Windows.Forms.Label();
            this.Description = new System.Windows.Forms.Label();
            this.SnipeLink2 = new System.Windows.Forms.LinkLabel();
            this.SnipeLink1 = new System.Windows.Forms.LinkLabel();
            this.label7 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NecroLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DonateLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // NecroLogo
            // 
            this.NecroLogo.BackgroundImage = global::PoGo.NecroBot.CLI.Properties.Resources.necro_logo;
            this.NecroLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.NecroLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NecroLogo.Location = new System.Drawing.Point(21, 16);
            this.NecroLogo.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.NecroLogo.Name = "NecroLogo";
            this.NecroLogo.Size = new System.Drawing.Size(312, 128);
            this.NecroLogo.TabIndex = 0;
            this.NecroLogo.TabStop = false;
            this.NecroLogo.Click += new System.EventHandler(this.NecroLogo_Click);
            // 
            // SupportLabel
            // 
            this.SupportLabel.AutoSize = true;
            this.SupportLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.SupportLabel.Location = new System.Drawing.Point(341, 83);
            this.SupportLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SupportLabel.Name = "SupportLabel";
            this.SupportLabel.Size = new System.Drawing.Size(75, 23);
            this.SupportLabel.TabIndex = 1;
            this.SupportLabel.Text = "Support:";
            // 
            // DownloadLabel
            // 
            this.DownloadLabel.AutoSize = true;
            this.DownloadLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.DownloadLabel.Location = new System.Drawing.Point(341, 16);
            this.DownloadLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DownloadLabel.Name = "DownloadLabel";
            this.DownloadLabel.Size = new System.Drawing.Size(91, 23);
            this.DownloadLabel.TabIndex = 2;
            this.DownloadLabel.Text = "Download:";
            // 
            // lnkDownload
            // 
            this.lnkDownload.AutoSize = true;
            this.lnkDownload.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lnkDownload.Location = new System.Drawing.Point(455, 16);
            this.lnkDownload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkDownload.Name = "lnkDownload";
            this.lnkDownload.Size = new System.Drawing.Size(168, 23);
            this.lnkDownload.TabIndex = 3;
            this.lnkDownload.TabStop = true;
            this.lnkDownload.Text = "http://bit.ly/2d9iCgU";
            this.lnkDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_click);
            // 
            // lnkSupport
            // 
            this.lnkSupport.AutoSize = true;
            this.lnkSupport.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lnkSupport.Location = new System.Drawing.Point(455, 83);
            this.lnkSupport.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkSupport.Name = "lnkSupport";
            this.lnkSupport.Size = new System.Drawing.Size(166, 23);
            this.lnkSupport.TabIndex = 4;
            this.lnkSupport.TabStop = true;
            this.lnkSupport.Text = "http://bit.ly/2p6k856";
            this.lnkSupport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_click);
            // 
            // DonateLogo
            // 
            this.DonateLogo.BackgroundImage = global::PoGo.NecroBot.CLI.Properties.Resources.PayPalDonateNow;
            this.DonateLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DonateLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DonateLogo.Location = new System.Drawing.Point(21, 160);
            this.DonateLogo.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.DonateLogo.Name = "DonateLogo";
            this.DonateLogo.Size = new System.Drawing.Size(312, 143);
            this.DonateLogo.TabIndex = 5;
            this.DonateLogo.TabStop = false;
            this.DonateLogo.Click += new System.EventHandler(this.DonateLogo_Click);
            // 
            // lnkWiki
            // 
            this.lnkWiki.AutoSize = true;
            this.lnkWiki.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lnkWiki.Location = new System.Drawing.Point(455, 49);
            this.lnkWiki.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkWiki.Name = "lnkWiki";
            this.lnkWiki.Size = new System.Drawing.Size(168, 23);
            this.lnkWiki.TabIndex = 7;
            this.lnkWiki.TabStop = true;
            this.lnkWiki.Text = "http://bit.ly/2pDyGKl";
            this.lnkWiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_click);
            // 
            // WikiLabel
            // 
            this.WikiLabel.AutoSize = true;
            this.WikiLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.WikiLabel.Location = new System.Drawing.Point(341, 49);
            this.WikiLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WikiLabel.Name = "WikiLabel";
            this.WikiLabel.Size = new System.Drawing.Size(46, 23);
            this.WikiLabel.TabIndex = 6;
            this.WikiLabel.Text = "Wiki:";
            // 
            // Description
            // 
            this.Description.AutoSize = true;
            this.Description.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Description.ForeColor = System.Drawing.Color.DarkRed;
            this.Description.Location = new System.Drawing.Point(341, 160);
            this.Description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Description.MaximumSize = new System.Drawing.Size(375, 0);
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(367, 161);
            this.Description.TabIndex = 8;
            this.Description.Text = resources.GetString("Description.Text");
            // 
            // SnipeLink2
            // 
            this.SnipeLink2.AutoSize = true;
            this.SnipeLink2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.SnipeLink2.Location = new System.Drawing.Point(17, 343);
            this.SnipeLink2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SnipeLink2.Name = "SnipeLink2";
            this.SnipeLink2.Size = new System.Drawing.Size(158, 23);
            this.SnipeLink2.TabIndex = 9;
            this.SnipeLink2.TabStop = true;
            this.SnipeLink2.Text = "http://msniper.com";
            // 
            // SnipeLink1
            // 
            this.SnipeLink1.AutoSize = true;
            this.SnipeLink1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.SnipeLink1.Location = new System.Drawing.Point(17, 309);
            this.SnipeLink1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SnipeLink1.Name = "SnipeLink1";
            this.SnipeLink1.Size = new System.Drawing.Size(213, 23);
            this.SnipeLink1.TabIndex = 10;
            this.SnipeLink1.TabStop = true;
            this.SnipeLink1.Text = "http://mypogosnipers.com";
            this.SnipeLink1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label7.Location = new System.Drawing.Point(514, 352);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(235, 19);
            this.label7.TabIndex = 13;
            this.label7.Text = "Thanks for your help, Happy Botting!";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.linkLabel1.Location = new System.Drawing.Point(455, 119);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(162, 23);
            this.linkLabel1.TabIndex = 15;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://bit.ly/2puJfB1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.label1.Location = new System.Drawing.Point(341, 119);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 23);
            this.label1.TabIndex = 14;
            this.label1.Text = "Contributors:";
            // 
            // InfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 380);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.SnipeLink1);
            this.Controls.Add(this.SnipeLink2);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.lnkWiki);
            this.Controls.Add(this.WikiLabel);
            this.Controls.Add(this.DonateLogo);
            this.Controls.Add(this.lnkSupport);
            this.Controls.Add(this.lnkDownload);
            this.Controls.Add(this.DownloadLabel);
            this.Controls.Add(this.SupportLabel);
            this.Controls.Add(this.NecroLogo);
            this.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.Name = "InfoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About NecroBot2";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.InfoForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.NecroLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DonateLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox NecroLogo;
        private System.Windows.Forms.Label SupportLabel;
        private System.Windows.Forms.Label DownloadLabel;
        private System.Windows.Forms.LinkLabel lnkDownload;
        private System.Windows.Forms.LinkLabel lnkSupport;
        private System.Windows.Forms.PictureBox DonateLogo;
        private System.Windows.Forms.LinkLabel lnkWiki;
        private System.Windows.Forms.Label WikiLabel;
        private System.Windows.Forms.Label Description;
        private System.Windows.Forms.LinkLabel SnipeLink2;
        private System.Windows.Forms.LinkLabel SnipeLink1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
    }
}
