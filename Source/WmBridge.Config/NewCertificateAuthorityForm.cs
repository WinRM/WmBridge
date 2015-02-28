//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace WmBridge.Config
{
    public partial class NewCertificateAuthorityForm : Form
    {
        public X509Certificate2 SelectedCertificate { get; set; }

        public NewCertificateAuthorityForm()
        {
            InitializeComponent();
        }

        private void txtCN_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrEmpty(txtCN.Text);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                SelectedCertificate = CertificateHelper.CreateSelfSignedCA(txtCN.Text, (int)numExpiration.Value);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }
    }
}
