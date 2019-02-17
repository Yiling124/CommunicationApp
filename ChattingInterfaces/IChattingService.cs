using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Collections.Concurrent;

namespace ChattingInterfaces
{
    [ServiceContract(CallbackContract = typeof(IClient))]
    public interface IChattingService
    {
        [OperationContract]
        string CreateSession(string userName);

        [OperationContract]
        string JoinSession(string userName, Tuple<string, int> ownerIpAddress);

        //[OperationContract]
        //void Logout();

        [OperationContract]
        void SendMessageToAll(string message, string userName, Tuple<string, int> sessionOwnerIpAddress);
    }
}
