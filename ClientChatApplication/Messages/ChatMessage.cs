using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientChatApplication.Messages
{
    /// <summary>
    /// A wrapper class for the messages that we intend to send and receive.
    /// The [ProtoContract] attribute informs NetworkComms .Net that we intend to
    /// serialise (turn into bytes) this object. At the base level the
    /// serialisation is performed by protobuf.net.
    /// </summary>
    class ChatMessage
    {

    }
}