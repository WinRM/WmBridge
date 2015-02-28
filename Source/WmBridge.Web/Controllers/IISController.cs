//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Management.Automation;
using System.Web.Http;
using WmBridge.Web.Filters;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("iis")]
    // Load WebAdministration as Module for IIS 7.5 and higher, otherwise as Snap-In
    [StartupPSScript("$iisVersion = Get-ItemProperty 'HKLM:\\software\\microsoft\\InetStp'; if (($iisVersion.MajorVersion -gt 7) -Or ($iisVersion.MajorVersion -eq 7 -And $iisVersion.MinorVersion -ge 5)) { Import-Module WebAdministration } else { Add-PSSnapIn WebAdministration }", "Load WebAdministration")]
    public class IISController : PSApiController
    {
        public class AppPoolAssignArgs
        {
            [Required]
            public string WebApplication { get; set; }
        }

        public class AppPooIdentityArgs
        {
            [Required]
            public string UserName { get; set; }

            public string Password { get; set; }
        }

        public class ChangeBindingArgs
        {
            [Required]
            public BindingInfo Old { get; set; }
            [Required]
            public BindingInfo New { get; set; }
        }

        public class BindingInfo : Dictionary<string, object>
        {
            public BindingInfo(Dictionary<string, object> properties)
                :base(properties)
            {
                if (this.ContainsKey("bindingInformation") && string.IsNullOrEmpty((string)this["bindingInformation"]))
                    this.Remove("bindingInformation");

                if (this.ContainsKey("bindingInformation") && (Protocol == "http" || Protocol == "https" || Protocol == "ftp"))
                {
                    string[] splitted = ((string)this["bindingInformation"]).Split(':');
                    this.Add("ip", splitted[0]);
                    this.Add("port", int.Parse(splitted[1]));
                    this.Add("hostName", splitted[2]);

                    this.Remove("bindingInformation");
                }

                if (this.ContainsKey("sslFlags"))
                {
                    int sslFlags = Convert.ToInt32((this["sslFlags"] ?? 0));
                    
                    if ((sslFlags & 1) != 0)
                        this.Add("sni", true);

                    if ((sslFlags & 2) != 0)
                        this.Add("ccs", true);

                    this.Remove("sslFlags");
                }
            }

            [JsonIgnore]
            public string Protocol { get { return (string)this["protocol"]; } }

            [JsonIgnore]
            public string BindingInformation { get { return this.ContainsKey("bindingInformation") ? (string)this["bindingInformation"] : string.Join(":", IP, Port, HostName); } }

            [JsonIgnore]
            public string HostName { get { return this.ContainsKey("hostName") && !string.IsNullOrEmpty(this["hostName"] as string) ? (string)this["hostName"] : ""; } }

            [JsonIgnore]
            public string IP { get { return (string)this["ip"]; } }

            [JsonIgnore]
            public int Port { get { return Convert.ToInt32(this["port"]); } }

            [JsonIgnore]
            public string CertThumbprint { get { return this.ContainsKey("certificateHash") ? (string)this["certificateHash"] : null; } }
            
            [JsonIgnore]
            public string CertStore { get { return this.ContainsKey("certificateStoreName") ? (string)this["certificateStoreName"] : null; } }

            [JsonIgnore]
            public int SslFlags { get { return (SNI ? 1 : 0) | (CCS ? 2 : 0); } }

            [JsonIgnore]
            public bool SNI { get { return this.ContainsKey("sni") && Convert.ToBoolean(this["sni"]); } }

            [JsonIgnore]
            public bool CCS { get { return this.ContainsKey("ccs") && Convert.ToBoolean(this["ccs"]); } }

            [JsonIgnore]
            public bool HasCertificate { get { return this.ContainsKey("certificateHash") && !string.IsNullOrEmpty(CertThumbprint); } }
            
            [JsonIgnore]
            public string SslBinding 
            { 
                get
                {
                    if (SslFlags == 0)
                        return (this.ContainsKey("ip") && this.ContainsKey("port")) ? string.Join("!", IP == "*" ? "0.0.0.0" : IP, Port) : null; 
                    else
                        return (this.ContainsKey("hostName") && this.ContainsKey("port")) ? string.Join("!", "", Port, HostName ?? "") : null; 
                }
            }

        }

        [Route("apppool"), HttpGet]
        public IHttpActionResult GetAppPools()
        {
            return Json(InvokePowerShell("Get-ChildItem IIS:\\AppPools\\", PSSelect("Name", "State")));
        }

        [Route("apppool/{apppool}/detail"), HttpGet]
        public IHttpActionResult GetAppPoolDetail(string apppool)
        {
            return Json(InvokePowerShell("$apps = @(Get-WebConfiguration \"/system.applicationHost/sites/site/application[@applicationPool='$($args[0])']\" -pspath MACHINE/WEBROOT/APPHOST).Count; Get-Item \"IIS:\\AppPools\\$($args[0])\"", PSSelect(
                "Name".Expression("$_.name"), 
                "State".Expression("$_.state"), 
                "ManagedPipeline".Expression("$_.managedPipelineMode"), 
                "Version".Expression("$_.managedRuntimeVersion"),
                "Enable32Bit".Expression("$_.enable32BitAppOnWin64"),
                "AutoStart".Expression("$_.autoStart"), 
                "Identity".Expression("if ( $_.processModel.identityType -eq \"SpecificUser\") { $_.processModel.userName } else { $_.processModel.identityType }"),
                "WorkerProcesses".Expression("$_.workerProcesses.Collection.Count"),
                "WebApplications".Expression("$apps")), apppool).Single());
        }

        [Route("apppool/{apppool}/processes"), HttpGet]
        public IHttpActionResult GetWorkerProcesses(string apppool)
        {
            return Json(InvokePowerShell("Get-ChildItem \"IIS:\\AppPools\\$($args[0])\\WorkerProcesses\" | % { Get-Process -Id $_.processId -ea SilentlyContinue }", ProcessController.GetProcessProperties(), apppool));
        }

        [Route("apppool/{apppool}/{operation:regex(^(start|stop|restart)$)}"), HttpGet]
        public IHttpActionResult AppPoolOperation(string apppool, string operation)
        {
            InvokePowerShell(operation + "-WebAppPool $args[0]", apppool);
            return Ok();
        }

        [Route("apppool/{apppool}/{operation:regex(^(enable32Bit|disable32Bit|enableautostart|disableautostart)$)}"), HttpGet]
        public IHttpActionResult AppPoolSetProperty(string apppool, string operation)
        {
            Dictionary<string, string> scripts = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"enable32Bit", "enable32BitAppOnWin64 $true"},
                {"disable32Bit", "enable32BitAppOnWin64 $false"},
                {"enableautostart", "autoStart $true"},
                {"disableautostart", "autoStart $false"},
            };

            InvokePowerShell("Set-ItemProperty \"IIS:\\AppPools\\$($args[0])\" " + scripts[operation], apppool);
            return Ok();
        }

        [Route("apppool/{apppool}/rename"), HttpPost, ValidateModel]
        public IHttpActionResult RenameAppPool(string apppool, [FromBody] string newName)
        {
            InvokePowerShell("Set-ItemProperty \"IIS:\\AppPools\\$($args[0])\" name $args[1]", apppool, newName);
            return Ok();
        }

        [Route("apppool/{apppool}/pipeline/{mode}"), HttpGet]
        public IHttpActionResult SetAppPoolPipelineMode(string apppool, string mode)
        {
            int value;

            if (mode == "Integrated")
                value = 0;
            else if (mode == "Classic")
                value = 1;
            else
                throw new ArgumentException("Invalid mode: " + mode);

            InvokePowerShell("Set-ItemProperty \"IIS:\\AppPools\\$($args[0])\" managedPipelineMode $args[1]", apppool, value);
            return Ok();
        }

        [Route("apppool/{apppool}/identity"), HttpPost, ValidateModel]
        public IHttpActionResult SetAppPoolIdentity(string apppool, [FromBody] AppPooIdentityArgs args)
        {
            string identityType, script = "";

            if (string.IsNullOrEmpty(args.Password))
            {
                identityType = args.UserName;
            }
            else
            {
                identityType = "SpecificUser";
                script = "$pool.processModel.username = $args[2]; $pool.processModel.password = $args[3];";
            }

            InvokePowerShell("$pool = (Get-Item \"IIS:\\AppPools\\$($args[0])\"); $pool.processModel.identityType = $args[1];" + script + "$pool | Set-Item ", 
                apppool, identityType, args.UserName, args.Password);

            return Ok();
        }

        [Route("site/{site}/apppool/{apppool}"), HttpGet]
        public IHttpActionResult SetAppPool(string site, string apppool)
        {
            InvokePowerShell("Set-ItemProperty \"IIS:\\Sites\\$($args[0])\" ApplicationPool $args[1]", site, apppool);
            return Ok();
        }

        [Route("apppool/{apppool}/assign"), HttpPost, ValidateModel]
        public IHttpActionResult AppPoolAssign(string apppool, [FromBody] AppPoolAssignArgs args)
        {
            InvokePowerShell("Set-ItemProperty \"IIS:\\Sites\\$($args[0])\" ApplicationPool $args[1]", args.WebApplication, apppool);
            return Ok();
        }

        [Route("site"), HttpGet]
        public IHttpActionResult GetWebsites()
        {
            return Json(InvokePowerShell("Get-Website", PSSelect("Id", "Name", "State", GetBindingsProperty())));
        }

        [Route("site/{site}/detail"), HttpGet]
        public IHttpActionResult GetWebsite(string site)
        {
            return Json(InvokePowerShell("Get-Item \"IIS:\\sites\\$($args[0])\"", PSSelect("Id", "Name", "State", "ApplicationPool", "PhysicalPath", GetBindingsProperty()), site).Single());
        }

        WmBridge.Web.Model.PSPropertySelector GetBindingsProperty()
        {
            return "Bindings".Expression("@($_.bindings.Collection | % { if ($_.protocol -eq 'https') { $_ | select protocol,bindingInformation,sslFlags,certificateStoreName,certificateHash }else{$_ | select protocol,bindingInformation}})").Transform(TransfromBindingsArray);
        }

        static object TransfromBindingsArray(object x)
        {
            if (((PSObject)x).ImmediateBaseObject is ArrayList)
                return (((PSObject)x).ImmediateBaseObject as ArrayList).Cast<PSObject>().Select(TransfromBindingsItem).ToArray();
            else
                return new[] { TransfromBindingsItem((PSObject)x) };
        }

        static object TransfromBindingsItem(PSObject obj)
        {
            return new BindingInfo(obj.Properties.ToDictionary(x => x.Name, x => x.Value));
        }

        [Route("site/{site}/{operation:regex(^(start|stop)$)}"), HttpGet]
        public IHttpActionResult SiteOperation(string site, string operation)
        {
            InvokePowerShell(operation + "-Website $args[0]", site);
            return Ok();
        }

        [Route("site/{site}/restart"), HttpGet]
        public IHttpActionResult RestartWebsite(string site)
        {
            InvokePowerShell("Restart-WebItem \"IIS:\\sites\\$($args[0])\"", site);
            return Ok();
        }

        [Route("site/{site}/applications"), HttpGet]
        public IHttpActionResult GetWebsiteApplications(string site)
        {
            return Json(InvokePowerShell("(Get-Item \"IIS:\\sites\\$($args[0])\").Collection | ? Path -ne '/'",
                PSSelect("Site".Transform(_ => site), "Path", "ApplicationPool", "EnabledProtocols"), site));
        }

        [Route("apppool/{apppool}/applications"), HttpGet]
        public IHttpActionResult GetAppPoolApplications(string apppool)
        {
            return Json(InvokePowerShell("$pool=$args[0];Get-WebConfiguration \"/system.applicationHost/sites/site/application[@applicationPool='$pool']/parent::node()\" -pspath MACHINE/WEBROOT/APPHOST" +
                                         "| % {$__=$_; $_.Collection | ? applicationPool -eq $pool | % {@{s=$__;a=$_}}}", PSSelect(
                "Site".Expression("$_.s.name"),
                "Path".Expression("$_.a.path"),
                "ApplicationPool".Expression("$_.a.applicationPool"), 
                "EnabledProtocols".Expression("$_.a.enabledProtocols")), apppool));
        }

        [Route("site/{site}/rename"), HttpPost, ValidateModel]
        public IHttpActionResult RenameSite(string site, [FromBody] string newName)
        {
            InvokePowerShell("Set-ItemProperty \"IIS:\\Sites\\$($args[0])\" name $args[1]", site, newName);
            return Ok();
        }

        [Route("site/{site}/binding/create"), HttpPost, ValidateModel]
        public IHttpActionResult CreateBinding(string site, [FromBody] BindingInfo binding)
        {
            InvokePowerShell(
                (binding.HasCertificate ?
                "New-Item \"IIS:\\SslBindings\\$($args[7])\" -Value (Get-Item \"cert:\\LocalMachine\\$($args[5])\\$($args[6])\") -Force -ErrorAction SilentlyContinue;" : "") +
                "New-WebBinding -Name $args[0] -IP $args[1] -Port $args[2] -HostHeader $args[3] -Protocol $args[4]" + (binding.SslFlags != 0 ? " -SslFlags " + binding.SslFlags : ""),
                    site, binding.IP, binding.Port, binding.HostName, binding.Protocol, binding.CertStore, binding.CertThumbprint, binding.SslBinding);

            return Ok();
        }

        [Route("site/{site}/binding/delete"), HttpPost, ValidateModel]
        public IHttpActionResult RemoveBinding(string site, [FromBody] BindingInfo binding)
        {
            InvokePowerShell(
                "Remove-WebBinding -Name $args[0] -BindingInformation $args[1];" +
                (binding.HasCertificate ? "Remove-Item \"IIS:\\SslBindings\\$($args[2])\"" : ""),
                    site, binding.BindingInformation, binding.SslBinding);

            return Ok();
        }

        [Route("site/{site}/binding/change"), HttpPost, ValidateModel]
        public IHttpActionResult ChangeBinding(string site, [FromBody] ChangeBindingArgs args)
        {
            string script = "";

            if (args.New.CertThumbprint != args.Old.CertThumbprint ||
                args.New.CertStore != args.Old.CertStore ||
                args.New.SslBinding != args.Old.SslBinding)
            {
                if (args.Old.SslBinding != args.New.SslBinding)
                    script += "if (Get-Item \"IIS:\\SslBindings\\$($args[4])\" -ErrorAction SilentlyContinue) {throw \"SSL binding for end point already exist.\"};";

                if (args.Old.HasCertificate)
                    script += "Get-Item \"IIS:\\SslBindings\\$($args[3])\" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue;";

                if (args.New.HasCertificate)
                    script += "New-Item \"IIS:\\SslBindings\\$($args[4])\" -Value (Get-Item \"cert:\\LocalMachine\\$($args[5])\\$($args[6])\") -Force -ErrorAction SilentlyContinue;";
            }

            script += "Set-WebConfigurationProperty \"/system.applicationHost/sites/site[@name='$($args[0])']/bindings/binding[@bindingInformation='$($args[1])']\" -Name bindingInformation -Value $args[2] -Force;";

            if (args.New.SslFlags != args.Old.SslFlags)
                script += "Set-WebBinding -Name $args[0] -BindingInformation $args[1] -PropertyName SslFlags -Value $args[7];";

            InvokePowerShell(script, site, 
                args.Old.BindingInformation, args.New.BindingInformation, 
                args.Old.SslBinding, args.New.SslBinding,
                args.New.CertStore, args.New.CertThumbprint,
                args.New.SslFlags);

            return Ok();
        }

    }
}
