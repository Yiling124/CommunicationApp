using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using ChattingInterfaces;
using System.Windows;


namespace ChattingClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientCallback : IClient
    {
        public void GetMessage(string message, string userName)
        {
            ((MainWindow)Application.Current.MainWindow).TakeMessage(message, userName);
        }

        //public void GetPeerList(Dictionary<string, string> peerList)
        //{
        //    Console.WriteLine("getPeerList get called");
        //    ((MainWindow)Application.Current.MainWindow).DisplayOnlinePeerList(peerList);
        //}

        public void GetPeerList(string peerList)
        {
            ((MainWindow)Application.Current.MainWindow).DisplayOnlinePeerList(peerList);
        }
    }
}
