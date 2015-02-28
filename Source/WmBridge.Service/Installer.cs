//
//  Copyright (c) 2015 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace WmBridge.Service
{
    internal static class Installer
    {
        private static readonly ILog logger = LogManager.GetLogger("WmBridge.Installer");

        private static readonly Type psObjType = typeof(PSObject); // force System.Management.Automation 3.0.0.0 dependency 

        public static void Install()
        {
            try
            {
                StringBuilder script = new StringBuilder();

                script.AppendLine("#" + psObjType.Assembly.FullName);

                // enable WinRM
                script.AppendLine(@"echo ""`n * Configuring WinRM""");
                script.AppendLine("winrm quickconfig -quiet");
                script.AppendLine("winrm create winrm/config/Listener?Address=*+Transport=HTTP");
                script.AppendLine("Set-Service winrm -StartupType Automatic");

                // enable CredSSP Client
                script.AppendLine(@"echo ""`n * Configuring CredSSP""");
                script.AppendLine("winrm set winrm/config/service/auth '@{CredSSP=\"true\"}'");
                script.AppendLine("Register-PSSessionConfiguration Microsoft.PowerShell -Force | Out-Null");
                script.AppendLine("Enable-WSManCredSSP -Role Client -DelegateComputer * -Force | Out-Null");
                script.AppendLine(@"Set-Item WSMan:\localhost\Client\TrustedHosts –Value * -Force");

                // enable AllowFreshCredentialsWhenNTLMOnly GPO for WSMAN/*
                script.AppendLine(@"&{");
                script.AppendLine(@"$key = 'hklm:\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation'");
                script.AppendLine("if (!(Test-Path $key)) { md $key }");
                script.AppendLine("New-ItemProperty -Path $key -Name AllowFreshCredentialsWhenNTLMOnly -Value 1 -PropertyType Dword -Force");
                script.AppendLine("$key = Join-Path $key 'AllowFreshCredentialsWhenNTLMOnly'");
                script.AppendLine("if (!(Test-Path $key)) { md $key }");
                script.AppendLine("New-ItemProperty -Path $key -Name 1 -Value 'WSMAN/*' -PropertyType String -Force");
                script.AppendLine(@"} | Out-Null");

                script.AppendLine(@"echo ""`n * Configuring Firewall""");
                // remove old firewall rules to prevent duplicity
                script.AppendLine("netsh advfirewall firewall set rule name=\"Windows Remote Management (HTTP-In)\" new enable=Yes");

                script.AppendLine("netsh advfirewall firewall delete rule name=\"WinRM Bridge (Discovery)\"");
                script.AppendLine("netsh advfirewall firewall delete rule name=\"WinRM Bridge (HTTP-In)\"");

                if (ConfigSection.Default.Installation.FirewallException || ConfigSection.Default.Installation.HasFirewallException == false)
                {
                    // firewall rule for service discovery
                    script.AppendLine("netsh advfirewall firewall add rule name=\"WinRM Bridge (Discovery)\" dir=in action=allow protocol=UDP localport=53581");

                    var ports = ConfigSection.Default.Listeners.Select(x => x.Port).Where(port => port > 0).ToArray();

                    // firewall rule for all HTTP(S) listeners
                    if (ports.Any())
                        script.AppendLine("netsh advfirewall firewall add rule name=\"WinRM Bridge (HTTP-In)\" dir=in action=allow protocol=TCP localport=" + string.Join(",", ports));
                }

                script.AppendLine(@"echo ""`n * Configuring server binding""");

                try
                {
                    X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    X509Store myStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                    rootStore.Open(OpenFlags.ReadWrite);
                    myStore.Open(OpenFlags.ReadWrite);

                    string netSvcAccount = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null).Translate(typeof(NTAccount)).Value;

                    foreach (WmBridge.Service.ConfigSection.ListenerUrlElement listener in ConfigSection.Default.Listeners)
                    {
                        script.AppendLine(string.Format("netsh http add urlacl url={0} user=\"{1}\"", listener.Url, netSvcAccount));

                        if (!string.IsNullOrEmpty(listener.Certificate))
                        {
                            // install self-signed CA certificate
                            if (!string.IsNullOrEmpty(listener.Issuer))
                                InstallCertificate(rootStore, listener.Issuer + ".cer");

                            // install server listener certificate with private key
                            InstallCertificate(myStore, listener.Certificate + ".pfx");

                            // add ssl binding
                            var ipport = listener.BindingIpPort;
                            if (ipport != null)
                                script.AppendLine(string.Format("netsh http add sslcert ipport={0} 'appid={{254948c6-8d95-4313-a8ac-a8a02adab90d}}' certhash={1}", ipport, listener.Certificate));
                        }
                    }

                    rootStore.Close();
                    myStore.Close();
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                }

                // create LOGS directory; apply full control for NETWORK SERVICE account
                CreateLogsDir();

                // app dir must have read & execute rights for NETWORK SERVICE
                GrantReadAndExecute();

                ExecuteScript(script.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Fatal(ex);
            }
        }

        public static void Uninstall()
        {
            try
            {
                StringBuilder script = new StringBuilder();

                script.AppendLine(@"echo ""`n * Configuring Firewall""");

                script.AppendLine("netsh advfirewall firewall delete rule name=\"WinRM Bridge (Discovery)\"");
                script.AppendLine("netsh advfirewall firewall delete rule name=\"WinRM Bridge (HTTP-In)\"");

                script.AppendLine(@"echo ""`n * Configuring server binding""");

                foreach (WmBridge.Service.ConfigSection.ListenerUrlElement listener in ConfigSection.Default.Listeners)
                {
                    if (!string.IsNullOrEmpty(listener.Certificate))
                        script.AppendLine(string.Format("netsh http delete sslcert ipport={0}", listener.BindingIpPort));

                    script.AppendLine(string.Format("netsh http delete urlacl url={0}", listener.Url));
                }

                ExecuteScript(script.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Fatal(ex);
            }
        }

        private static void ExecuteScript(string script)
        {
            StringBuilder outputString = new StringBuilder();

            outputString.AppendLine("Installation script result:");

            using (PowerShell ps = PowerShell.Create())
            {
                using (var outputCollection = new PSDataCollection<string>())
                {
                    ps.AddScript("&{" + script + "} | % { $_ | Out-String }");
                    ps.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    // write any output to console
                    outputCollection.DataAdded += (_, e) => { var text = outputCollection[e.Index]; Console.Write(text); outputString.Append(text); };

                    ps.Invoke<string>(null, outputCollection, new PSInvocationSettings() { ErrorActionPreference = ActionPreference.Continue });
                }
            }

            // log whole output from executed script
            logger.Info(outputString.ToString());
        }

        private static string GetLocalPath()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }

        private static void CreateLogsDir()
        {
            try
            {
                var dir = Directory.CreateDirectory(Path.Combine(GetLocalPath(), "logs"));
                
                var acl = dir.GetAccessControl();
                acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null),
                    FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                
                dir.SetAccessControl(acl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex);
            }
        }

        private static void GrantReadAndExecute()
        {
            try
            {
                var dir = new DirectoryInfo(GetLocalPath());

                var acl = dir.GetAccessControl();
                acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null),
                    FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                dir.SetAccessControl(acl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex);
            }
        }

        private static void InstallCertificate(X509Store store, string certFileName)
        {
            try
            {
                string path = Path.Combine(GetLocalPath(), certFileName);
                if (File.Exists(path))
                {
                    store.Add(new X509Certificate2(path, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet));
                    File.Delete(path); // delete after importing
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex);
            }
        }

    }
}
