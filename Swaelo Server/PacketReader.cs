using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADODB;

public enum ClientPacketType
{
    ConsoleMessage = 1, //client sent a message to be displayed in the console log window
    PlayerMessage = 2,  //player sent a message to be spread to other clients chat windows

    RegisterRequest = 3,    //client wants to register a new account
    LoginRequest = 4,   //client wants to log into their account
    CreateCharacterRequest = 5, //client wants to create a new character
    EnterWorldRequest = 6,  //client wants to enter into the game world with their selected character
    GetCharacterDataRequest = 7,    //client wants info about all their created characters

    PlayerUpdatePosition = 8    //client sent us their updated position data to be spread to everyone else
}

namespace Swaelo_Server
{
    class PacketReader
    {
        public delegate void Packet(int index, byte[] data);
        public static Dictionary<int, Packet> Packets = new Dictionary<int, Packet>();

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
            Console.WriteLine("a client has sent us their chat message");
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string CharacterName = PacketReader.ReadString();
            string Message = PacketReader.ReadString();
            foreach (var OtherClient in ClientManager.Clients)
            {
                int ID = OtherClient.Key;
                Client client = OtherClient.Value;
                //Dont tell the client about themself
                if (ID == ClientID)
                    continue;
                //Send the message to all the other clients
                PacketSender.SendPlayerMessage(ID, CharacterName, Message);
            }
        }

        //Gets a request from a client to register a new account <int:PacketType, string:Username, string:Password>
        public static void HandleRegisterRequest(int ClientID, byte[] PacketData)
        {
            Console.WriteLine("trying to register a new account");
            //Extract the account credentials from the packet data
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            string Name = PacketReader.ReadString();
            string Pass = PacketReader.ReadString();
            PacketReader.Dispose();
            //Make sure the username provided doesnt contain any banned characters
            if(!IsValidUsername(Name))
            {
                Console.WriteLine("Requested username " + Name + " contains banned characters, registration request denied");
                PacketSender.SendRegisterReply(ClientID, false, "Username contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Make sure the password provided doesnt contain any banned characters either
            if(!IsValidUsername(Pass))
            {
                Console.WriteLine("Requested password " + Pass + " contains banned characters, registration request denied");
                PacketSender.SendRegisterReply(ClientID, false, "Password contains banned characters, only letters, numbers, dashes(-), periods(.) and underscores(_) are allowed");
                return;
            }
            //Check that this account doesnt already exist
            bool AccountExists = Globals.database.DoesAccountExist(ClientID, Name);
            if (AccountExists)
            {//if the account already exists we need to tell the user the name is already taken
                Console.WriteLine(Name + " account already exists");
                PacketSender.SendRegisterReply(ClientID, false, "This account name has already been taken");
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
                PacketSender.SendRegisterReply(ClientID, true, "Account has been created");
            }
        }

        //Gets a request from a client to log into an account <int:PacketType, string:Username, string:Password>
        public static void HandleLoginRequest(int ClientID, byte[] PacketData)
        {
            Console.WriteLine("client is trying to log into an account");
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
                PacketSender.SendLoginReply(ClientID, false, "That account does not exist");
                return;
            }
            //Make sure someone else isnt already logged into this account
            foreach (var OtherClient in ClientManager.Clients)
            {
                //Grab this clients info out of the dictionary
                int ID = OtherClient.Key;
                Client ExternalClient = OtherClient.Value;
                //Find out what account they are logged into
                string AccountName = ExternalClient.AccountName;
                //If this is the same account the players trying to log into, deny their request
                if(Name == AccountName)
                {
                    PacketSender.SendLoginReply(ClientID, false, "Someone is already logged into that account");
                    return;
                }
            }
            //Now check they have given the correct password
            bool PasswordMatches = Globals.database.DoesPasswordMatch(ClientID, Name, Pass);
            //If the password is wrong they cant log into the account
            if(!PasswordMatches)
            {
                PacketSender.SendLoginReply(ClientID, false, "The password was incorrect");
                return;
            }

            PacketSender.SendLoginReply(ClientID, true, "Login success");
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
            if(CharacterExists)
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
            //Extract the data about the character wanting to enter into the game world
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            int PacketType = PacketReader.ReadInteger();
            CharacterData Data = new CharacterData();
            Data.Account = PacketReader.ReadString();
            Data.Name = PacketReader.ReadString();
            Data.IsMale = PacketReader.ReadInteger() == 1;
            Data.Position = new Vector3(PacketReader.ReadFloat(), PacketReader.ReadFloat(), PacketReader.ReadFloat());
            PacketReader.Dispose();

            //Tell this player to enter into the world with their desired character and save their info in their client class
            PacketSender.SendPlayerEnterWorld(ClientID, Data);
            ClientManager.Clients[ClientID].AccountName = Data.Account;
            ClientManager.Clients[ClientID].InGame = true;
            ClientManager.Clients[ClientID].CharacterPosition = new Vector3(0, 0, 0);
            ClientManager.Clients[ClientID].CurrentCharacterName = Data.Name;

            //Get a list of all the other active clients playing in the game right now
            List<Client> OtherActiveClients = ClientManager.GetActiveClientsExceptFor(ClientID);
            //Tell the new client to spawn in all of these clients characters into the world too
            PacketSender.SendSpawnOtherPlayers(ClientID, OtherActiveClients);
            //All of these others clients also need to be told to spawn this new clients character into their world too
            foreach(Client OtherClient in OtherActiveClients) { PacketSender.SendSpawnOtherPlayer(OtherClient.ClientID, Data.Account, Data); }
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
            //start the packet reader and give it the packet data information
            ByteBuffer.ByteBuffer PacketReader = new ByteBuffer.ByteBuffer();
            PacketReader.WriteBytes(PacketData);
            //extract all the information from the packet
            int PacketType = PacketReader.ReadInteger();
            string CharacterName = PacketReader.ReadString();
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
                PacketSender.SendPlayerUpdatePosition(ID, CharacterName, NewPosition);
            }
        }
    }
}
