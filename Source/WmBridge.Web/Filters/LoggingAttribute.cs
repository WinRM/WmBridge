//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WmBridge.Web.Controllers;

namespace WmBridge.Web.Filters
{
    public class LoggingAttribute : ActionFilterAttribute
    {
        static ILog Log { get { return PSApiController.Log; } }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            log4net.LogicalThreadContext.Properties["RemoteIp"] = actionContext.Request.GetOwinContext().Request.RemoteIpAddress;
            log4net.LogicalThreadContext.Properties["Method"] = actionContext.Request.Method;
            log4net.LogicalThreadContext.Properties["Request"] = actionContext.Request.RequestUri;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                if (actionExecutedContext.Exception != null)
                {
                    var httpException = actionExecutedContext.Exception as HttpResponseException;
                    if (httpException != null && httpException.Response != null && httpException.Response.Content != null)
                    {
                        if (httpException.Response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            Log.Warn(FormatLog(actionExecutedContext.Request, HttpStatusCode.ServiceUnavailable));
                            return;
                        }

                        string errString = httpException.Response.Content.ReadAsStringAsync().Result;
                        Log.Error(FormatLog(actionExecutedContext.Request, string.Format("{0} ({1}) {2}",
                            (int)httpException.Response.StatusCode, httpException.Response.StatusCode, errString)));
                    }
                    else
                        Log.Error(actionExecutedContext.Exception);

                    return;
                }

                if (actionExecutedContext.Response == null)
                    return;

                log4net.LogicalThreadContext.Properties["StatusCode"] = (int)actionExecutedContext.Response.StatusCode;

                if (actionExecutedContext.Response.StatusCode == HttpStatusCode.OK)
                    Log.Debug(FormatLog(actionExecutedContext.Request, actionExecutedContext.Response.StatusCode));
                else
                    Log.Warn(FormatLog(actionExecutedContext.Request, actionExecutedContext.Response.StatusCode));

            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
            finally
            {
                log4net.LogicalThreadContext.Properties.Clear();
            }
        }

        private static string FormatLog(HttpRequestMessage request, HttpStatusCode statusCode)
        {
            return FormatLog(request, string.Format("{0} ({1})", (int)statusCode, statusCode));
        }

        private static string FormatLog(HttpRequestMessage request, string message = null)
        {
            return string.Format("{0} {1} {2}{3}", request.GetOwinContext().Request.RemoteIpAddress, request.Method, request.RequestUri, string.IsNullOrEmpty(message) ? ("") : (" -> " + message));
        }

    }
}
