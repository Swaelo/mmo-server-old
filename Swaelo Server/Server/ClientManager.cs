using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Swaelo_Server
{
    class ClientManager
    {
        //Client ID is mapped to that clients socket connection
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        //Returns a list of all the clients which are currently in the game world playing with their character
        public static List<Client> GetActiveClientsExceptFor(int ClientID)
        {
            //Create a list to store each of the active clients
            List<Client> ActiveClients = new List<Client>();

            //Loop through the list of all active client connections
            foreach(var Client in Clients)
            {
                //Get the info about this client
                int ID = Client.Key;
                Client Data = Client.Value;
                //Ignore clients who arent playing yet, they are still in the menu somewhere
                if (!Data.InGame)
                    continue;
                //Ignore the client that we should be ignoring
                if (ClientID == ID)
                    continue;

                //This is one of the clients we are looking for, add them to the list
                ActiveClients.Add(Data);
            }

            //Return the list we have created
            return ActiveClients;
        }

        //Returns a list of all active clients
        public static List<Client> GetAllActiveClients()
        {
            List<Client> ActiveClients = new List<Client>();
            foreach(var Client in Clients)
            {
                if (Client.Value.InGame)
                    ActiveClients.Add(Client.Value);
            }
            return ActiveClients;
        }

        //Returns a list of all the clients except for one
        public static List<Client> GetOtherClients(int ClientID)
        {
            List<Client> OtherClients = new List<Client>();
            foreach(var Client in Clients)
            {
                if (ClientID == Client.Key)
                    continue;
                OtherClients.Add(Client.Value);
            }
            return OtherClients;
        }

        //When a new client has connected to the server, map their connection into the dictionary
        public static void CreateNewConnection(TcpClient TempClient)
        {
            Client NewClient = new Client();
            NewClient.ClientSocket = TempClient;
            NewClient.ClientID = ((IPEndPoint)TempClient.Client.RemoteEndPoint).Port;
            NewClient.Start();
            Clients.Add(NewClient.ClientID, NewClient);

            //Tell this client to spawn in all of the other clients player characters
        }

        //Sends a packet to the client with the matching connection ID
        public static void SendPacketTo(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteBytes(PacketData);
            Clients[ClientID].ClientStream.BeginWrite(PacketWriter.ToArray(), 0, PacketWriter.ToArray().Length, null, null);
            PacketWriter.Dispose();
        }

        //Sends a packet to all the clients in the given list
        public static void SendPacketToList(byte[] PacketData, List<Client> ClientList)
        {
            foreach (Client Player in ClientList)
                SendPacketTo(Player.ClientID, PacketData);
        }

        //Sends the packet to every single client that is connected to the server
        public static void SendPacketToAll(byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteBytes(PacketData);
            foreach (var Client in Clients)
            {
                int ID = Client.Key;
                Client client = Client.Value;
                SendPacketTo(ID, PacketWriter.ToArray());
            }
            PacketWriter.Dispose();
        }

        //Sends a packet to every single client that is connected for the server except for one of them whom nothing will be sent to
        public static void SendPacketToAllBut(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteBytes(PacketData);
            foreach(var Client in Clients)
            {
                int ID = Client.Key;
                Client ClientData = Client.Value;
                if (ClientID == ID)
                    continue;
                SendPacketTo(ID, PacketWriter.ToArray());
            }
            PacketWriter.Dispose();
        }
    }
}
