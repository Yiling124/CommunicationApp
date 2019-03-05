
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using ChattingInterfaces;
using System.Windows;
using System.Collections.Concurrent;


namespace ChattingClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientCallback : IClient
    {
        public void GetMessage(string message, string userName, bool isPrivate)
        {
            ((MainWindow)Application.Current.MainWindow).TakeMessage(message, userName, isPrivate);
        }

        public void GetPeerList(string clientList)
        {
            ((MainWindow)Application.Current.MainWindow).DisplayOnlinePeerList(clientList);
        }

        public bool ApproveJoin(string userName)
        {
            return ((MainWindow)Application.Current.MainWindow).isApproved(userName);
        }
    }
}
 