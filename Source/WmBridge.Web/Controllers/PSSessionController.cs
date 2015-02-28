//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
                return Json(new { 
                    Session = PSSessionManager.Default.Connect(options),
                    Version = GetVersion()
                });
            }
            catch (PSRemotingTransportException ex)
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
            var components = new List<object>();

            var discover = InvokePowerShell(@"New-Object PSObject -Property @{IIS = (Get-Item HKLM:\software\microsoft\InetStp -ea si) -ne $null}", 
                PSSelect("IIS")).Single();
            
            components.Add(new { Name = "process", Caption = "Processes" });
            components.Add(new { Name = "service", Caption = "Services" });
            
            if (Convert.ToBoolean(discover["IIS"])) 
                components.Add(new { Name = "iis", Caption = "IIS Management" });

            components.Add(new { Name = "eventlog", Caption = "Event Viewer" });
            components.Add(new { Name = "certificate", Caption = "Certificates" });

            return Json(new
            {
                Properties = new { BridgeVersion = GetVersion() },
                Components = components
            });
        }

    }
}
