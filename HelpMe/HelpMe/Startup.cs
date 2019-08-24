using HelpMe.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Web.Http;

[assembly: OwinStartupAttribute(typeof(HelpMe.Startup))]
namespace HelpMe
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          
            var idProvider = new CustomUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);
          
            ConfigureAuth(app);
           // GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            app.MapSignalR();
            GlobalHost.HubPipeline.RequireAuthentication();
          
        }
    }
}
