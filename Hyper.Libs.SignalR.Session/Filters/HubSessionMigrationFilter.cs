using Hyper.SignalR.Session.Attributes;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hyper.SignalR.Session.Extensions;
using System.Web;
using Hyper.SignalR.Session.Core;
using Hyper.SignalR.Session.Config;

namespace Hyper.SignalR.Session.Filters
{
    internal class HubSessionMigrationFilter : HubPipelineModule
    {
        protected override bool OnBeforeConnect(IHub hub)
        {
            SignalRContext.ConnectionIdToSessionId[hub.Context.ConnectionId] = hub.GetSessionId();

            Storage session = hub.GetSession();
            session[InternalSessionKeys.DisconnectTimeKey] = null;

            ClearExpiredSessions();
            return base.OnBeforeConnect(hub);
        }

        protected override bool OnBeforeReconnect(IHub hub)
        {
            SignalRContext.ConnectionIdToSessionId[hub.Context.ConnectionId] = hub.GetSessionId();

            Storage session = hub.GetSession();
            session[InternalSessionKeys.DisconnectTimeKey] = null;
            return base.OnBeforeReconnect(hub);
        }

        protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
        {
            string sessionId;
            SignalRContext.ConnectionIdToSessionId.TryRemove(hub.Context.ConnectionId, out sessionId);

            Storage session = hub.GetSession();
            session[InternalSessionKeys.DisconnectTimeKey] = DateTime.Now;
            base.OnAfterDisconnect(hub, stopCalled);
        }

        private void ClearExpiredSessions()
        {
            foreach (KeyValuePair<string, Storage> sessionPair in SignalRContext.SessionProvider)
            {
                DateTime? sessionDisconnectTime = sessionPair.Value[InternalSessionKeys.DisconnectTimeKey] as DateTime?;
                DateTime timeoutDateTime = DateTime.Now.AddMinutes(Configuration.SessionTimeout);
                if (sessionDisconnectTime.HasValue && sessionDisconnectTime <= timeoutDateTime)
                {
                    SignalRContext.SessionProvider.ClearSession(sessionPair.Key);
                }
            }
        }
    }
}
