//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WmBridge.Web.Filters
{
    public class PSVersionAttribute : AuthorizationFilterAttribute
    {
        Version requiredVersion;

        public PSVersionAttribute(string requiredVersion)
        {
            this.requiredVersion = Version.Parse(requiredVersion);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            object runspace = actionContext.Request.GetPSConnection();
            if (runspace != null)
            {
                var psVersion = actionContext.Request.GetPSVersion();
                if (psVersion != null && psVersion < requiredVersion)
                    throw new HttpResponseException(actionContext.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, "PowerShell version " + requiredVersion + " is required."));
            }
        }
    }
}
