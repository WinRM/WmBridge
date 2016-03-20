//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Web.Http;
using WmBridge.Web.Filters;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("machine")]
    public class MachineController : PSApiController
    {
        public class Sample
        {
            public long BaseValue { get; set; }
            public long CounterFrequency { get; set; }
            public long RawValue { get; set; }
            public long SystemFrequency { get; set; }
            public long TimeStamp { get; set; }
            public long TimeStamp100nSec { get; set; }
            public int CounterType { get; set; }

            public object[] GetConstructorArguments()
            {
                return new object[] {
                    this.RawValue, 
                    this.BaseValue, 
                    this.CounterFrequency, 
                    this.SystemFrequency, 
                    this.TimeStamp, 
                    this.TimeStamp100nSec, 
                    (System.Diagnostics.PerformanceCounterType)this.CounterType };
            }
        }

        public class PerformanceInfoArgs
        {
            public Sample UserSample { get; set; }
            public Sample KernelSample { get; set; }
        }

        [Route("restart"), HttpGet] // usage: /machine/restart?force=1
        public IHttpActionResult Restart(int force = 0)
        {
            InvokePowerShell("Restart-Computer" + (force == 1 ? " -Force" : ""));
            return Ok();
        }

        [Route("shutdown"), HttpGet]
        public IHttpActionResult Shutdown(int force = 0)
        {
            InvokePowerShell("Stop-Computer" + (force == 1 ? " -Force" : ""));
            return Ok();
        }

        [Route("cpuinfo"), HttpGet]
        public IHttpActionResult CpuInfo()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_Processor", PSSelect(
                "DeviceID", "Name", "Description", "Manufacturer", "SocketDesignation", "MaxClockSpeed", "NumberOfCores", "NumberOfLogicalProcessors", "LoadPercentage")));
        }

        [Route("osinfo"), HttpGet]
        public IHttpActionResult OsInfo()
        {
            var result = InvokePowerShell("Get-WmiObject Win32_OperatingSystem", PSSelect(
                "CSName".Alias("ComputerName"),
                "Caption".Alias("OSName"),
                "CSDVersion".Alias("ServicePack"),
                "OSArchitecture",
                "Version",
                "SerialNumber",
                "InstallDate".TransformDmtfDate(),
                "TotalVirtualMemorySize",
                "FreeVirtualMemory",
                "TotalVisibleMemorySize",
                "FreePhysicalMemory",
                "SizeStoredInPagingFiles",
                "FreeSpaceInPagingFiles",
                "LastBootUpTime".TransformDmtfDate(),
                "LocalDateTime".TransformDmtfDate(),
                "NumberOfProcesses",
                "Description",
                "Organization",
                "RegisteredUser",
                "PSVersion".Expression("$PSVersionTable.PSVersion.ToString()")
                )).Single();

            result["UpTime"] = ((DateTime)result["LocalDateTime"] - (DateTime)result["LastBootUpTime"]).Ticks;

            return Json(result);
        }

        [Route("performance"), HttpPost]
        public IHttpActionResult PerformanceInfo([FromBody] PerformanceInfoArgs args)
        {
            var counterProperties = PSSelectTagged(1,
                "BaseValue",
                "CounterFrequency",
                "RawValue",
                "SystemFrequency",
                "TimeStamp",
                "TimeStamp100nSec",
                "CounterType".As<int>());

            var memoryProperties = PSSelectTagged(2,
                    "TotalVirtualMemorySize",
                    "FreeVirtualMemory",
                    "TotalVisibleMemorySize",
                    "FreePhysicalMemory");

            var statsProperties = PSSelectTagged(3,
                    "NumberOfProcesses",
                    "LastBootUpTime".TransformDmtfDate(),
                    "LocalDateTime".TransformDmtfDate());

            var utilizationProperties = PSSelectTagged(4, "u1", "u2");

            string script =
                "function CalcPerfSample ($s1,$s2) { return [System.Diagnostics.CounterSample]::Calculate((New-Object System.Diagnostics.CounterSample $s1),$s2)}" +
                "function GetPerfSample ($name) { return (New-Object System.Diagnostics.PerformanceCounter 'Processor',$name,'_Total').NextSample()}"+
                "$c1 = (GetPerfSample '% Privileged Time');$c2 = (GetPerfSample '% User Time');" +
                    PSArray(
                        AppendSelectCommand("$c1", counterProperties), // Privileged Time
                        AppendSelectCommand("$c2", counterProperties), // User Time
                        AppendSelectCommand("gwmi Win32_OperatingSystem", memoryProperties), // Memory usage
                        AppendSelectCommand("gwmi Win32_OperatingSystem", statsProperties), // Stats
                        args == null ? "$null" : AppendSelectCommand("New-Object PSObject -Property @{u1=(CalcPerfSample $args[0] $c1);u2=(CalcPerfSample $args[1] $c2)}", utilizationProperties)); // Compute CPU utilization

            var result = InvokePowerShell(script, PSSelect(counterProperties, memoryProperties, utilizationProperties, statsProperties),
                args == null ? new object[0] : new object[] { args.KernelSample.GetConstructorArguments(), args.UserSample.GetConstructorArguments() });

            var mem = result[2];
            var stats = result[3];

            return Json(new
            {
                Cpu = new
                {
                    Sample = new
                    {
                        Kernel = result[0],
                        User = result[1]
                    },
                    Utilization = new
                    {
                        Kernel = result.Length == 5 ? result[4]["u1"] : 0,
                        User = result.Length == 5 ? result[4]["u2"] : 0
                    }
                },
                Memory = result[2],
                Processes = stats["NumberOfProcesses"],
                UpTime = ((DateTime)stats["LocalDateTime"] - (DateTime)stats["LastBootUpTime"]).Ticks
            });

        }
    }
}
