namespace Protocol
{
    public static class Protocol
    {
        // Basic Messaging
        // {"user", "messageText", "timestamp", "roomId", "protocol"}
        public const int PUBLIC_MESSAGE = 100;
        public const int PRIVATE_MESSAGE = 101;

        // Global Messaging
        // {"user", "messageText", "timestamp", "roomId", "protocol"}
        public const int GLOBAL_INFO_MESSAGE = 110;
        public const int GLOBAL_WARNING_MESSAGE = 111;

        // User Actions
        public const int CREATE_ACCOUNT = 200;
        public const int ADD_FRIEND = 201;
        public const int REMOVE_FRIEND = 202;
        public const int BLOCK_USER = 203;
        public const int UNBLOCK_USER = 204;

        public const int RETRIEVE_FRIENDS = 210;
        public const int RETRIEVE_BLOCKED_USERS = 211;

        // Room Actions
        // {"ownerUser", "null", "timestamp", "null", "protocol"}
        public const int CREATE_PUBLIC_ROOM = 300;
        public const int CREATE_PRIVATE_ROOM = 301;

        // {"null", "null", "timestamp", "null", "protocol"}
        public static int RETRIEVE_PUBLIC_ROOMS = 302;

        public const int INVITE_USER_TO_ROOM = 310;
        public const int KICK_USER_FROM_ROOM = 311;
        public const int PROMOTE_USER = 312;
        public const int DEMOTE_USER = 313;

        // {"user", "null", "timestamp", "roomId", "protocol"}
        public const int ENTER_ROOM = 320;
        public const int LEAVE_ROOM = 321;

        public const int CLOSE_ROOM = 399;
    }
}
