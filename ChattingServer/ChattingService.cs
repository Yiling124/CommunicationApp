using ChattingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Collections.Concurrent;

namespace ChattingServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ChattingService : IChattingService
    {
        public ConcurrentDictionary<string, ConnectedClient> _connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        public string Login(string userName)
        {
            Console.Write("user login: " + userName);
            string userList = "";
            foreach (var client in _connectedClients)
            {
                if (client.Key.ToLower() != userName.ToLower())
                {
                    userList += client.Key + " ";
                }
            }

            var establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserName = userName;

            userList += userName;
            foreach (var client in _connectedClients)
            {
                client.Value.connection.GetPeerList(userList);
            }

            _connectedClients.TryAdd(userName, newClient);

            return userList;
        }


        public void UpdatePeerList()
        {
            //Dictionary<string, string> list = new Dictionary<string, string>();
            //foreach (var client in _connectedClients)
            //{
            //    list.Add(client.Key, "welcome");
            //    Console.WriteLine("client.key" + client.Key);
            //}
            foreach (var client in _connectedClients)
            {
                client.Value.connection.GetPeerList("one hardCoded string");

            }
        }


        public void SendMessageToAll(string message, string userName)
        {
            Console.WriteLine("Entering SendMessageToAll...");
            foreach (var client in _connectedClients) 
            {
                Console.WriteLine("Fetched client: " + client.Key);
                if (client.Key.ToLower() != userName.ToLower())
                {
                    Console.WriteLine("Sending message to " + client.Key);
                    client.Value.connection.GetMessage(message, userName);
                }
            }
        }
    }
}