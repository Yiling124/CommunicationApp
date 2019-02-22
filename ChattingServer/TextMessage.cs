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
        public string TextMessageContent { get; set; }
        public string TextsenderName { get; set; }
        public Tuple<string, int> TextSessionOwnerAdrs { get; set; }
        public Tuple<string, int> TextReceiverAdrs { get; set; }

        public TextMessage(string textConent, string sderName, Tuple<string, int> receiverIp, Tuple<string, int> ownerIp)
        {
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

        public void Send(IClient receipient, bool isPrivate)
        {
            receipient.GetMessage(this.TextMessageContent, this.TextsenderName, isPrivate);
        }

        public string GetSenderName()
        {
            return this.TextsenderName;
        }
    }
}
