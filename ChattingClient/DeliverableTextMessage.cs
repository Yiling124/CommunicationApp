using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChattingClient;
using System.Windows;
using ChattingInterfaces;


namespace ChattingClient
{
    class DeliverableTextMessage : IDeliverable
    {
        MsgType msgType;
        string textConent;
        string senderName;
        Tuple<string, int> receipientAdrs;
        Tuple<string, int> ssOwnerAdrs;

        public DeliverableTextMessage(MsgType msgType, string textConent, string senderName, Tuple<string, int> receipientAdrs, Tuple<string, int> ssOwnerAdrs)
        {
            this.msgType = msgType;
            this.textConent = textConent;
            this.senderName = senderName;
            this.receipientAdrs = receipientAdrs;
            this.ssOwnerAdrs = ssOwnerAdrs;
        }

        public bool SendOut(IChattingService Server)
        {
            return Server.SendTextMessage(this.msgType, this.textConent, this.senderName, this.receipientAdrs, this.ssOwnerAdrs);
        }
    }
}
