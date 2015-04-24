using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("script")]
    public class ScriptController : PSApiController
    {
        private static readonly MemoryCache executingScripts = new MemoryCache("ExecutingScripts");
        private static readonly CacheItemPolicy executingScriptsPolicy = new CacheItemPolicy() { RemovedCallback = ExecutingScriptRemoved, SlidingExpiration = TimeSpan.FromMinutes(20) };

        private class ExecutingScript
        {
            public IAsyncResult AsyncResult { get; set; }

            public PowerShell Pipeline { get; set; }

            public static ExecutingScript Start(PowerShell pipeline)
            {
                return new ExecutingScript()
                {
                    Pipeline = pipeline,
                    AsyncResult = pipeline.BeginInvoke()
                };
            }
        }

        [Route("execute"), HttpPost]
        public IHttpActionResult Execute([FromBody] string script)
        {
            var powershell = Request.CreatePowerShell();

            powershell.AddScript(script + " | Out-String");

            string token = Guid.NewGuid().ToString();

            executingScripts.Add(token, ExecutingScript.Start(powershell), executingScriptsPolicy);

            return Json(new { Token = token });
        }

        [Route("status/{token}"), HttpGet]
        public IHttpActionResult GetStatus(string token)
        {
            var exec = executingScripts.Get(token) as ExecutingScript;

            if (exec == null)
                return Json(new { State = "NotFound" });

            if (exec.AsyncResult.IsCompleted)
            {
                try
                {
                    string output = string.Join("\n", exec.Pipeline.EndInvoke(exec.AsyncResult));
                    if (output.Length > 1000)
                        output = output.Substring(0, 1000) + "...";

                    return Json(new { State = "Completed", Output = output });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Error", Exception = ex.Message });
                }
                finally
                {
                    executingScripts.Remove(token);
                    Request.CloseSharedConnection();
                }
            }
            else
            {
                exec.AsyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
                return Json(new { State = "Executing" });
            }
        }

        [Route("abort/{token}"), HttpGet]
        public IHttpActionResult Abort(string token)
        {
            var exec = executingScripts.Get(token) as ExecutingScript;

            if (exec != null)
            {
                exec.Pipeline.Stop();
                executingScripts.Remove(token);
                Request.CloseSharedConnection();
            }

            return Ok();
        }

        private static void ExecutingScriptRemoved(CacheEntryRemovedArguments arguments)
        {
            var exec = arguments.CacheItem.Value as ExecutingScript;

            if (exec != null)
            {
                exec.Pipeline.Dispose();
            }
        }
    }
}
