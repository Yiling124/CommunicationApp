//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ChattingClient;
//using System.Windows;

//namespace ChattingClient
//{
//    class DisplayableTextMessage : IDisplayable
//    {
//        public bool isPrivate;
//        public string userName;
//        public string textMessage;

//        public DisplayableTextMessage(string messageContent, string userName,bool isPrivate)
//        {
//            this.textMessage = messageContent;
//            this.userName = userName;
//            this.isPrivate = isPrivate;
//        }

//        public string getContent()
//        {
//            return this.textMessage;
//        }

//        public void Display(MainWindow window)
//        {
//            string messageForDisplay = "";
//            if (isPrivate)
//            {
//                messageForDisplay += "(Private Msg) " + this.userName + ":" + this.textMessage + "\n";
//            }
//            else
//            {
//                messageForDisplay += this.userName + ":" + this.textMessage + "\n";
//            }
//            window.MessageTextBox.Text += messageForDisplay;
//            window.TextDisplayTextBox.ScrollToEnd();
//            window.MessageTextBox.Text = "";
//        }
//    }
//}