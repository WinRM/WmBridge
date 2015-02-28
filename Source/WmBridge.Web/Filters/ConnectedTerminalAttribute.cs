//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WmBridge.Web.Model;

namespace WmBridge.Web.Filters
{
    public class ConnectedTerminalAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var dict = actionContext.Request.GetPSState();

            object host;
            if (dict != null && dict.TryGetValue(PSSessionManager.PSHostClientKey, out host))
                actionContext.Request.Properties[PSSessionManager.PSHostClientKey] = host;
            else
                throw new HttpResponseException(actionContext.Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, new HttpError("Not Connected")));
            
        }
    }
}
