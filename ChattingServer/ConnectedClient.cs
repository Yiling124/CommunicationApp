using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChattingInterfaces;

namespace ChattingServer
{
    public class ConnectedClient
    { 
        public IClient connection;
        public string UserName { get; set; }
        public Tuple<string, int> IpAddress { get; set; }
        public Tuple<string, int> JoinedSessionOwnerIp = null;

        public void setJoinedSessionOwnerIp(Tuple<string, int> ipAddress)
        {
            this.JoinedSessionOwnerIp = ipAddress;
        }

        public Tuple<string, int> getJoinedSessionOwnerIp()
        {
            return this.JoinedSessionOwnerIp;
        }
    }
}
