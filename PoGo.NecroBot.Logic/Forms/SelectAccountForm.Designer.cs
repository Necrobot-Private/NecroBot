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
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(483, 17);
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
            this.lvAcc.Location = new System.Drawing.Point(15, 69);
            this.lvAcc.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvAcc.MultiSelect = false;
            this.lvAcc.Name = "lvAcc";
            this.lvAcc.OwnerDraw = true;
            this.lvAcc.Size = new System.Drawing.Size(601, 242);
            this.lvAcc.TabIndex = 1;
            this.lvAcc.UseCompatibleStateImageBehavior = false;
            this.lvAcc.View = System.Windows.Forms.View.Details;
            this.lvAcc.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
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
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(517, 336);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.Btnclose_Click);
            // 
            // SelectAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 379);
            this.ControlBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lvAcc);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
        private System.Windows.Forms.Button btnClose;
    }
}