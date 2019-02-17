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

            return clientListForDisplay;
        }

        public string JoinSession(string userName, Tuple<string, int> ownerIpAddress)
        {
            Console.WriteLine("Join Session OWNER Ip:  {0} , port : {1} " , ownerIpAddress.Item1, ownerIpAddress.Item2);

            if (sessionMg.getAllSessions().ContainsKey(ownerIpAddress))
            {
                ConnectedClient newClientToJoin = createNewConnectedClient(userName);
                newClientToJoin.setJoinedSessionOwnerIp(ownerIpAddress);
                Session sessionToJoin = sessionMg.getAllSessions()[ownerIpAddress];
                sessionToJoin.AddClient(newClientToJoin.IpAddress, newClientToJoin);

                ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList = sessionToJoin.getClientList();
                ConcurrentDictionary<Tuple<string, int>, string> clientListForDisplay = BuildSessionClientsListDisp(currentSessionClientList);

                // send session client list to all the peers except for sender himself
                string listForDisplay = BuildSessionClientList(currentSessionClientList);
                SendUpdatedClientList(listForDisplay, currentSessionClientList, newClientToJoin.IpAddress);
                return listForDisplay;
            }
            return null;
        }

        private void SendUpdatedClientList(string listForDisplay, ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList, Tuple<string, int> IpAddressToAvoid)
        {
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in currentSessionClientList)
            {
                if (IpAddressToAvoid != null && IpAddressToAvoid == entry.Key)
                {
                    continue;
                }
                entry.Value.connection.GetPeerList(listForDisplay);
            }

        }

        public string BuildSessionClientList(ConcurrentDictionary<Tuple<string, int>, ConnectedClient> currentSessionClientList)
        {
            string listForDisplay = "";
            foreach (KeyValuePair<Tuple<string, int>, ConnectedClient> entry in currentSessionClientList)
            {
                listForDisplay += String.Format("{0}, IP: {1}, Port: {2} \n", entry.Value.UserName, entry.Key.Item1, entry.Key.Item2);
            }
            return listForDisplay;
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

        public void Logout(Tuple<string, int> sessionOwnerIpAddress)
        {
            var currentConnectedConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            ConcurrentDictionary<Tuple<string, int>, ConnectedClient> sessionClientList = sessionMg.getAllSessions()[sessionOwnerIpAddress].getClientList();
            string clientListForDisplay = "";

            foreach (var client in sessionClientList)
            {
                if (client.Value.connection == currentConnectedConnection)
                {
                    ConnectedClient removedClient;
                    sessionClientList.TryRemove(client.Value.IpAddress, out removedClient);
                    clientListForDisplay = BuildSessionClientList(sessionClientList);
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
                if (endpoint != null)
                {
                    Console.WriteLine("Client Address:{0}, Port:{1}", endpoint.Address, endpoint.Port);

                    Tuple<string, int> IpAddress = new Tuple<string, int>(endpoint.Address, endpoint.Port);
                    return IpAddress;
                }
            }
            return null;
        }
    }
}