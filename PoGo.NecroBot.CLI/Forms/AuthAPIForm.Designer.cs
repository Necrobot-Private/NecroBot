using PokemonGo.RocketAPI;

namespace PoGo.NecroBot.CLI.Forms
{
    partial class AuthAPIForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAPIKey = new System.Windows.Forms.TextBox();
            this.radHashServer = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.radCustomHash = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCustomHash = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnOK.Location = new System.Drawing.Point(71, 295);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label1.Location = new System.Drawing.Point(14, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select API:";
            // 
            // txtAPIKey
            // 
            this.txtAPIKey.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtAPIKey.Location = new System.Drawing.Point(15, 170);
            this.txtAPIKey.Margin = new System.Windows.Forms.Padding(4);
            this.txtAPIKey.Name = "txtAPIKey";
            this.txtAPIKey.Size = new System.Drawing.Size(305, 25);
            this.txtAPIKey.TabIndex = 2;
            // 
            // radHashServer
            // 
            this.radHashServer.AutoSize = true;
            this.radHashServer.Checked = true;
            this.radHashServer.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.radHashServer.Location = new System.Drawing.Point(15, 35);
            this.radHashServer.Margin = new System.Windows.Forms.Padding(4);
            this.radHashServer.Name = "radHashServer";
            this.radHashServer.Size = new System.Drawing.Size(205, 25);
            this.radHashServer.TabIndex = 5;
            this.radHashServer.TabStop = true;
            this.radHashServer.Text = "PogoDev Hash - API v" + Constants.API_VERSION;
            this.radHashServer.UseVisualStyleBackColor = true;
            this.radHashServer.Click += new System.EventHandler(this.RadHashServer_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label2.Location = new System.Drawing.Point(15, 150);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "API Key:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label3.Location = new System.Drawing.Point(10, 200);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.MaximumSize = new System.Drawing.Size(315, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(314, 76);
            this.label3.TabIndex = 7;
            this.label3.Text = "We don\'t provide keys, you will have to buy it from an API provider. RPM = Reques" +
    "ts per minute, it depends on how fast your config setup is. 150RPM will be suffi" +
    "cient for 2-3 normal bots.";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnCancel.Location = new System.Drawing.Point(165, 295);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // radCustomHash
            // 
            this.radCustomHash.AutoSize = true;
            this.radCustomHash.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.radCustomHash.Location = new System.Drawing.Point(15, 65);
            this.radCustomHash.Margin = new System.Windows.Forms.Padding(4);
            this.radCustomHash.Name = "radCustomHash";
            this.radCustomHash.Size = new System.Drawing.Size(200, 25);
            this.radCustomHash.TabIndex = 9;
            this.radCustomHash.Text = "Custom Hash - API v" + Constants.API_VERSION;
            this.radCustomHash.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label4.Location = new System.Drawing.Point(14, 95);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Hash URL:";
            // 
            // txtCustomHash
            // 
            this.txtCustomHash.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtCustomHash.Location = new System.Drawing.Point(15, 120);
            this.txtCustomHash.Margin = new System.Windows.Forms.Padding(4);
            this.txtCustomHash.Name = "txtCustomHash";
            this.txtCustomHash.Size = new System.Drawing.Size(305, 25);
            this.txtCustomHash.TabIndex = 10;
            this.txtCustomHash.TextChanged += new System.EventHandler(this.TxtCustomHash_TextChanged);
            // 
            // AuthAPIForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(332, 341);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCustomHash);
            this.Controls.Add(this.radCustomHash);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.radHashServer);
            this.Controls.Add(this.txtAPIKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AuthAPIForm";
            this.ShowInTaskbar = false;
            this.Text = "APIConfig";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAPIKey;
        private System.Windows.Forms.RadioButton radHashServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton radCustomHash;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCustomHash;
    }
}
