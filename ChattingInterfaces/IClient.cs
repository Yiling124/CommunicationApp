/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                              //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements a Client interface exposed to Server side 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ChattingInterfaces
{
    public interface IClient
    {
        [OperationContract]
        void GetMessage(string message, string userName, bool isPrivate);

        [OperationContract]
        void GetPeerList(string clientListForDisplay);

        [OperationContract]
        bool ApproveJoin(string userName);
    }
}