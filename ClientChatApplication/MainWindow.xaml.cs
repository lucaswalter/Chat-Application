using ClientChatApplication.Messages;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ClientChatApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        // Communication Variables
        private Socket chatSocket;
        private EndPoint epLocal, epRemote;

        // Receive Data
        private byte[] buffer;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Setup Socket
                chatSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                chatSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);


                // Bind Local Socket
                // TODO: Decide Port Configuration
                epLocal = new IPEndPoint(IPAddress.Parse(GetLocalIP()), Convert.ToInt32("3001"));
                chatSocket.Bind(epLocal);

                // Attempt Remote Connection
                // TODO: Add Actual Server IP Address & Port
                epRemote = new IPEndPoint(IPAddress.Parse(GetLocalIP()), Convert.ToInt32("3001"));
                chatSocket.Connect(epRemote);

                // Begin Listening To A Specific Port
                buffer = new byte[1500];
                chatSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                
            }
            catch (Exception e)
            {
                AppendLineToChatBox(e.ToString());
            }


            // Write Line To Chat Box
            AppendLineToChatBox("Successfull Connection To Server!");


        }

        #region Networking

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

        /// <summary>
        /// Message Callback Attached To BeginReceiveFrom
        /// </summary>
        /// <param name="result"></param>
        private void MessageCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int size = chatSocket.EndReceiveFrom(asyncResult, ref epRemote);

                // Check If Data Exists
                if (size > 0)
                {
                    // Auxiliary Buffer
                    byte[] aux = new byte[1500];

                    // Retrieve Data
                    aux = (byte[])asyncResult.AsyncState;

                    // Converte Data To JSON String
                    string jsonStr = Encoding.UTF8.GetString(aux);

                    // TODO: Deserialize JSON & Handle Message

                    // Start To Listen Again
                    buffer = new byte[1500];
                    chatSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                }
            }
            catch (Exception e)
            {
                AppendLineToChatBox(e.ToString());
            }

        }

        #endregion

        #region UI Methods

        /// <summary>
        /// Append the provided message to the chatBox text box.
        /// </summary>
        /// <param name="message"></param>
        private void AppendLineToChatBox(string message)
        {
            //To ensure we can successfully append to the text box from any thread
            //we need to wrap the append within an invoke action.
            chatBox.Dispatcher.BeginInvoke(new Action<string>((messageToAdd) =>
            {
                chatBox.AppendText(messageToAdd + "\n");
                chatBox.ScrollToEnd();
            }), new object[] { message });
        }

        /// <summary>
        /// Send any entered message when we click the send button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Send any entered message when we press enter or the return key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                SendMessage();
        }

        /// <summary>
        /// Correctly shutdown NetworkComms .Net when closing the WPF application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Ensure we shutdown comms when we are finished
            //TODO: Update when deciding upon final networking solution 
        }

        #endregion

        #region Messaging

        /// <summary>
        /// Send our message.
        /// </summary>
        private void SendMessage()
        {
            if (!string.IsNullOrEmpty(messageText.Text))
            {
                try
                {
                    // Create POCO Message
                    Message message = new Message
                    {
                        Who = "Me",
                        What = messageText.Text,
                        When = DateTime.Now.ToShortTimeString(),
                        Where = String.Empty, // TODO: Use Room ID
                        Why = Protocol.PUBLIC_MESSAGE
                    };

                    // Serialize JSON Object
                    string jsonMessage = JsonConvert.SerializeObject(message);

                    // Encode Into Byte Array
                    var enc = new ASCIIEncoding();
                    byte[] msg = new byte[1500];
                    msg = enc.GetBytes(jsonMessage);

                    // Send The Message
                    chatSocket.Send(msg);

                    // TODO: Wait For Server Callback To Display Message
                    AppendLineToChatBox("[" + message.When + "] " + message.Who + " : " + messageText.Text);
                    messageText.Clear();
                }
                catch (Exception e)
                {
                    AppendLineToChatBox(e.ToString());
                }
            }           
        }

        #endregion
    }
}
