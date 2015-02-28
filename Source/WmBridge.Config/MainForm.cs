//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace WmBridge.Config
{
    public partial class MainForm : Form
    {
        private const string s_AllInterfaces = "(All Interfaces)";
        private const string s_Automatic = "(Automatic)";

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(HandleRef hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private X509Certificate2 selectedCertificate;
        private X509Certificate2 issuerCertificate;

        private ConfigBuilder cfg;

        private bool repairMode = false;

        public MainForm()
        {
            InitializeComponent();
        }

        public void EnableRepairMode()
        {
            repairMode = true;
            //btnConfigure.Text = "Reinstall";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetButtonShield(btnConfigure);

            cmbListenerProtocol.SelectedIndex = 1;

            cmbListenerAddress.Items.AddRange(new string[] { s_AllInterfaces }.Concat(GetNetInterfaces()).ToArray());
            cmbListenerAddress.SelectedIndex = 0;

            try
            {
                cfg = ConfigBuilder.GetDefault();
                ReadConfig();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void ReadConfig()
        {
            if (!string.IsNullOrEmpty(cfg.ListenerProtocol))
                cmbListenerProtocol.Text = cfg.ListenerProtocol;

            if (!string.IsNullOrEmpty(cfg.ListenerAddress))
            {
                if (cfg.ListenerAddress == "*")
                    cmbListenerAddress.Text = s_AllInterfaces;
                else
                    cmbListenerAddress.Text = cfg.ListenerAddress;
            }

            if (cfg.ListenerPort > 0)
                numListenerPort.Value = cfg.ListenerPort;

            if (!string.IsNullOrEmpty(cfg.SslCertificateHash))
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var cert = store.Certificates.Find(X509FindType.FindByThumbprint, cfg.SslCertificateHash, true)
                    .OfType<X509Certificate2>().Where(c => c.HasPrivateKey).FirstOrDefault();
                if (cert != null)
                    SelectCert(cert);

                store.Close();
            }

            if (!string.IsNullOrEmpty(cfg.DiscoveryProtocol))
                cmbDiscoveryProtocol.Text = cfg.DiscoveryProtocol;

            if (!string.IsNullOrEmpty(cfg.DiscoveryHost))
            {
                if (cfg.DiscoveryHost == "*")
                    cmbDiscoveryAddress.Text = s_Automatic;
                else
                    cmbDiscoveryAddress.Text = cfg.DiscoveryHost;
            }

            if (cfg.DiscoveryPort > 0)
                numDiscoveryPort.Value = cfg.DiscoveryPort;

            checkDiscovery.Checked = cfg.DiscoveryEnabled;

            checkFirewall.Checked = cfg.SetupFirewall;
        }

        private void CopyToConfig()
        {
            cfg.ListenerProtocol = cmbListenerProtocol.Text;

            if (cmbListenerAddress.Text == s_AllInterfaces)
                cfg.ListenerAddress = "*";
            else
                cfg.ListenerAddress = cmbListenerAddress.Text;

            cfg.ListenerPort = (UInt16)numListenerPort.Value;

            cfg.DiscoveryProtocol = cmbDiscoveryProtocol.Text;

            if (cmbDiscoveryAddress.Text == s_Automatic)
                cfg.DiscoveryHost = "*";
            else
                cfg.DiscoveryHost = cmbDiscoveryAddress.Text;

            cfg.DiscoveryPort = (UInt16)numDiscoveryPort.Value;

            cfg.DiscoveryEnabled = checkDiscovery.Checked;

            cfg.SetupFirewall = checkFirewall.Checked;


            cfg.SslCertificateHash = null;
            cfg.SslIssuerCertificateHash = null;

            if (cmbListenerProtocol.SelectedIndex == 1)
            {
                if (selectedCertificate != null)
                    cfg.SslCertificateHash = selectedCertificate.Thumbprint;

                if (issuerCertificate != null)
                    cfg.SslIssuerCertificateHash = issuerCertificate.Thumbprint;
            }
        }

        private void numListenerPort_ValueChanged(object sender, EventArgs e)
        {
            numDiscoveryPort.Value = numListenerPort.Value;
        }

        private void cmbListenerProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDiscoveryProtocol.SelectedIndex = cmbListenerProtocol.SelectedIndex;
            groupSSL.Enabled = cmbDiscoveryProtocol.SelectedIndex == 1;
        }

        private void linkCreateCert_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var dlg = new NewCertificateForm())
            {
                dlg.cmbCN.Items.AddRange(GetNetInterfaces());

                if (cmbListenerAddress.Text == s_AllInterfaces)
                {
                    if (dlg.cmbCN.Items.Count > 0)
                        dlg.cmbCN.SelectedIndex = 0;
                }
                else
                {
                    dlg.cmbCN.Text = cmbListenerAddress.Text;
                }

                if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    SelectCert(dlg.SelectedCertificate);

            }
        }

        private void btnSelectCert_Click(object sender, EventArgs e)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var allCerts = store.Certificates.Find(X509FindType.FindByKeyUsage, "DigitalSignature", true)
                .OfType<X509Certificate2>().Where(c => c.HasPrivateKey).ToArray();
            store.Close();

            var cert = X509Certificate2UI.SelectFromCollection(new X509Certificate2Collection(allCerts), "Select server certificate", string.Empty, X509SelectionFlag.SingleSelection, this.Handle)
                .OfType<X509Certificate2>().SingleOrDefault();

            if (cert != null)
                SelectCert(cert);
        }

        private void SelectCert(X509Certificate2 cert)
        {
            txtCert.Text = CertificateHelper.CertificateName(cert);
            selectedCertificate = cert;
            btnCertActions.Enabled = true;

            issuerCertificate = null;
            try
            {
                var chain = new X509Chain();
                chain.Build(selectedCertificate);

                if (chain.ChainElements.Count > 1)
                {
                    issuerCertificate = chain.ChainElements[1].Certificate;

                    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    var issuerWithPrivateKey = store.Certificates.Find(X509FindType.FindByThumbprint, issuerCertificate.Thumbprint, true).OfType<X509Certificate2>().Where(c => c.HasPrivateKey).FirstOrDefault();
                    store.Close();

                    if (issuerWithPrivateKey != null)
                        issuerCertificate = issuerWithPrivateKey;
                }

            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void txtCert_TextChanged(object sender, EventArgs e)
        {
            if (cmbDiscoveryProtocol.SelectedIndex == 1)
                UpdateDiscoveryAddress();
            else
                cmbDiscoveryProtocol.SelectedIndex = 1;
        }

        private void cmbDiscoveryProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDiscoveryAddress();
        }

        private void UpdateDiscoveryAddress()
        {
            if (cmbDiscoveryProtocol.SelectedIndex == 0) // http
            {
                cmbDiscoveryAddress.Items.Clear();

                cmbDiscoveryAddress.Items.AddRange(new string[] { s_Automatic }.Concat(GetNetInterfaces()).ToArray());

                if (cmbListenerAddress.Text == s_AllInterfaces)
                    cmbDiscoveryAddress.SelectedIndex = 0;
                else
                    cmbDiscoveryAddress.Text = cmbListenerAddress.Text;
            }

            if (cmbDiscoveryProtocol.SelectedIndex == 1) // https
            {
                cmbDiscoveryAddress.Items.Clear();
                cmbDiscoveryAddress.Text = "";
                if (!string.IsNullOrEmpty(txtCert.Text))
                {
                    cmbDiscoveryAddress.Items.Add(txtCert.Text);
                    cmbDiscoveryAddress.SelectedIndex = 0;
                }
            }
        }

        private void cmbListenerAddress_TextChanged(object sender, EventArgs e)
        {
            UpdateDiscoveryAddress();
        }

        private static string[] GetNetInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
                .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
                .Where(unicast => unicast.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && unicast.IPv4Mask != null)
                .Select(ua => ua.Address.ToString()).Where(ip => ip != "127.0.0.1").ToArray();
        }

        private void checkDiscovery_CheckedChanged(object sender, EventArgs e)
        {
            labDiscoveryInfo.Enabled = checkDiscovery.Checked;
            cmbDiscoveryAddress.Enabled = checkDiscovery.Checked;
            cmbDiscoveryProtocol.Enabled = checkDiscovery.Checked;
            numDiscoveryPort.Enabled = checkDiscovery.Checked;
        }

        private void btnCertActions_Click(object sender, EventArgs e)
        {
            exportIssuerToolStripMenuItem.Enabled = issuerCertificate != null;
            menuCertActions.Show(btnCertActions, -1, btnCertActions.Height + 1);
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedCertificate != null)
                X509Certificate2UI.DisplayCertificate(selectedCertificate, this.Handle);
        }

        private void exportCertificateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedCertificate != null)
            {
                saveCertDlg.Title = "Export certificate";
                PromptAndExportCert(false);
            }
        }

        private void exportIssuerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedCertificate != null)
            {
                saveCertDlg.Title = "Export issuer certificate";
                PromptAndExportCert(true);
            }
        }

        private void PromptAndExportCert(bool issuer)
        {
            try
            {
                X509Certificate2 cert;

                if (issuer)
                    cert = issuerCertificate;
                else
                    cert = selectedCertificate;

                saveCertDlg.FileName = "";

                if (saveCertDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    string ext = Path.GetExtension(saveCertDlg.FileName).ToLower();

                    if (ext == ".cer")
                        File.WriteAllBytes(saveCertDlg.FileName, cert.Export(X509ContentType.Cert));

                    if (ext == ".pfx")
                        File.WriteAllBytes(saveCertDlg.FileName, cert.Export(X509ContentType.Pfx));
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        static void SetButtonShield(Button btn)
        {
            // BCM_SETSHIELD = 0x0000160C
            SendMessage(new HandleRef(btn, btn.Handle), 0x160C, IntPtr.Zero, new IntPtr(1));
        }

        private bool Validate()
        {
            if (selectedCertificate == null && cmbListenerProtocol.Text.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show(this, "Please select server certificate.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnConfigure_Click(object sender, EventArgs e)
        {
            if (Validate() == false)
                return;

            try
            {
                CopyToConfig();

                cfg.WriteTo();

                Application.UseWaitCursor = true;
                Application.DoEvents();

                bool installationStatus;

                if (this.repairMode)
                    installationStatus = Installer.Reinstall();
                else
                    installationStatus = Installer.Install();

                Application.UseWaitCursor = false;

                if (installationStatus)
                    MessageBox.Show(this, "WinRM Bridge Service was installed successfully.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(this, "WinRM Bridge Service installation is completed with errors.\nCheck the log file for error details.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                EnableRepairMode();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");

            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));
        }

        static void CopyDirectory(string source, string target)
        {
            var sourceInfo = new DirectoryInfo(source);

            foreach (DirectoryInfo dir in sourceInfo.GetDirectories())
                CopyDirectory(dir.FullName, Path.Combine(target, dir.Name));

            bool targetCreated = false;

            foreach (FileInfo file in GetFilesByExtensions(sourceInfo, ".exe", ".dll", ".config", ".txt"))
            {
                if (!targetCreated)
                {
                    Directory.CreateDirectory(target);
                    targetCreated = true;
                }

                file.CopyTo(Path.Combine(target, file.Name));
            }
        }

        private void btnSavePackage_Click(object sender, EventArgs e)
        {
            if (Validate() == false)
                return;
            
            string tmpDir = null;

            try
            {
                CopyToConfig();

                savePackageDlg.FileName = "WmBridge.zip";

                if (savePackageDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    tmpDir = Path.Combine(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), "WinRM Bridge");

                    Directory.CreateDirectory(tmpDir);

                    CopyDirectory(
                        Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                        tmpDir);

                    cfg.WriteTo(tmpDir);

                    if (issuerCertificate != null)
                        File.WriteAllBytes(Path.Combine(tmpDir, issuerCertificate.Thumbprint + ".cer"), issuerCertificate.Export(X509ContentType.Cert));

                    if (selectedCertificate != null)
                        File.WriteAllBytes(Path.Combine(tmpDir, selectedCertificate.Thumbprint + ".pfx"), selectedCertificate.Export(X509ContentType.Pfx));

                    if (File.Exists(savePackageDlg.FileName))
                        File.Delete(savePackageDlg.FileName);

                    File.WriteAllText(Path.Combine(tmpDir, "ReadMe.txt"), Properties.Resources.ReadMe);

                    ZipFile.CreateFromDirectory(tmpDir, savePackageDlg.FileName, CompressionLevel.Optimal, true);
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }

            try
            {
                if (tmpDir != null)
                    Directory.Delete(tmpDir, true);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }

        }

    }

    internal static class FormExtensions
    {
        public static void ShowError(this Form form, Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

}
