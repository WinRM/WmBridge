//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Topshelf;

namespace WmBridge.Service
{
    public class Service : ServiceControl
    {
        private static readonly ILog logger = LogManager.GetLogger("WmBridge.Service");

        IDisposable webApp;
        IDisposable autodiscovery;

        [ImportMany("WmBridgeStartAction")] 
        List<Action> startActions = null;
        
        [ImportMany("WmBridgeStopAction")]
        List<Action> stopActions = null;

        public Service()
        {
            using (var catalog = new DirectoryCatalog("."))
            using (var container = new CompositionContainer(catalog))
                container.SatisfyImportsOnce(this);
        }

        public bool Start(HostControl hostControl)
        {
            if (ConfigSection.Default.Listeners.Any())
            {
                StartOptions options = new StartOptions();

                foreach (string url in ConfigSection.Default.Listeners.Select(x => x.Url))
                {
                    logger.InfoFormat("Listening on {0}", url);
                    options.Urls.Add(url);
                }

                // Start OWIN host
                webApp = WebApp.Start<WmBridge.Web.Startup>(options);
            }

            if (ConfigSection.Default.Autodiscovery.Any())
            {
                // Start autodiscovery service
                autodiscovery = Autodiscovery.StartResponder(ConfigSection.Default.Autodiscovery.Select(x => x.Url));
            }

            if (startActions != null)
                startActions.ForEach(_ => _());

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            if (stopActions != null)
                stopActions.ForEach(_ => _());

            if (webApp != null)
            {
                webApp.Dispose();
                webApp = null;
            }

            if (autodiscovery != null)
            {
                autodiscovery.Dispose();
                autodiscovery = null;
            }
            
            return true;
        }

    }
}
