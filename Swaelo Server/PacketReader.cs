using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADODB;

public enum ClientPacketType
{
    Message = 1,    //client sends a message to the server <int:PacketType, string:Message>
    Register = 2,   //clients sends request to the server to register a new account <int:PacketType, string:Username, string:Password>
    Login = 3,   //client sends a request to the server to log into an account <int:PacketType, string:Username, string:Password>
    PlayerUpdate = 4,    //clients updating the server on their position and rotation information <int:PacketType, vector3:position, vector4:rotation>
    Disconnect = 5,
    PlayerMessage = 6
}

namespace Swaelo_Server
{
    class PacketReader
    {
        public delegate void Packet(int index, byte[] data);
        public static Dictionary<int, Packet> Packets = new Dictionary<int, Packet>();

        public static void InitializePackets()
        {
            Packets.Add((int)ClientPacketType.Message, HandleMessage);
            Packets.Add((int)ClientPacketType.Register, HandleRegisterRequest);
            Packets.Add((int)ClientPacketType.Login, HandleLoginRequest);
            Packets.Add((int)ClientPacketType.PlayerUpdate, HandlePlayerUpdate);
            Packets.Add((int)ClientPacketType.Disconnect, HandleClientDisconnect);
            Packets.Add((int)ClientPacketType.PlayerMessage, HandleChatMessage);
        }

        //Gets a packet from the client, passes it onto whatever function is registered to handle whatever type of packet it is
        public static void HandlePacket(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();   //start the packet reader
            PacketReader.WriteBytes(PacketData);    //fill it with the data from the packet that was sent to us
            int PacketType = PacketReader.ReadInteger();    //find out what type of packet this is
            //Ignore empty packets
            if (PacketType == 0)
                return;
            //Invoke whatever function has been registered to this packet type in our dictionary
            Packet NetworkPacket;
            if (Packets.TryGetValue(PacketType, out NetworkPacket))
                NetworkPacket.Invoke(ClientID, PacketData);
        }

        //Gets a message sent to us from one of the clients
        public static void HandleMessage(int ClientID, byte[] PacketData)
        {
            //Extract the message from the packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Message = PacketReader.ReadString();
            //Display the message to the console window
            Console.WriteLine("Client " + ClientID + ": " + Message);
            //Close the packet reader
            PacketReader.Dispose();
        }

        //Gets a request from a client to register a new account <int:PacketType, string:Username, string:Password>
        public static void HandleRegisterRequest(int ClientID, byte[] PacketData)
        {
            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Name = PacketReader.ReadString();
            string Pass = PacketReader.ReadString();
            PacketReader.Dispose();
            //Check that this account doesnt already exist
            bool AccountExists = Globals.database.DoesAccountExist(ClientID, Name);
            if (AccountExists)
            {//if the account already exists we need to tell the user the name is already taken
                Console.WriteLine(Name + " account already exists");
                PacketSender.SendMessage(ClientID, "That username is taken");
                return;
            }
            else
            {//Otherwise, we need to register the account into the database and tell the user its okay
                Console.WriteLine(Name + " account is free to be created");
                //Get the table of accounts from the database and write in the info for the new account
                var db = Globals.database;
                var rec = db.recorder;
                string Query = "SELECT * FROM accounts WHERE 0=1";  //define a query to fetch all the accounts
                rec.Open(Query, db.connection, db.cursorType, db.lockType); //get the table of accaounts
                rec.AddNew();   //add a new row into the table
                rec.Fields["Username"].Value = Name;    //save the account name
                rec.Fields["Password"].Value = Pass;    //save the password
                //Update the table and close the database
                rec.Update();
                rec.Close();
                //Tell the client the new account has successfully been registered
                PacketSender.SendMessage(ClientID, Name + " account has been registered");
            }
        }

        //Gets a request from a client to log into an account <int:PacketType, string:Username, string:Password>
        public static void HandleLoginRequest(int ClientID, byte[] PacketData)
        {
            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Name = PacketReader.ReadString();
            string Pass = PacketReader.ReadString();
            PacketReader.Dispose();
            //Make sure the user isnt trying to log into an account that doesnt exist
            bool AccountExists = Globals.database.DoesAccountExist(ClientID, Name);
            //If the account doesnt exist they cant log into it obviously
            if(!AccountExists)
            {
                PacketSender.SendMessage(ClientID, "The " + Name + " account does not exist");
                return;
            }
            //Now check they have given the correct password
            bool PasswordMatches = Globals.database.DoesPasswordMatch(ClientID, Name, Pass);
            //If the password is wrong they cant log into the account
            if(!PasswordMatches)
            {
                PacketSender.SendMessage(ClientID, "The password was incorrect");
                return;
            }
            //Everything is correct, now load the character data from that account and send it to the client
            //so they can use it to enter into the game world
            PacketSender.SendMessage(ClientID, "Login Success");
            //Get the characters position and rotation data from the database
            Vector3 CharacterPosition = Globals.database.GetPlayerLocation(ClientID, Name);
            Vector4 CharacterRotation = Globals.database.GetPlayerRotation(ClientID, Name);
            //Send this information to the client and instruct them to enter into the game world with their character
            PacketSender.SendEnterGame(ClientID, Name, CharacterPosition, CharacterRotation);
            //wait for a quarter of a second for the player to enter the game before we go on to spawn external clients
            System.Threading.Thread.Sleep(250);
            //Take note in this client class that this player is in the world now
            ClientManager.Clients[ClientID].AccountName = Name;
            ClientManager.Clients[ClientID].InGame = true;
            ClientManager.Clients[ClientID].CharacterPosition = CharacterPosition;
            ClientManager.Clients[ClientID].CharacterRotation = CharacterRotation;
            //The client must also be told to spawn in the player characters of anyone else already playing the game
            //We need to tell this client about all the players already in the game
            foreach (var Client in ClientManager.Clients)
            {
                int ID = Client.Key;
                Client client = Client.Value;
                //Dont tell the client about themself
                if (ID == ClientID)
                    continue;
                //Ignore clients who arent logged into the game yet
                if (!client.InGame)
                    continue;
                //All other clients need to be spawned in
                PacketSender.SendSpawnOther(ClientID, client.AccountName, client.CharacterPosition, client.CharacterRotation);
                System.Threading.Thread.Sleep(250); //sleep for a quarter of a second between spawning in each client so packets dont get jammed
                //They also need to be told to spawn in this new player too
                PacketSender.SendSpawnOther(ID, Name, CharacterPosition, CharacterRotation);
                System.Threading.Thread.Sleep(250);
            }
        }

        //Gets updated character information data from one of the connect clients, this needs to be sent out to everyone else so they know where that character is at
        public static void HandlePlayerUpdate(int ClientID, byte[] PacketData)
        {
            //start the packet reader and give it the packet data information
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            //extract all the information from the packet
            int PacketType = PacketReader.ReadInteger();
            string AccountName = PacketReader.ReadString();
            //get position data
            float PosX = PacketReader.ReadFloat();
            float PosY = PacketReader.ReadFloat();
            float PosZ = PacketReader.ReadFloat();
            Vector3 NewPosition = new Vector3(PosX, PosY, PosZ);
            //close the packet reader
            PacketReader.Dispose();
            //update the client class with the characters new information
            var Client = ClientManager.Clients[ClientID];
            Client.CharacterPosition = NewPosition;
            //send this new data out to all the other clients
            foreach (var OtherClient in ClientManager.Clients)
            {
                //Grab this clients info out of the dictionary
                int ID = OtherClient.Key;
                Client ExternalClient = OtherClient.Value;
                //clients who arent in the game world dont need to be told anything
                if (!ExternalClient.InGame)
                    continue;
                //other active clients need to be sent this players new pos/rot data
                PacketSender.SendPlayerUpdate(ID, AccountName, NewPosition);
            }
        }

        public static void HandleClientDisconnect(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string AccountName = PacketReader.ReadString();
            //disconnect from and remove the client from list of connected clients
            ClientManager.Clients[ClientID].ClientSocket.Close();
            ClientManager.Clients.Remove(ClientID);
            //tell all the other clients this player is no longer playing
            foreach(var OtherClient in ClientManager.Clients)
            {
                PacketSender.SendRemovePlayer(OtherClient.Key, AccountName);
            }
        }

        public static void HandleChatMessage(int ClientID, byte[] PacketData)
        {
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string AccountName = PacketReader.ReadString();
            string Message = PacketReader.ReadString();
            foreach(var OtherClient in ClientManager.Clients)
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
                PacketSender.SendPlayerMessage(ID, AccountName, Message);
            }
        }
    }
}
