// ================================================================================================================================
// File:        PacketReaderLogic.cs
// Description: Any time information is received from one of our clients, one of these functions will be used to handle that info
// Author:      Harley Laurie          
// Notes:       This will be split up into multiple seperate classes later on, this file is way too big right now
// ================================================================================================================================

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using MySql.Data.MySqlClient;

public enum ClientPacketType
{
    ConsoleMessage = 1, //client sent a message to be displayed in the console log window
    PlayerMessage = 2,  //player sent a message to be spread to other clients chat windows

    RegisterRequest = 3,    //client wants to register a new account
    LoginRequest = 4,   //client wants to log into their account
    CreateCharacterRequest = 5, //client wants to create a new character
    EnterWorldRequest = 6,  //client wants to enter into the game world with their selected character
    GetCharacterDataRequest = 7,    //client wants info about all their created characters

    PlayerUpdatePosition = 8,   //spread a players position update info to other clients
    PlayerMeleeAttack = 9,
    PlayerDisconnectNotice = 10,  //tell everyone else they stopped playing
    AccountLogoutNotice = 11    //let the server know we have logged out of this user account
}

namespace Swaelo_Server
{
    class PacketReaderLogic
    {
        public delegate void Packet(int index, byte[] data);
        public static Dictionary<int, Packet> Packets = new Dictionary<int, Packet>();
        public TcpClient ClientSocket;
        public NetworkStream ClientStream;
        public byte[] ClientBuffer;
        public ByteBuffer.ByteBuffer Reader;
        public static int Count = 0;
        public static int MaxOn = 0;

        public static void InitializePackets()
        {
            Packets.Add((int)ClientPacketType.ConsoleMessage, HandleConsoleMessage);  //Prints a message in the clients debug log
            Packets.Add((int)ClientPacketType.PlayerMessage, HandlePlayerMessage);

            Packets.Add((int)ClientPacketType.RegisterRequest, HandleRegisterRequest); //Attempts to register a new account with the information sent from a client
            Packets.Add((int)ClientPacketType.LoginRequest, HandleLoginRequest);   //Sends a new player into the game and synchronizes them across the network if the login information provided is correct
            Packets.Add((int)ClientPacketType.CreateCharacterRequest, HandleCreateCharacterRequest);
            Packets.Add((int)ClientPacketType.EnterWorldRequest, HandleEnterWorldRequest);
            Packets.Add((int)ClientPacketType.GetCharacterDataRequest, HandleGetCharacterDataRequest);

            Packets.Add((int)ClientPacketType.PlayerUpdatePosition, HandlePlayerUpdate);
            Packets.Add((int)ClientPacketType.PlayerMeleeAttack, HandlePlayerMeleeAttack);
            Packets.Add((int)ClientPacketType.PlayerDisconnectNotice, HandlePlayerDisconnect);
            Packets.Add((int)ClientPacketType.AccountLogoutNotice, HandleAccountLogout);
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

        //Checks if the given username doesnt break any rules
        private static bool IsValidUsername(string username)
        {
            l.o("checking if " + username + " is a valid username");
            //Loop through and check each character in the username, making sure none of them are banned characters
            for (int CharacterIterator = 0; CharacterIterator < username.Length; CharacterIterator++)
            {
                char CurrentCharacter = username[CharacterIterator];
                //Letters are allowed
                if (Char.IsLetter(CurrentCharacter))
                    continue;
                //Numbers are allowed
                if (Char.IsNumber(CurrentCharacter))
                    continue;
                //Dashes are allowed
                if (CurrentCharacter == '-')
                    continue;
                //Periods are allowed
                if (CurrentCharacter == '.')
                    continue;
                //Underscores are allowed
                if (CurrentCharacter == '_')
                    continue;
                //Anything else means the username is invalid
                return false;
            }
            return true;
        }

        //Gets a message sent to us from one of the clients
        public static void HandleConsoleMessage(int ClientID, byte[] PacketData)
        {
            l.o("Handle Console Message");

            //Extract the message from the packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Message = PacketReader.ReadString();
            //Display the message to the console window
            //Close the packet reader
            PacketReader.Dispose();
        }

        //Clients send chat messages to us and we send them to everyone else
        public static void HandlePlayerMessage(int ClientID, byte[] PacketData)
        {
            l.o("Handle Player Message");

            //Read the message info from the network packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string CharacterName = PacketReader.ReadString();
            string Message = PacketReader.ReadString();
            PacketReader.Dispose();
            //Send this message to all game client, except for the client who sent it
            List<Client> OtherClients = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketSenderLogic.SendPlayersMessage(OtherClients, CharacterName, Message);
        }

        //Gets a request from a client to register a new account <int:PacketType, string:Username, string:Password>
        public static void HandleRegisterRequest(int ClientID, byte[] PacketData)
        {
            l.o("Handle Register Request");

            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Name = PacketReader.ReadString();
            string Pass = PacketReader.ReadString();
            PacketReader.Dispose();
            //Make sure the username provided doesnt contain any banned characters
            if (!IsValidUsername(Name))
            {
                PacketSenderLogic.SendRegisterReply(ClientID, false, "Username contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Make sure the password provided doesnt contain any banned characters either
            if (!IsValidUsername(Pass))
            {
                PacketSenderLogic.SendRegisterReply(ClientID, false, "Password contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Check that this account doesnt already exist
            bool AccountExists = !Globals.database.IsAccountNameAvailable(Name);
            if (AccountExists)
            {//if the account already exists we need to tell the user the name is already taken
                PacketSenderLogic.SendRegisterReply(ClientID, false, "This account name has already been taken");
                return;
            }
            else
            {//Otherwise, we need to register the account into the database and tell the user its okay
                l.o(Name + " account is free to be created");
                //Get the table of accounts from the database and write in the info for the new account
                Globals.database.SaveNewAccount(Name, Pass);
                ////Tell the client the new account has successfully been registered
                PacketSenderLogic.SendRegisterReply(ClientID, true, "Account has been created");
            }
        }

        //Gets a request from a client to log into an account <int:PacketType, string:Username, string:Password>
        public static void HandleLoginRequest(int ClientID, byte[] PacketData)
        {
            l.o("Handle Login Request");

            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Name = PacketReader.ReadString();
            string Pass = PacketReader.ReadString();
            PacketReader.Dispose();
            //Make sure the user isnt trying to log into an account that doesnt exist
            bool AccountExists = !Globals.database.IsAccountNameAvailable(Name);
            //If the account doesnt exist they cant log into it obviously
            if (!AccountExists)
            {
                PacketSenderLogic.SendLoginReply(ClientID, false, "That account does not exist");
                return;
            }
            //Make sure someone else isnt already logged into this account
            if (ClientManager.IsAccountLoggedIn(Name))
            {
                PacketSenderLogic.SendLoginReply(ClientID, false, "Someone is already logged into that account.");
                return;
            }
            //Now check they have given the correct password
            bool PasswordMatches = Globals.database.IsPasswordCorrect(Name, Pass);
            //If the password is wrong they cant log into the account
            if (!PasswordMatches)
            {
                PacketSenderLogic.SendLoginReply(ClientID, false, "The password was incorrect");
                return;
            }
            l.o(Name + " has logged in");
            ClientManager.Clients[ClientID].AccountName = Name;
            PacketSenderLogic.SendLoginReply(ClientID, true, "Login success");
        }

        //Tries to log into user account, tells the client if it worked or not
        public static void HandleAccountLogout(int ClientID, byte[] PacketData)
        {
            l.o("Handle Account Logout");

            //Get this clients account information
            Client Client = ClientManager.Clients[ClientID];
            //Announce their logging out
            l.o(Client.AccountName + " has logged out");
            Client.AccountName = "";
            Client.CurrentCharacterName = "";
        }

        //Trys to create a new character for the user and tells them how it went
        public static void HandleCreateCharacterRequest(int ClientID, byte[] PacketData)
        {
            l.o("Handle Create Character Request");

            //Open the packet and extract all the relevant information, then close it
            l.o("checking if we can create a new character for this player");
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string AccountName = PacketReader.ReadString();
            string CharacterName = PacketReader.ReadString();
            bool IsMale = PacketReader.ReadInteger() == 1;
            l.o(AccountName + " wants to register " + CharacterName + " as a new character");
            PacketReader.Dispose();
            //Query the database to check if this character name has already been taken by someone else
            bool CharacterExists = !Globals.database.IsCharacterNameAvailable(CharacterName);
            //If this character already exists, send the reply message telling them the name is already taken
            if (CharacterExists)
            {
                PacketSenderLogic.SendCreateCharacterReply(ClientID, false, "This character name is already taken");
                return;
            }
            //Otherwise, we need to create this new character and save it into the database registered under this users account
            Globals.database.SaveNewCharacter(AccountName, CharacterName, IsMale);
            PacketSenderLogic.SendCreateCharacterReply(ClientID, true, "Character Created");
        }

        //client wants to enter into the game world with their selected character
        public static void HandleEnterWorldRequest(int ClientID, byte[] PacketData)
        {
            l.o("Handle Enter World Request");

            //Extract information from the packet and save it into this clients class in the client manager list
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            Client Client = ClientManager.Clients[ClientID];
            Client.AccountName = PacketReader.ReadString();
            Client.InGame = true;
            Client.CurrentCharacterName = PacketReader.ReadString();
            Client.CharacterPosition = new Vector3(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            PacketReader.Dispose();

            //Send this player into the world with their character now
            l.o(Client.AccountName + ":" + Client.CurrentCharacterName + " has entered the world");
            PacketSenderLogic.SendPlayerEnterWorld(ClientID);

            //Spawn a representation of this player in the server physics scene
            Client.ServerCollider = new Sphere(Client.CharacterPosition, 1);
            Globals.space.Add(Client.ServerCollider);
            Globals.game.ModelDrawer.Add(Client.ServerCollider);

            //Any other players who are already playing the game need to be told to spawn this client
            List<Client> OtherActivePlayers = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketSenderLogic.SendListSpawnOther(OtherActivePlayers, Client.CurrentCharacterName, Client.CharacterPosition);
        }

        //Sends to the client all the infor for any characters they have created so far
        public static void HandleGetCharacterDataRequest(int ClientID, byte[] PacketData)
        {
            l.o("Handle Get Character Data Request");

            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Username = PacketReader.ReadString();
            PacketReader.Dispose();
            PacketSenderLogic.SendCharacterData(ClientID, Username);
        }

        //Gets updated character information data from one of the connect clients, this needs to be sent out to everyone else so they know where that character is at
        public static void HandlePlayerUpdate(int ClientID, byte[] PacketData)
        {
            l.o("Handle Player Update");

            //Extract the new position data from the network packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string CharacterName = PacketReader.ReadString();
            Vector3 CharacterPosition = new Vector3(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            Quaternion CharacterRotation = new Quaternion(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            PacketReader.Dispose();

            //Update the position in the server physics scene
            Client Client = ClientManager.Clients[ClientID];
            //Make sure there's a server collider for this client to actually update
            if (Client.ServerCollider != null)
                Client.ServerCollider.Position = CharacterPosition;

            //Store the players new information in their assigned client object
            ClientManager.Clients[ClientID].CharacterPosition = CharacterPosition;

            //Send this new data out to all of the other connected clients
            List<Client> OtherClients = ClientManager.GetOtherClients(ClientID);
            foreach (Client OtherClient in OtherClients)
                PacketSenderLogic.SendPlayerUpdatePosition(OtherClient.ClientID, CharacterName, CharacterPosition, CharacterRotation);
        }

        public static void HandlePlayerMeleeAttack(int ClientID, byte[] PacketData)
        {
            l.o("handle player melee attack");
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            Vector3 AttackPosition = new Vector3(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            Vector3 AttackScale = new Vector3(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            Quaternion AttackRotation = new Quaternion(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            PacketReader.Dispose();
            //Check if this box collides with any enemies, damage them
            BoxShape A = new BoxShape(AttackScale.X, AttackScale.Y, AttackScale.Z);
            RigidTransform ATransform = new RigidTransform(AttackPosition);
            for(int i = 0; i < EntityManager.ActiveEntities.Count; i++)
            {
                BoxShape B = new BoxShape(EntityManager.ActiveEntities[i].Scale.X, EntityManager.ActiveEntities[i].Scale.Y, EntityManager.ActiveEntities[i].Scale.Z);
                RigidTransform BTransform = new RigidTransform(EntityManager.ActiveEntities[i].Position);
                bool Colliding = BoxBoxCollider.AreBoxesColliding(A, B, ref ATransform, ref BTransform);
                if (Colliding)
                {
                    l.o("Attack Collided!");
                    PacketSenderLogic.UpdateEntityHealth(EntityManager.ActiveEntities[i].ID, EntityManager.ActiveEntities[i].HealthPoints--);
                }
                else
                    l.o("Attack Missed");
            }
        }

        //Handles when a player has disconnected from the server
        public static void HandlePlayerDisconnect(int ClientID, byte[] PacketData)
        {
            l.o("Handle Player Disconnect");

            //read the packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            PacketReader.Dispose();

            //Find this clients info
            Client DisconnectedClient = ClientManager.Clients[ClientID];
            string Account = DisconnectedClient.AccountName;
            string Character = DisconnectedClient.CurrentCharacterName;
            Vector3 Position = DisconnectedClient.CharacterPosition;

            //Remove them from the physics scene
            Globals.space.Remove(DisconnectedClient.ServerCollider);

            //Open up the database and backup their characters world position values
            Globals.database.SaveCharacterLocation(Character, Position);
            //Remove them from the client list, then tell all the other active players they have logged out
            ClientManager.Clients.Remove(ClientID);
            List<Client> ActiveClients = ClientManager.GetAllActiveClients();
            PacketSenderLogic.SendListRemoveOtherPlayer(ActiveClients, Character);
            l.o(Account + ":" + Character + " has disconnected");
        }
    }
}
