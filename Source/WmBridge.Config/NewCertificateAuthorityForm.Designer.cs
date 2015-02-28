namespace WmBridge.Config
{
    partial class NewCertificateAuthorityForm
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
            this.labCN = new System.Windows.Forms.Label();
            this.txtCN = new System.Windows.Forms.TextBox();
            this.labExpiration = new System.Windows.Forms.Label();
            this.numExpiration = new System.Windows.Forms.NumericUpDown();
            this.labDays = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numExpiration)).BeginInit();
            this.SuspendLayout();
            // 
            // labCN
            // 
            this.labCN.AutoSize = true;
            this.labCN.Location = new System.Drawing.Point(12, 37);
            this.labCN.Name = "labCN";
            this.labCN.Size = new System.Drawing.Size(77, 13);
            this.labCN.TabIndex = 0;
            this.labCN.Text = "Common name";
            // 
            // txtCN
            // 
            this.txtCN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCN.Location = new System.Drawing.Point(12, 53);
            this.txtCN.Name = "txtCN";
            this.txtCN.Size = new System.Drawing.Size(282, 20);
            this.txtCN.TabIndex = 1;
            this.txtCN.Text = "WinRM Bridge Local CA";
            this.txtCN.TextChanged += new System.EventHandler(this.txtCN_TextChanged);
            // 
            // labExpiration
            // 
            this.labExpiration.AutoSize = true;
            this.labExpiration.Location = new System.Drawing.Point(12, 90);
            this.labExpiration.Name = "labExpiration";
            this.labExpiration.Size = new System.Drawing.Size(53, 13);
            this.labExpiration.TabIndex = 2;
            this.labExpiration.Text = "Expiration";
            // 
            // numExpiration
            // 
            this.numExpiration.Location = new System.Drawing.Point(12, 106);
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
            this.numExpiration.Size = new System.Drawing.Size(107, 20);
            this.numExpiration.TabIndex = 3;
            this.numExpiration.Value = new decimal(new int[] {
            3652,
            0,
            0,
            0});
            // 
            // labDays
            // 
            this.labDays.AutoSize = true;
            this.labDays.Location = new System.Drawing.Point(125, 109);
            this.labDays.Name = "labDays";
            this.labDays.Size = new System.Drawing.Size(29, 13);
            this.labDays.TabIndex = 4;
            this.labDays.Text = "days";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(138, 142);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Create";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(219, 142);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(275, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "You should send and install this self-signed CA to device.";
            // 
            // NewCertificateAuthorityForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(306, 177);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.labDays);
            this.Controls.Add(this.numExpiration);
            this.Controls.Add(this.labExpiration);
            this.Controls.Add(this.txtCN);
            this.Controls.Add(this.labCN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewCertificateAuthorityForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Certificate Authority";
            ((System.ComponentModel.ISupportInitialize)(this.numExpiration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labCN;
        private System.Windows.Forms.TextBox txtCN;
        private System.Windows.Forms.Label labExpiration;
        private System.Windows.Forms.NumericUpDown numExpiration;
        private System.Windows.Forms.Label labDays;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
    }
}