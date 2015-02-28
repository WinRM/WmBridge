//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Management.Automation;
using System.Web.Http;
using System.Net.Http;
using WmBridge.Web.Filters;
using System;
using System.ServiceProcess;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("service")]
    public class ServiceController : PSApiController
    {
        public class SetAccountArgs
        {
            [Required]
            public string Account { get; set; }

            public string Password { get; set; }
        }

        [Route(""), HttpGet]
        public IHttpActionResult Get()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_Service", PSSelect(
                "Name", "StartMode", "AcceptPause", "AcceptStop", "DisplayName", "Started", "StartName", "State")));
        }

        [Route("{service}/detail"), HttpGet]
        public IHttpActionResult Detail(string service)
        {
            var wmiSvc = PSSelectTagged(0,
                "Name", "DesktopInteract", "PathName", "StartMode", "AcceptPause", "AcceptStop", "Description", "DisplayName", "ProcessId", "Started", "StartName", "State");

            var psSvc = PSSelectTagged(1,
                "DelayedAutostart".Expression(@"[bool](Get-ItemProperty ""HKLM:\SYSTEM\CurrentControlSet\Services\$($_.Name)"" -Name DelayedAutostart -ErrorAction SilentlyContinue | select -expand DelayedAutostart)"),
                "RequiredServices".Transform(CastDepArray), "DependentServices".Transform(CastDepArray));

            string script = PSArray(
                AppendSelectCommand("Get-WmiObject Win32_Service -F \"Name='$($args[0])'\"", wmiSvc),
                AppendSelectCommand("Get-Service $args[0] | % { New-Object PSObject -Property @{ Name = $_.Name;" +
                                        "RequiredServices  = @($_.ServicesDependedOn | % { @{0=$_.ServiceName;1=$_.DisplayName;2=$_.ServiceType}});" +
                                        "DependentServices = @($_.DependentServices | % { @{0=$_.ServiceName;1=$_.DisplayName;2=$_.ServiceType}}) }}", psSvc));

            return Json(InvokePowerShell(script, PSSelect(wmiSvc, psSvc), service).Merge());        
        }

        [Route("{service}/{operation:regex(^(restart|suspend|resume)$)}"), HttpGet]
        public IHttpActionResult ServiceOperation(string service, string operation)
        {
            InvokePowerShell(operation + "-Service $args[0]", service);
            return Ok();
        }

        [Route("{service}/{method:regex(^(start|stop)$)}"), HttpGet]
        public IHttpActionResult InvokeServiceMethod(string service, string method)
        {
            InvokePowerShell("(Get-Service $args[0])." + method + "()", service);
            return Ok();
        }

        [Route("{service}/startup/{startupType:regex(^(auto|manual|disabled|delayed)$)}"), HttpGet]
        public IHttpActionResult SetStartupType(string service, string startupType)
        {
            if (string.Equals(startupType, "delayed", StringComparison.InvariantCultureIgnoreCase))
                InvokePowerShell("$out = (sc.exe config $args[0] start= delayed-auto); if ($LASTEXITCODE) {Write-Error ([string]$out)} $out", service);
            else
                InvokePowerShell("Set-Service $args[0] -StartupType $args[1]", service, startupType);

            return Ok();
        }

        [Route("{service}/{operation:regex(^(displayname|description)$)}"), HttpPost]
        public IHttpActionResult SetNaming(string service, string operation, [FromBody] string value)
        {
            InvokePowerShell("Set-Service $args[0] -" + operation + " $args[1]", service, value);
            return Ok();
        }

        [Route("{service}/path"), HttpPost]
        public IHttpActionResult SetPath(string service, [FromBody] string value)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_Service -F \"Name='$($args[0])'\").Change($null,$args[1])", PSSelectReturnValue(), service, value).Single());
        }

        [Route("{service}/account"), HttpPost, ValidateModel]
        public IHttpActionResult SetAccount(string service, [FromBody] SetAccountArgs args)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_Service -F \"Name='$($args[0])'\").Change($null,$null,$null,$null,$null,$null,$args[1],$args[2])",
                PSSelectReturnValue(), service, args.Account, args.Password ?? "").Single());
        }

        private object CastDepArray(object source)
        {
            if (source == null)
                return null;
            else
                return ((ArrayList)((PSObject)source).ImmediateBaseObject)
                    .Cast<PSObject>().Select(x => (Hashtable)x.ImmediateBaseObject)
                    .Select(x => new { Name = x[0], Caption = x[1], IsService = IsService((ServiceType)((PSObject)x[2]).ImmediateBaseObject) });
        }

        private bool IsService(ServiceType serviceType)
        {
            return (serviceType.HasFlag(ServiceType.Win32OwnProcess) || serviceType.HasFlag(ServiceType.Win32ShareProcess));
        }

        static readonly Dictionary<int, string> Win32_Service_Change_ErrorCodes = new Dictionary<int, string>()
        {
            { 0, "The request was accepted." },
            { 1, "The request is not supported." },
            { 2, "The user did not have the necessary access." },
            { 3, "The service cannot be stopped because other services that are running are dependent on it." },
            { 4, "The requested control code is not valid, or it is unacceptable to the service." },
            { 5, "The requested control code cannot be sent to the service because the state of the service (Win32_BaseService State property) is equal to 0, 1, or 2." },
            { 6, "The service has not been started." },
            { 7, "The service did not respond to the start request in a timely fashion." },
            { 8, "Unknown failure when starting the service." },
            { 9, "The directory path to the service executable file was not found." },
            { 10, "The service is already running." },
            { 11, "The database to add a new service is locked." },
            { 12, "A dependency this service relies on has been removed from the system." },
            { 13, "The service failed to find the service needed from a dependent service." },
            { 14, "The service has been disabled from the system." },
            { 15, "The service does not have the correct authentication to run on the system." },
            { 16, "This service is being removed from the system." },
            { 17, "The service has no execution thread." },
            { 18, "The service has circular dependencies when it starts." },
            { 19, "A service is running under the same name." },
            { 20, "The service name has invalid characters." },
            { 21, "Invalid parameters have been passed to the service." },
            { 22, "The account under which this service runs is either invalid or lacks the permissions to run the service." },
            { 23, "The service exists in the database of services available from the system." },
            { 24, "The service is currently paused in the system." },
        };

        private WmBridge.Web.Model.PSPropertySelector[] PSSelectReturnValue()
        {
            return PSSelect("ReturnValue".Transform(CheckReturnValue));
        }

        private object CheckReturnValue(object returnValue)
        {
            int errorCode = Convert.ToInt32(returnValue);

            if (errorCode == 0)
                return errorCode;

            string error;
            if (!Win32_Service_Change_ErrorCodes.TryGetValue(errorCode, out error))
                error = "Undefined error";

            throw new HttpResponseException(Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, error));
        }

    }
}
