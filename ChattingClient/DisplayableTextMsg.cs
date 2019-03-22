using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChattingInterfaces;

namespace ChattingClient
{

    public class DisplayableTextMessage : IDisplayable
    {
        public MsgType msgType;
        public string content;
        public bool isPrivate;
        public string userName;

        public DisplayableTextMessage(string content, bool isPrivate, string userName) {
            this.msgType = MsgType.Text;
            this.content = content;
            this.isPrivate = isPrivate;
            this.userName = userName;
        }

        public MsgType getType() {
            return this.msgType;
        }

        public bool Display()
        {
            if (this.msgType == MsgType.Text)
            {
                ((MainWindow)Application.Current.MainWindow).DisplayTextMsg(this);
            }
            return true;
        }
    }
}
