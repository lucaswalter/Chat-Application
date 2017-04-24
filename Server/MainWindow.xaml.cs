using System;
using System.Text;
using System.Windows;

using System.Net.Sockets;
using System.Net;
using System.Collections;

using Protocol;
using Newtonsoft.Json;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        // Structure to store the client information
        private struct Client
        {
            public EndPoint endPoint;
            public string name;
        }

        // Listing of clients
        private ArrayList clientList;

        // Server socket
        private Socket serverSocket;

        // Data stream
        private byte[] dataStream = new byte[1500];

        // Status delegate
        private delegate void UpdateStatusDelegate(string status);
        private UpdateStatusDelegate updateStatusDelegate = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializeServerConnection();
        }

        #endregion

        #region Events

        private void InitializeServerConnection()
        {
            try
            {
                // Initialise the ArrayList of connected clients
                this.clientList = new ArrayList();

                // Initialise the delegate which updates the status
                this.updateStatusDelegate = new UpdateStatusDelegate(this.UpdateStatus);

                // Initialise the socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Initialise the IPEndPoint for the server and listen on port 30000
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);

                // Associate the socket with this IP address and port
                serverSocket.Bind(server);

                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;

                // Start listening for incoming data
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                lblStatus.Content = "Listening";
            }
            catch (Exception ex)
            {
                lblStatus.Content = "Error";
                MessageBox.Show("Load Error: " + ex.Message, "UDP Server");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // InitializeComponent a new message for logoff
                Message sendData = new Message
                {
                    Who = "SERVER",
                    What = "--- !!! SERVER IS SHUTTING DOWN !!! ---",
                    When = DateTime.Now.ToShortTimeString(),
                    Where = "0", // Default Chat Room
                    Why = Protocol.Protocol.GLOBAL_WARNING_MESSAGE
                };

                string jsonMessage = JsonConvert.SerializeObject(sendData);

                // Encode Into Byte Array
                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                // Send message to server
                foreach (Client client in this.clientList)
                {
                    this.serverSocket.SendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint);
                }

                serverSocket.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region Send And Receive

        public void SendData(IAsyncResult asyncResult)
        {
            try
            {
                serverSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendData Error: " + ex.Message, "UDP Server");
            }
        }

        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;


                // Check if Data Exists
                int size = this.serverSocket.EndReceiveFrom(asyncResult, ref epSender);

                if (size > 0)
                {

                    // Auxiliary Buffer
                    byte[] aux = new byte[1500];

                    // Retrieve Data
                    aux = (byte[])dataStream;

                    // Decode Byte Array
                    string jsonStr = Encoding.ASCII.GetString(aux);
                    jsonStr = jsonStr.Remove(jsonStr.LastIndexOf("}") + 1);

                    // Initialise a message object to store the received data
                    Message message = JsonConvert.DeserializeObject<Message>(jsonStr);

                    Message sending = new Message();


                    this.dataStream = new byte[1500];

                    // Initialise a packet object to store the data to be sent
                    // Message message;

                    // Receive all data
                    // serverSocket.EndReceiveFrom(asyncResult, ref epSender);

                    // Start populating the packet to be sent
                    //sendData.ChatDataIdentifier = receivedData.ChatDataIdentifier;
                    // sendData.ChatName = receivedData.ChatName;


                    switch (message.Why)
                    {
                        case Protocol.Protocol.PUBLIC_MESSAGE:
                            sending = message;

                            break;

                        case Protocol.Protocol.CREATE_ACCOUNT:
                            // Populate client object
                            Client client = new Client();
                            client.endPoint = epSender;
                            client.name = message.Who;

                            // Add client to list
                            this.clientList.Add(client);

                            sending.What = string.Format("-- {0} is online --", message.Who);
                            sending.Why = 100;
                            break;

                        case Protocol.Protocol.LEAVE_ROOM:
                            // Remove current client from list
                            foreach (Client c in this.clientList)
                            {
                                if (c.endPoint.Equals(epSender))
                                {
                                    this.clientList.Remove(c);
                                    break;
                                }
                            }
                            sending.What = string.Format("-- {0} has gone offline --", message.Who);
                            sending.Why = 100;
                            break;

                        default:
                            sending = message;
                            sending.What = "--UNKNOWN 'WHY'--";
                            break;
                    }
                    // Get packet as byte array


                    string sendstring = "";
                    if (sending.When != null)
                    {
                        sendstring += "[ " + sending.When + "] ";
                    }
                    if (sending.Who != null)
                    {
                        sendstring += sending.Who + " : ";
                    }
                    if (sending.What != null)
                    {
                        sendstring += sending.What;
                    }

                    // Serialize JSON Object
                    string jsonMessage = JsonConvert.SerializeObject(sending);

                    // Encode Into Byte Array
                    var enc = new ASCIIEncoding();
                    byte[] msg = new byte[1500];
                    msg = enc.GetBytes(jsonMessage);

                    foreach (Client client in this.clientList)
                    {
                        if (client.endPoint != epSender)// || sendData.ChatDataIdentifier != DataIdentifier.LogIn)
                        {
                            // Broadcast to all logged on users
                            serverSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                        }
                    }

                    // Listen for more connections again...

                    serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);

                    // Update status through a delegate
                    this.Dispatcher.Invoke(this.updateStatusDelegate, new object[] { sendstring });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReceiveData Error: " + ex.Message + "\n++++++++++\n" + ex.StackTrace, "UDP Server");
            }
        }

        #endregion

        #region Other Methods

        private void UpdateStatus(string status)
        {
            txtStatus.Text += status + Environment.NewLine;
            txtStatus.ScrollToEnd();
        }

        #endregion
    }
}
