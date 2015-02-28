//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Web.Http;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("process/{id}")]
    public class ProcessController : PSApiController
    {
        [Route("~/process"), HttpGet]
        public IHttpActionResult Get()
        {
            return Json(InvokePowerShell("Get-Process", GetProcessProperties()));
        }

        [Route("detail"), HttpGet]
        public IHttpActionResult Detail(int id)
        {
            var processSelect = PSSelectTagged(0,
                "Id",
                "Name",
                "StartTime".Expression("($_.StartTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "RunningTime".Expression("((Get-Date) - $_.StartTime).Ticks"),
                "Company",
                "Description",
                "FileVersion",
                "Path",
                "Product",
                "ProductVersion",
                "Responding",
                "SessionId",
                "PriorityClass".As<int>(),
                "ProcessorAffinity".As<long>(),
                "ProcessorCount".Expression("[int]$env:NUMBER_OF_PROCESSORS"));
            
            var wmiProcessSelect = PSSelectTagged(1,
                "ParentProcessId",
                "CommandLine",
                "HandleCount",
                "ThreadCount",
                "KernelModeTime",
                "UserModeTime",
                "VirtualSize",
                "WorkingSetSize",
                "PrivatePageCount".Alias("PrivateMemorySize"),
                "CurrentTime".Expression("(Get-Date).ToUniversalTime().Ticks"));

            var wmiProcessOwnerSelect = PSSelectTagged(2,
                "User",
                "Domain");

            var wmiServiceSelect = PSSelectTagged(3,
                "Name".Alias("ServiceName"),
                "Caption".Alias("ServiceCaption"));

            string script =
                "&{param($_args)" +
                "$wmiProcess = Get-WmiObject Win32_Process -F \"ProcessId=$($_args[0])\";" +
                "$wmiService = Get-WmiObject Win32_Service -F \"ProcessId=$($_args[0])\";" +
                PSArray(
                    AppendSelectCommand("Get-Process -Id $_args[0]", processSelect),
                    AppendSelectCommand("$wmiService", wmiServiceSelect),
                    AppendSelectCommand("$wmiProcess", wmiProcessSelect),
                    AppendSelectCommand("$wmiProcess.GetOwner()", wmiProcessOwnerSelect)) +
                "} $args";

            return Json(InvokePowerShell(script, PSSelect(processSelect, wmiServiceSelect, wmiProcessSelect, wmiProcessOwnerSelect), id).Merge());
        }

        [Route("stop"), HttpGet]
        public IHttpActionResult Stop(int id)
        {
            InvokePowerShell("Stop-Process $args[0] -Force", id);
            return Ok();
        }

        [Route("modules"), HttpGet]
        public IHttpActionResult Modules(int id)
        {
            return Json(InvokePowerShell("(Get-Process -Id $args[0]).Modules", PSSelect(
                "ModuleName".Alias("Name"), 
                "BaseAddress".As<long>(), 
                "ModuleMemorySize".Alias("MemorySize"), 
                "FileName"), id));
        }

        [Route("priority/{priority}"), HttpGet]
        public IHttpActionResult SetPriority(int id, int priority)
        {
            InvokePowerShell("((Get-Process -Id $args[0]).PriorityClass=$args[1])", id, priority);
            return Ok();
        }

        [Route("affinity/{affinity}"), HttpGet]
        public IHttpActionResult SetAffinity(int id, long affinity)
        {
            InvokePowerShell("((Get-Process -Id $args[0]).ProcessorAffinity=$args[1])", id, affinity);
            return Ok();
        }

        [Route("stats"), HttpGet]
        public IHttpActionResult SingleStats(int id)
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_Process -F \"ProcessId=$($args[0])\"", GetStatsProperties(), id).Single());
        }

        [Route("~/process/stats"), HttpGet]
        public IHttpActionResult AllStats()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_Process", GetStatsProperties()));
        }

        internal static WmBridge.Web.Model.PSPropertySelector[] GetProcessProperties()
        {
            return PSSelect(
                "Id",
                "Name",
                "Description",
                "PrivateMemorySize64".Alias("PrivateMemorySize"),
                "TotalProcessorTime".Expression("($_.TotalProcessorTime).Ticks"),
                "CurrentTime".Expression("(Get-Date).ToUniversalTime().Ticks"),
                "ProcessorCount".Expression("[int]$env:NUMBER_OF_PROCESSORS"));
        }

        WmBridge.Web.Model.PSPropertySelector[] GetStatsProperties()
        {
            return PSSelect(
                "ProcessId".Alias("Id"),
                "ProcessorCount".Expression("[int]$env:NUMBER_OF_PROCESSORS"),
                "CurrentTime".Expression("(Get-Date).ToUniversalTime().Ticks"),
                "KernelModeTime",
                "UserModeTime",
                "VirtualSize",
                "WorkingSetSize",
                "PrivatePageCount".Alias("PrivateMemorySize"),
                "HandleCount",
                "ThreadCount");
        }
    }
}
