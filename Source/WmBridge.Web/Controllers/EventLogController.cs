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
    [RoutePrefix("eventlog")]
    public class EventLogController : PSApiController
    {
        public class SearchListLogArgs
        {
            [Required]
            public string[] Search { get; set; }
            
            [Required]
            public int MaxCount { get; set; }
        }

        public class GetEventDetailArgs
        {
            [Required]
            public string LogName { get; set; }

            [Required]
            public DateTime Time { get; set; }
        }

        public class GetEventsArgs
        {
            [Required]
            public string LogName { get; set; }

            [Required]
            public int Count { get; set; }

            public long? ToRecordId { get; set; }

            public DateTime? EndTime { get; set; }
            public string[] ProviderName { get; set; }
            public int[] EventID { get; set; }
            public int[] Level { get; set; } // "Critical" = 1;"Error" = 2; "Warning" = 3; "Information" = 4; "Verbose" = 5;
        }

        [Route("event"), HttpPost, ValidateModel]
        public IHttpActionResult GetEvents([FromBody] GetEventsArgs args)
        {
            string filter = "";

            if (args.EndTime != null)
                filter += "EndTime = $args[2];";

            if (args.EventID != null)
                filter += "ID = @($args[4]);";

            if (args.ProviderName != null)
                filter += "ProviderName = @($args[5]);";

            if (args.Level != null)
                filter += "Level = @($args[6]);";

            string script =
                // workaround a bug documented here: https://connect.microsoft.com/PowerShell/feedback/details/716533/get-winevent-does-not-return-the-content-of-the-event-message-in-v3-ctp2
                "[System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]\"en-US\";" +
                "$lastId = $args[3]; $array = @(); $max = $args[1];" +
                "while ($array.count -lt $args[1]) {" +
                "$c = 0; foreach ($e in (Get-WinEvent -ea SilentlyContinue -MaxEvents $max -FilterHashtable @{LogName = $args[0];" + filter + "}))" +
                "{$c++; if ($lastId -eq $null -or $e.RecordId -lt $lastId) {$array += $e; $lastId = $e.RecordId; if ($array.count -eq $args[1]) {break} } }" +
                "if ($c -lt $max) {break} else {$max *= 2}}; $array";

            return Json(InvokePowerShell(script, PSSelect(
                    "Id".Alias("EventId"), "Level", "RecordId", "ProviderName", "ProviderId",
                    "TimeCreated".Expression("$_.TimeCreated.ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                    "Message".Expression("if ($_.Message.Length -gt 100) { $_.Message.Substring(0,100) + '...'} else {$_.Message}")),
                        args.LogName, 
                        args.Count, 
                        args.EndTime == null ? null : (DateTime?)args.EndTime.Value.ToLocalTime().AddSeconds(1), 
                        args.ToRecordId,
                        args.EventID,
                        args.ProviderName,
                        args.Level));
        }

        [Route("event/{recordId}"), HttpPost, ValidateModel]
        public IHttpActionResult GetEventDetail(long recordId, [FromBody] GetEventDetailArgs args)
        {
            string script =
                "$id = $args[3];" +
                "[System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]\"en-US\";" +
                "Get-WinEvent -FilterHashtable @{LogName = $args[0]; StartTime = $args[1]; EndTime = $args[2]}" +
                "| ? {$_.RecordId -eq $id}";

            return Json(InvokePowerShell(script, PSSelect(
                    "Id".Alias("EventId"), "Level", "Task", "TaskDisplayName", "RecordId", "ProviderName", "ProviderId", "ProcessId", "ThreadId",
                    "TimeCreated".Expression("$_.TimeCreated.ToUniversalTime().Ticks").TransformTruncUtcTicks(), "Message", "MachineName", "UserId".As<string>()),
                        args.LogName, args.Time.ToLocalTime(), args.Time.ToLocalTime().AddSeconds(1), recordId).Single());
        }

        [Route("sid/{sid}"), HttpGet]
        public IHttpActionResult ResolveSid(string sid)
        {
            return Json(InvokePowerShell("(New-Object System.Security.Principal.SecurityIdentifier($args[0])).Translate([System.Security.Principal.NTAccount])", 
                PSSelect("Value".Alias("User")), sid).Single());
        }

        [Route("log"), HttpPost]
        public IHttpActionResult GetListLog([FromBody] string[] logNames)
        {
            return Json(InvokePowerShell("Get-WinEvent -ea SilentlyContinue -ListLog @($args[0])", PSSelect(
                "LogName", "FileSize".As<long>(), "MaximumSizeInBytes", "RecordCount".As<long>()),
                    logNames, null));
        }

        [Route("log/detail"), HttpPost]
        public IHttpActionResult GetLogDetail([FromBody] string logName)
        {
            return Json(InvokePowerShell("Get-WinEvent -ListLog $args[0]", PSSelect(
                "LogName", "FileSize".As<long>(), "MaximumSizeInBytes", "RecordCount".As<long>(),
                "LogType".As<string>(), "LogMode".As<string>(),
                "IsEnabled", "IsClassicLog", "LogFilePath", "IsLogFull".As<bool>(),
                "LastAccessTime".Expression("$_.LastAccessTime.ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "LastWriteTime".Expression("$_.LastWriteTime.ToUniversalTime().Ticks").TransformTruncUtcTicks()),
                    logName).Single());
        }

        [Route("log/source"), HttpPost]
        public IHttpActionResult GetLogSrouces([FromBody] string logName)
        {
            return Json(InvokePowerShell("Get-WinEvent -ListLog $args[0]", PSSelect("ProviderNames".TransformArray()), logName).Single().Values.Single());
        }

        [Route("log/search"), HttpPost, ValidateModel]
        public IHttpActionResult SearchListLog([FromBody] SearchListLogArgs args)
        {
            var countSelect = PSSelectTagged(0, "TotalCount");
            var select = PSSelectTagged(1,
                "LogName", "FileSize".As<long>(), "MaximumSizeInBytes", "RecordCount".As<long>(),
                "LogType".As<string>(), "LogMode".As<string>(),
                "IsEnabled", "IsClassicLog");

            string script = "$list = (Get-WinEvent -ea SilentlyContinue -ListLog @($args[0])); [array](" +
                AppendSelectCommand("New-Object PSObject -Property @{TotalCount = $list.Count}", countSelect) + ") + (" +
                AppendSelectCommand("($list | Select -First $args[1])", select) + ")";

            var list = InvokePowerShell(script, PSSelect(select, countSelect), args.Search, args.MaxCount).ToLookup(x => x.Tag);

            return Json(new { TotalCount = list[0].Single().Values.Single(), Result = list[1] });
        }

    }
}
