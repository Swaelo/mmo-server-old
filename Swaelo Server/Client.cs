using System;
using System.Net.Sockets;


namespace Swaelo_Server
{
    class Client
    {
        public int ClientID;
        public TcpClient ClientSocket;
        public NetworkStream ClientStream;
        public byte[] ClientBuffer;
        public ByteBuffer.ByteBuffer Reader;

        public string AccountName = "";
        public bool InGame = false;
        public Vector3 CharacterPosition;
        public Vector4 CharacterRotation;

        public void Start()
        {
            ClientSocket.SendBufferSize = 4096;
            ClientSocket.ReceiveBufferSize = 4096;
            ClientStream = ClientSocket.GetStream();
            ClientBuffer = new byte[4096];
            ClientStream.BeginRead(ClientBuffer, 0, ClientSocket.ReceiveBufferSize, ReadPacket, null);
            Console.WriteLine("Incoming connection from '{0}'.", ClientSocket.Client.RemoteEndPoint.ToString());
        }

        private void ReadPacket(IAsyncResult result)
        {
            try
            {
                //Get the amount of data sent to us
                int PacketSize = ClientStream.EndRead(result);
                //If the size of the packet is 0 the connection to the client has been lost
                if(PacketSize <= 0)
                {
                    Console.WriteLine("Connection from '{0}' has been lost", ClientSocket.Client.RemoteEndPoint.ToString());
                    CloseConnection();
                    return;
                }
                //Read in the packet from the server
                byte[] PacketData = new byte[PacketSize];
                Array.Copy(ClientBuffer, PacketData, PacketSize);
                //Send the packet on to be handled by its handler function
                PacketReader.HandlePacket(ClientID, PacketData);
                //start listening for packets again
                ClientStream.BeginRead(ClientBuffer, 0, ClientSocket.ReceiveBufferSize, ReadPacket, null);
            }
            catch (Exception)
            {
                Console.WriteLine("exception reading packet from client, closing their connection");
                CloseConnection();
                return;
            }
        }

        private void CloseConnection()
        {
            Console.WriteLine("Connection from '{0}' has been closed.", ClientSocket.Client.RemoteEndPoint.ToString());
            ClientManager.Clients.Remove(ClientID);
            ClientSocket.Close();
            //Tell all the remaining clients to remove this clients character from the game
            foreach (var OtherClient in ClientManager.Clients)
            {
                int ID = OtherClient.Key;
                Client client = OtherClient.Value;
                //Dont tell the client about themself
                if (ID == ClientID)
                    continue;
                //Ignore clients who arent logged into the game yet
                if (!client.InGame)
                    continue;
                //Send the message to all the other clients
                PacketSender.SendRemovePlayer(ID, AccountName);
            }
        }
    }
}