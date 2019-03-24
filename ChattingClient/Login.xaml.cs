using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ServiceModel;
using ChattingInterfaces;

namespace ChattingClient
{
    public partial class Login : Window
    {
        public static IChattingService Server;
        private static DuplexChannelFactory<IChattingService> _channelFactory;

        public Login()
        {
            InitializeComponent();
            _channelFactory = new DuplexChannelFactory<IChattingService>(new ClientCallback(), "ChattingServiceEndPoint");
            Server = _channelFactory.CreateChannel();
        }

        private void CreateSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (userNameTextBox.Text.Length == 0) return;
            Tuple<string, string, int> newSSInfo = Server.CreateSession(userNameTextBox.Text);
            if (newSSInfo.Item1 != null)
            {
                MainWindow main = new MainWindow(userNameTextBox.Text, newSSInfo.Item2, newSSInfo.Item3, newSSInfo.Item1);
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            } else
            {
                MessageBox.Show("You already have a session created!");
            }
        }



        private void RequestJoin()
        {
            // this is temp for dev
            this.ssIpTextBox.Text = "172.16.132.128";
            Tuple<string, int> sessionOwnerIpAddress = buildIpAdrs("172.16.132.128", ssPortBoxTextBox.Text);

            //The following line needs to be uncommented
            //Tuple<string, int> sessionOwnerIpAddress = buildIpAdrs(ssIpTextBox.Text, ssPortBoxTextBox.Text);
            Tuple<string, string, int> newSSInfo = Server.RequestJoin(userNameTextBox.Text, sessionOwnerIpAddress);
            if (newSSInfo.Item1 != null)
            {
                MainWindow main = new MainWindow(userNameTextBox.Text, newSSInfo.Item2, newSSInfo.Item3, newSSInfo.Item1);
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            }
            else
            {
                MessageBox.Show("OOPS, Pls double check session address!");
                ssPortBoxTextBox.Text = "";
                ssIpTextBox.Text = "";
            }
        }

        private Tuple<string, int> buildIpAdrs(string ip, string port)
        {
            int portVal;
            int.TryParse(port, out portVal);
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) || portVal == 0) return null;
            Tuple<string, int> adrs;
            return adrs = new Tuple<string, int>(ip, Convert.ToInt32(port));
        }

        private void JoinSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (userNameTextBox.Text.Length > 0 && ssIpTextBox.Text.Length > 0 && ssPortBoxTextBox.Text.Length > 0)
            {
                RequestJoin();
            }
        }
    }
}
