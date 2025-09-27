using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;

namespace ChatGPTBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static KeyValueConfigurationCollection LocalAppSettings { get; private set; }
        protected void Application_Start()
        {

            // Load local secrets
            var localConfigPath = Server.MapPath("~/web.local.config");
            if (System.IO.File.Exists(localConfigPath))
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = localConfigPath };
                var localConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                LocalAppSettings = localConfig.AppSettings.Settings;
            }
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
