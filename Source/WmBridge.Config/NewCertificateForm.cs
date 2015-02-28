//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WmBridge.Config
{
    public partial class NewCertificateForm : Form
    {
        public X509Certificate2 SelectedCertificate { get; set; }

        private X509Certificate2 selectedCA;
        
        public NewCertificateForm()
        {
            InitializeComponent();
            ValidateInputs();
        }

        private void linkCreateCA_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var dlg = new NewCertificateAuthorityForm())
            {
                if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    SelectCA(dlg.SelectedCertificate);
            }
        }

        private void cmbCN_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void txtCA_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void ValidateInputs()
        {
            btnOk.Enabled = !string.IsNullOrEmpty(txtCA.Text) && !string.IsNullOrEmpty(cmbCN.Text);
        }

        private void btnSelectCA_Click(object sender, EventArgs e)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var allCerts = store.Certificates.Find(X509FindType.FindByKeyUsage, "KeyCertSign", true)
                .OfType<X509Certificate2>().Where(c => c.HasPrivateKey).ToArray();
            store.Close();

            var cert = X509Certificate2UI.SelectFromCollection(new X509Certificate2Collection(allCerts), "Select certificate authority", string.Empty, X509SelectionFlag.SingleSelection, this.Handle)
                .OfType<X509Certificate2>().SingleOrDefault();

            if (cert != null)
                SelectCA(cert);
        }

        private void SelectCA(X509Certificate2 ca)
        {
            txtCA.Text = CertificateHelper.CertificateName(ca);
            selectedCA = ca;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                SelectedCertificate = CertificateHelper.CreateCertificate(cmbCN.Text, (int)numExpiration.Value, selectedCA);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }
    }
}
