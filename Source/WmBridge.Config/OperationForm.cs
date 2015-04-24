//
//  Copyright (c) 2015 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace WmBridge.Config
{
    public partial class OperationForm : Form
    {
        MainForm mainForm = new MainForm();

        public OperationForm(ServiceControllerStatus status)
        {
            InitializeComponent();

            serviceStatusValue.Text = status.ToString();
            serviceStatusValue.Image = status == ServiceControllerStatus.Running ? Properties.Resources.Ok : Properties.Resources.Warning;
        }

        private void OperationForm_Load(object sender, EventArgs e)
        {
            if (VisualStyleInformation.IsEnabledByUser)
                panel.BackColor = Color.White;

            btnReinstall.SetImage(Properties.Resources.Reinstall);
            btnUninstall.SetImage(Properties.Resources.Uninstall);

            this.Text = mainForm.Text;
            this.Icon = mainForm.Icon;
            this.versionValue.Text = GetVersion();
        }

        string GetVersion()
        {
            return ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute)).Single()).Version;
        }

        private void SwitchToMainForm(bool repairMode)
        {
            this.Hide();

            if (repairMode)
                mainForm.EnableRepairMode();
            
            mainForm.ShowDialog();
            this.Close();
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            SwitchToMainForm(true);
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(
                new Action(() =>
                {
                    try
                    {
                        Application.UseWaitCursor = true;
                        Application.DoEvents();

                        Installer.Uninstall();

                        Application.UseWaitCursor = false;

                        MessageBox.Show(this, "WinRM Bridge Service was successfully uninstalled.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        this.ShowError(ex);
                    }
                }));
        }

        private void OperationForm_Shown(object sender, EventArgs e)
        {
            AutoUpdater.CheckForUpdates();
        }

    }
}
