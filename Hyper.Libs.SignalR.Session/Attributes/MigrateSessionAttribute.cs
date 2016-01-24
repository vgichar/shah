using Hyper.SignalR.Session.Config;
using Hyper.SignalR.Session.Core;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Hyper.SignalR.Session.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class MigrateSession : Attribute
    {
        internal bool MigrateBack { get; set; }

        public MigrateSession(bool migrateBack = false)
        {
            MigrateBack = migrateBack;
        }

        public static void Execute(bool migrateBack = false)
        {
            MigrateSession.HttpToSignalR(new MigrateSession(migrateBack));
        }


        public static void HttpToSignalR(MigrateSession attribute, bool migrate = true)
        {
            string SessionIDCookieKey = InternalSessionKeys.SessionIDCookieKey;
            string MigrateBackFlagKey = InternalSessionKeys.MigrateBackFlagKey;

            HttpSessionState Session = HttpContext.Current.Session;
            if (Session == null)
            {
                throw new Exception("HttpSession doesn't exsist");
            }

            SignalRContext.SessionProvider.ClearSession(Session.SessionID);

            if (migrate)
            {
                for (int i = 0; i < Session.Keys.Count; i++)
                {
                    string key = Session.Keys[i];
                    SignalRContext.SessionProvider[Session.SessionID][key] = Session[key];
                }
            }
            SignalRContext.SessionProvider[Session.SessionID][MigrateBackFlagKey] = attribute.MigrateBack;


            // cookie cleenup
            HttpCookie requestCookie = HttpContext.Current.Request.Cookies[SessionIDCookieKey];
            HttpCookie responseCookie = HttpContext.Current.Response.Cookies[SessionIDCookieKey];

            if (requestCookie != null)
            {
                HttpContext.Current.Request.Cookies.Remove(SessionIDCookieKey);
            }
            if (responseCookie != null)
            {
                HttpContext.Current.Response.Cookies.Remove(SessionIDCookieKey);
            }

            HttpCookie cookie = new HttpCookie(SessionIDCookieKey, Session.SessionID);
            cookie.Expires = DateTime.MaxValue;
            cookie.HttpOnly = true;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void SignalRToHttp()
        {
            string SessionIDCookieKey = InternalSessionKeys.SessionIDCookieKey;
            string MigrateBackFlagKey = InternalSessionKeys.MigrateBackFlagKey;

            HttpCookie sessionIDCookie = HttpContext.Current.Request.Cookies[SessionIDCookieKey];
            bool hasSessionIDCookie = sessionIDCookie != null;
            if (hasSessionIDCookie)
            {
                string sessionID = sessionIDCookie.Value;
                bool? migrateBackHub = SignalRContext.SessionProvider[sessionID][MigrateBackFlagKey] as bool?;
                if (migrateBackHub.HasValue && migrateBackHub.Value)
                {
                    foreach (KeyValuePair<string, StorageValue> pair in SignalRContext.SessionProvider[sessionIDCookie.Value])
                    {
                        if (pair.Key != MigrateBackFlagKey)
                        {
                            HttpContext.Current.Session[pair.Key] = SignalRContext.SessionProvider[sessionIDCookie.Value][pair.Key];
                        }
                    }
                }
            }
            else
            {
                HttpToSignalR(new MigrateSession(), false);
            }
        }
    }
}
