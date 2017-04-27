using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public string UserName;
        public List<Room> roomList;

        public MainWindow(string userName)
        {
            InitializeComponent();
            UserName = userName;
            roomList = new List<Room>();
            InitializeServerConnection();
            AddRoom("Default", 0);
            AddRoom("Test", 1);
        }

        #region Private Members

        // Client socket
        private Socket clientSocket;

        // Server End Point
        private EndPoint epServer;

        // Data stream
        private byte[] dataStream = new byte[1024];

        // Display Message Delegate
        private delegate void DisplayMessageDelegate(string message, int roomId);
        private DisplayMessageDelegate displayMessageDelegate = null;

        #endregion

        #region Networking

        private void InitializeServerConnection()
        {
            try
            {
                // Initialize Delegate
                this.displayMessageDelegate = new DisplayMessageDelegate(this.AppendLineToChatBox);

                // Initialise socket
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Initialize server IP
                // TODO: Change To Server IP
                IPAddress serverIP = IPAddress.Parse(GetLocalIP());
                /*
                // TODO: Change To recieve input from upcoming text box
                // WORKAROUND: VPN to school network (or be on mst computer)
                //             and visit icanhazip.com in a browser and copy IP address here before building
                // IPAddress serverIP = IPAddress.Parse("131.151.89.23");
                */

                // Initialize the IPEndPoint for the server and use port 30000
                IPEndPoint server = new IPEndPoint(serverIP, 30000);

                // Initialize the EndPoint for the server
                epServer = (EndPoint)server;

                // Initialize Login Message
                Message login = new Message();
                login.Who = UserName;
                login.Why = 200;

                string jsonMessage = JsonConvert.SerializeObject(login);

                // Encode Into Byte Array
                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                // Send The Message
                clientSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                // Initialize data stream
                this.dataStream = new byte[1024];

                // Begin listening for broadcasts
                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void SendData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                // Check If Data Exists
                int size = this.clientSocket.EndReceiveFrom(ar, ref epServer);               

                if (size > 0)
                {
                    // Auxiliary Buffer
                    byte[] aux = new byte[1500];

                    // Retrieve Data
                    aux = (byte[])dataStream;

                    // Decode Byte Array
                    string jsonStr = Encoding.ASCII.GetString(aux);

                    // Deserialize JSON
                    Message message = JsonConvert.DeserializeObject<Message>(jsonStr);

                    // TODO: Handle Messange
                    // Update Message Box Through Delegate
                    if (!string.IsNullOrEmpty(message.What))
                        this.Dispatcher.Invoke(this.displayMessageDelegate, new object[] { "[" + message.When + "] " + message.Who + " : " + message.What, message.Where });

                    // Reset data stream
                    this.dataStream = new byte[1500];

                    // Continue listening for broadcasts
                    clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Return Your Own IP Address
        /// </summary>
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        // Returns The Currently Selected Room
        private Room GetCurrentRoom()
        {
            var currentTabItem = tabControl.SelectedItem as TabItem;
            string header = currentTabItem.Header.ToString();

            var currentRoom = roomList.Find(x => x.Header == header);
            return currentRoom;
        }

        #endregion

        #region UI Methods

        /// <summary>
        /// Append the provided message to the chatBox text box.
        /// </summary>
        /// <param name="message"></param>
        private void AppendLineToChatBox(string message, int roomId)
        {
            // To ensure we can successfully append to the text box from any thread
            // we need to wrap the append within an invoke action.

            try
            {
                // Append Message To TextBox
                var room = roomList.Find(x => x.Id == roomId);
                var tabItem = room.Tab;
                TextBox chatBox = (TextBox)tabItem.Content;

                chatBox.AppendText(message + "\n");
                chatBox.ScrollToEnd();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Add rooms by name
        /// </summary>
        /// <param name="header"></param>
        private void AddRoom(string header, int roomId)
        {
            Room room = new Room();
            room.Tab = new TabItem();
            room.Tab.Header = header;
            room.ChatBox = new TextBox();
            room.Tab.Content = room.ChatBox;

            room.Id = roomId;
            room.Header = header;

            roomList.Add(room); 

            tabControl.Items.Add(room.Tab);
            tabControl.SelectedItem = room.Tab;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            ScrollViewer scroller = (ScrollViewer)tabControl.Template.FindName("TabControlScroller", tabControl);
            if (scroller != null)
            {
                double index = (double)(tabControl.SelectedIndex);
                double offset = index * (scroller.ScrollableWidth / (double)(tabControl.Items.Count));
                scroller.ScrollToHorizontalOffset(offset);

            }
        }

        /// <summary>
        /// Send any entered message when we click the send button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage();
        }

        /// <summary>
        /// Send any entered message when we press enter or the return key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                SendChatMessage();
        }

        /// <summary>
        /// Correctly shutdown network communication when closing the WPF application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // TODO: Update when deciding upon final networking solution
            try
            {
                if(this.clientSocket != null)
                {
                    // InitializeComponent a new message for logoff
                    Message sendData = new Message
                    {
                        Who = UserName,
                        What = "",
                        When = DateTime.Now.ToShortTimeString(),
                        Where = 0, // Default Chat Room
                        Why = Protocol.Protocol.USEREXIT
                    };

                    string jsonMessage = JsonConvert.SerializeObject(sendData);

                    // Encode Into Byte Array
                    var enc = new ASCIIEncoding();
                    byte[] msg = new byte[1500];
                    msg = enc.GetBytes(jsonMessage);

                    // Send message to server
                    this.clientSocket.SendTo(msg, 0, msg.Length, SocketFlags.None, epServer);

                    // Close the socket
                    this.clientSocket.Close();

                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Application.Current.Shutdown();
        }

        #endregion

        #region Messaging

        /// <summary>
        /// Send our message.
        /// </summary>
        private void SendChatMessage()
        {
            if (!string.IsNullOrEmpty(messageText.Text))
            {
                try
                {
                    // Create POCO Message
                    Message message = new Message
                    {
                        Who = UserName,
                        What = messageText.Text,
                        When = DateTime.Now.ToShortTimeString(),
                        Where = GetCurrentRoom().Id, // Default Chat Room
                        Why = Protocol.Protocol.PUBLIC_MESSAGE
                    };

                    // Serialize JSON Object
                    string jsonMessage = JsonConvert.SerializeObject(message);

                    // Encode Into Byte Array
                    var enc = new ASCIIEncoding();
                    byte[] msg = new byte[1500];
                    msg = enc.GetBytes(jsonMessage);

                    // Send The Message
                    clientSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                    // TODO: Wait For Server Callback To Display Message
                    //AppendLineToChatBox("[" + message.When + "] " + message.Who + " : " + messageText.Text);
                    messageText.Clear();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }           
        }

        #endregion
    }
}
