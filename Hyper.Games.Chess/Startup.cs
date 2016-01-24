using Hyper.SignalR.Session;
using Hyper.SignalR.Session.Config;
using Hyper.Games.Chess.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Hyper.Games.Chess.Configuration.DI;
using Hyper.Games.Chess.Configuration.DB;
using Hyper.Games.Chess.Hubs.Client;
using Autofac;

[assembly: OwinStartup(typeof(Hyper.Games.Chess.Startup))]
namespace Hyper.Games.Chess
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            DependencyInjectionConfiguration.Bootstrap(typeof(MvcApplication).Assembly);
            MongoModelMapper.Activate();

            ConfigureAuth(app);
            app.MapSignalR();
            Bootstrapper.Bootstrap();
            GlobalHost.HubPipeline.RequireAuthentication();
        }

        public class AutofacModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<MatchmakingHubClient>().SingleInstance();
                base.Load(builder);
            }
        }
    }
}
