using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WindowsManagement.Filters;

namespace WindowsManagement
{
    public class Startup
    {
        // This code configures Web API contained in the class Startup, which is additionally specified as the type parameter in WebApplication.Start
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for Self-Host
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            
            appBuilder.UseWebApi(config);
        }
    }
}
