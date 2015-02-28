//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using WmBridge.Web.Filters;

namespace WmBridge.Web.Controllers
{
    [PSVersion("3.0")]
    [RoutePrefix("scheduler")]
    public class SchedulerController : PSApiController
    {
        public class ScheduledTasksArgs
        {
            [Required]
            public string Path { get; set; }
        }

        public class TaskOperationArgs
        {
            [Required]
            public string Task { get; set; }

            [Required]
            public string Path { get; set; }
        }

        [Route("paths"), HttpGet]
        public IHttpActionResult SchedulerPaths()
        {
            return Json(InvokePowerShell("Get-ScheduledTask | Group TaskPath", PSSelect(
                "Name".Alias("Path"), 
                "Count".Expression("$_.Count"))));
        }

        [Route("tasks"), HttpPost, ValidateModel]
        public IHttpActionResult ScheduledTasks([FromBody] ScheduledTasksArgs args)
        {
            return Json(InvokePowerShell("Get-ScheduledTask -TaskPath $args[0] | Select *, @{N='TaskInfo';E={Get-ScheduledTaskInfo $_}}", PSSelect(
                "State".As<string>(), 
                "Author",
                "Date",
                "Description",
                "LastRunTime".Expression("$_.TaskInfo.LastRunTime"),
                "LastTaskResult".Expression("$_.TaskInfo.LastTaskResult"),
                "NextRunTime".Expression("$_.TaskInfo.NextRunTime"),
                "NumberOfMissedRuns".Expression("$_.TaskInfo.NumberOfMissedRuns"),
                "TaskName".Expression("$_.TaskInfo.TaskName"),
                "Path".Expression("$args[0]")), args.Path));
        }

        [Route("{operation:regex(^(enable|disable|start|stop)$)}"), HttpPost, ValidateModel]
        public IHttpActionResult TaskOperation(string operation, [FromBody] TaskOperationArgs args)
        {
            InvokePowerShell( operation + "-ScheduledTask $args[0] -TaskPath $args[1]", args.Task, args.Path);
            return Ok();
        }
    }
}
