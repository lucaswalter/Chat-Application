using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientChatApplication.Messages
{

    /// <summary>
    /// All messages will be of the structure {"who", "what","when", "where", "why"}
    /// </summary>
    public class Message
    {
        public string Who { get; set; }
        public string What { get; set; }
        public string When { get; set; }
        public string Where { get; set; }
        public string Why { get; set; }
    }
}