using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace ClientChatApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private Socket chatSocket;
        private EndPoint epRemote;
        private byte[] buffer;


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Setup Socket
                chatSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                chatSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Attempt Remote Connection
                // TODO: Add Actuall Server IP Address & Port
                epRemote = new IPEndPoint(IPAddress.Parse(GetLocalIP()), Convert.ToInt32("3002"));
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
        /// Return Your Ownn IP Address
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

        private void MessageCallBack(IAsyncResult result)
        {

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
                AppendLineToChatBox(messageText.Text);
                messageText.Clear();
            }           
        }

        #endregion
    }
}
