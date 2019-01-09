using System;
using System.Collections.Generic;

namespace Server
{
    class NetworkHandleData
    {
        private delegate void Packet_(int Index, byte[] Data);
        private Dictionary<int, Packet_> Packets; //We create a new dictionary and we add our network packets into it

        public void InitMessages()
        {
            Packets = new Dictionary<int, Packet_>();
            Packets.Add((int)PacketTypes.ClientPackets.CNewAccount, HandleNewAccount);
            Packets.Add((int)PacketTypes.ClientPackets.CLoginAccount, HandleLogin);
            Packets.Add((int)PacketTypes.ClientPackets.CMovePlayer, HandlePlayerMovement);
        }

        //reads in a packet sent from the client
        public void HandleData(int index, byte[] data)
        {
            int packetnum;
            Packet_ Packet;
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadInteger();
            buffer = null;

            if (packetnum == 0)
                return;

            if (Packets.TryGetValue(packetnum, out Packet))
            {
                Packet.Invoke(index, data);
            }
        }

        //packet type has been read from the HandleData function as a new account request
        void HandleNewAccount(int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            //make sure to always read the data in the same way you sent it
            int packetNum = buffer.ReadInteger();
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            Globals.database.AddAccount(username, password);
        }

        //packet type has been read from the HandleData function as an account login
        void HandleLogin(int index, byte[] data)
        {
            //Get the login information the user has sent
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            int packetNum = buffer.ReadInteger();
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            Console.WriteLine("requesting to login to account: " + username + " with password: " + password);
            //Check if the login information is correct
            bool accountExists = Globals.database.DoesAccountExist(index, username);
            if(!accountExists)
            {
                //the login request failed if the account doesnt exist
                Console.WriteLine(username + " account does not exist");
                return;
            }
            //check if the password is correct
            if(!Globals.database.DoesPasswordMatch(index, username, password))
            {
                Console.WriteLine(username + " password was incorrect");
                return;
            }

            //after all checks have passed we tell the client they can log in
            Console.WriteLine(username + " logged in with client id " + index);
            //TODO, track what accounts are already logged in
            //Load the players information from the server, tell them where they are, their character information etc
            Globals.database.LoadPlayerData(index, username);
            Globals.LivePlayers[index].IsPlaying = true;
            //Send them into the game
            Globals.networkSendData.SendIngame(index);
        }

        //Packets are sent from each client telling us of their update position and rotation information after being moved locally on their client
        void HandlePlayerMovement(int index, byte[] data)
        {
            //read the packet into a buffer object so we can read out the components correctly
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            int packetNum = buffer.ReadInteger();

            //read out the players brand new position and rotation information they have sent us
            Vector3 PlayerPosition = new Vector3(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
            Vector4 PlayerRotation = new Vector4(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
            //get the account name
            string PlayerName = buffer.ReadString();

            //update this info in the server, then send the new info to all the other clients
            Globals.general.SetPlayerPosition(index, PlayerPosition);
            Globals.general.SetPlayerRotation(index, PlayerRotation);
            Globals.networkSendData.SendPlayerMovement(index, PlayerPosition, PlayerRotation, PlayerName);
        }
    }
}
