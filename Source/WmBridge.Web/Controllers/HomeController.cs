//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WmBridge.Web.Filters;
using WmBridge.Web.Properties;

namespace WmBridge.Web.Controllers
{
    [ExceptionFilter, Logging]
    public class HomeController : ApiController
    {
        [Route(""), HttpGet]
        public HttpResponseMessage Index()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<html><head><title>WinRM Bridge Service</title></head>" +
                    "<body style=\"margin:0px\"><div style=\"width:100%;height:100%;text-align:center\">" +
                    "<span style=\"display:inline-block;vertical-align:middle;height:100%\"></span>" +
                    "<a title=\"www.winrmapp.com\" href=\"http://www.winrmapp.com\">" +
                    "<img src=\"" + Request.GetUrlHelper().Route("Logo", null) + "\" style=\"vertical-align:middle;border:none\"/></a>" +
                    "</div></body></html>")
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            return response;
        }

        [Route("images/logo", Name = "Logo"), HttpGet]
        public HttpResponseMessage Logo()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Resources.Logo) };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            return response;
        }

    }
}
