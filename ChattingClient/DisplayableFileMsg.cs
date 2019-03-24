using ChattingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChattingClient
{
    class DisplayableFileMsg : IDisplayable
    {
        public MsgType msgType;
        public string content;
        public bool isPrivate;
        public string userName;

        public DisplayableFileMsg(string content, bool isPrivate, string userName)
        {
            this.msgType = MsgType.File;
            this.content = content;
            this.isPrivate = isPrivate;
            this.userName = userName;
        }

        public MsgType getType()
        {
            return this.msgType;
        }

        public bool Display()
        {
            ((MainWindow)Application.Current.MainWindow).DisplayFileMsg(this.content);
            return true;
        }
    }
}
