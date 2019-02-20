using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ADODB;
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
    PlayerDisconnectNotice = 9,  //tell everyone else they stopped playing
    AccountLogoutNotice = 10    //let the server know we have logged out of this user account
}

namespace Swaelo_Server
{
    class PacketReaderr
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

        private static bool IsValidUsername(string username)
        {
            Console.WriteLine("checking if " + username + " is a valid username");
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
            //Console.WriteLine("getting a message from one of the clients");
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
            //Read the message info from the network packet
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string CharacterName = PacketReader.ReadString();
            string Message = PacketReader.ReadString();
            PacketReader.Dispose();
            //Send this message to all game client, except for the client who sent it
            List<Client> OtherClients = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketSender.SendPlayersMessage(OtherClients, CharacterName, Message);
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
            //Make sure the username provided doesnt contain any banned characters
            if (!IsValidUsername(Name))
            {
                PacketSender.SendRegisterReply(ClientID, false, "Username contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Make sure the password provided doesnt contain any banned characters either
            if (!IsValidUsername(Pass))
            {
                PacketSender.SendRegisterReply(ClientID, false, "Password contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Check that this account doesnt already exist
            bool AccountExists = Globals.database.DoesAccountExist(ClientID, Name);
            if (AccountExists)
            {//if the account already exists we need to tell the user the name is already taken
                PacketSender.SendRegisterReply(ClientID, false, "This account name has already been taken");
                return;
            }
            else
            {//Otherwise, we need to register the account into the database and tell the user its okay
                Console.WriteLine(Name + " account is free to be created");
                //Get the table of accounts from the database and write in the info for the new account
                string Query = "INSERT INTO accounts (Username,Password, CharactersCreated) VALUES('" + Name + "','" + Pass + "','" + 0 + "')";
                MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                PacketSender.SendRegisterReply(ClientID, true, "Account has been created");
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
            if (!AccountExists)
            {
                PacketSender.SendLoginReply(ClientID, false, "That account does not exist");
                return;
            }
            //Make sure someone else isnt already logged into this account
            if(ClientManager.IsAccountLoggedIn(Name))
            {
                PacketSender.SendLoginReply(ClientID, false, "Someone is already logged into that account.");
                return;
            }
            //Now check they have given the correct password
            bool PasswordMatches = Globals.database.DoesPasswordMatch(ClientID, Name, Pass);
            //If the password is wrong they cant log into the account
            if (!PasswordMatches)
            {
                PacketSender.SendLoginReply(ClientID, false, "The password was incorrect");
                return;
            }
           
            Count++;
            MaxOn++;
            Console.WriteLine(Name + " has logged in");
            Console.Title = "MMO" + " -- Players online: " + Count + " / Max online: " + MaxOn + "";
            ClientManager.Clients[ClientID].AccountName = Name;
            PacketSender.SendLoginReply(ClientID, true, "Login success");

            
        }

        public static void HandleAccountLogout(int ClientID, byte[] PacketData)
        {
            //Get this clients account information
            Client Client = ClientManager.Clients[ClientID];
            //Announce their logging out
            Console.WriteLine(Client.AccountName + " has logged out");
            Client.AccountName = "";
            Client.CurrentCharacterName = "";
        }

        //Trys to create a new character for the user and tells them how it went
        public static void HandleCreateCharacterRequest(int ClientID, byte[] PacketData)
        {
            //Open the packet and extract all the relevant information, then close it
            Console.WriteLine("checking if we can create a new character for this player");
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string AccountName = PacketReader.ReadString();
            string CharacterName = PacketReader.ReadString();
            bool IsMale = PacketReader.ReadInteger() == 1;
            Console.WriteLine(AccountName + " wants to register " + CharacterName + " as a new character");
            PacketReader.Dispose();
            //Query the database to check if this character name has already been taken by someone else
            bool CharacterExists = Globals.database.DoesCharacterExist(ClientID, CharacterName);
            //If this character already exists, send the reply message telling them the name is already taken
            if (CharacterExists)
            {
                PacketSender.SendCreateCharacterReply(ClientID, false, "This character name is already taken");
                return;
            }
            //Otherwise, we need to create this new character and save it into the database registered under this users account
            Globals.database.RegisterNewCharacter(AccountName, CharacterName, IsMale);
            PacketSender.SendCreateCharacterReply(ClientID, true, "Character Created");
        }
        //client wants to enter into the game world with their selected character
        public static void HandleEnterWorldRequest(int ClientID, byte[] PacketData)
        {
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
            Console.WriteLine(Client.AccountName + ":" + Client.CurrentCharacterName + " has entered the world");
            
            PacketSender.SendPlayerEnterWorld(ClientID);

            //Give information to the new player about any entities that are active in the world
            PacketSender.SendActiveEntities(ClientID);

            //Spawn a representation of this player in the server physics scene
            Client.ServerCollider = new Sphere(Client.CharacterPosition, 1);
            Globals.space.Add(Client.ServerCollider);
            Globals.game.ModelDrawer.Add(Client.ServerCollider);

            //Any other players who are already playing the game need to be told to spawn this client
            List<Client> OtherActivePlayers = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketSender.SendListSpawnOther(OtherActivePlayers, Client.CurrentCharacterName, Client.CharacterPosition);
        }

        //Sends to the client all the infor for any characters they have created so far
        public static void HandleGetCharacterDataRequest(int ClientID, byte[] PacketData)
        {
            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Username = PacketReader.ReadString();
            PacketReader.Dispose();
            PacketSender.SendCharacterData(ClientID, Username);
        }

        //Gets updated character information data from one of the connect clients, this needs to be sent out to everyone else so they know where that character is at
        public static void HandlePlayerUpdate(int ClientID, byte[] PacketData)
        {
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
            Client.ServerCollider.Position = CharacterPosition;

            //Store the players new information in their assigned client object
            ClientManager.Clients[ClientID].CharacterPosition = CharacterPosition;

            //Send this new data out to all of the other connected clients
            List<Client> OtherClients = ClientManager.GetOtherClients(ClientID);
            foreach (Client OtherClient in OtherClients)
                PacketSender.SendPlayerUpdatePosition(OtherClient.ClientID, CharacterName, CharacterPosition, CharacterRotation);
        }

        public static void HandlePlayerDisconnect(int ClientID, byte[] PacketData)
        {
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

            //Open up the database and backup their characters world position

            string UpdatePos = "UPDATE characters SET XPosition='" + Position.X + "', YPosition='" + Position.Y + "', ZPosition='" + Position.Z+ "' WHERE CharacterName='" + Character + "'";
            MySqlCommand cmd = new MySqlCommand(UpdatePos, MySQL.mySQLSettings.connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            //Remove them from the client list, then tell all the other active players they have logged out
            ClientManager.Clients.Remove(ClientID);
            List<Client> ActiveClients = ClientManager.GetAllActiveClients();
            PacketSender.SendListRemoveOtherPlayer(ActiveClients, Character);
            Console.WriteLine(Account + ":" + Character + " has disconnected");
            Count--;
            Console.Title = "MMO" + " -- Players online: " + Count + " / Max online: " + MaxOn + "";
        }
    }
}
