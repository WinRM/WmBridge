//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using System.Net.Http;
using WmBridge.Web.Filters;
using WmBridge.Web.Model;

namespace WmBridge.Web.Controllers
{
    [PSVersion("3.0")]
    [RoutePrefix("hyperv")]
    public class HyperVController : PSApiController
    {
        public class MemoryArgs
        {
            [Required]
            public bool Dynamic { get; set; }

            [Required]
            public Int64 Minimum { get; set; }

            [Required]
            public Int64 Maximum { get; set; }

            [Required]
            public Int64 Startup { get; set; }

            [Required]
            public Int32 Buffer { get; set; }

            [Required]
            public Int32 Priority { get; set; }
        }

        public class StartStopActionArgs
        {
            [Required]
            public string AutomaticStartAction { get; set; }

            [Required]
            public int AutomaticStartDelay { get; set; } // in seconds

            [Required]
            public string AutomaticStopAction { get; set; }
        }

        public class CpuArgs
        {
            [Required]
            public bool CompatibilityForMigration { get; set; }

            [Required]
            public bool CompatibilityForOlderOperatingSystems { get; set; }

            [Required]
            public int Count { get; set; }

            [Required]
            public int Maximum { get; set; } // VM limit (percentage)

            [Required]
            public int MaximumCountPerNumaNode { get; set; }

            [Required]
            public int MaximumCountPerNumaSocket { get; set; }

            [Required]
            public int RelativeWeight { get; set; } // from 1 to 10000

            [Required]
            public int Reserve { get; set; } // VM reserve (percentage)
        }

        public class ResizeVhdArgs
        {
            [Required]
            public string Path { get; set; }

            [Required]
            public long Size { get; set; }
        }

        [Route(""), HttpGet]
        public IHttpActionResult GetAll()
        {
            return Json(InvokePowerShell("Get-VM", VmProperties()));
        }

        [Route("{guid}/detail"), HttpGet]
        public IHttpActionResult GetVmDetail(string guid)
        {
            return Json(InvokePowerShell("(Get-VM -Id $args[0])", VmProperties(), guid).Single());
        }

        PSPropertySelector[] VmProperties()
        {
            return PSSelect(
                "Id",
                "Name",
                "State".As<string>(),
                "Status",
                "Generation",
                "Uptime".Expression("$_.Uptime.Ticks"),
                "CPUUsage",
                "MemoryAssigned",
                "AutomaticStartAction".As<string>(),
                "AutomaticStartDelay",
                "AutomaticStopAction".As<string>());
        }

        [Route("{guid}/{operation:regex(^(start|stop|restart)$)}"), HttpGet]
        public IHttpActionResult VmOperation(string guid, string operation, int force = 0)
        {
            return Json(InvokePowerShell(operation + "-VM (Get-VM -Id $args[0])" + 
                (force == 1 ? " -Force" : "") +
                (force == 1 && operation == "stop" ? " -TurnOff" : "") +
                " -AsJob", PSSelect("InstanceId"), guid).Single());
        }

        [Route("{guid}/memory"), HttpGet]
        public IHttpActionResult GetVmMemory(string guid)
        {
            return Json(InvokePowerShell("$vm = Get-VM -Id $args[0]; Get-VMMemory $vm", PSSelect(
                "Assigned".Expression("$vm.MemoryAssigned"),
                "Startup", 
                "DynamicMemoryEnabled".Alias("Dynamic"),
                "TotalVisibleHostMemory".Expression("(gwmi Win32_OperatingSystem).TotalVisibleMemorySize*1kB"),
                "Minimum", 
                "Maximum", 
                "Buffer",
                "Priority"), guid).Single());
        }

        [Route("{guid}/memory"), HttpPost, ValidateModel]
        public IHttpActionResult SetVmMemory(string guid, [FromBody] MemoryArgs args)
        {
            InvokePowerShell("Set-VMMemory (Get-VM -Id $args[0]) -DynamicMemoryEnabled $args[1] -StartupBytes $args[2] -Priority $args[3]" +
                            (args.Dynamic ? " -MinimumBytes $args[4] -MaximumBytes $args[5] -Buffer $args[6]" : ""),
                guid, args.Dynamic, args.Startup, args.Priority, args.Minimum, args.Maximum, args.Buffer);
            
            return Ok();
        }

        [Route("{guid}/action"), HttpPost, ValidateModel]
        public IHttpActionResult SetVmStartStopAction(string guid, [FromBody] StartStopActionArgs args)
        {
            InvokePowerShell("Set-VM (Get-VM -Id $args[0]) -AutomaticStartAction $args[1] -AutomaticStartDelay $args[2] -AutomaticStopAction $args[3]",
                guid, args.AutomaticStartAction, args.AutomaticStartDelay, args.AutomaticStopAction);
            
            return Ok();
        }

        [Route("{guid}/cpu"), HttpGet]
        public IHttpActionResult GetVmCpu(string guid)
        {
            return Json(InvokePowerShell("Get-VMProcessor (Get-VM -Id $args[0])", PSSelect(
                "Count",
                "CountLimit".Expression("[int]$env:NUMBER_OF_PROCESSORS"),
                "CompatibilityForMigrationEnabled".Alias("CompatibilityForMigration"),
                "CompatibilityForOlderOperatingSystemsEnabled".Alias("CompatibilityForOlderOperatingSystems"),
                "Maximum", 
                "Reserve", 
                "RelativeWeight", 
                "MaximumCountPerNumaNode", 
                "MaximumCountPerNumaSocket"), guid).Single());
        }

        [Route("{guid}/cpu"), HttpPost, ValidateModel]
        public IHttpActionResult SetVmCpu(string guid, [FromBody] CpuArgs args)
        {
            InvokePowerShell("Set-VMProcessor (Get-VM -Id $args[0]) -CompatibilityForMigrationEnabled $args[1] -CompatibilityForOlderOperatingSystemsEnabled $args[2] -Count $args[3] -Maximum $args[4] -MaximumCountPerNumaNode $args[5] -MaximumCountPerNumaSocket $args[6] -RelativeWeight $args[7] -Reserve $args[8]",
                guid, args.CompatibilityForMigration, args.CompatibilityForOlderOperatingSystems, args.Count, args.Maximum, args.MaximumCountPerNumaNode, args.MaximumCountPerNumaSocket, args.RelativeWeight, args.Reserve);
            return Ok();
        }

        [Route("{guid}/hdd"), HttpGet]
        public IHttpActionResult GetHDD(string guid)
        {
            return Json(InvokePowerShell("Get-VMHardDiskDrive (Get-VM -Id $args[0]) | % { $vhd = Get-VHD $_.Path -ea si; New-Object PSObject -Property @{ DiskNumber=$_.DiskNumber;ControllerType=$_.ControllerType; ControllerNumber=$_.ControllerNumber; ControllerLocation=$_.ControllerLocation;Name=$_.Name;Path=$_.Path;VMId=$_.VMId;VMSnapshotId=$_.VMSnapshotId; VMSnapshotName =$_.VMSnapshotName;VhdFormat = $vhd.VhdFormat;VhdType = $vhd.VhdType;FileSize = $vhd.FileSize;Size = $vhd.Size;MinimumSize =$vhd.MinimumSize;LogicalSectorSize = $vhd.LogicalSectorSize;PhysicalSectorSize = $vhd.PhysicalSectorSize;BlockSize = $vhd.BlockSize;ParentPath = $vhd.ParentPath;FragmentationPercentage = $vhd.FragmentationPercentage } }", PSSelect(
                    "DiskNumber",
                    "ControllerType".As<string>(),
                    "ControllerNumber",
                    "ControllerLocation",
                    "Name",
                    "Path",
                    "VMId",
                    "VMSnapshotId",
                    "VMSnapshotName",
                    "VhdFormat".As<string>(),
                    "VhdType".As<string>(),
                    "FileSize".As<long>(),
                    "Size".As<long>(),
                    "MinimumSize".As<long>(),
                    "LogicalSectorSize".As<int>(),
                    "PhysicalSectorSize".As<int>(),
                    "BlockSize".As<long>(),
                    "ParentPath",
                    "FragmentationPercentage".As<int>()), guid));
        }

        [Route("vhd/detail"), HttpPost]
        public IHttpActionResult GetVHD([FromBody] string path)
        {
           return Json(InvokePowerShell("Get-VHD $args[0]", PSSelect(
                    "VhdFormat".As<string>(),
                    "VhdType".As<string>(),
                    "FileSize".As<long>(),
                    "Size".As<long>(),
                    "MinimumSize".As<long>(),
                    "LogicalSectorSize".As<int>(),
                    "PhysicalSectorSize".As<int>(),
                    "BlockSize".As<long>(),
                    "ParentPath",
                    "FragmentationPercentage".As<int>()), path).Single());
        }

        [Route("vhd/resize"), HttpPost, ValidateModel]
        public IHttpActionResult ResizeVHD([FromBody] ResizeVhdArgs args)
        {
            return Json(InvokePowerShell("Resize-VHD $args[0] $args[1] -AsJob", PSSelect("InstanceId"), args.Path, args.Size).Single());
        }

        [Route("progress/{jobInstanceId}"), HttpGet]
        public IHttpActionResult GetProgress(string jobInstanceId)
        {
            return Json(PSJobProgress(jobInstanceId, 1));
        }

    }
}
