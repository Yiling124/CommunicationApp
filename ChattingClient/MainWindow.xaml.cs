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
using System.Diagnostics;
using System.Reflection;

namespace ChattingClient
{
    public partial class MainWindow : Window
    {
        public static IChattingService Server;
        private static DuplexChannelFactory<IChattingService> _channelFactory;
        Thread msgOutThrd = null;
        Thread msgInThrd = null;
        public String userName = null;
        public String sessionIp = null;
        int sessionPort = 0;
        BlockingQueue<IDeliverable> msgOutBlockingQ;
        BlockingQueue<IDisplayable> msgInBlockingQ;
        public CanvasContainer cvContainer = new CanvasContainer();

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

            msgInBlockingQ = new BlockingQueue<IDisplayable>();
            msgInThrd = new Thread(MsgInThreadProc);
            msgInThrd.IsBackground = true;
            msgInThrd.Start();
        }

        public void MsgInThreadProc() {
            Action act = () => { };
            while (true)
            {
                IDisplayable newmsg = msgInBlockingQ.deQ();
                act = () => {
                    if (newmsg.getType() == MsgType.Text)
                    {
                        DisplayableTextMessage msg = (DisplayableTextMessage)newmsg;
                        msg.Display();
                    }
                    else {
                        DisplayableUMLmsg msg = (DisplayableUMLmsg)newmsg;
                        msg.Display();
                    }
                };
                string[] args = new string[] { };
                Dispatcher.Invoke(act, args);
            }
        }

        // this child thread dequeue message and post message to server 
        public void MsgOutThreadProc()
        {
            while (true)
            {
                IDeliverable msgForSendOut = msgOutBlockingQ.deQ();
                bool isSent = msgForSendOut.SendOut(Server);
                if (isSent) continue;
            }
        }

        private void SetLabels(String _userName, String _sessionIp, int _sessionPort, String peerList)
        {
            this.userName = _userName;
            this.sessionIp = _sessionIp;
            this.sessionPort = _sessionPort;
            WelcomeLabel.Content = "Welcome " + this.userName + "!";
            SessionIpLabel.Content = "Session IP: " + this.sessionIp;
            SessionPortLabel.Content = "Session Port: " + this.sessionPort;
            DisplayOnlinePeerList(peerList);
        }

        public void DisplayUMLmsg(String msgContent) {
            List<UMLShape> deserializedUML =  (List<UMLShape>)new XmlSerializer(typeof(List<UMLShape>)).Deserialize(new StringReader(msgContent));
            DisplayUMLFromList(deserializedUML);
            this.updateCanvasItems();
        }

        // Incoming message will be displayed on the UI 
        public void DisplayTextMsg(DisplayableTextMessage newMsg) {
            if (newMsg.isPrivate)
            {
                TextDisplayTextBox.Text += "(Privte Msg) " + newMsg.userName + ":" + newMsg.content + "\n";
            }
            else
            {
                TextDisplayTextBox.Text += newMsg.userName + ":" + newMsg.content + "\n";
            }

            receiverIpTextBox.Text = "";
            receiverPortTextBox.Text = "";
            MessageTextBox.Text = "";
            //TextDisplayTextBox.ScrollToEnd();
        }

        public void TakeMessage(MsgType msgType, string msg, string userName, bool isPrivate)
        {
            IDisplayable amsg;
            if (msgType == MsgType.Text)
            {
                amsg = new DisplayableTextMessage(msg, isPrivate, userName);
            }
            else {
                amsg = new DisplayableUMLmsg(msg, isPrivate, userName);
            }
            msgInBlockingQ.enQ(amsg);
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (MessageTextBox.Text.Length == 0) return;

            Tuple<string, int> privatReceipientAdrs = buildIpAdrs(receiverIpTextBox.Text, receiverPortTextBox.Text);
            if ((receiverIpTextBox.Text.Length > 0 || receiverPortTextBox.Text.Length > 0) && privatReceipientAdrs == null) {
                MessageBox.Show("Please double check recepient address !");
                return;
            }

            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(this.sessionIp, sessionPort.ToString());
            IDeliverable msgOut = new DeliverableTextMessage(MsgType.Text, MessageTextBox.Text, this.userName, privatReceipientAdrs, ssOwnerAdrs);
            msgOutBlockingQ.enQ(msgOut);

            bool isPrivate = privatReceipientAdrs == null ? false : true;
            TakeMessage(MsgType.Text, MessageTextBox.Text, "you", isPrivate);
        }

        private string SerializeUMLmsg(List<UMLShape> shapeList)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(shapeList.GetType());
            StringWriter stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, shapeList);
            return stringWriter.ToString();

        }


        private void SendUML()
        {
            Tuple<string, int> ssOwnerAdrs = buildIpAdrs(this.sessionIp, sessionPort.ToString());
            string umlMsg = SerializeUMLmsg(this.cvContainer.ShapeList);
            IDeliverable msgOut = new DeliverableTextMessage(MsgType.UML, umlMsg, this.userName, null, ssOwnerAdrs);
            msgOutBlockingQ.enQ(msgOut);
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
            string messageBoxText = String.Format("{0} wants to joint, approve?", userName);
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
            if (e.Handled == false)
            {
                Panel _panel = (Panel)sender;
                UIElement _element = (UIElement)e.Data.GetData("Object");
                Point dropPoint = e.GetPosition(this.dropPanel);
                if (_panel != null && _element != null)
                {

                    Panel _parent = (Panel)VisualTreeHelper.GetParent(_element);
                    ShapeType shapetype;
                    if (_element is Rectangle)
                    {
                        shapetype = ShapeType.Rectangle;
                    }
                    else {
                        shapetype = ShapeType.UsingConnector;
                    }
                   
                    UMLShape newShape = new UMLShape(shapetype, dropPoint.Y, dropPoint.X);
                    if (_parent != null)
                    {
                        if (e.KeyStates == DragDropKeyStates.ControlKey &&
                            e.AllowedEffects.HasFlag(DragDropEffects.Copy))
                        {
                            if (_element is Rectangle)
                            {
                                Rectangle _rectangle = new Rectangle((Rectangle)_element);
                                _rectangle.left = dropPoint.X;
                                _rectangle.top = dropPoint.Y;
                                _panel.Children.Add(_rectangle);
                                Canvas.SetLeft(_rectangle, dropPoint.X);
                                Canvas.SetTop(_rectangle, dropPoint.Y);
                                cvContainer.AddShape(newShape);
                            }
                            else
                            {
                                UsingConnector _uc = new UsingConnector((UsingConnector)_element);
                                _uc.left = dropPoint.X;
                                _uc.top = dropPoint.Y;
                                _panel.Children.Add(_uc);
                                Canvas.SetLeft(_uc, dropPoint.X);
                                Canvas.SetTop(_uc, dropPoint.Y);
                                cvContainer.AddShape(newShape);
                            }
                            this.updateCanvasItems();
                            e.Effects = DragDropEffects.Copy;


                        }
                        else if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                        {
                            if (_element is Rectangle)
                            {
                                _parent.Children.Remove(_element);
                                Rectangle _rectangle = new Rectangle((Rectangle)_element);
                                _rectangle.left = dropPoint.X;
                                _rectangle.top = dropPoint.Y;
                                _panel.Children.Add(_rectangle);
                                Canvas.SetLeft(_rectangle, dropPoint.X);
                                Canvas.SetTop(_rectangle, dropPoint.Y);
                                this.updateCanvasItems();
                            }
                            else
                            {
                                _parent.Children.Remove(_element);
                                UsingConnector _updatedUc = new UsingConnector((UsingConnector)_element);
                                _updatedUc.left = dropPoint.X;
                                _updatedUc.top = dropPoint.Y;
                                _panel.Children.Add(_updatedUc);
                                Canvas.SetLeft(_updatedUc, dropPoint.X);
                                Canvas.SetTop(_updatedUc, dropPoint.Y);
                                this.updateCanvasItems();
                            }
                            e.Effects = DragDropEffects.Move;
                        }
                    }
                }
            }
            SendUML();
        }

        private void updateCanvasItems() {
            this.cvContainer.ShapeList.Clear();
            foreach (UIElement elem in this.dropPanel.Children) {
                ShapeType shapetype;
                double top = 0;
                double left = 0;
                if (elem is Rectangle)
                {
                    shapetype = ShapeType.Rectangle;
                } else {
                    shapetype = ShapeType.UsingConnector;
                }

                Type t = elem.GetType();
                PropertyInfo[] pi = t.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    if (p.Name.Equals("top")) top = (double)p.GetValue(elem);
                    if (p.Name.Equals("left")) left = (double)p.GetValue(elem);
                }
                UMLShape newShape = new UMLShape(shapetype, top, left);
                this.cvContainer.ShapeList.Add(newShape);
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

        private void SaveUMLButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.saveUMLTextBox.Text.Length == 0)
            {
                MessageBox.Show("please enter a file you want to load");
                return;
            }
            SerializeToXML(this.cvContainer.ShapeList, this.saveUMLTextBox.Text);
        }

        private void SerializeToXML(List<UMLShape> shapeList, string fileNm)
        {
            //source file 
            string finalFileName = fileNm + ".xml";
            string tempFileName = System.IO.Path.GetTempFileName() + ".xml";
            XmlSerializer serializer = new XmlSerializer(typeof(List<UMLShape>));
            TextWriter writer = new StreamWriter(tempFileName);
            serializer.Serialize(writer, shapeList);
            writer.Close();
            
            //dest file
            string destPath = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            destPath = System.IO.Path.Combine(destPath, finalFileName);
           
            if (!System.IO.File.Exists(destPath))
            {
                File.Move(tempFileName, destPath);
            }
            else
            {
                MessageBox.Show("Failed to Save UML, Please Retry");
                return;
            }
            MessageBox.Show("Your UML was successfully saved to your Desktop!");
            this.saveUMLTextBox.Text = "";
        }

        private void DeserializeXML(string filename) {

            string path = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            string fileName = filename + ".xml";
            string file = System.IO.Path.Combine(path, fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(List<UMLShape>));

            using (Stream reader = new FileStream(file, FileMode.Open))
            {
                List<UMLShape> shapeList = (List<UMLShape>)serializer.Deserialize(reader);
                DisplayUMLFromList(shapeList);
            }
            this.updateCanvasItems();
            this.saveUMLTextBox.Text = "";
        }

        private void DisplayUMLFromList(List<UMLShape> list)
        {
            this.dropPanel.Children.Clear();
            foreach (UMLShape ushape in list)
            {
                if (ushape.ShpType == ShapeType.UsingConnector)
                {
                    UsingConnector uc = new UsingConnector();
                    uc.left = ushape.Left;
                    uc.top = ushape.Top;
                    this.dropPanel.Children.Add(uc);
                    Canvas.SetLeft(uc, ushape.Left);
                    Canvas.SetTop(uc, ushape.Top);
                }
                if (ushape.ShpType == ShapeType.Rectangle)
                {
                    Rectangle rect = new Rectangle();
                    rect.left = ushape.Left;
                    rect.top = ushape.Top;
                    this.dropPanel.Children.Add(rect);
                    Canvas.SetLeft(rect, ushape.Left);
                    Canvas.SetTop(rect, ushape.Top);
                }
            }
        }

        private void LoadUMLButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.saveUMLTextBox.Text.Length == 0)
            {
                MessageBox.Show("please enter a file you want to load");
                return;
            }
            DeserializeXML(this.saveUMLTextBox.Text);
            SendUML();
        }

        private void ClearUMLButton_Click(object sender, RoutedEventArgs e)
        {
            this.dropPanel.Children.Clear();
            cvContainer.ShapeList.Clear();
            SendUML();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Rectangle rect = new Rectangle();
            this.dropPanel.Children.Add(rect);
            Canvas.SetTop(rect, 500);
            Canvas.SetLeft(rect, 150);
        }
    }   
}
