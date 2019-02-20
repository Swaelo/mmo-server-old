using System;
using System.Net;
using System.Net.Sockets;

namespace Swaelo_Server
{
    class Server
    {
        //Server socket for clients to connect to
        static TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5500);
        public static bool ServerOpen = true;

        public static void InitializeServer()
        {
            Console.WriteLine("Starting server...");
            PacketReader.InitializePackets();   //register packet handler functions
            ServerSocket.Start();   //start the server tcp
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);    //start listening for new client connections
        }

        //triggered when a new client connects to the game server
        private static void OnClientConnect(IAsyncResult result)
        {
            TcpClient NewClient = ServerSocket.EndAcceptTcpClient(result);  //Stop accepting new connections into this socket
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);    //start accpeting new connections in new callback
            ClientManager.CreateNewConnection(NewClient);   //map the new client connection into our dictionary in the client manager class
        }
    }
}
