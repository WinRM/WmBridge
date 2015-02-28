//
//  Copyright (c) 2015 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;

namespace WmBridge.Config
{
    internal static class Installer
    {
        private const string WmBridgeExecutable = "WmBridge.exe";

        public static bool Install()
        {
            Run(GetProcessStartInfo("install"));
            return ControlService(svc => svc.Start());
        }

        public static void Uninstall()
        {
            ControlService(svc => svc.Stop());
            Run(GetProcessStartInfo("uninstall"));
        }

        public static bool Reinstall()
        {
            Uninstall();
            return Install();
        }

        private static ProcessStartInfo GetProcessStartInfo(string verb)
        {
            var psi = new ProcessStartInfo(Path.Combine(Application.StartupPath, "WmBridge.exe"), verb);
            psi.WorkingDirectory = Application.StartupPath;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.ErrorDialog = true;

            return psi;
        }

        private static bool ControlService(Action<ServiceController> action)
        {
            try
            {
                using (var svc = new ServiceController("WmBridge")) action(svc);
                return true;
            }
            catch { }

            return false;
        }

        private static void Run(ProcessStartInfo psi)
        {
            using (var p = Process.Start(psi))
            {
                p.WaitForExit();
            }
        }
    }
}
