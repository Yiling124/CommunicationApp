/////////////////////////////////////////////////////////////////////////////                                     
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                             //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements an interface for messages to be put inside of a generic blockingQueue for the server side 
 *   all the inherited class have to implement Send() method
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ChattingInterfaces
{
    public interface IMessage
    {
        Tuple<string, int> GetSessionOwnerAdrs();
        Tuple<string, int> GetRecipientAdrs();
        void Send(IClient receipient, bool isPrivate);
        string GetSenderName();
    }
}