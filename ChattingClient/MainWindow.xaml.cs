
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


namespace ChattingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IChattingService Server;
        private static DuplexChannelFactory<IChattingService> _channelFactory;
      

        public MainWindow()
        {
            InitializeComponent();
            _channelFactory = new DuplexChannelFactory<IChattingService>(new ClientCallback(), "ChattingServiceEndPoint");
            Server = _channelFactory.CreateChannel();

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
            TextDisplayTextBox.ScrollToEnd();
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
            if (MessageTextBox.Text.Length == 0)
            {
                return;
            }

            int sessionOwnerPort = Convert.ToInt32(sessionOwnerPortTextBox.Text);
            Tuple<string, int> sessionOwnerIpAddress = new Tuple<string, int>(sessionOwnerIpTextBox.Text, sessionOwnerPort);

            if (!string.IsNullOrEmpty(receiverIpTextBox.Text) && !string.IsNullOrEmpty(receiverPortTextBox.Text))
            {
                int privateReceiverPort = Convert.ToInt32(receiverPortTextBox.Text);
                Tuple<string, int> privateReceiverIpAddress = new Tuple<string, int>(receiverIpTextBox.Text, privateReceiverPort);

                if (Server.SendTextMessage(MessageTextBox.Text, userNameTextBox.Text, privateReceiverIpAddress, sessionOwnerIpAddress))
                {
                    TakeMessage(MessageTextBox.Text, "you", true);
                }
                else {
                    MessageBox.Show("The message receiver is not in your session");
                }
                receiverIpTextBox.Text = "";
                receiverPortTextBox.Text = "";
                
            } else {
                Server.SendTextMessage(MessageTextBox.Text, userNameTextBox.Text, null, sessionOwnerIpAddress);
                TakeMessage(MessageTextBox.Text, "you", false);
            }
            MessageTextBox.Text = "";
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

            int sessionOwnerPort = Convert.ToInt32(sessionOwnerPortTextBox.Text);
            Tuple<string, int> sessionOwnerIpAddress = new Tuple<string, int>(sessionOwnerIpTextBox.Text, sessionOwnerPort);
            string userList = Server.JoinSession(userNameTextBox.Text, sessionOwnerIpAddress).Item1;

            MessageBox.Show("join session got called" + userList);

            DisplayOnlinePeerList(userList);
            sessionOwnerPortTextBox.IsEnabled = false;
            sessionOwnerIpTextBox.IsEnabled = false;
            JoinSessionButton.IsEnabled = false;
        }
    }
}
