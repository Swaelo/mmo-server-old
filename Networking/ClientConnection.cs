// ================================================================================================================================
// File:        ClientConnection.cs
// Description: Handles a network connection between the server and a single game client, helps transfer packets between the two and
//              keeps track of what account and character is being used by that game client
// ================================================================================================================================

using System;
using System.Net;
using System.Net.Sockets;
using BEPUutilities;
using BEPUphysics.Entities.Prefabs;

namespace Server.Networking
{
    public class ClientConnection
    {
        public Sphere ServerCollider = null;
        public int ID;  //Each client connected to the server unique ID number is just their IP address
        public string AccountName;  //The name of the account this client is logged into
        public bool InGame = false; //Is this user active in the game world right now or not
        public string CharacterName;    //The name of the character the user is currently playing
        public Vector3 CharacterPosition;   //The world position of the character currently being played with
        public TcpClient Connection;    //Current tcp connection to this game client
        public NetworkStream DataStream;    //Current datastream for transmitting data back and forth with this client
        public byte[] DataBuffer;   //Buffer to store data received through the datastream until everything has been sent

        public ClientConnection(TcpClient Connection)
        {
            this.Connection = Connection;
            ID = ((IPEndPoint)Connection.Client.RemoteEndPoint).Port;
            Connection.SendBufferSize = 4096;
            Connection.ReceiveBufferSize = 4096;
            DataStream = Connection.GetStream();
            DataBuffer = new byte[4096];
            DataStream.BeginRead(DataBuffer, 0, 4096, ReadPacket, null);
        }

        private void ReadPacket(IAsyncResult result)
        {
            //Grab the size of the packet and make sure this client connection is still open
            int PacketSize = 0;
            //IOExceptions may occur when client connections have been lost (at this time for unknown reasons)
            try { PacketSize = DataStream.EndRead(result); }
            catch(System.IO.IOException e)
            {
                ConnectionManager.CloseConnection(this);
                return;
            }

            //PacketSize being read in with a value of 0 means the connection has been closed from the users side
            if (PacketSize == 0)
            {
                ConnectionManager.CloseConnection(this);
                return;
            }
            
            //Otherwise we read in this packet as normal, make a copy of the data buffer and send that through to the packet manager
            byte[] PacketBuffer = new byte[PacketSize];
            Array.Copy(DataBuffer, PacketBuffer, PacketSize);
            PacketManager.ReadClientPacket(ID, PacketBuffer);

            //Start listening again for packets from this client
            DataStream.BeginRead(DataBuffer, 0, Connection.ReceiveBufferSize, ReadPacket, null);
        }
    }
}
