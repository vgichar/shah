using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Config
{
    internal class InternalSessionKeys
    {
        internal static string SessionIDCookieKey { get { return "hyper.sr.SessionID"; } }
        internal static string MigrateBackFlagKey { get { return "hyper.sr.MigrateBack"; } }
        internal static string DisconnectTimeKey { get { return "hyper.sr.DisconnectTimeKey"; } }
    }
}
