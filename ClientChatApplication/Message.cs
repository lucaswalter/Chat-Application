using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientChatApplication.Messages
{

    public enum Code
    {
        // Basic Messaging
        PUBLIC_MESSAGE = 100,
        PRIVATE_MESSAGE = 101,

        // Global Messaging
        GLOBAL_INFO_MESSAGE = 110,
        GLOBAL_WARNING_MESSAGE = 111,
        
        // User Actions
        CREATE_ACCOUNT = 200,
        ADD_FRIEND = 201,
        REMOVE_FRIEND = 202,
        BLOCK_USER = 203,
        UNBLOCK_USER = 204,

        RETRIEVE_FRIENDS = 210,
        RETRIEVE_BLOCKED_USERS = 211,

        // Room Actions
        CREATE_PUBLIC_ROOM = 300,
        CREATE_PRIVATE_ROM = 301,

        INVITE_USER_TO_ROOM = 310,
        KICK_USER_FROM_ROOM = 311,
        PROMOTE_USER = 312,
        DEMOTE_USER = 313,

        ENTER_ROOM = 320,
        LEAVE_ROOM = 321,

        CLOSE_ROOM = 399
    }


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