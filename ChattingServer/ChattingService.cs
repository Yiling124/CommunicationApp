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

namespace ChattingServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]

    public class ChattingService : IChattingService
    {
        SessionManager sessionMg = new SessionManager();

        private ConnectedClient createNewConnectedClient(string userName)
        {
            var establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserName = userName;
            newClient.IpAddress = GetIpAddress(OperationContext.Current);

            return newClient;
        }

        public string CreateSession(string userName)
        {
            // create new client as owner of new session
            ConnectedClient newClient = createNewConnectedClient(userName);

            // Check if this session Owner already has own session created
            if (sessionMg.getAllSessions().ContainsKey(newClient.IpAddress))
            {
                return null;
            }
            Session newSession = new Session();
            newSession.setOwnerAddress(newClient.IpAddress);
            sessionMg.AddSession(newSession.getOwnerAddress(), newSession);
            string clientListForDisplay = JoinSession(userName, newClient.IpAddress);

            //Checking values sessionManager and session clientList
            foreach (KeyValuePair<Tuple<string, int>, Session> entry in sessionMg.getAllSessions())
            {
                Console.WriteLine("sessionMg includes sessions - Owner Address: {0}: ", entry.Key);
            }

            // print session information in the console
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in newSession.getClientList())
            {
                Console.WriteLine("session clientList memebers: " + entry.Value.UserName + "\n");
            }
            Console.WriteLine("=======================create session done ============================");
            return clientListForDisplay;
        }

        public string JoinSession(string userName, Tuple<string, int> ownerIpAddress)
        {
            Console.WriteLine("Join Session OWNER Ip:  {0} , port : {1} " , ownerIpAddress.Item1, ownerIpAddress.Item2);

            if (sessionMg.getAllSessions().ContainsKey(ownerIpAddress))
            {
                Console.WriteLine("find the session - let's join session");
                ConnectedClient newClientToJoin = createNewConnectedClient(userName);
                newClientToJoin.setJoinedSessionOwnerIp(ownerIpAddress);
                Session sessionToJoin = sessionMg.getAllSessions()[ownerIpAddress];
                sessionToJoin.AddClient(newClientToJoin.IpAddress, newClientToJoin);

                ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList = sessionToJoin.getClientList();
                ConcurrentDictionary<Tuple<string, int>, string> clientListForDisplay = BuildSessionClientsListDisp(currentSessionClientList);

                // send session client list to all the peers except for sender himself
                string listForDisplay = "";
                foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in currentSessionClientList)
                {
                    listForDisplay += String.Format("{0}, IP: {1}, Port: {2} \n", entry.Value.UserName, entry.Key.Item1, entry.Key.Item2);
                    if (entry.Key != newClientToJoin.IpAddress)
                    {
                        entry.Value.connection.GetPeerList(listForDisplay);
                    }
                }              
                return listForDisplay;
            }
            return null;
        }

        private ConcurrentDictionary<Tuple<string, int>, string> BuildSessionClientsListDisp(ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList)
        {
            ConcurrentDictionary<Tuple<string, int>, string> SessionClientListForDisplay = new ConcurrentDictionary<Tuple<string, int>, string>();
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> client in currentSessionClientList)
            {
                SessionClientListForDisplay.TryAdd(client.Key, client.Value.UserName);
            }
            return SessionClientListForDisplay;
        }

        public static Tuple<string, int> GetIpAddress(System.ServiceModel.OperationContext context)
        {
            var prop = context.IncomingMessageProperties;
            if (context.IncomingMessageProperties.ContainsKey(System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name))
            {
                var endpoint = prop[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]
                    as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                if (endpoint != null)
                {
                    Console.WriteLine("Client Address:{0}, Port:{1}", endpoint.Address, endpoint.Port);

                    Tuple<string, int> IpAddress = new Tuple<string, int>(endpoint.Address, endpoint.Port);
                    return IpAddress;
                }
            }
            return null;
        }


        public void SendMessageToAll(string message, string userName, Tuple<string, int> sessionOwnerIp)
        {
            Tuple<string, int> MsgSenderIpAddress = GetIpAddress(OperationContext.Current);
            Session targetSession = sessionMg.getAllSessions()[sessionOwnerIp];
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in targetSession.getClientList())
            {
                if (!(entry.Key).Equals(MsgSenderIpAddress))
                {
                    Console.WriteLine("sending message - receiver IP: " + entry.Key + "receiver Name: " + entry.Value.UserName);
                    entry.Value.connection.GetMessage(message, userName);
                }
            }
        }

        //public void Logout()
        //{
        //    var currentConnectedConnection = OperationContext.Current.GetCallbackChannel<IClient>();

        //    foreach (var client in _connectedClients)
        //    {
        //        if (client.Value.connection == currentConnectedConnection)
        //        {
        //            ConnectedClient removedClient;
        //            _connectedClients.TryRemove(client.Value.UserName, out removedClient);
        //            break;
        //        }
        //    }
        //    return;
        //}
    }
}