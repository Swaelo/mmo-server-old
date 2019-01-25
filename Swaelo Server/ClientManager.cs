using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class ClientManager
    {
        //Client ID is mapped to that clients socket connection
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        //When a new client has connected to the server, map their connection into the dictionary
        public static void CreateNewConnection(TcpClient TempClient)
        {
            Client NewClient = new Client();
            NewClient.ClientSocket = TempClient;
            NewClient.ClientID = ((IPEndPoint)TempClient.Client.RemoteEndPoint).Port;
            NewClient.Start();
            Clients.Add(NewClient.ClientID, NewClient);
            PacketSender.SendMessage(NewClient.ClientID, "Welcome");
        }

        //Sends a packet to the client with the matching connection ID
        public static void SendPacketTo(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            //PacketWriter.WriteInteger((PacketData.GetUpperBound(0) - PacketData.GetLowerBound(0)) + 1);
            PacketWriter.WriteBytes(PacketData);
            Clients[ClientID].ClientStream.BeginWrite(PacketWriter.ToArray(), 0, PacketWriter.ToArray().Length, null, null);
            PacketWriter.Dispose();
        }
    }
}
