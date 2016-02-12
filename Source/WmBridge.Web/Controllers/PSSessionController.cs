//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using WmBridge.Web.Filters;
using WmBridge.Web.Model;

namespace WmBridge.Web.Controllers
{
    public class PSSessionController : PSApiController
    {
        [Route("connect"), HttpPost, AllowAnonymous, ValidateModel]
        public IHttpActionResult Connect([FromBody] PSConnection options)
        {
            Log.InfoFormat("New connection request: {0}", options);

            try
            {
                object state;
                var session = PSSessionManager.Default.Connect(options, out state);
                Request.Properties[PSSessionManager.PSConnectionStateKey] = state;

                return Json(new {
                    Session = session,
                    Version = GetVersion(),
                    PSVersion = Request.GetPSVersionString()
                });
            }
            catch (Exception ex) when (ex is PSRemotingTransportException || ex is RemoteException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [Route("winrmconfig"), HttpPost, AllowAnonymous, ValidateModel]
        public IHttpActionResult EnableWinRM([FromBody] PSConnection options)
        {
            try
            {
                RemoteActivation.ConfigureWinRM(options.ComputerName, options.UserName, options.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, new HttpError(ex.Message)));
            }
        }

        string GetVersion()
        {
            return ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute)).Single()).Version;
        }

        [Route("disconnect"), HttpGet]
        public IHttpActionResult Disconnect()
        {
            string session = Request.Headers.GetValues(PSSessionManager.XPSSessionHeader).SingleOrDefault();

            if (string.IsNullOrEmpty(session))
                return BadRequest();

            PSSessionManager.Default.Disconnect(session);

            return Ok();
        }

        [Route("capabilities"), HttpGet]
        public IHttpActionResult Capabilities()
        {
            Version clientVersion = Request.GetClientVersion() ?? new Version();

            var components = new List<object>();

            var discover = InvokePowerShell(@"New-Object PSObject -Property @{" +
                   @"IIS = (gi ""HKLM:\software\microsoft\InetStp"" -ea SilentlyContinue) -ne $null;" +
                @"HyperV = (gp ""HKLM:\software\microsoft\Windows NT\CurrentVersion\Virtualization"" ""Version"" -ea SilentlyContinue).Version -ne $null}",
                            PSSelect("IIS", "HyperV")).Single();
            
            components.Add(new { Name = "process", Caption = "Processes" });
            components.Add(new { Name = "service", Caption = "Services" });
            
            if (Convert.ToBoolean(discover["IIS"])) 
                components.Add(new { Name = "iis", Caption = "IIS Management" });

            components.Add(new { Name = "eventlog", Caption = "Event Viewer" });
            components.Add(new { Name = "certificate", Caption = "Certificates" });

            if (clientVersion >= Version.Parse("1.3") && Convert.ToBoolean(discover["HyperV"]))
                components.Add(new { Name = "hyperv", Caption = "Hyper-V" });

            return Json(new
            {
                Properties = new { BridgeVersion = GetVersion() },
                Components = components
            });
        }

    }
}
