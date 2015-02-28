//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Collections.Concurrent;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WmBridge.Web.Filters
{
    /// <summary>
    /// Invoke specified sript at first time usage on Runspace instance
    /// </summary>
    public class StartupPSScriptAttribute : ActionFilterAttribute
    {
        private class StatusInfo
        {
            public bool Invoked;
        }

        string script;
        string uniqueId;

        public StartupPSScriptAttribute(string script, string uniqueId)
        {
            this.script = script;
            this.uniqueId = uniqueId;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var info = (StatusInfo)((ConcurrentDictionary<string, object>)actionContext.Request.GetPSState())
                .GetOrAdd("Script: " + uniqueId, k => new StatusInfo());

            if (info.Invoked)
                return;

            lock(info)
            {
                if (info.Invoked)
                    return;

                using (var powershell = actionContext.Request.CreatePowerShell())
                    actionContext.Request.InvokeScript(powershell, script);

                info.Invoked = true;
            }
        }

    }
}
