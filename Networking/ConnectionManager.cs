// ================================================================================================================================
// File:        ConnectionManager.cs
// Description: Keeps track of all the game clients which are currently connected to the server, and allows you to managed those
//              connections, and send packets to the clients as needed
// ================================================================================================================================

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using BEPUutilities;

namespace Server.Networking
{
    public static class ConnectionManager
    {
        public static TcpListener NewClientListener;    //Listens for new incoming client connections
        public static Dictionary<int, ClientConnection> ClientConnections = new Dictionary<int, ClientConnection>();    //Store all active client connections indexed by their client ID

        //constructor sets up the listener for new client connections
        public static void InitializeManager()
        {
            //Register packet reader handler functions
            PacketManager.RegisterPacketReaders();
            //Start listening for new incoming client connections
            NewClientListener = new TcpListener(IPAddress.Any, 5500);
            NewClientListener.Start();
            NewClientListener.BeginAcceptTcpClient(new AsyncCallback(NewClientConnected), null);
        }

        //Returns a list of all characters currently active in the game world
        public static List<ClientConnection> GetActiveClients()
        {
            List<ClientConnection> ActiveClients = new List<ClientConnection>();
            foreach(var Client in ClientConnections)
            {
                if (Client.Value.InGame)
                    ActiveClients.Add(Client.Value);
            }
            return ActiveClients;
        }

        //returns a list of all characters currently active in the game world, except for 1
        public static List<ClientConnection> GetActiveClientsExceptFor(int ClientID)
        {
            List<ClientConnection> ActiveClients = GetActiveClients();
            ActiveClients.Remove(ClientConnections[ClientID]);
            return ActiveClients;
        }

        //Returns whatever active client is closest to the given world position location
        public static ClientConnection GetClosestActiveClient(Vector3 TargetPosition)
        {
            //return null if there arent any clients to check
            if (GetActiveClients().Count == 0)
                return null;

            List<ClientConnection> ActiveClients = GetActiveClients();
            ClientConnection ClosestClient = ActiveClients[0];
            float ClosestDistance = Vector3.Distance(TargetPosition, ClosestClient.CharacterPosition);
            for (int i = 1; i < ActiveClients.Count; i++)
            {
                ClientConnection ClientCompare = ActiveClients[i];
                float ClientDistance = Vector3.Distance(TargetPosition, ClientCompare.CharacterPosition);
                if (ClientDistance < ClosestDistance)
                {
                    ClosestDistance = ClientDistance;
                    ClosestClient = ClientCompare;
                }
            }
            return ClosestClient;
        }

        //triggered when a new client connection is detected
        private static void NewClientConnected(IAsyncResult result)
        {
            //reset the listener after grabbing the new client connection from it
            TcpClient NewConnection = NewClientListener.EndAcceptTcpClient(result);
            NewClientListener.BeginAcceptTcpClient(new AsyncCallback(NewClientConnected), null);

            //Setup the new client connection and store them with all the others
            ClientConnection NewClient = new ClientConnection(NewConnection);
            ClientConnections.Add(NewClient.ID, NewClient);

            l.og("new client connected from " + NewClient.ID);
        }

        //Cleans up a connection from one of the clients no longer connected
        public static void CloseConnection(ClientConnection Connection)
        {
            l.og("player disconnecting");
            Entities.EntityManager.HandleClientDisconnect(Connection);
            HandleClientDisconnect(Connection);
        }

        //Notifies all active client connections that a player has disconnected from the game world
        public static void HandleClientDisconnect(ClientConnection Client)
        {
            //Remove them from the client list
            ClientConnections.Remove(Client.ID);
            //Tell all the other clients this player has left the game world
            List<ClientConnection> ActiveClients = GetActiveClients();
            Networking.PacketManager.SendListRemoveOther(ActiveClients, Client.CharacterName);
        }

        //Sends network packet to active game client
        public static void SendPacketTo(int ClientID, byte[] PacketData)
        {
            ClientConnections[ClientID].DataStream.BeginWrite(PacketData, 0, PacketData.Length, null, null);
        }

        //Sends network packet to a list of active game clients
        public static void SendPacketTo(List<ClientConnection> Clients, byte[] PacketData)
        {
            foreach (ClientConnection Client in Clients)
                Client.DataStream.BeginWrite(PacketData, 0, PacketData.Length, null, null);
        }

        //Checks if any of the active clients are logged into the given user account
        public static bool IsAccountLoggedIn(string AccountName)
        {
            foreach(var ActiveClient in ClientConnections)
            {
                if (ActiveClient.Value.AccountName == AccountName)
                    return true;
            }
            return false;
        }
    }
}
