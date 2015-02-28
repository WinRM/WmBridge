
using System;
using System.Web.Http;

using Owin;

using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

namespace WmBridge.Web
{
    public class Startup
    {
        // This code configures Web API contained in the class Startup, which is additionally specified as the type parameter in WebApplication.Start
        public void Configuration(IAppBuilder appBuilder)
        {
            log4net.Config.XmlConfigurator.Configure();

            HttpConfiguration config = new HttpConfiguration();
            
            config.MapHttpAttributeRoutes();
            
            config.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));            

            appBuilder.UseWebApi(config);
        }
    }
}
