using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Hyper.Games.Chess.Configuration.DB;
using Hyper.Games.Chess.Infrastructure.DB;
using Microsoft.AspNet.SignalR;
using MongoDB.Driver;
using SignalR.Extras.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Hyper.Games.Chess.Configuration.DI
{
    public class DependencyInjectionConfiguration
    {
        public static void Bootstrap(Assembly entryAssembly)
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(entryAssembly);
            builder.RegisterLifetimeHubManager();
            builder.RegisterHubs(entryAssembly);
            builder.RegisterAssemblyModules(typeof(DependencyInjectionConfiguration).Assembly);
            builder.RegisterAssemblyModules(entryAssembly);

            var container = builder.Build();

            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
        }
    }
}