using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Hyper.SignalR.Session;
using Microsoft.AspNet.SignalR;
using MongoDB.Driver;
using SignalR.Extras.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hyper.Games.Chess.Configuration;
using Hyper.Games.Chess.Configuration.DI;
using Hyper.Games.Chess.Configuration.DB;

namespace Hyper.Games.Chess
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
