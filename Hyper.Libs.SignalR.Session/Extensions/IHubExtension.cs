using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Hyper.SignalR.Session.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hyper.SignalR.Session.Attributes;
using Hyper.SignalR.Session.Config;

namespace Hyper.SignalR.Session.Extensions
{
    public static class IHubExtension
    {
        internal static Cookie GetSessionCookie(this IHub hub)
        {
            if (hub.Context.RequestCookies.ContainsKey(InternalSessionKeys.SessionIDCookieKey))
            {
                return hub.Context.RequestCookies[InternalSessionKeys.SessionIDCookieKey];
            }
            return null;
        }

        public static string GetSessionId(this IHub hub)
        {
            var id = hub.Context.ConnectionId;
            Cookie cookie = hub.GetSessionCookie();
            if(cookie != null){
                id = cookie.Value;
            }
            return id;
        }

        public static Storage GetSession(this IHub hub)
        {
            return SignalRContext.SessionProvider[hub.GetSessionId()];
        }

        public static Storage GetStorage(this IHub hub)
        {
            return SignalRContext.StorageProvider.Current;
        }

        public static dynamic GetProxiedSession(this IHub hub)
        {
            Storage session = hub.GetSession();
            return new StorageProxy(session);
        }

        public static dynamic GetProxiedStorage(this IHub hub)
        {
            return new StorageProxy(SignalRContext.StorageProvider.Current);
        }

        public static void ClearSession(this IHub hub)
        {
            SignalRContext.SessionProvider.ClearSession(hub.GetSessionId());
        }

        public static void ClearStorage(this IHub hub)
        {
            SignalRContext.StorageProvider.ClearStorage();
        }
    }
}
