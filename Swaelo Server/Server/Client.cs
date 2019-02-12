using System;
using System.Net.Sockets;


namespace Swaelo_Server
{
    class Client
    {
        public CharacterData CurrentCharacterData;

        public int ClientID;
        public TcpClient ClientSocket;
        public NetworkStream ClientStream;
        public byte[] ClientBuffer;
        public ByteBuffer.ByteBuffer Reader;

        public string AccountName = "";
        public string CurrentCharacterName = "";
        public bool InGame = false;
        public SwaeloMath.Vector3 CharacterPosition;
        public bool IsMale = true;

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
            //Get the size of the packet
            int PacketSize = ClientStream.EndRead(result);

            //0 bytes being received means the connection was closed by the client so we can shut down this connection now
            if(PacketSize == 0)
            {
                CloseConnection();
                return;
            }

            //read in the packet data send from the client
            byte[] PacketData = new byte[PacketSize];
            Array.Copy(ClientBuffer, PacketData, PacketSize);
            PacketReader.HandlePacket(ClientID, PacketData);
            //start listening for new packets again
            ClientStream.BeginRead(ClientBuffer, 0, ClientSocket.ReceiveBufferSize, ReadPacket, null);
        }

        private void CloseConnection()
        {
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
                PacketSender.SendRemoveOtherPlayer(ID, CurrentCharacterName);
            }
        }
    }
}