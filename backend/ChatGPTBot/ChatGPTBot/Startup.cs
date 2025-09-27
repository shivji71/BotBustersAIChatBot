using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Web.Http.Cors;
using ChatGPTBot.services;
using ChatGPTBot.APIController;
using ChatGPTBot.Helper;

[assembly: OwinStartup(typeof(ChatGPTBot.Startup))]

namespace ChatGPTBot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Create Web API configuration
            HttpConfiguration config = new HttpConfiguration();

            // ✅ Enable CORS globally
            // Replace the URL below with your Angular app URL
            var cors = new EnableCorsAttribute("http://localhost:4300", "*", "*");
            config.EnableCors(cors);
            

            // Manual DI: create your service
            var chatBotService = new ChatBotService();
            var sqliteHelper = new SQLiteHelper();

            // Replace controller activator
            config.Services.Replace(
                typeof(System.Web.Http.Dispatcher.IHttpControllerActivator),
                new ManualDependencyResolver(chatBotService, sqliteHelper)
            );

            WebApiConfig.Register(config);

            // Web API routes
            //config.MapHttpAttributeRoutes();
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            // Tell OWIN to use Web API
            app.UseWebApi(config);

            // If you have auth setup
            ConfigureAuth(app);
        }
    }
    // Custom controller activator for manual DI
    public class ManualDependencyResolver : System.Web.Http.Dispatcher.IHttpControllerActivator
    {
        private readonly IChatBotRepository _chatBotService;
        private readonly ISQLiteHelperRepository _sqliteHelper;

        public ManualDependencyResolver(IChatBotRepository chatBotService, ISQLiteHelperRepository sqliteHelper)
        {
            _chatBotService = chatBotService;
            _sqliteHelper = sqliteHelper;
        }

        public System.Web.Http.Controllers.IHttpController Create(
            System.Net.Http.HttpRequestMessage request,
            System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor,
            Type controllerType)

        {
            if (controllerType == typeof(ChatGPTBotController))
            {
                return new ChatGPTBotController(_chatBotService, _sqliteHelper);
            }

            // fallback to default
            return (System.Web.Http.Controllers.IHttpController)Activator.CreateInstance(controllerType);
        }
    }

}
