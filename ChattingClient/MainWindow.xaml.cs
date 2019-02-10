
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

        public void TakeMessage(string message, string userName)
        {
            TextDisplayTextBox.Text += userName + ":" + message + "\n";
            TextDisplayTextBox.ScrollToEnd();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //int returnValue = Server.Login(userNameTextBox.Text);
            //if (returnValue == 1)
            //{
            //    MessageBox.Show("You are already logged in");
            //}
            //else if (returnValue == 0)
            //{
            //    WelcomeLabel.Content = "Welcome " + userNameTextBox.Text + "!";
            //    TextDisplayTextBox.IsEnabled = false;
            //    MessageBox.Show("You logged in!");


            //    userNameTextBox.IsEnabled = false;
            //    LoginButton.IsEnabled = false;
            //}
            string userList = Server.Login(userNameTextBox.Text);
            WelcomeLabel.Content = "welcome " + userNameTextBox.Text + "!";
            TextDisplayTextBox.IsEnabled = false;
            MessageBox.Show("you logged in " + userList);

            userNameTextBox.IsEnabled = false;
            LoginButton.IsEnabled = false;

            DisplayOnlinePeerList(userList);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Length == 0)
            {
                return;
            }
            Server.SendMessageToAll(MessageTextBox.Text, userNameTextBox.Text);
            TakeMessage(MessageTextBox.Text, "you");
            MessageTextBox.Text = "";

            //Server.UpdatePeerList();
        }

        //public void DisplayOnlinePeerList(Dictionary<string, string> peerList)
        //{ 
        //    Console.WriteLine("DisplayOnlinePeerList got called");
        //    foreach (var peer in peerList)
        //    {
        //        TextDisplayTextBox_OnlinePeers.Text += peer.Key + " " + peer.Value + "\n";
        //    }
        //    TextDisplayTextBox_OnlinePeers.ScrollToEnd();
        //}

        public void DisplayOnlinePeerList(string peerList)
        {
            Console.WriteLine("DisplayOnlinePeerList got called");
            TextDisplayTextBox_OnlinePeers.Text = "peerList: " + peerList + "\n";
            TextDisplayTextBox_OnlinePeers.ScrollToEnd();
            // TextDisplayTextBox_OnlinePeers.Text += peerList + "\n";
            //foreach (var peer in peerList)
            //{
            //    TextDisplayTextBox_OnlinePeers.Text += peer.Key + " " + peer.Value + "\n";
            //}
            //TextDisplayTextBox_OnlinePeers.ScrollToEnd();
        }
    }
}
