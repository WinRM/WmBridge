namespace WmBridge.Config
{
    partial class OperationForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.serviceStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.serviceStatusValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.versionValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel = new System.Windows.Forms.Panel();
            this.btnReinstall = new MaxKnor.VistaUI.CommandLink();
            this.btnUninstall = new MaxKnor.VistaUI.CommandLink();
            this.statusStrip1.SuspendLayout();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serviceStatusLabel,
            this.serviceStatusValue,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.versionValue});
            this.statusStrip1.Location = new System.Drawing.Point(0, 208);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(336, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // serviceStatusLabel
            // 
            this.serviceStatusLabel.Enabled = false;
            this.serviceStatusLabel.Name = "serviceStatusLabel";
            this.serviceStatusLabel.Size = new System.Drawing.Size(81, 17);
            this.serviceStatusLabel.Text = "Service status:";
            // 
            // serviceStatusValue
            // 
            this.serviceStatusValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serviceStatusValue.Image = global::WmBridge.Config.Properties.Resources.Ok;
            this.serviceStatusValue.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.serviceStatusValue.Name = "serviceStatusValue";
            this.serviceStatusValue.Size = new System.Drawing.Size(69, 17);
            this.serviceStatusValue.Text = "Running";
            this.serviceStatusValue.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(91, 17);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabel2.Enabled = false;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(49, 17);
            this.toolStripStatusLabel2.Text = "Version:";
            // 
            // versionValue
            // 
            this.versionValue.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.versionValue.Name = "versionValue";
            this.versionValue.Size = new System.Drawing.Size(31, 17);
            this.versionValue.Text = "1.0.0";
            // 
            // panel
            // 
            this.panel.Controls.Add(this.btnReinstall);
            this.panel.Controls.Add(this.btnUninstall);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(336, 208);
            this.panel.TabIndex = 5;
            // 
            // btnReinstall
            // 
            this.btnReinstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnReinstall.Location = new System.Drawing.Point(12, 41);
            this.btnReinstall.Name = "btnReinstall";
            this.btnReinstall.Note = "Modify bindings or create pre-configured package";
            this.btnReinstall.Size = new System.Drawing.Size(312, 60);
            this.btnReinstall.TabIndex = 3;
            this.btnReinstall.Text = "Open configuration";
            this.btnReinstall.UseVisualStyleBackColor = true;
            this.btnReinstall.Click += new System.EventHandler(this.btnReinstall_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUninstall.Location = new System.Drawing.Point(12, 107);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Note = "Remove service and bindings";
            this.btnUninstall.Size = new System.Drawing.Size(312, 60);
            this.btnUninstall.TabIndex = 4;
            this.btnUninstall.Text = "Uninstall service";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // OperationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(336, 230);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OperationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.OperationForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripStatusLabel serviceStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel versionValue;
        private System.Windows.Forms.ToolStripStatusLabel serviceStatusValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private MaxKnor.VistaUI.CommandLink btnReinstall;
        private MaxKnor.VistaUI.CommandLink btnUninstall;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel;
    }
}