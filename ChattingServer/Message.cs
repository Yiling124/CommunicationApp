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
        public Tuple<string, int> sessionOwnerIPAddress { get; set; }
        public Tuple<string, int> receiverIPAddress { get; set; }
        public MessageType type;

        public Message(string textConent, string sderName, Tuple<string, int> receiverIp, Tuple<string, int> ownerIp, MessageType msgType)
        {
            this.messageContent = textConent;
            this.senderName = sderName;
            this.sessionOwnerIPAddress = ownerIp;
            this.receiverIPAddress = receiverIp;
            this.type = msgType;
        }
    }


}
