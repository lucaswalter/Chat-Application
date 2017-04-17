using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientChatApplication
{
    public static class Protocol
    {
        // Basic Messaging
        public static int PUBLIC_MESSAGE = 100;
        public static int PRIVATE_MESSAGE = 101;

        // Global Messaging
        public static int GLOBAL_INFO_MESSAGE = 110;
        public static int GLOBAL_WARNING_MESSAGE = 111;

        // User Actions
        public static int CREATE_ACCOUNT = 200;
        public static int ADD_FRIEND = 201;
        public static int REMOVE_FRIEND = 202;
        public static int BLOCK_USER = 203;
        public static int UNBLOCK_USER = 204;

        public static int RETRIEVE_FRIENDS = 210;
        public static int RETRIEVE_BLOCKED_USERS = 211;

        // Room Actions
        public static int CREATE_PUBLIC_ROOM = 300;
        public static int CREATE_PRIVATE_ROM = 301;

        public static int INVITE_USER_TO_ROOM = 310;
        public static int KICK_USER_FROM_ROOM = 311;
        public static int PROMOTE_USER = 312;
        public static int DEMOTE_USER = 313;

        public static int ENTER_ROOM = 320;
        public static int LEAVE_ROOM = 321;

        public static int CLOSE_ROOM = 399;
    }
}
