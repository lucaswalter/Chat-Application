using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
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
        public int Why { get; set; }
    }
}