/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                             //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements all the code related to UI controls, users will be able to interact with the application 
 *   through this front end UI 
 */

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
using DragAndDrop;
using Rectangle = DragAndDrop.Rectangle;
using System.Windows.Markup;
using System.IO;
using System.Xml.Serialization;

namespace ChattingClient
{
    public partial class MainWindow : Window
    {
        public static IChattingService Server;
        private static DuplexChannelFactory<IChattingService> _channelFactory;
        Thread msgOutThrd = null;
        public String userName = null;
        public String sessionIp = null;
        int sessionPort = 0;
        BlockingQueue<IDeliverable> msgOutBlockingQ;
        String path = "";

        // all the message sending out are enqueued into a Blocking Queue in a child thread
        public MainWindow(String _userName, String _sessionIp, int _sessionPort, String peerList)
        {
            InitializeComponent();
            SetLabels(_userName, _sessionIp, _sessionPort, peerList);
            _channelFactory = new DuplexChannelFactory<IChattingService>(new ClientCallback(), "ChattingServiceEndPoint");
            Server = _channelFactory.CreateChannel();

            msgOutBlockingQ = new BlockingQueue<IDeliverable>();
            msgOutThrd = new Thread(MsgOutThreadProc);
            msgOutThrd.IsBackground = true;
            msgOutThrd.Start();
        }

        private void SetLabels(String _userName, String _sessionIp, int _sessionPort, String peerList) {
            this.userName = _userName;
            this.sessionIp = _sessionIp;
            this.sessionPort = _sessionPort;
            WelcomeLabel.Content = "Welcome " + this.userName + "!";
            SessionIpLabel.Content = "Session IP: " + this.sessionIp;
            SessionPortLabel.Content = "Session Port: " + this.sessionPort;
            DisplayOnlinePeerList(peerList);
        }

        // this child thread dequeue message and post message to server 
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

        // Incoming message will be displayed on the UI 
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
            //TextDisplayTextBox.ScrollToEnd();
        }


        //private void CreateSessionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (userNameTextBox.Text.Length == 0) return;
        //    Tuple<string, string, int>  newSessionInfo = Server.CreateSession(userNameTextBox.Text);
        //    string ClientListForDisplay = newSessionInfo.Item1;

        //    if (ClientListForDisplay != null)
        //    {
        //        WelcomeLabel.Content = "welcome " + userNameTextBox.Text + "!";
        //        TextDisplayTextBox.IsEnabled = false;
        //        DisplayOnlinePeerList(ClientListForDisplay);
        //        MessageBox.Show("Your New Session Created " + userNameTextBox.Text);

        //        userNameTextBox.IsEnabled = false;
        //        CreateSessionButton.IsEnabled = false;

        //        sessionOwnerPortTextBox.Text = newSessionInfo.Item3.ToString();
        //        sessionOwnerIpTextBox.Text = newSessionInfo.Item2;
        //        sessionOwnerPortTextBox.IsEnabled = false;
        //        sessionOwnerIpTextBox.IsEnabled = false;
        //        JoinSessionButton.IsEnabled = false;
        //    } else{
        //        MessageBox.Show("You already have a session created!");
        //    }
        //}

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (MessageTextBox.Text.Length == 0) return;

            Tuple<string, int> privatReceipientAdrs = buildIpAdrs(receiverIpTextBox.Text, receiverPortTextBox.Text);
            if ((receiverIpTextBox.Text.Length > 0 || receiverPortTextBox.Text.Length > 0) && privatReceipientAdrs == null) {
                MessageBox.Show("Please double check recepient address !");
                return;
            }

            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(this.sessionIp, sessionPort.ToString());
            IDeliverable msgOut = new DeliverableTextMessage(MessageTextBox.Text, this.userName, privatReceipientAdrs, ssOwnerAdrs);
            msgOutBlockingQ.enQ(msgOut);

            bool isPrivate = privatReceipientAdrs == null ? false : true;
            TakeMessage(MessageTextBox.Text, "you", isPrivate);
        }

        // Updated peerList will be displayed on UI with this function 
        public void DisplayOnlinePeerList(string userList)
        {
            TextDisplayTextBox_OnlinePeers.Text = userList;
            TextDisplayTextBox_OnlinePeers.IsEnabled = false;
           // TextDisplayTextBox_OnlinePeers.ScrollToEnd();
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
            if (this.sessionIp.Length == 0 && this.sessionPort == 0) return;
            Tuple<string, int> sessionOwnerIpAddress = buildIpAdrs(this.sessionIp, this.sessionPort.ToString());
            Server.Logout(sessionOwnerIpAddress);
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

        private void panel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Object"))
            {
                // These Effects values are used in the drag source's
                // GiveFeedback event handler to determine which cursor to display.
                if (e.KeyStates == DragDropKeyStates.ControlKey)
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
        }

        private void panel_Drop(object sender, DragEventArgs e)
        {
            // If an element in the panel has already handled the drop,
            // the panel should not also handle it.
            if (e.Handled == false)
            {
                Panel _panel = (Panel)sender;
                UIElement _element = (UIElement)e.Data.GetData("Object");
                Point dropPoint = e.GetPosition(this.dropPanel);
                MessageBox.Show(dropPoint.X + "-" + dropPoint.Y);
                if (_panel != null && _element != null)
                {
                    // Get the panel that the element currently belongs to,
                    // then remove it from that panel and add it the Children of
                    // the panel that its been dropped on.
                    Panel _parent = (Panel)VisualTreeHelper.GetParent(_element);

                    if (_parent != null)
                    {
                        if (e.KeyStates == DragDropKeyStates.ControlKey &&
                            e.AllowedEffects.HasFlag(DragDropEffects.Copy))
                        {
                            if (_element is Rectangle)
                            {
                                Rectangle _rectangle = new Rectangle((Rectangle)_element);
                                _panel.Children.Add(_rectangle);
                                Canvas.SetLeft(_rectangle, dropPoint.X);
                                Canvas.SetTop(_rectangle, dropPoint.Y);
                            }
                            else
                            {
                                UsingConnector _uc = new UsingConnector((UsingConnector)_element);
                                _panel.Children.Add(_uc);
                                Canvas.SetLeft(_uc, dropPoint.X);
                                Canvas.SetTop(_uc, dropPoint.Y);
                            }
                            // set the value to return to the DoDragDrop call
                            e.Effects = DragDropEffects.Copy;
                        }
                        else if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                        {
                            _parent.Children.Remove(_element);
                            _panel.Children.Add(_element);
                            Canvas.SetLeft(_element, dropPoint.X);
                            Canvas.SetTop(_element, dropPoint.Y);
                            // set the value to return to the DoDragDrop call
                            e.Effects = DragDropEffects.Move;
                        }
                    }
                }
            }
        }

        private void UMLTogle_Click(object sender, RoutedEventArgs e)
        {
            var visibility = ULMelem.Visibility;

            switch (visibility)
            {
                case Visibility.Visible: ULMelem.Visibility = Visibility.Collapsed; break;
                case Visibility.Collapsed: ULMelem.Visibility = Visibility.Visible; break;
            }
        }

        private void TextDisplayTextBox_OnlinePeers_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SaveUMLButton_Click(object sender, RoutedEventArgs e)
        {
            SerializeToXML(this.dropPanel);
        }

        private void SerializeToXML(Canvas canvas)
        {
            //source file 
            string fileName = System.IO.Path.GetTempFileName();
            string mystrXAML = XamlWriter.Save(canvas);
            FileStream filestream = File.Create(fileName);
            StreamWriter streamwriter = new StreamWriter(filestream);
            streamwriter.Write(mystrXAML);
            streamwriter.Close();
            filestream.Close();

            //dest file
            string destFileName = System.IO.Path.GetRandomFileName() + ".xml";
            string destPath = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            destPath = System.IO.Path.Combine(destPath, destFileName);
            this.path = destPath;

            if (!System.IO.File.Exists(destPath))
            {
                File.Move(fileName, destPath);
                MessageBox.Show("destPath:" + destPath);
            }
            else
            {
                MessageBox.Show("Failed to Save UML, Please Retry");
                return;
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer.deserialize?view=netframework-4.7.2

        private void DeserializeXML(string filename) {
            XmlSerializer serializer = new XmlSerializer(typeof(Canvas));

            Canvas deseriallizedCanvas;

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                MessageBox.Show("reader got called");
                deseriallizedCanvas = (Canvas)serializer.Deserialize(reader);
            }
            MessageBox.Show("canvas Background: " + deseriallizedCanvas.Background);
            this.dropPanel = deseriallizedCanvas;
        }

        private void LoadUMLButton_Click(object sender, RoutedEventArgs e)
        {
            DeserializeXML(this.path);
        }
    }   
}
