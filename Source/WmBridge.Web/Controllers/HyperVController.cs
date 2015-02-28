//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using WmBridge.Web.Filters;

namespace WmBridge.Web.Controllers
{
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
            public bool CompatibilityForOlderOperatingSystemsEnabled { get; set; }

            [Required]
            public int Count { get; set; }

            [Required]
            public int Maximum { get; set; }

            [Required]
            public int MaximumCountPerNumaNode { get; set; }

            [Required]
            public int MaximumCountPerNumaSocket { get; set; }

            [Required]
            public int RelativeWeight { get; set; }

            [Required]
            public int Reserve { get; set; }
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
            return Json(InvokePowerShell("Get-VM", PSSelect(
                "Id",
                "Name", 
                "State".As<string>(), 
                "Status", 
                "CPUUsage", 
                "MemoryAssigned", 
                "AutomaticStartAction".As<string>(),
                "AutomaticStartDelay",
                "AutomaticStopAction".As<string>())));
        }

        [Route("{guid}/{operation:regex(^(start|stop|restart)$)}"), HttpGet]
        public IHttpActionResult VmOperation(string guid, string operation)
        {
            InvokePowerShell(operation + "-VM (Get-VM -Id $args[0])", guid);
            return Ok();
        }

        [Route("{guid}/memory"), HttpGet]
        public IHttpActionResult GetVmMemory(string guid)
        {
            return Json(InvokePowerShell("Get-VMMemory (Get-VM -Id $args[0])", PSSelect(
                "Startup", 
                "DynamicMemoryEnabled".Alias("Dynamic"), 
                "Minimum", 
                "Maximum"), guid).Single());
        }

        [Route("{guid}/memory"), HttpPost, ValidateModel]
        public IHttpActionResult SetVmMemory(string guid, [FromBody] MemoryArgs args)
        {
            if (args.Dynamic)
                InvokePowerShell("Set-VMMemory (Get-VM -Id $args[0]) -DynamicMemory -MemoryStartupBytes $args[1] -MemoryMinimumBytes $args[2] -MemoryMaximumBytes $args[3]",
                    guid, args.Startup, args.Minimum, args.Maximum);
            else
                InvokePowerShell("Set-VMMemory (Get-VM -Id $args[0]) -StaticMemory -MemoryStartupBytes $args[1]", 
                    guid, args.Startup);
            
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
            InvokePowerShell("Get-VMProcessor (Get-VM -Id $args[0]) -CompatibilityForMigrationEnabled $args[1] -CompatibilityForOlderOperatingSystemsEnabled $args[2] -Count $args[3] -Maximum $args[4] -MaximumCountPerNumaNode $args[5] -MaximumCountPerNumaSocket $args[6] -RelativeWeight $args[7] -Reserve $args[8]",
                args.CompatibilityForMigration, args.CompatibilityForOlderOperatingSystemsEnabled, args.Count, args.Maximum, args.MaximumCountPerNumaNode, args.MaximumCountPerNumaSocket, args.RelativeWeight, args.Reserve);
            return Ok();
        }

        [Route("{guid}/vhd"), HttpGet]
        public IHttpActionResult GetVHD(string guid)
        {
            return Json(InvokePowerShell("Get-VMHardDiskDrive (Get-VM -Id $args[0]) | % {$vhd = Get-VHD $_.Path; New-Object PSObject -Property @{ DiskNumber=$_.DiskNumber;ControllerType=$_.ControllerType; ControllerNumber=$_.ControllerNumber; ControllerLocation=$_.ControllerLocation;Name=$_.Name;Path=$_.Path;VMId=$_.VMId;VMSnapshotId=$_.VMSnapshotId; VMSnapshotName =$_.VMSnapshotName;VhdFormat = $vhd.VhdFormat;VhdType = $vhd.VhdType;FileSize = $vhd.FileSize;Size = $vhd.Size;MinimumSize =$vhd.MinimumSize;LogicalSectorSize = $vhd.LogicalSectorSize;PhysicalSectorSize = $vhd.PhysicalSectorSize;BlockSize = $vhd.BlockSize;ParentPath = $vhd.ParentPath;FragmentationPercentage = $vhd.FragmentationPercentage } }",
                PSSelect(
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
                    "FileSize",
                    "Size",
                    "MinimumSize",
                    "LogicalSectorSize",
                    "PhysicalSectorSize",
                    "BlockSize",
                    "ParentPath",
                    "FragmentationPercentage"
                ), guid));
        }

        [Route("vhd/resize"), HttpPost, ValidateModel]
        public IHttpActionResult ResizeVHD([FromBody] ResizeVhdArgs args)
        {
            InvokePowerShell("Resize-VHD $args[0] $args[1]", args.Path, args.Size);
            return Ok();
        }
    }
}
