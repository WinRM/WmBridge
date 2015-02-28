//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WmBridge.Config
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            ServiceControllerStatus status;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ServiceExists("wmbridge", out status) ? (Form)new OperationForm(status) : (Form)new MainForm());
        }

        private static bool ServiceExists(string svcName, out ServiceControllerStatus status)
        {
            ServiceControllerStatus status_ = ServiceControllerStatus.Stopped;

            var ret = ServiceController.GetServices().Any(svc =>
            {
                bool result = svc.ServiceName.Equals(svcName, StringComparison.InvariantCultureIgnoreCase);
                status_ = svc.Status;
                svc.Dispose();
                return result;
            });

            status = status_;

            return ret;
        }
    }
}
