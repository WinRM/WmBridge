namespace WmBridge.Config
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupListener = new System.Windows.Forms.GroupBox();
            this.lablListenerPort = new System.Windows.Forms.Label();
            this.lablListenerAddress = new System.Windows.Forms.Label();
            this.numListenerPort = new System.Windows.Forms.NumericUpDown();
            this.cmbListenerAddress = new System.Windows.Forms.ComboBox();
            this.lablListenerProtocol = new System.Windows.Forms.Label();
            this.cmbListenerProtocol = new System.Windows.Forms.ComboBox();
            this.groupSSL = new System.Windows.Forms.GroupBox();
            this.txtCert = new System.Windows.Forms.TextBox();
            this.btnCertActions = new System.Windows.Forms.Button();
            this.btnSelectCert = new System.Windows.Forms.Button();
            this.linkCreateCert = new System.Windows.Forms.LinkLabel();
            this.labCertificate = new System.Windows.Forms.Label();
            this.groupAutodiscovery = new System.Windows.Forms.GroupBox();
            this.lablDiscoveryPort = new System.Windows.Forms.Label();
            this.lablDiscoveryAddress = new System.Windows.Forms.Label();
            this.numDiscoveryPort = new System.Windows.Forms.NumericUpDown();
            this.cmbDiscoveryAddress = new System.Windows.Forms.ComboBox();
            this.lablDiscoveryProtocol = new System.Windows.Forms.Label();
            this.cmbDiscoveryProtocol = new System.Windows.Forms.ComboBox();
            this.checkDiscovery = new System.Windows.Forms.CheckBox();
            this.labDiscoveryInfo = new System.Windows.Forms.Label();
            this.checkFirewall = new System.Windows.Forms.CheckBox();
            this.menuCertActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportCertificateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportIssuerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCertDlg = new System.Windows.Forms.SaveFileDialog();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.btnSavePackage = new System.Windows.Forms.Button();
            this.savePackageDlg = new System.Windows.Forms.SaveFileDialog();
            this.groupListener.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numListenerPort)).BeginInit();
            this.groupSSL.SuspendLayout();
            this.groupAutodiscovery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryPort)).BeginInit();
            this.menuCertActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupListener
            // 
            this.groupListener.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupListener.Controls.Add(this.lablListenerPort);
            this.groupListener.Controls.Add(this.lablListenerAddress);
            this.groupListener.Controls.Add(this.numListenerPort);
            this.groupListener.Controls.Add(this.cmbListenerAddress);
            this.groupListener.Controls.Add(this.lablListenerProtocol);
            this.groupListener.Controls.Add(this.cmbListenerProtocol);
            this.groupListener.Location = new System.Drawing.Point(12, 12);
            this.groupListener.Name = "groupListener";
            this.groupListener.Size = new System.Drawing.Size(360, 80);
            this.groupListener.TabIndex = 0;
            this.groupListener.TabStop = false;
            this.groupListener.Text = "Web server listener";
            // 
            // lablListenerPort
            // 
            this.lablListenerPort.AutoSize = true;
            this.lablListenerPort.Location = new System.Drawing.Point(284, 24);
            this.lablListenerPort.Name = "lablListenerPort";
            this.lablListenerPort.Size = new System.Drawing.Size(26, 13);
            this.lablListenerPort.TabIndex = 11;
            this.lablListenerPort.Text = "Port";
            // 
            // lablListenerAddress
            // 
            this.lablListenerAddress.AutoSize = true;
            this.lablListenerAddress.Location = new System.Drawing.Point(96, 24);
            this.lablListenerAddress.Name = "lablListenerAddress";
            this.lablListenerAddress.Size = new System.Drawing.Size(57, 13);
            this.lablListenerAddress.TabIndex = 10;
            this.lablListenerAddress.Text = "IP address";
            // 
            // numListenerPort
            // 
            this.numListenerPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numListenerPort.Location = new System.Drawing.Point(284, 40);
            this.numListenerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numListenerPort.Name = "numListenerPort";
            this.numListenerPort.Size = new System.Drawing.Size(66, 20);
            this.numListenerPort.TabIndex = 2;
            this.numListenerPort.Value = new decimal(new int[] {
            4444,
            0,
            0,
            0});
            this.numListenerPort.ValueChanged += new System.EventHandler(this.numListenerPort_ValueChanged);
            // 
            // cmbListenerAddress
            // 
            this.cmbListenerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbListenerAddress.FormattingEnabled = true;
            this.cmbListenerAddress.Location = new System.Drawing.Point(96, 40);
            this.cmbListenerAddress.Name = "cmbListenerAddress";
            this.cmbListenerAddress.Size = new System.Drawing.Size(182, 21);
            this.cmbListenerAddress.TabIndex = 1;
            this.cmbListenerAddress.TextChanged += new System.EventHandler(this.cmbListenerAddress_TextChanged);
            // 
            // lablListenerProtocol
            // 
            this.lablListenerProtocol.AutoSize = true;
            this.lablListenerProtocol.Location = new System.Drawing.Point(10, 24);
            this.lablListenerProtocol.Name = "lablListenerProtocol";
            this.lablListenerProtocol.Size = new System.Drawing.Size(46, 13);
            this.lablListenerProtocol.TabIndex = 7;
            this.lablListenerProtocol.Text = "Protocol";
            // 
            // cmbListenerProtocol
            // 
            this.cmbListenerProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbListenerProtocol.FormattingEnabled = true;
            this.cmbListenerProtocol.Items.AddRange(new object[] {
            "HTTP",
            "HTTPS"});
            this.cmbListenerProtocol.Location = new System.Drawing.Point(10, 40);
            this.cmbListenerProtocol.Name = "cmbListenerProtocol";
            this.cmbListenerProtocol.Size = new System.Drawing.Size(80, 21);
            this.cmbListenerProtocol.TabIndex = 0;
            this.cmbListenerProtocol.SelectedIndexChanged += new System.EventHandler(this.cmbListenerProtocol_SelectedIndexChanged);
            // 
            // groupSSL
            // 
            this.groupSSL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupSSL.Controls.Add(this.txtCert);
            this.groupSSL.Controls.Add(this.btnCertActions);
            this.groupSSL.Controls.Add(this.btnSelectCert);
            this.groupSSL.Controls.Add(this.linkCreateCert);
            this.groupSSL.Controls.Add(this.labCertificate);
            this.groupSSL.Location = new System.Drawing.Point(12, 102);
            this.groupSSL.Name = "groupSSL";
            this.groupSSL.Size = new System.Drawing.Size(360, 89);
            this.groupSSL.TabIndex = 1;
            this.groupSSL.TabStop = false;
            this.groupSSL.Text = "SSL";
            // 
            // txtCert
            // 
            this.txtCert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCert.Location = new System.Drawing.Point(10, 40);
            this.txtCert.Name = "txtCert";
            this.txtCert.ReadOnly = true;
            this.txtCert.Size = new System.Drawing.Size(245, 20);
            this.txtCert.TabIndex = 0;
            this.txtCert.TextChanged += new System.EventHandler(this.txtCert_TextChanged);
            // 
            // btnCertActions
            // 
            this.btnCertActions.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnCertActions.Enabled = false;
            this.btnCertActions.FlatAppearance.BorderSize = 0;
            this.btnCertActions.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
            this.btnCertActions.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnCertActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCertActions.Image = ((System.Drawing.Image)(resources.GetObject("btnCertActions.Image")));
            this.btnCertActions.Location = new System.Drawing.Point(254, 40);
            this.btnCertActions.Name = "btnCertActions";
            this.btnCertActions.Size = new System.Drawing.Size(21, 20);
            this.btnCertActions.TabIndex = 18;
            this.btnCertActions.TabStop = false;
            this.btnCertActions.UseVisualStyleBackColor = false;
            this.btnCertActions.Click += new System.EventHandler(this.btnCertActions_Click);
            // 
            // btnSelectCert
            // 
            this.btnSelectCert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCert.Location = new System.Drawing.Point(283, 39);
            this.btnSelectCert.Name = "btnSelectCert";
            this.btnSelectCert.Size = new System.Drawing.Size(66, 22);
            this.btnSelectCert.TabIndex = 3;
            this.btnSelectCert.Text = "Select...";
            this.btnSelectCert.UseVisualStyleBackColor = true;
            this.btnSelectCert.Click += new System.EventHandler(this.btnSelectCert_Click);
            // 
            // linkCreateCert
            // 
            this.linkCreateCert.AutoSize = true;
            this.linkCreateCert.Cursor = System.Windows.Forms.Cursors.Default;
            this.linkCreateCert.Location = new System.Drawing.Point(123, 63);
            this.linkCreateCert.Name = "linkCreateCert";
            this.linkCreateCert.Size = new System.Drawing.Size(131, 13);
            this.linkCreateCert.TabIndex = 1;
            this.linkCreateCert.TabStop = true;
            this.linkCreateCert.Text = "New self-signed certificate";
            this.linkCreateCert.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCreateCert_LinkClicked);
            // 
            // labCertificate
            // 
            this.labCertificate.AutoSize = true;
            this.labCertificate.Location = new System.Drawing.Point(10, 24);
            this.labCertificate.Name = "labCertificate";
            this.labCertificate.Size = new System.Drawing.Size(87, 13);
            this.labCertificate.TabIndex = 17;
            this.labCertificate.Text = "Server certificate";
            // 
            // groupAutodiscovery
            // 
            this.groupAutodiscovery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupAutodiscovery.Controls.Add(this.lablDiscoveryPort);
            this.groupAutodiscovery.Controls.Add(this.lablDiscoveryAddress);
            this.groupAutodiscovery.Controls.Add(this.numDiscoveryPort);
            this.groupAutodiscovery.Controls.Add(this.cmbDiscoveryAddress);
            this.groupAutodiscovery.Controls.Add(this.lablDiscoveryProtocol);
            this.groupAutodiscovery.Controls.Add(this.cmbDiscoveryProtocol);
            this.groupAutodiscovery.Controls.Add(this.checkDiscovery);
            this.groupAutodiscovery.Controls.Add(this.labDiscoveryInfo);
            this.groupAutodiscovery.Location = new System.Drawing.Point(12, 197);
            this.groupAutodiscovery.Name = "groupAutodiscovery";
            this.groupAutodiscovery.Size = new System.Drawing.Size(360, 125);
            this.groupAutodiscovery.TabIndex = 2;
            this.groupAutodiscovery.TabStop = false;
            this.groupAutodiscovery.Text = "Service autodiscovery";
            // 
            // lablDiscoveryPort
            // 
            this.lablDiscoveryPort.AutoSize = true;
            this.lablDiscoveryPort.Location = new System.Drawing.Point(284, 69);
            this.lablDiscoveryPort.Name = "lablDiscoveryPort";
            this.lablDiscoveryPort.Size = new System.Drawing.Size(26, 13);
            this.lablDiscoveryPort.TabIndex = 17;
            this.lablDiscoveryPort.Text = "Port";
            // 
            // lablDiscoveryAddress
            // 
            this.lablDiscoveryAddress.AutoSize = true;
            this.lablDiscoveryAddress.Location = new System.Drawing.Point(96, 69);
            this.lablDiscoveryAddress.Name = "lablDiscoveryAddress";
            this.lablDiscoveryAddress.Size = new System.Drawing.Size(79, 13);
            this.lablDiscoveryAddress.TabIndex = 16;
            this.lablDiscoveryAddress.Text = "Host name / IP";
            // 
            // numDiscoveryPort
            // 
            this.numDiscoveryPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numDiscoveryPort.Location = new System.Drawing.Point(284, 85);
            this.numDiscoveryPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numDiscoveryPort.Name = "numDiscoveryPort";
            this.numDiscoveryPort.Size = new System.Drawing.Size(66, 20);
            this.numDiscoveryPort.TabIndex = 3;
            this.numDiscoveryPort.Value = new decimal(new int[] {
            4444,
            0,
            0,
            0});
            // 
            // cmbDiscoveryAddress
            // 
            this.cmbDiscoveryAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDiscoveryAddress.FormattingEnabled = true;
            this.cmbDiscoveryAddress.Location = new System.Drawing.Point(96, 85);
            this.cmbDiscoveryAddress.Name = "cmbDiscoveryAddress";
            this.cmbDiscoveryAddress.Size = new System.Drawing.Size(182, 21);
            this.cmbDiscoveryAddress.TabIndex = 2;
            // 
            // lablDiscoveryProtocol
            // 
            this.lablDiscoveryProtocol.AutoSize = true;
            this.lablDiscoveryProtocol.Location = new System.Drawing.Point(10, 69);
            this.lablDiscoveryProtocol.Name = "lablDiscoveryProtocol";
            this.lablDiscoveryProtocol.Size = new System.Drawing.Size(46, 13);
            this.lablDiscoveryProtocol.TabIndex = 13;
            this.lablDiscoveryProtocol.Text = "Protocol";
            // 
            // cmbDiscoveryProtocol
            // 
            this.cmbDiscoveryProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDiscoveryProtocol.FormattingEnabled = true;
            this.cmbDiscoveryProtocol.Items.AddRange(new object[] {
            "HTTP",
            "HTTPS"});
            this.cmbDiscoveryProtocol.Location = new System.Drawing.Point(10, 85);
            this.cmbDiscoveryProtocol.Name = "cmbDiscoveryProtocol";
            this.cmbDiscoveryProtocol.Size = new System.Drawing.Size(80, 21);
            this.cmbDiscoveryProtocol.TabIndex = 1;
            this.cmbDiscoveryProtocol.SelectedIndexChanged += new System.EventHandler(this.cmbDiscoveryProtocol_SelectedIndexChanged);
            // 
            // checkDiscovery
            // 
            this.checkDiscovery.AutoSize = true;
            this.checkDiscovery.Checked = true;
            this.checkDiscovery.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDiscovery.Location = new System.Drawing.Point(13, 23);
            this.checkDiscovery.Name = "checkDiscovery";
            this.checkDiscovery.Size = new System.Drawing.Size(65, 17);
            this.checkDiscovery.TabIndex = 0;
            this.checkDiscovery.Text = "Enabled";
            this.checkDiscovery.UseVisualStyleBackColor = true;
            this.checkDiscovery.CheckedChanged += new System.EventHandler(this.checkDiscovery_CheckedChanged);
            // 
            // labDiscoveryInfo
            // 
            this.labDiscoveryInfo.AutoSize = true;
            this.labDiscoveryInfo.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labDiscoveryInfo.Location = new System.Drawing.Point(8, 46);
            this.labDiscoveryInfo.Name = "labDiscoveryInfo";
            this.labDiscoveryInfo.Size = new System.Drawing.Size(329, 13);
            this.labDiscoveryInfo.TabIndex = 0;
            this.labDiscoveryInfo.Text = "This connection info will be promoted to clients on the local network.";
            // 
            // checkFirewall
            // 
            this.checkFirewall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkFirewall.AutoSize = true;
            this.checkFirewall.Checked = true;
            this.checkFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFirewall.Location = new System.Drawing.Point(12, 334);
            this.checkFirewall.Name = "checkFirewall";
            this.checkFirewall.Size = new System.Drawing.Size(184, 17);
            this.checkFirewall.TabIndex = 3;
            this.checkFirewall.Text = "Add Windows Firewall exceptions";
            this.checkFirewall.UseVisualStyleBackColor = true;
            // 
            // menuCertActions
            // 
            this.menuCertActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.toolStripSeparator1,
            this.exportCertificateToolStripMenuItem,
            this.exportIssuerToolStripMenuItem});
            this.menuCertActions.Name = "menuCertActions";
            this.menuCertActions.ShowImageMargin = false;
            this.menuCertActions.Size = new System.Drawing.Size(132, 76);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.viewToolStripMenuItem.Text = "&View Certificate";
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(128, 6);
            // 
            // exportCertificateToolStripMenuItem
            // 
            this.exportCertificateToolStripMenuItem.Name = "exportCertificateToolStripMenuItem";
            this.exportCertificateToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.exportCertificateToolStripMenuItem.Text = "&Export...";
            this.exportCertificateToolStripMenuItem.Click += new System.EventHandler(this.exportCertificateToolStripMenuItem_Click);
            // 
            // exportIssuerToolStripMenuItem
            // 
            this.exportIssuerToolStripMenuItem.Name = "exportIssuerToolStripMenuItem";
            this.exportIssuerToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.exportIssuerToolStripMenuItem.Text = "Export &issuer...";
            this.exportIssuerToolStripMenuItem.Click += new System.EventHandler(this.exportIssuerToolStripMenuItem_Click);
            // 
            // saveCertDlg
            // 
            this.saveCertDlg.Filter = "X.509 Certificate (*.cer)|*.cer|Personal Information Exchange (*.pfx)|*.pfx";
            // 
            // btnConfigure
            // 
            this.btnConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfigure.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnConfigure.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigure.Location = new System.Drawing.Point(273, 387);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Size = new System.Drawing.Size(99, 32);
            this.btnConfigure.TabIndex = 6;
            this.btnConfigure.Text = "Apply";
            this.btnConfigure.UseVisualStyleBackColor = true;
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // btnSavePackage
            // 
            this.btnSavePackage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSavePackage.Image = global::WmBridge.Config.Properties.Resources.Save;
            this.btnSavePackage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSavePackage.Location = new System.Drawing.Point(67, 387);
            this.btnSavePackage.Name = "btnSavePackage";
            this.btnSavePackage.Size = new System.Drawing.Size(200, 32);
            this.btnSavePackage.TabIndex = 5;
            this.btnSavePackage.Text = "Save pre-configured package... ";
            this.btnSavePackage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSavePackage.UseVisualStyleBackColor = true;
            this.btnSavePackage.Click += new System.EventHandler(this.btnSavePackage_Click);
            // 
            // savePackageDlg
            // 
            this.savePackageDlg.Filter = "Zip File (*.zip)|*.zip";
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnConfigure;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 431);
            this.Controls.Add(this.btnSavePackage);
            this.Controls.Add(this.btnConfigure);
            this.Controls.Add(this.checkFirewall);
            this.Controls.Add(this.groupAutodiscovery);
            this.Controls.Add(this.groupSSL);
            this.Controls.Add(this.groupListener);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinRM Bridge Service Configuration";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupListener.ResumeLayout(false);
            this.groupListener.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numListenerPort)).EndInit();
            this.groupSSL.ResumeLayout(false);
            this.groupSSL.PerformLayout();
            this.groupAutodiscovery.ResumeLayout(false);
            this.groupAutodiscovery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryPort)).EndInit();
            this.menuCertActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lablListenerPort;
        private System.Windows.Forms.Label lablListenerAddress;
        private System.Windows.Forms.NumericUpDown numListenerPort;
        private System.Windows.Forms.ComboBox cmbListenerAddress;
        private System.Windows.Forms.Label lablListenerProtocol;
        private System.Windows.Forms.ComboBox cmbListenerProtocol;
        private System.Windows.Forms.GroupBox groupSSL;
        private System.Windows.Forms.GroupBox groupAutodiscovery;
        private System.Windows.Forms.CheckBox checkFirewall;
        private System.Windows.Forms.LinkLabel linkCreateCert;
        private System.Windows.Forms.Button btnSelectCert;
        private System.Windows.Forms.TextBox txtCert;
        private System.Windows.Forms.Label labCertificate;
        private System.Windows.Forms.GroupBox groupListener;
        private System.Windows.Forms.CheckBox checkDiscovery;
        private System.Windows.Forms.Label labDiscoveryInfo;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.Button btnSavePackage;
        private System.Windows.Forms.Label lablDiscoveryPort;
        private System.Windows.Forms.Label lablDiscoveryAddress;
        private System.Windows.Forms.NumericUpDown numDiscoveryPort;
        private System.Windows.Forms.ComboBox cmbDiscoveryAddress;
        private System.Windows.Forms.Label lablDiscoveryProtocol;
        private System.Windows.Forms.ComboBox cmbDiscoveryProtocol;
        private System.Windows.Forms.Button btnCertActions;
        private System.Windows.Forms.ContextMenuStrip menuCertActions;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCertificateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportIssuerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SaveFileDialog saveCertDlg;
        private System.Windows.Forms.SaveFileDialog savePackageDlg;

    }
}

