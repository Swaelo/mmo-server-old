using System;
using System.Net.Sockets;

//This class allows the game server to interact with the mysql game database
namespace Server
{
    //Stores networking data for a single player who is connected to the server
    //With this we can send data between the client and the server, and vice versa
    class Client
    {
        public int Index;   //Player connection ID number
        public string IP;   //Players IP address
        public TcpClient Socket;    //Players TCP Socket
        public NetworkStream myStream;
        private byte[] readBuff;

        //When a new client is connecting to the server we create a new instance of the client class
        //We will have this new client immediately start listening to the stream of the server it is connected to
        public void Start()
        {
            Socket.SendBufferSize = 4096;
            Socket.ReceiveBufferSize = 4096;
            myStream = Socket.GetStream();
            Array.Resize(ref readBuff, Socket.ReceiveBufferSize);
            myStream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
        }

        void CloseConnection()
        {
            Console.WriteLine("Closing a clients connection");
            //remove that players character from the game world and take note they are no longer playing
            Globals.LivePlayers[Index].IsPlaying = false;
            //backup the players current world position and save that in the database for future use
            Globals.database.SavePlayerData(Index);
            Socket.Close();
            Socket = null;
        }

        void OnReceiveData(IAsyncResult result)
        {
            int readBytes = myStream.EndRead(result);
            if (Socket == null)
            {
                return;
            }
            if (readBytes <= 0)
            {
                Console.WriteLine("Closing client connection because readBytes <= 0");
                CloseConnection();
                return;
            }

            byte[] newBytes = null;
            Array.Resize(ref newBytes, readBytes);
            Buffer.BlockCopy(readBuff, 0, newBytes, 0, readBytes);

            Globals.networkHandleData.HandleData(Index, newBytes);

            if (Socket == null)
            {
                return;
            }

            myStream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
        }
    }
}
