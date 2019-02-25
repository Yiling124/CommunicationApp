using ChattingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlockingQueue;
using System.Threading;


namespace ChattingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IChattingService Server;
        private static DuplexChannelFactory<IChattingService> _channelFactory;
        Thread msgOutThrd = null;
        BlockingQueue<IDeliverable> msgOutBlockingQ;

        public MainWindow()
        {
            InitializeComponent();
            _channelFactory = new DuplexChannelFactory<IChattingService>(new ClientCallback(), "ChattingServiceEndPoint");
            Server = _channelFactory.CreateChannel();

            msgOutBlockingQ = new BlockingQueue<IDeliverable>();
            msgOutThrd = new Thread(MsgOutThreadProc);
            msgOutThrd.IsBackground = true;
            msgOutThrd.Start();
        }

        public void MsgOutThreadProc()
        {
            while (true)
            {
                IDeliverable msgForSendOut = msgOutBlockingQ.deQ();
                bool isSent = msgForSendOut.SendOut(Server);
                if (isSent) continue;
                MessageBox.Show("Message failed.");
            }
        }

        public void TakeMessage(string message, string userName, bool isPrivate)
        {
            if (isPrivate)
            {
                TextDisplayTextBox.Text += "(Privte Msg) " + userName + ":" + message + "\n";
            }
            else {
                TextDisplayTextBox.Text += userName + ":" + message + "\n";
            }

            receiverIpTextBox.Text = "";
            receiverPortTextBox.Text = "";
            MessageTextBox.Text = "";
            TextDisplayTextBox.ScrollToEnd();
        }


        private void CreateSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (userNameTextBox.Text.Length == 0) return;
            Tuple<string, string, int>  newSessionInfo = Server.CreateSession(userNameTextBox.Text);
            string ClientListForDisplay = newSessionInfo.Item1;

            if (ClientListForDisplay != null)
            {
                WelcomeLabel.Content = "welcome " + userNameTextBox.Text + "!";
                TextDisplayTextBox.IsEnabled = false;
                DisplayOnlinePeerList(ClientListForDisplay);
                MessageBox.Show("Your New Session Created " + userNameTextBox.Text);

                userNameTextBox.IsEnabled = false;
                CreateSessionButton.IsEnabled = false;

                sessionOwnerPortTextBox.Text = newSessionInfo.Item3.ToString();
                sessionOwnerIpTextBox.Text = newSessionInfo.Item2;
                sessionOwnerPortTextBox.IsEnabled = false;
                sessionOwnerIpTextBox.IsEnabled = false;
                JoinSessionButton.IsEnabled = false;
            } else{
                MessageBox.Show("You already have a session created!");
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Length == 0) return;

            Tuple<string, int> privatReceipientAdrs = buildIpAdrs(receiverIpTextBox.Text, receiverPortTextBox.Text);
            if ((receiverIpTextBox.Text.Length > 0 || receiverPortTextBox.Text.Length > 0) && privatReceipientAdrs == null) {
                MessageBox.Show("Please double check recepient address !");
                return;
            }

            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(sessionOwnerIpTextBox.Text, sessionOwnerPortTextBox.Text);
            IDeliverable msgOut = new DeliverableTextMessage(MessageTextBox.Text, userNameTextBox.Text, privatReceipientAdrs, ssOwnerAdrs);

            msgOutBlockingQ.enQ(msgOut);

            bool isPrivate = privatReceipientAdrs == null ? false : true;
            TakeMessage(MessageTextBox.Text, "you", isPrivate);
        }

        public void DisplayOnlinePeerList(string userList)
        {
            TextDisplayTextBox_OnlinePeers.Text = userList;
            TextDisplayTextBox_OnlinePeers.IsEnabled = false;
            TextDisplayTextBox_OnlinePeers.ScrollToEnd();
        }

        private Tuple<string, int> buildIpAdrs(string ip, string port)
        {
            int portVal;
            int.TryParse(port, out portVal);
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) || portVal == 0) return null;
            Tuple<string, int> adrs;
            return adrs = new Tuple<string, int>(ip, Convert.ToInt32(port));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sessionOwnerIpTextBox.Text.Length == 0 && sessionOwnerPortTextBox.Text.Length == 0) return;
            Tuple<string, int> sessionOwnerIpAddress = buildIpAdrs(sessionOwnerIpTextBox.Text, sessionOwnerPortTextBox.Text);
            Server.Logout(sessionOwnerIpAddress);
        }

        private void RequestJoin()
        {
            Tuple<string, int> sessionOwnerIpAddress = buildIpAdrs(sessionOwnerIpTextBox.Text, sessionOwnerPortTextBox.Text);
            Tuple<string, string, int> peerList = Server.RequestJoin(userNameTextBox.Text, sessionOwnerIpAddress);
            if (peerList != null)
            {
                DisplayOnlinePeerList(peerList.Item1);
                MessageBox.Show("Welcome to the chat session!");
                userNameTextBox.IsEnabled = false;
                sessionOwnerPortTextBox.IsEnabled = false;
                sessionOwnerIpTextBox.IsEnabled = false;
                JoinSessionButton.IsEnabled = false;
            }
            else {
                MessageBox.Show("OOPS, Pls double check session address!");
                sessionOwnerPortTextBox.Text = "";
                sessionOwnerIpTextBox.Text = "";
            }
        }

        public bool isApproved(string userName)
        {
            string messageBoxText = String.Format("{0} wants to joint, approved?", userName);
            string caption = "Join Session Request";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.No:
                    return false;
            }
            return false;
        }

        private void JoinSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (userNameTextBox.Text.Length > 0 && sessionOwnerIpTextBox.Text.Length > 0 && sessionOwnerPortTextBox.Text.Length > 0)
            {
                RequestJoin();
            }
        }
    }
}
