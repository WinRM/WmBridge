//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Net;

namespace WmBridge.Web.Filters
{
    public class ExceptionFilterAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        public override void OnException(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
        {
            // send exception as error 500 - if current session is authenticated, exception details will be sent also
            actionExecutedContext.Request.ThrowHttpException(HttpStatusCode.InternalServerError, actionExecutedContext.Exception);
        }
    }
}
