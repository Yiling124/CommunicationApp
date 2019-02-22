
using ChattingClient;
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


        //public void MsgInThreadProc()
        //{
        //    while (true)
        //    {
        //        IDisplayable msgForDisplay = msgInBlockingQ.deQ();
        //        string dispContent = msgForDisplay.Display();
        //        MessageBox.Show("dispContent: " + dispContent);

        //        TextDisplayTextBox.Text += dispContent;
        //        TextDisplayTextBox.ScrollToEnd();
        //        this.MessageTextBox.Text = "";
        //    }
        //}

        public void MsgOutThreadProc()
        {
            while (true)
            {
                IDeliverable msgForSendOut = msgOutBlockingQ.deQ();
                msgForSendOut.SendOut(Server);
            }
        }

        public void TakeMessage(string message, string userName, bool isPrivate)
        {
            if (isPrivate)
            {
                TextDisplayTextBox.Text += "(Privte Msg) " + userName + ":" + message + "\n";
            }
            else
            {
                TextDisplayTextBox.Text += userName + ":" + message + "\n";
            }
            TextDisplayTextBox.ScrollToEnd();
            MessageTextBox.Text = "";
        }

        private void SendMessage()
        {
            Tuple<string, int> privatReceipientAdrs = buildIpAdrs(receiverIpTextBox.Text, receiverPortTextBox.Text);
            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(sessionOwnerIpTextBox.Text, sessionOwnerPortTextBox.Text);
            MessageBox.Show("sendMessage- privatReceipientAdrs: " + privatReceipientAdrs);
            MessageBox.Show("sendMessage- ssOwnerAdrs: " + ssOwnerAdrs);
            IDeliverable msgOut = new DeliverableTextMessage(MessageTextBox.Text, userNameTextBox.Text, privatReceipientAdrs, ssOwnerAdrs);
            //msgOutBlockingQ.enQ(msgOut);
            TakeMessage(MessageTextBox.Text, "you", privatReceipientAdrs != null);
        }


        private void CreateSessionButton_Click(object sender, RoutedEventArgs e)
        {
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
            SendMessage();
        }

        private Tuple<string, int> buildIpAdrs(string ip, string port)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port)) return null;
            Tuple<string, int> adrs;
            return adrs = new Tuple<string, int>(ip, Convert.ToInt32(port));
        }

        public void DisplayOnlinePeerList(string userList)
        {
            TextDisplayTextBox_OnlinePeers.Text = userList;
            TextDisplayTextBox_OnlinePeers.ScrollToEnd();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int sessionOwnerPort = Convert.ToInt32(sessionOwnerPortTextBox.Text);
            Tuple<string, int> sessionOwnerIpAddress = new Tuple<string, int>(sessionOwnerIpTextBox.Text, sessionOwnerPort);
            Server.Logout(sessionOwnerIpAddress);
        }

        private void JoinSessionButton_Click(object sender, RoutedEventArgs e)
        {
            WelcomeLabel.Content = "welcome " + userNameTextBox.Text + "!";
            userNameTextBox.IsEnabled = false;

            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(sessionOwnerIpTextBox.Text, sessionOwnerPortTextBox.Text);
            string userList = Server.JoinSession(userNameTextBox.Text, ssOwnerAdrs).Item1;

            MessageBox.Show("welcome to the chat session!");

            DisplayOnlinePeerList(userList);
            sessionOwnerPortTextBox.IsEnabled = false;
            sessionOwnerIpTextBox.IsEnabled = false;
            JoinSessionButton.IsEnabled = false;
        }
    }
}
