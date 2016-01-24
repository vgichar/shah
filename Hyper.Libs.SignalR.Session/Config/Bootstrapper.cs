using Hyper.SignalR.Session.Filters;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hyper.SignalR.Session.Config
{
    public class Bootstrapper
    {
        public static void Bootstrap()
        {
            GlobalHost.HubPipeline.AddModule(new HubSessionMigrationFilter());
            GlobalFilters.Filters.Add(new ActionSessionMigrationFilter());
        }
    }
}
