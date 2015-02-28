namespace WmBridge.Config
{
    partial class NewCertificateForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.labDays = new System.Windows.Forms.Label();
            this.numExpiration = new System.Windows.Forms.NumericUpDown();
            this.labExpiration = new System.Windows.Forms.Label();
            this.labCN = new System.Windows.Forms.Label();
            this.txtCA = new System.Windows.Forms.TextBox();
            this.labCA = new System.Windows.Forms.Label();
            this.btnSelectCA = new System.Windows.Forms.Button();
            this.linkCreateCA = new System.Windows.Forms.LinkLabel();
            this.cmbCN = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numExpiration)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(237, 206);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(156, 206);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Create";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // labDays
            // 
            this.labDays.AutoSize = true;
            this.labDays.Location = new System.Drawing.Point(138, 169);
            this.labDays.Name = "labDays";
            this.labDays.Size = new System.Drawing.Size(29, 13);
            this.labDays.TabIndex = 10;
            this.labDays.Text = "days";
            // 
            // numExpiration
            // 
            this.numExpiration.Location = new System.Drawing.Point(12, 166);
            this.numExpiration.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numExpiration.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numExpiration.Name = "numExpiration";
            this.numExpiration.Size = new System.Drawing.Size(120, 20);
            this.numExpiration.TabIndex = 4;
            this.numExpiration.Value = new decimal(new int[] {
            3652,
            0,
            0,
            0});
            // 
            // labExpiration
            // 
            this.labExpiration.AutoSize = true;
            this.labExpiration.Location = new System.Drawing.Point(12, 150);
            this.labExpiration.Name = "labExpiration";
            this.labExpiration.Size = new System.Drawing.Size(53, 13);
            this.labExpiration.TabIndex = 8;
            this.labExpiration.Text = "Expiration";
            // 
            // labCN
            // 
            this.labCN.AutoSize = true;
            this.labCN.Location = new System.Drawing.Point(12, 97);
            this.labCN.Name = "labCN";
            this.labCN.Size = new System.Drawing.Size(79, 13);
            this.labCN.TabIndex = 6;
            this.labCN.Text = "Host name / IP";
            // 
            // txtCA
            // 
            this.txtCA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCA.Location = new System.Drawing.Point(12, 34);
            this.txtCA.Name = "txtCA";
            this.txtCA.ReadOnly = true;
            this.txtCA.Size = new System.Drawing.Size(219, 20);
            this.txtCA.TabIndex = 0;
            this.txtCA.TextChanged += new System.EventHandler(this.txtCA_TextChanged);
            // 
            // labCA
            // 
            this.labCA.AutoSize = true;
            this.labCA.Location = new System.Drawing.Point(12, 18);
            this.labCA.Name = "labCA";
            this.labCA.Size = new System.Drawing.Size(84, 13);
            this.labCA.TabIndex = 13;
            this.labCA.Text = "Issuer certificate";
            // 
            // btnSelectCA
            // 
            this.btnSelectCA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCA.Location = new System.Drawing.Point(237, 33);
            this.btnSelectCA.Name = "btnSelectCA";
            this.btnSelectCA.Size = new System.Drawing.Size(75, 22);
            this.btnSelectCA.TabIndex = 2;
            this.btnSelectCA.Text = "Select...";
            this.btnSelectCA.UseVisualStyleBackColor = true;
            this.btnSelectCA.Click += new System.EventHandler(this.btnSelectCA_Click);
            // 
            // linkCreateCA
            // 
            this.linkCreateCA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkCreateCA.AutoSize = true;
            this.linkCreateCA.Location = new System.Drawing.Point(57, 57);
            this.linkCreateCA.Name = "linkCreateCA";
            this.linkCreateCA.Size = new System.Drawing.Size(174, 13);
            this.linkCreateCA.TabIndex = 1;
            this.linkCreateCA.TabStop = true;
            this.linkCreateCA.Text = "New self-signed certificate authority";
            this.linkCreateCA.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCreateCA_LinkClicked);
            // 
            // cmbCN
            // 
            this.cmbCN.FormattingEnabled = true;
            this.cmbCN.Location = new System.Drawing.Point(12, 113);
            this.cmbCN.Name = "cmbCN";
            this.cmbCN.Size = new System.Drawing.Size(300, 21);
            this.cmbCN.TabIndex = 3;
            this.cmbCN.TextChanged += new System.EventHandler(this.cmbCN_TextChanged);
            // 
            // NewCertificateForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(324, 241);
            this.Controls.Add(this.cmbCN);
            this.Controls.Add(this.linkCreateCA);
            this.Controls.Add(this.btnSelectCA);
            this.Controls.Add(this.txtCA);
            this.Controls.Add(this.labCA);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.labDays);
            this.Controls.Add(this.numExpiration);
            this.Controls.Add(this.labExpiration);
            this.Controls.Add(this.labCN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewCertificateForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Certificate";
            ((System.ComponentModel.ISupportInitialize)(this.numExpiration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label labDays;
        private System.Windows.Forms.NumericUpDown numExpiration;
        private System.Windows.Forms.Label labExpiration;
        private System.Windows.Forms.Label labCN;
        private System.Windows.Forms.TextBox txtCA;
        private System.Windows.Forms.Label labCA;
        private System.Windows.Forms.Button btnSelectCA;
        private System.Windows.Forms.LinkLabel linkCreateCA;
        internal System.Windows.Forms.ComboBox cmbCN;
    }
}