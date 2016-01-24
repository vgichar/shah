using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hyper.SignalR.Session.Extensions;

namespace Hyper.SignalR.Session.Filters
{
    public class SessionLifetimeFilter : HubPipelineModule
    {
        protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
        {
            if (stopCalled)
            {
                hub.ClearSession();
            }
            return base.OnBeforeDisconnect(hub, stopCalled);
        }
    }
}
