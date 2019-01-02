using HelpMe.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

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
            app.MapSignalR();
            GlobalHost.HubPipeline.RequireAuthentication();
        }
    }
}
