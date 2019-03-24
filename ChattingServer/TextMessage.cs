/////////////////////////////////////////////////////////////////////////////                                     
//  Author:       YiLing Jiang                                             //
/////////////////////////////////////////////////////////////////////////////

/*
 *   This package implements a TextMessage class that works closely with a generic BlockingQueue, 
 *   the BlockingQue allows to enqueue and dequeue TextMessages 
 *   This class implements a IMessage interface, including all its required methods
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChattingInterfaces;
using ChattingServer;

namespace ChattingServer
{
    public class TextMessage : IMessage
    {
        public MsgType msgType { get; set; }
        public string TextMessageContent { get; set; }
        public string TextsenderName { get; set; }
        public Tuple<string, int> TextSessionOwnerAdrs { get; set; }
        public Tuple<string, int> TextReceiverAdrs { get; set; }

        public TextMessage(MsgType msgType, string textConent, string sderName, Tuple<string, int> receiverIp, Tuple<string, int> ownerIp)
        {
            this.msgType = msgType;
            this.TextMessageContent = textConent;
            this.TextsenderName = sderName;
            this.TextReceiverAdrs = receiverIp;
            this.TextSessionOwnerAdrs = ownerIp;
        }

        public Tuple<string, int> GetRecipientAdrs()
        {
            return this.TextReceiverAdrs;
        }

        public Tuple<string, int> GetSessionOwnerAdrs()
        {
            return this.TextSessionOwnerAdrs;
        }

        // this implements an important functionality required by IMessage Interface
        public void Send(IClient receipient, bool isPrivate)
        {
            receipient.GetMessage(this.msgType, this.TextMessageContent, this.TextsenderName, isPrivate);
        }

        public string GetSenderName()
        {
            return this.TextsenderName;
        }
    }
}
