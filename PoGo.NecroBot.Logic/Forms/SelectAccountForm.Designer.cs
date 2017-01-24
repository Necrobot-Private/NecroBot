namespace PoGo.NecroBot.Logic.Forms
{
    partial class SelectAccountForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.lvAcc = new PoGo.NecroBot.Logic.Forms.EXListView();
            this.colType = ((PoGo.NecroBot.Logic.Forms.EXColumnHeader)(new PoGo.NecroBot.Logic.Forms.EXColumnHeader()));
            this.colUsername = ((PoGo.NecroBot.Logic.Forms.EXColumnHeader)(new PoGo.NecroBot.Logic.Forms.EXColumnHeader()));
            this.colRuntime = ((PoGo.NecroBot.Logic.Forms.EXColumnHeader)(new PoGo.NecroBot.Logic.Forms.EXColumnHeader()));
            this.colStatus = ((PoGo.NecroBot.Logic.Forms.EXColumnHeader)(new PoGo.NecroBot.Logic.Forms.EXColumnHeader()));
            this.colStart = ((PoGo.NecroBot.Logic.Forms.EXColumnHeader)(new PoGo.NecroBot.Logic.Forms.EXColumnHeader()));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(374, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PLEASE SELECT AN ACCOUNT TO START. AUTO START AFTER 30 SEC";
            // 
            // lvAcc
            // 
            this.lvAcc.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colType,
            this.colUsername,
            this.colRuntime,
            this.colStatus,
            this.colStart});
            this.lvAcc.ControlPadding = 0;
            this.lvAcc.FullRowSelect = true;
            this.lvAcc.Location = new System.Drawing.Point(11, 56);
            this.lvAcc.MultiSelect = false;
            this.lvAcc.Name = "lvAcc";
            this.lvAcc.OwnerDraw = true;
            this.lvAcc.Size = new System.Drawing.Size(452, 197);
            this.lvAcc.TabIndex = 1;
            this.lvAcc.UseCompatibleStateImageBehavior = false;
            this.lvAcc.View = System.Windows.Forms.View.Details;
            this.lvAcc.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // colType
            // 
            this.colType.Text = "Account Type";
            this.colType.Width = 100;
            // 
            // colUsername
            // 
            this.colUsername.Text = "Username";
            this.colUsername.Width = 82;
            // 
            // colRuntime
            // 
            this.colRuntime.Text = "Runtime";
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status ";
            this.colStatus.Width = 91;
            // 
            // colStart
            // 
            this.colStart.Text = "Start";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(388, 273);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "CLOSE";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SelectAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 308);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lvAcc);
            this.Controls.Add(this.label1);
            this.Name = "SelectAccountForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Startup Account";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectAccountForm_FormClosing);
            this.Load += new System.EventHandler(this.SelectAccountForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private EXListView lvAcc;
        private EXColumnHeader colType;
        private EXColumnHeader colUsername;
        private EXColumnHeader colRuntime;
        private EXColumnHeader colStatus;
        private EXColumnHeader colStart;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button1;
    }
}