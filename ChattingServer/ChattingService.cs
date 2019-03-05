/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                              //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements the Server of the Application, it allows all user to communicate through
 *   Messages, a BlockingQueue is used to enque all the received messages and send them out to client 
 */


using ChattingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading;
using BlockingQueue;

namespace ChattingServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]

    public class ChattingService : IChattingService
    {
        SessionManager sessionMg = null;
        BlockingQueue<IMessage> messageBlockingQ = null;
        Thread messageThrd = null;

        public SessionManager GetSessionManager()
        {
            return this.sessionMg;
        }

        public ChattingService()
        {
            sessionMg = new SessionManager();
            messageBlockingQ = new BlockingQueue<IMessage>();
            messageThrd = new Thread(ThreadProc);
            messageThrd.IsBackground = true;
            messageThrd.Start();
        }
        
        // all incoming message will be posted onto a BlockingQueue, and it will be dequeued to be sent to the recipient
        private void ThreadProc()
        {
            while (true)
            {
                IMessage msg = messageBlockingQ.deQ();

                Session targetSession = sessionMg.getAllSessions()[msg.GetSessionOwnerAdrs()];
                if (targetSession == null) continue;
                ConnectedClient receipient;
                if (msg.GetRecipientAdrs() != null)
                {
                    receipient = targetSession.getClientList()[msg.GetRecipientAdrs()];
                    msg.Send(receipient.connection, true);
                    continue;
                }
                foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in targetSession.getClientList())
                {
                    if (!(entry.Value.UserName).Equals(msg.GetSenderName()))
                    {
                        msg.Send(entry.Value.connection, false);
                    }
                }
               
            }
        }

        // Connection channel is created by this function 
        private ConnectedClient createNewConnectedClient(string userName)
        {
            var establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserName = userName;
            newClient.IpAddress = GetIpAddress(OperationContext.Current);

            return newClient;
        }

        public Tuple<string, string, int> CreateSession(string userName)
        {
            ConnectedClient newClient = createNewConnectedClient(userName);

            // Check if this session Owner already has own session created
            if (sessionMg.getAllSessions().ContainsKey(newClient.IpAddress))
            {
                return null;
            }
            Session newSession = new Session();
            newSession.setOwnerAddress(newClient.IpAddress);
            sessionMg.AddSession(newSession.getOwnerAddress(), newSession);
            return  JoinSession(userName, newClient.IpAddress);
        }

        public Tuple<string, string, int> RequestJoin(string userName, Tuple<string, int> ssOwnerAdrs)
        {
            if (ssOwnerAdrs == null) return null;
            Session sessionFound;
            sessionMg.getAllSessions().TryGetValue(ssOwnerAdrs, out sessionFound);
            if (sessionFound != null)
            {
                ConnectedClient ssOwner;
                sessionFound.getClientList().TryGetValue(ssOwnerAdrs, out ssOwner);
                if (ssOwner.connection.ApproveJoin(userName)) {
                    return JoinSession(userName, ssOwnerAdrs);
                }
            }
            return null;
        }
        
        public Tuple<string, string, int> JoinSession(string userName, Tuple<string, int> ownerIpAddress)
        {
            if (sessionMg.getAllSessions().ContainsKey(ownerIpAddress))
            {
                ConnectedClient newClientToJoin = createNewConnectedClient(userName);
                newClientToJoin.setJoinedSessionOwnerIp(ownerIpAddress);
                Session sessionToJoin = sessionMg.getAllSessions()[ownerIpAddress];
                sessionToJoin.AddClient(newClientToJoin.IpAddress, newClientToJoin);

                ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList = sessionToJoin.getClientList();
                ConcurrentDictionary<Tuple<string, int>, string> clientStrListForDisplay = BuildSessionClientsListDisp(currentSessionClientList);

                // send session client list to all the peers except for sender himself
                string strListForDisplay = BuildSessionStrClientList(currentSessionClientList);
                SendUpdatedClientList(strListForDisplay, currentSessionClientList, newClientToJoin.IpAddress);
                return new Tuple<string, string, int>(strListForDisplay, ownerIpAddress.Item1, ownerIpAddress.Item2);
            }
            return null;
        }

        private void SendUpdatedClientList(string strListForDisplay, ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList, Tuple<string, int> IpAddressToAvoid)
        {
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in currentSessionClientList)
            {
                if (IpAddressToAvoid != null && IpAddressToAvoid == entry.Key)
                {
                    continue;
                }
                entry.Value.connection.GetPeerList(strListForDisplay);
            }

        }

        private string BuildSessionStrClientList(ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList)
        {
            string listForDisplay = "";
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in currentSessionClientList)
            {
                listForDisplay += String.Format("{0}, IP: {1}, Port: {2} \n", entry.Value.UserName, entry.Key.Item1, entry.Key.Item2);
            }
            return listForDisplay;
        }

        // updated peer list in each session will be rebuilt after new peer joining, or quitting 
        private ConcurrentDictionary<Tuple<string, int>, string> BuildSessionClientsListDisp(ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList)
        {
            ConcurrentDictionary<Tuple<string, int>, string> SessionClientListForDisplay = new ConcurrentDictionary<Tuple<string, int>, string>();
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> client in currentSessionClientList)
            {
                SessionClientListForDisplay.TryAdd(client.Key, client.Value.UserName);
            }
            return SessionClientListForDisplay;
        }

        public bool SendTextMessage(string message, string userName, Tuple<string, int> receiverIP, Tuple<string, int> sessionOwnerIP)
        {
            Session targetSession = sessionMg.getAllSessions()[sessionOwnerIP];
            if (targetSession == null) return false;
            if (receiverIP != null && !targetSession.getClientList().ContainsKey(receiverIP)) return false;

            IMessage msgToSend = new TextMessage(message, userName, receiverIP, sessionOwnerIP);
            messageBlockingQ.enQ(msgToSend);
            return true;
        }

        // peer will be deleted from the chat session when logging out, and the whole session will be deleted from the session Manager
        // when the last peer of a chat session left 
        public void Logout(Tuple<string, int> sessionOwnerIpAddress)
        {
            var currentConnectedConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            Session sessionFound = sessionMg.getAllSessions()[sessionOwnerIpAddress];
            ConcurrentDictionary<Tuple<string, int>, ConnectedClient> sessionClientList = sessionFound.getClientList();
            string clientListForDisplay = "";

            foreach (var client in sessionClientList)
            {
                if (client.Value.connection == currentConnectedConnection && sessionClientList.Count == 1)
                {
                    Session ssRemoved;
                    sessionMg.getAllSessions().TryRemove(sessionOwnerIpAddress, out ssRemoved);
                    return;
                } else if (client.Value.connection == currentConnectedConnection) {
                    ConnectedClient removedClient;
                    sessionClientList.TryRemove(client.Value.IpAddress, out removedClient);
                    clientListForDisplay = BuildSessionStrClientList(sessionClientList);
                    SendUpdatedClientList(clientListForDisplay, sessionClientList, null);
                    return;
                }
            }
            return;
        }

        private static Tuple<string, int> GetIpAddress(System.ServiceModel.OperationContext context)
        {
            var prop = context.IncomingMessageProperties;
            if (context.IncomingMessageProperties.ContainsKey(System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name))
            {
                var endpoint = prop[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]
                    as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                if (endpoint != null) return new Tuple<string, int>(endpoint.Address, endpoint.Port);
            }
            return null;
        }
    }
}