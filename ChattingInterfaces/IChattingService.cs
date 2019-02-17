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
        Tuple<string, string, int> CreateSession(string userName);

        [OperationContract]
        Tuple<string, string, int> JoinSession(string userName, Tuple<string, int> ownerIpAddress);

        [OperationContract]
        void Logout(Tuple<string, int> sessionOwnerIpAddress);

        [OperationContract]
        void SendMessageToAll(string message, string userName, Tuple<string, int> sessionOwnerIpAddress);

        [OperationContract]
        bool SendPrivateMessage(string message, string userName, Tuple<string, int> receiverIpAddress, Tuple<string, int> sessionOwnerIpAddress);
    }
}
