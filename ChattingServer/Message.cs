using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingServer
{
    public enum MessageType { text, file, UML };

    public class Message
    {
        public string messageContent { get; set; }
        public string senderName { get; set; }
        public Tuple<string, int> sessionOwnerAddress { get; set; }
        public Tuple<string, int> receiverIPAddress { get; set; }
        public MessageType type;

        public Message(string textConent, string sderName, Tuple<string, int> ownerIp, Tuple<string, int> receiverIp, MessageType msgType)
        {
            this.messageContent = textConent;
            this.senderName = sderName;
            this.sessionOwnerAddress = ownerIp;
            this.receiverIPAddress = receiverIPAddress;
            this.type = msgType;
        }
    }


}
