using System;
using System.Collections.Generic;
using ChattingClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ChattingServer
{
    public class Session
    {
        private ConcurrentDictionary<Tuple<string, int>, ConnectedClient> clientList = new ConcurrentDictionary<Tuple<string, int>, ConnectedClient>();
        private Tuple<string, int> OwnerAddress;

        public ConnectedClient RemoveClient(Tuple<string, int> ipAddress)
        {
            ConnectedClient removedClient;
            this.clientList.TryRemove(ipAddress, out removedClient);
            return removedClient;
        }

        public bool AddClient(Tuple<string, int> ipAddress, ConnectedClient newClient) {
            return this.clientList.TryAdd(ipAddress, newClient);
        }

        public void setOwnerAddress(Tuple<string, int> ipAddress)
        {
            this.OwnerAddress = ipAddress;
        }

        public Tuple<string, int> getOwnerAddress()
        {
            return this.OwnerAddress;
        }

        public ConcurrentDictionary<Tuple<string, int>, ConnectedClient> getClientList()
        {
            return this.clientList;
        }


    }
}
