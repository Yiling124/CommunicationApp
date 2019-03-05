/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                              //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements a Session Manager class that locates in the Server, it collects all the information about current active chat sessions
 *   including peers inside of the session, and their owners 
 */


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
        // session manager locates in the server, it holds all the information about all session, all each session will have unique session owner Ip
        // session IP are used to identify session
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
