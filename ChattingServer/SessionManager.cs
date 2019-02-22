using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;

namespace ChattingServer
{
    public class SessionManager
    {
        private ConcurrentDictionary<Tuple<string, int>, Session> sessionsList = new ConcurrentDictionary<Tuple<string, int>, Session>();

        public bool AddSession(Tuple<string, int> ownerIpAddress, Session session)
        {
            return sessionsList.TryAdd(ownerIpAddress, session);
        }

        public Session RemoveSession(Tuple<string, int> ipAddress)
        {
            Session removedSession;
            sessionsList.TryRemove(ipAddress, out removedSession);
            return removedSession;
        }

        public ConcurrentDictionary<Tuple<string, int>, Session> getAllSessions()
        {
            return sessionsList;
        }
    }
}
