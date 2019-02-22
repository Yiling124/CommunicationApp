using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ChattingClient
{
    public interface IClient
    {
        [OperationContract]
        void GetMessage(string message, string userName, bool isPrivate);

        [OperationContract]
        void GetPeerList(string clientListForDisplay);
    }
}
