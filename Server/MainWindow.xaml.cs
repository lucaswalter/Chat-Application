using System;
using System.Text;
using System.Windows;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using Protocol;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Input;

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
            public string password;
        }
        
        private class Room
        {
            public string header;
            public int id;
            public Client owner;
            private DateTime openTime;      // Time that room was opened
            public bool isPrivate;         // Boolean for room access type
            public List<Client> clients;
            public TabItem tab;
            public TextBox txtbox;

            public Room()
            {
                header = "New Room";
                id = 0;
                owner.endPoint = null;
                owner.name = "Server";
                owner.password = "";
                openTime = DateTime.Now;
                isPrivate = false;
                clients = new List<Client>();
                tab = new TabItem();
                txtbox = new TextBox();
            }
        }



        // Listing of clients
        private ArrayList clientList;
        
        // Listing of rooms
        private List<Room> roomList;

        // Server socket
        private Socket serverSocket;

        // Data stream
        private byte[] dataStream = new byte[1500];

        // Rooms delegate
        private delegate void UpdateRoomsDelegate(Message message);
        private UpdateRoomsDelegate updateRoomsDelegate = null;

        // Server Status delegate
        private delegate void UpdateStatusDelegate(string status);
        private UpdateStatusDelegate updateStatusDelegate = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializeServerConnection();
            // Add Default Room
            roomList = new List<Room>();
            AddRoom("Welcome!");
            AddRoom("CS 3100");
            AddRoom("ProgLang");
            AddRoom("#Team4");
        }
        private void InitializeServerConnection()
        {
            try
            {
                // Initialise the ArrayList of connected clients
                this.clientList = new ArrayList();

                // Initialise the delegate which updates the Rooms
                this.updateRoomsDelegate = new UpdateRoomsDelegate(this.UpdateRooms);

                // Initialise the delegate which updates the Server Status
                this.updateStatusDelegate = new UpdateStatusDelegate(this.UpdateStatus);

                // Initialise the socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Initialise the IPEndPoint for the server and listen on port 30000
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);

                // Associate the socket with this IP address and port
                serverSocket.Bind(server);

                // Initialise the IPEndPoint for the clients
                IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clientEP;

                // Start listening for incoming data
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                ServerComTextBox.Text = "Listening for Clients..." + "\n";
            }
            catch (Exception ex)
            {
                ServerComTextBox.Text = "Error" + "\n";
                MessageBox.Show("Load Error: " + ex.Message, "Time4aChat Server Command Center Error");
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
                    Where = -1, // All Rooms
                    Why = Protocol.Protocol.PUBLIC_MESSAGE
                };

                string jsonMessage = JsonConvert.SerializeObject(sendData);

                // Encode Into Byte Array
                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                // Send message to server
                foreach (Client client in clientList)
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
                MessageBox.Show("SendData Error: " + ex.Message, "Time4aChat Server Command Center Error");
            }
        }

        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                // Initialise the IPEndPoint for the clients
                IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clientEP;

                bool singleUser = false;
                bool multiMessage = false;


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

                    // Update Server status to show recieved json
                    this.Dispatcher.Invoke(this.updateStatusDelegate, new object[] { "<-- " + jsonStr });

                    // Initialise a message object to store the received data
                    Message message = JsonConvert.DeserializeObject<Message>(jsonStr);

                    // Initialize new message to be sent
                    Message sending = new Message();

                    // Initialize new message to be sent to only one user
                    Message sendOne = new Message();

                    this.dataStream = new byte[1500];

                    # region Protocol Handling
                    switch (message.Why)
                    {
                        // -----------------------------------------------------------
                        // Basic Messaging
                        // {"user", "messageText", "timestamp", "roomId", "protocol"}
                        // -----------------------------------------------------------
                        // Standard Message
                        case Protocol.Protocol.PUBLIC_MESSAGE:
                            sending = message;
                            break;

                        case Protocol.Protocol.PRIVATE_MESSAGE:

                            break;

                        // Global Messaging
                        // {"user", "messageText", "timestamp", "roomId", "protocol"}
                        case Protocol.Protocol.GLOBAL_INFO_MESSAGE:

                            break;

                        case Protocol.Protocol.GLOBAL_WARNING_MESSAGE:

                            break;

                        // User Actions
                        case Protocol.Protocol.CREATE_ACCOUNT:
                            // Populate client object
                            Client client = new Client()
                            {
                                endPoint = epSender,
                                name = message.Who
                            };

                            // Add client to list
                            this.clientList.Add(client);
                            foreach(Room r in roomList)
                            {
                                if(!r.isPrivate)
                                {
                                    r.clients.Add(client);
                                }
                            }
                            //roomList[0].clients.Add(client);
                            sendOne = SendRooms(message);
                            multiMessage = true;

                            sending.Where = -1;
                            sending.What = string.Format("-- {0} is online --", message.Who);
                            sending.Why = Protocol.Protocol.PUBLIC_MESSAGE;
                            break;

                        case Protocol.Protocol.ADD_FRIEND:

                            break;

                        case Protocol.Protocol.REMOVE_FRIEND:

                            break;

                        case Protocol.Protocol.BLOCK_USER:

                            break;

                        case Protocol.Protocol.UNBLOCK_USER:

                            break;

                        case Protocol.Protocol.LOGIN:

                            break;

                        case Protocol.Protocol.LOGOUT:

                            break;

                        case Protocol.Protocol.USEREXIT:
                            // Remove current client from list
                            foreach (Client c in this.clientList)
                            {
                                if (c.endPoint.Equals(epSender))
                                {
                                    this.clientList.Remove(c);
                                    bool clientInRoom = false;
                                    foreach(Room r in roomList)
                                    {
                                        clientInRoom = r.clients.Contains(c);
                                        if(clientInRoom)
                                        {
                                            r.clients.Remove(c);
                                        }
                                    }
                                    break;
                                }
                            }
                            sending.What = string.Format("-- {0} has gone offline --", message.Who);
                            sending.Where = -1;
                            sending.Why = Protocol.Protocol.PUBLIC_MESSAGE;
                            break;

                        case Protocol.Protocol.RETRIEVE_FRIENDS:

                            break;

                        case Protocol.Protocol.RETRIEVE_BLOCKED_USERS:

                            break;

                        // Room Actions
                        // {"ownerUser", "null", "timestamp", "null", "protocol"}
                        case Protocol.Protocol.CREATE_PUBLIC_ROOM:

                            break;

                        case Protocol.Protocol.CREATE_PRIVATE_ROOM:

                            break;

                        // {"null", "null", "timestamp", "null", "protocol"}
                        case Protocol.Protocol.SEND_PUBLIC_ROOMS:
                            // Sends a string of all current public rooms to the user
                            sendOne = SendRooms(message);
                            singleUser = true;

                            break;

                        case Protocol.Protocol.INVITE_USER_TO_ROOM:

                            break;

                        case Protocol.Protocol.KICK_USER_FROM_ROOM:

                            break;

                        case Protocol.Protocol.PROMOTE_USER:

                            break;

                        case Protocol.Protocol.DEMOTE_USER:

                            break;

                        // {"user", "null", "timestamp", "roomId", "protocol"}
                        case Protocol.Protocol.ENTER_ROOM:

                            break;

                        case Protocol.Protocol.LEAVE_ROOM:

                            break;

                        case Protocol.Protocol.CLOSE_ROOM:


                            break;

                        default:
                            sending = message;
                            sending.What = "--UNKNOWN 'WHY'--";
                            break;
                    }
                    # endregion

                    // Serialize JSON Object
                    string jsonMessage = JsonConvert.SerializeObject(sending);

                    // Encode Into Byte Array
                    var enc = new ASCIIEncoding();
                    byte[] msg = new byte[1500];
                    msg = enc.GetBytes(jsonMessage);

                    // Will be used if a message is to be sent to a sinle user
                    byte[] msgOne = new byte[1500];

                    // Build message to send to single Client and Send
                    if (singleUser || multiMessage)
                    {
                        string jsonMessageOne = JsonConvert.SerializeObject(sendOne);

                        // Encode Into Byte Array
                        var encOne = new ASCIIEncoding();
                        msgOne = encOne.GetBytes(jsonMessageOne);
                        foreach (Client client in clientList)
                        {
                            if (client.name == message.Who)
                            {
                                serverSocket.BeginSendTo(msgOne, 0, msgOne.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                                this.Dispatcher.Invoke(this.updateStatusDelegate, new object[] { "--> " + jsonMessageOne });
                            }
                        }
                    }

                    if (sending.Where == -1)
                    {
                        foreach (Client client in clientList)
                        {
                            serverSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                        }
                    }

                    // Begin Sending to all clients in the Room
                    if (!singleUser && sending.Where != -1)
                    {
                        foreach (Room r in roomList)
                        {
                            if (sending.Where == r.id)
                            {
                                foreach (Client client in r.clients)  // Send if the client is in the list of clients able to view the room being sent to
                                {
                                    if (client.endPoint != epSender)
                                    {
                                        // Broadcast to all logged on users
                                        serverSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                                    }
                                }
                            }
                        }
                    }
                    // Listen for more connections again...
                    serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);

                    // Update Rooms through a delegate
                    this.Dispatcher.Invoke(this.updateRoomsDelegate, new object[] { sending });

                    // Update Server Status to show sent json
                    this.Dispatcher.Invoke(this.updateStatusDelegate, new object[] { "--> " + jsonMessage });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReceiveData Error: " + ex.Message + "\n__________________________________\n" + ex.StackTrace, "Time4aChat Server Command Center Error");
            }
        }

        #endregion

        #region Other Methods

        private void UpdateRooms(Message message)
        {
            string formattedText = "";
            if (message.When != null)
            {
                formattedText += "[ " + message.When + "] ";
            }
            if (message.Who != null)
            {
                formattedText += message.Who + " : ";
            }
            if (message.What != null)
            {
                formattedText += message.What;
            }

            foreach (Room r in roomList)
            {
                if ( message.Where == r.id || message.Where == -1)
                {
                    r.txtbox.Text += formattedText + Environment.NewLine;
                    r.txtbox.ScrollToEnd();
                }
            }
        }

        private void UpdateStatus(string json)
        {
            ServerComTextBox.Text += json + Environment.NewLine;
            //ServerComTextBox.ScrollToEnd();

            UserOnlineTextBox.Clear();
            foreach (Client client in this.clientList)
            {
                UserOnlineTextBox.Text += client.name;
                UserOnlineTextBox.Text += "\n";
            }
        }

        #endregion
        private void Command_Button(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrEmpty(ServerCommInput.Text))
            {
                Message message = new Message
                {
                    Who = "SERVER",
                    What = ServerCommInput.Text,
                    When = DateTime.Now.ToShortTimeString(),
                    Where = roomList[tabCtrl.SelectedIndex].id,
                    Why = Protocol.Protocol.PUBLIC_MESSAGE
                };

                string jsonMessage = JsonConvert.SerializeObject(message);

                // Encode Into Byte Array
                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                // Send message to server

                foreach (Client client in roomList[tabCtrl.SelectedIndex].clients)
                {
                    this.serverSocket.SendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint);
                }
                //ServerComTextBox.Text += ServerCommInput.Text + "\n";
                this.Dispatcher.Invoke(this.updateRoomsDelegate, new object[] { message });
                ServerCommInput.Clear();
            }
             
        }

        private void btnNewRoom_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrEmpty(ServerCommInput.Text))
            {
              AddRoom(ServerCommInput.Text);
              ServerCommInput.Clear();
            }
            else
            {
                MessageBox.Show("Please Type a Room Name in Server Control and try again.");
            }
        }
        
        private void AddRoom(string header)
        {
            Room room = new Room();

            room.tab = new TabItem();
            room.tab.Header = header;
            room.txtbox = new TextBox();
            room.tab.Content = room.txtbox;

            room.id = 0;
            bool containsItem = roomList.Any(x => room.id == x.id);
            while(containsItem)
            {
                room.id++;
                containsItem = roomList.Any(x => room.id == x.id);
            }

            room.header = header;
            foreach (Client client in clientList)
            {
                room.clients.Add(client);
            }
            roomList.Add(room);

            SendRoomChange(room, Protocol.Protocol.CREATE_PUBLIC_ROOM);
            
            tabCtrl.Items.Add(room.tab);
            tabCtrl.SelectedItem = room.tab;
        }

        private Message SendRooms(Message recievedMessage)
        {
            Message sendingRooms = new Message();
            string roomids = "";
            string roomheaders = "";

            foreach (Room room in roomList)
            {
                if (!room.isPrivate)
                {
                    roomids += room.id.ToString() + ",";
                    roomheaders += room.header + ",";
                }
            }
            roomids = roomids.TrimEnd(',');
            roomheaders = roomheaders.TrimEnd(',');
           
            sendingRooms.Who = roomids;
            sendingRooms.What = roomheaders;
            sendingRooms.When = recievedMessage.When;
            sendingRooms.Where = -1;
            sendingRooms.Why = Protocol.Protocol.SEND_PUBLIC_ROOMS;

            return sendingRooms;
        }

        private void SendRoomChange(Room room, int protocol)
        {
            Message message = new Message
            {
                Who = null,
                What = room.header,
                When = DateTime.Now.ToShortTimeString(),
                Where = room.id,
                Why = protocol
            };

            string jsonMessage = JsonConvert.SerializeObject(message);

            // Encode Into Byte Array
            var enc = new ASCIIEncoding();
            byte[] msg = new byte[1500];
            msg = enc.GetBytes(jsonMessage);

            // Send message to server
            foreach (Client client in clientList)
            {
                this.serverSocket.SendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint);
            }
        }

        private void tabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (tabCtrl.SelectedIndex > 0)
            {
                Room room = roomList[tabCtrl.SelectedIndex];
                SendRoomChange(room, Protocol.Protocol.CLOSE_ROOM);

                tabCtrl.Items.Remove(roomList[tabCtrl.SelectedIndex].tab);
                roomList.RemoveAt(tabCtrl.SelectedIndex);

            }
            else
            {
                MessageBox.Show("Cannot Delete the initial Room");
            }
        }

        private void ServerCommInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                Command_Button(sender, e);
        }

        private void Blast_Button(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ServerCommInput.Text))
            {
                Message message = new Message
                {
                    Who = "SERVER",
                    What = ServerCommInput.Text,
                    When = DateTime.Now.ToShortTimeString(),
                    Where = -1,
                    Why = Protocol.Protocol.PUBLIC_MESSAGE
                };

                string jsonMessage = JsonConvert.SerializeObject(message);

                // Encode Into Byte Array
                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                // Send message to server

                foreach (Client client in clientList)
                {
                    this.serverSocket.SendTo(msg, 0, msg.Length, SocketFlags.None, client.endPoint);
                }
                //ServerComTextBox.Text += ServerCommInput.Text + "\n";
                this.Dispatcher.Invoke(this.updateRoomsDelegate, new object[] { message });
                ServerCommInput.Clear();
            }

        }
    }
}
