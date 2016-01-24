using Hyper.SignalR.Session.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Providers
{
    public class SessionProvider : IEnumerable<KeyValuePair<string, Storage>>
    {
        private ConcurrentDictionary<string, Storage> _sessionDictionary;

        public SessionProvider()
        {
            _sessionDictionary = new ConcurrentDictionary<string, Storage>();
        }

        public Storage this[string sessionId]
        {
            get
            {
                if (!HasSession(sessionId))
                {
                    if (HasSessionByConnectionId(sessionId))
                    {
                        sessionId = ConnectionIdToSessionId(sessionId);
                    }
                    else
                    {
                        _sessionDictionary[sessionId] = new Storage();
                    }
                }
                return _sessionDictionary[sessionId];
            }
            set
            {
                _sessionDictionary[sessionId] = value;
            }
        }

        public void ClearSession(string sessionId)
        {
            if (!HasSession(sessionId) && HasSessionByConnectionId(sessionId))
            {
                string connectionId = sessionId;
                SignalRContext.ConnectionIdToSessionId.TryRemove(connectionId, out sessionId);
            }
            Storage storage;
            while (_sessionDictionary.TryRemove(sessionId, out storage)) ;
        }

        public bool HasSession(string sessionId)
        {
            return _sessionDictionary.ContainsKey(sessionId);
        }

        private bool HasSessionByConnectionId(string connectionId)
        {
            return SignalRContext.ConnectionIdToSessionId.ContainsKey(connectionId);
        }

        private string ConnectionIdToSessionId(string connectionId)
        {
            return HasSessionByConnectionId(connectionId) ? SignalRContext.ConnectionIdToSessionId[connectionId] : null;
        }

        public IEnumerator<KeyValuePair<string, Storage>> GetEnumerator()
        {
            foreach (KeyValuePair<string, Storage> session in _sessionDictionary)
            {
                yield return session;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
