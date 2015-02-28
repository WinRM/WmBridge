//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WmBridge.Web.Filters
{
    public class ImportPSModuleAttribute : ActionFilterAttribute
    {
        const string ModulesKey = "modules";

        string moduleName;

        public ImportPSModuleAttribute(string moduleName)
        {
            this.moduleName = moduleName;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modules = (List<string>)((ConcurrentDictionary<string, object>)actionContext.Request.GetPSState())
                .GetOrAdd(ModulesKey, k => new List<string>());

            lock (modules)
            {
                if (modules.Contains(moduleName))
                {
                    // module is already imported
                    return;
                }
                else
                {
                    using (var powershell = actionContext.Request.CreatePowerShell())
                        actionContext.Request.InvokeScript(powershell, "If (!(Get-Module $args[0])) {Import-Module $args[0]}", moduleName);

                    modules.Add(moduleName);
                }

            }
        }
    }
}
