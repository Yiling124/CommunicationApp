using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChattingInterfaces;

namespace ChattingClient
{
    public class DisplayableUMLmsg : IDisplayable
    {
        public MsgType msgType;
        public string content { get; set; }
        public bool isPrivate;
        public string userName;

        public DisplayableUMLmsg(string content, bool isPrivate, string userName)
        {
            this.msgType = MsgType.UML;
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
            if (this.msgType == MsgType.UML)
            {
                ((MainWindow)Application.Current.MainWindow).DisplayUMLmsg(this.content);
            }
            return true;
        }
    }
}
