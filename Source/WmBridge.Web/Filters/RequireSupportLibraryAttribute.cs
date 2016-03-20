//
//  Copyright (c) 2016 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Collections.Concurrent;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WmBridge.Web.Properties;

namespace WmBridge.Web.Filters
{
    /// <summary>
    /// Inject WmBridge.Support.dll into current PS session
    /// </summary>
    public class RequireSupportLibraryAttribute : ActionFilterAttribute
    {
        private class StatusInfo
        {
            public bool Loaded;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var state = (StatusInfo)((ConcurrentDictionary<string, object>)actionContext.Request.GetPSState())
                .GetOrAdd("SupportLibraryState", k => new StatusInfo());

            if (state.Loaded)
                return;

            lock(state)
            {
                if (state.Loaded)
                    return;

                using (var powershell = actionContext.Request.CreatePowerShell())
                    actionContext.Request.InvokeScript(powershell, "[System.Reflection.Assembly]::Load($args[0])", Resources.WmBridgeSupportDll);

                state.Loaded = true;
            }
        }

    }
}
