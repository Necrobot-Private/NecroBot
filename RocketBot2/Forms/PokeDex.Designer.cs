namespace RocketBot2.Forms
{
    partial class PokeDex
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PokeDex));
            this.flpdex = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flpdex
            // 
            this.flpdex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpdex.AutoScroll = true;
            this.flpdex.BackColor = System.Drawing.SystemColors.Window;
            this.flpdex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpdex.Location = new System.Drawing.Point(0, 0);
            this.flpdex.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.flpdex.Name = "flpdex";
            this.flpdex.Size = new System.Drawing.Size(1259, 517);
            this.flpdex.TabIndex = 33;
            // 
            // PokeDex
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1259, 517);
            this.Controls.Add(this.flpdex);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PokeDex";
            this.Text = "PokeDex";
            this.Load += new System.EventHandler(this.PokeDex_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpdex;
    }
}