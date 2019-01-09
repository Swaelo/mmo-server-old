using System;

namespace Server
{
    class Globals
    {
        public static Globals instance = new Globals();

        //Global instances of classes.
        public static General general = new General();
        public static Network network = new Network();
        public static Database database = new Database();
        public static MySQL mysql = new MySQL();
        public static NetworkHandleData networkHandleData = new NetworkHandleData();
        public static NetworkSendData networkSendData = new NetworkSendData();
        public static ServerLoop serverLoop = new ServerLoop();

        public static LivePlayer[] LivePlayers = new LivePlayer[Constants.MAX_PLAYERS];
        public static Client[] Clients = new Client[Constants.MAX_PLAYERS];

        //
        public int Player_HighIndex;
    }
}
