//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using Topshelf;

namespace WmBridge.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                
                x.Service<Service>();

                x.SetServiceName("WmBridge");
                x.SetDisplayName("WinRM Bridge Service");
                x.SetDescription("Provides a subset of WinRM operations accessible via Web API.");

                x.AfterInstall(() => Installer.Install());
                x.AfterUninstall(() => Installer.Uninstall());

                x.RunAsNetworkService();
            });
        }
    }
}
