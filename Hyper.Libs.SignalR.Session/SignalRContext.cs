using Hyper.SignalR.Session.Core;
using Hyper.SignalR.Session.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session
{
    public class SignalRContext
    {
        private SignalRContext() { }

        internal readonly static ConcurrentDictionary<string, string> ConnectionIdToSessionId = new ConcurrentDictionary<string, string>();

        public readonly static SessionProvider SessionProvider = new SessionProvider();

        public readonly static StorageProvider StorageProvider = new StorageProvider();

        public static string GetConnectionIdBySessionId(string sessionId)
        {
            KeyValuePair<string, string> pair = ConnectionIdToSessionId.Where(x=>x.Value == sessionId).SingleOrDefault();
            if (!string.IsNullOrEmpty(pair.Key))
            {
                return pair.Key;
            }
            return null;
        }

        public static string GetSessionIdByConnectionId(string connectionId)
        {
            if(ConnectionIdToSessionId.ContainsKey(connectionId)){
                return ConnectionIdToSessionId[connectionId];
            }
            return null;
        }

        public static List<string> GetBulkConnectionIdsBySessionIds(List<string> sessionIds)
        {
            List<string> connectionIds = new List<string>();
            foreach (var sessionId in sessionIds)
            {
                connectionIds.Add(GetConnectionIdBySessionId(sessionId));
            }
            return connectionIds;
        }


        public static List<string> GetBulkSessionIdsByConnectionIds(List<string> connectionIds)
        {
            List<string> sessionIds = new List<string>();
            foreach (var connectionId in connectionIds)
            {
                sessionIds.Add(GetSessionIdByConnectionId(connectionId));
            }
            return sessionIds;
        }
    }
}
