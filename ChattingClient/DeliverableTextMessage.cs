using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChattingClient;
using System.Windows;

namespace ChattingClient
{
    public class DeliverableTextMessage : IDeliverable
    {
        string textConent;
        string senderName;
        Tuple<string, int> receipientAdrs;
        Tuple<string, int> ssOwnerAdrs;

        public DeliverableTextMessage(string textConent, string senderName, Tuple<string, int> receipientAdrs, Tuple<string, int> ssOwnerAdrs)
        {
            this.textConent = textConent;
            this.senderName = senderName;
            this.receipientAdrs = receipientAdrs;
            this.ssOwnerAdrs = ssOwnerAdrs;
        }

        public bool SendOut(IChattingService Server)
        {
            
            bool isSent = Server.SendTextMessage(this.textConent, this.senderName, this.receipientAdrs, this.ssOwnerAdrs);
            ((MainWindow)Application.Current.MainWindow).receiverIpTextBox.Text = "";
            ((MainWindow)Application.Current.MainWindow).receiverPortTextBox.Text = "";
            return isSent;
        }
    }
}