using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;

public enum ClientPacketType
{
    AccountRegistrationRequest = 1,
    AccountLoginRequest = 2,
    CharacterCreationRequest = 3,
    CharacterDataRequest = 4,
    EnterWorldRequest = 5,
    ActiveEntityRequest = 6,
    NewPlayerReady = 7,
    PlayerChatMessage = 8,
    PlayerUpdate = 9,
    DisconnectionNotice = 10,
    ConnectionCheckReply = 11
}
public enum ServerPacketType
{
    AccountRegistrationReply = 1,
    AccountLoginReply = 2,
    CharacterCreationReply = 3,
    CharacterDataReply = 4,
    ActivePlayerList = 5,
    ActiveEntityList = 6,
    EntityUpdates = 7,
    SpawnPlayer = 8,
    PlayerChatMessage = 9,
    PlayerUpdate = 10,
    RemovePlayer = 11,
    ConnectionCheckRequest = 12
}

namespace Server.Networking
{
    public static class PacketManager
    {
        public delegate void Packet(int index, byte[] data);
        public static Dictionary<int, Packet> Packets = new Dictionary<int, Packet>();

        public static void RegisterPacketReaders()
        {
            Packets.Add((int)ClientPacketType.AccountRegistrationRequest, HandleAccountRegistrationRequest);
            Packets.Add((int)ClientPacketType.AccountLoginRequest, HandleAccountLoginRequest);
            Packets.Add((int)ClientPacketType.CharacterCreationRequest, HandleCharacterCreationRequest);
            Packets.Add((int)ClientPacketType.CharacterDataRequest, HandleCharacterDataRequest);
            Packets.Add((int)ClientPacketType.EnterWorldRequest, HandleEnterWorldRequest);
            Packets.Add((int)ClientPacketType.ActiveEntityRequest, HandleActiveEntityRequest);
            Packets.Add((int)ClientPacketType.NewPlayerReady, HandleNewPlayerReady);
            Packets.Add((int)ClientPacketType.PlayerChatMessage, HandlePlayerChatMessage);
            Packets.Add((int)ClientPacketType.PlayerUpdate, HandlePlayerUpdate);
            Packets.Add((int)ClientPacketType.DisconnectionNotice, HandlePlayerDisconnect);
            Packets.Add((int)ClientPacketType.ConnectionCheckReply, HandleConnectionCheckReply);
        }

        public static void ReadClientPacket(int ClientID, byte[] PacketBuffer)
        {
            //Find out what type of packet has been sent to us from the user
            PacketReader Reader = new PacketReader(PacketBuffer);
            int PacketType = Reader.ReadInt();
            //Invoke whatever function is registered to handle this type of packet
            Packet Packet;
            if (Packets.TryGetValue(PacketType, out Packet))
                Packet.Invoke(ClientID, PacketBuffer);
        }

        //Allows users to register new accounts into the database
        public static void HandleAccountRegistrationRequest(int ClientID, byte[] PacketData)
        {
            //Extract the desired account credentials from the packet data
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string AccountName = Reader.ReadString();
            string AccountPass = Reader.ReadString();

            //Reject this request if the desired username or password contain any banned characters
            if (!Data.StringChecker.IsValidUsername(AccountName))
            {
                SendAccountRegistrationReply(ClientID, false, "username is banned");
                return;
            }
            if (!Data.StringChecker.IsValidUsername(AccountPass))
            {
                SendAccountRegistrationReply(ClientID, false, "password is banned");
                return;
            }

            //Reject this request if this username is already being used
            if(!Data.Database.IsAccountNameAvailable(AccountName))
            {
                SendAccountRegistrationReply(ClientID, false, "username is already taken");
                return;
            }

            //Account credentials are valid and the username is available, register this into the database
            Data.Database.RegisterNewAccount(AccountName, AccountPass);
            SendAccountRegistrationReply(ClientID, true, "Account registered successfully");
            l.og(AccountName + " registered as a new account");
        }
        public static void SendAccountRegistrationReply(int ClientID, bool RegistrationSuccess, string ReplyMessage)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.AccountRegistrationReply);
            Writer.WriteInt(RegistrationSuccess ? 1 : 0);
            Writer.WriteString(ReplyMessage);
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Allows users to login to their accounts
        public static void HandleAccountLoginRequest(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);

            //Extract the provided account credentials from the network packet
            int PacketType = Reader.ReadInt();
            string AccountName = Reader.ReadString();
            string AccountPass = Reader.ReadString();

            //Make sure the user isnt trying to log into an account which doesnt exist
            if(Data.Database.IsAccountNameAvailable(AccountName))
            {
                SendAccountLoginReply(ClientID, false, "account does not exist");
                return;
            }

            //Make sure someone else isnt already logged into this account
            if(Networking.ConnectionManager.IsAccountLoggedIn(AccountName))
            {
                SendAccountLoginReply(ClientID, false, "this account is already logged in");
                return;
            }

            //Check that the user has provided the correct password for the account
            if(!Data.Database.IsPasswordCorrect(AccountName, AccountPass))
            {
                SendAccountLoginReply(ClientID, false, "password was incorrect");
                return;
            }

            //After all checks have passed we will finally allow this user to log into their account
            ConnectionManager.ClientConnections[ClientID].AccountName = AccountName;
            SendAccountLoginReply(ClientID, true, "login success");
            l.og(AccountName + " logged in");
        }
        public static void SendAccountLoginReply(int ClientID, bool LoginSuccess, string ReplyMessage)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.AccountLoginReply);
            Writer.WriteInt(LoginSuccess ? 1 : 0);
            Writer.WriteString(ReplyMessage);
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Allows users to create new characters once logged into their accounts
        public static void HandleCharacterCreationRequest(int ClientID, byte[] PacketData)
        {
            //Extract the desired character data from the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string AccountName = Reader.ReadString();
            string CharacterName = Reader.ReadString();
            bool IsMale = Reader.ReadInt() == 1;

            //Make sure this character name hasnt already been taken by someone else
            if(!Data.Database.IsCharacterNameAvailable(CharacterName))
            {
                SendCharacterCreationReply(ClientID, false, "character name already taken");
                return;
            }

            //If the name is available create that for them
            Data.Database.SaveNewCharacter(AccountName, CharacterName, IsMale);
            SendCharacterCreationReply(ClientID, true, "character created");
            l.og(AccountName + " created " + CharacterName + " as a new character");
        }
        public static void SendCharacterCreationReply(int ClientID, bool CreationSuccess, string ReplyMessage)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.CharacterCreationReply);
            Writer.WriteInt(CreationSuccess ? 1 : 0);
            Writer.WriteString(ReplyMessage);
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Sends a user data about all the created characters after they have logged into their account
        public static void HandleCharacterDataRequest(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string AccountName = Reader.ReadString();
            SendCharacterData(ClientID, AccountName);
        }
        public static void SendCharacterData(int ClientID, string AccountName)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.CharacterDataReply);
            int CharacterCount = Data.Database.GetCharacterCount(AccountName);
            Writer.WriteInt(CharacterCount);
            //Loop through for each character registered under this users account
            for(int i = 0; i < CharacterCount; i++)
            {
                string CharacterName = Data.Database.GetCharacterName(AccountName, i + 1);
                Data.CharacterData CharacterData = Data.Database.GetCharacterData(CharacterName);
                //Write each of their characters info into the packet
                Writer.WriteString(CharacterData.Account);
                Writer.WriteVector3(CharacterData.Position);
                Writer.WriteString(CharacterData.Name);
                Writer.WriteInt(CharacterData.Experience);
                Writer.WriteInt(CharacterData.ExperienceToLevel);
                Writer.WriteInt(CharacterData.Level);
                Writer.WriteInt(CharacterData.IsMale ? 1 : 0);
            }

            //Send all the data off to the client
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Once a user wants to enter the game world, makes sure all the relevant information is sent to them before allowing them to
        public static void HandleEnterWorldRequest(int ClientID, byte[] PacketData)
        {
            //When sending an enter world request, the user tells us all the relevant info regarding which character they want to use and where they are
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            //Extract all the information from the packet and save it into this clients class
            ClientConnection Client = ConnectionManager.ClientConnections[ClientID];
            Client.AccountName = Reader.ReadString();
            Client.CharacterName = Reader.ReadString();
            Client.CharacterPosition = Reader.ReadVector3();
            SendActivePlayerList(ClientID);
        }
        //Tells a client where all the other players are in the world to have them spawned in before they can enter into the world
        public static void SendActivePlayerList(int ClientID)
        {
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClientsExceptFor(ClientID);
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.ActivePlayerList);
            Writer.WriteInt(OtherClients.Count);
            foreach(ClientConnection OtherClient in OtherClients)
            {
                Writer.WriteString(OtherClient.CharacterName);
                Writer.WriteVector3(OtherClient.CharacterPosition);
            }
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }
        //After recieving the active player list, the new client will then request info about all the active entities in the game
        public static void HandleActiveEntityRequest(int ClientID, byte[] PacketData)
        {
            SendActiveEntities(ClientID);
        }
        //Tells a client where all the active entities are in the world to have them spawned in before they are allowed to enter into the game world
        public static void SendActiveEntities(int ClientID)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.ActiveEntityList);
            List<Entities.BaseEntity> EntityList = Entities.EntityManager.ActiveEntities;
            Writer.WriteInt(EntityList.Count);
            foreach(Entities.BaseEntity Entity in EntityList)
            {
                Writer.WriteString(Entity.Type);
                Writer.WriteString(Entity.ID);
                Writer.WriteVector3(Entity.Position);
            }
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
            l.og("telling new client about " + EntityList.Count + " active entities");
        }
        //After recieving the active entity list, the new client will tell us they are ready to enter into the game world
        public static void HandleNewPlayerReady(int ClientID, byte[] PacketData)
        {
            ClientConnection NewClient = ConnectionManager.ClientConnections[ClientID];
            NewClient.InGame = true;
            NewClient.ServerCollider = new BEPUphysics.Entities.Prefabs.Sphere(NewClient.CharacterPosition, 1);
            Physics.WorldSimulator.Space.Add(NewClient.ServerCollider);
            Rendering.GameWindow.CurrentWindow.ModelDrawer.Add(NewClient.ServerCollider);
            //All the other players need to be told about the new player
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClientsExceptFor(ClientID);
            l.og(ClientID + " has entered the game");
        }
        
        //Goes through a list of clients, and updates them all on all the entities in the given list
        public static void SendListEntityUpdates(List<ClientConnection> ClientList, List<Entities.BaseEntity> EntityList)
        {
            //Define the network packet that will be sent to everyone
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.EntityUpdates);
            Writer.WriteInt(EntityList.Count);
            foreach(var Entity in EntityList)
            {
                //ID
                Writer.WriteString(Entity.ID);
                //Position
                Writer.WriteVector3(Entity.Position);
                //Rotation
                Writer.WriteQuaternion(Entity.Rotation);
            }

            //Send this packet out to all the clients in the given list
            foreach (ClientConnection Client in ClientList)
                ConnectionManager.SendPacketTo(Client.ID, Writer.ToArray());
        }
        //Goes thourhg a list of clients, and has them all spawn in a newly connected client to their game world
        public static void SendListSpawnOther(List<ClientConnection> Clients, string CharacterName, Vector3 CharacterPosition)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.SpawnPlayer);
            Writer.WriteString(CharacterName);
            Writer.WriteVector3(CharacterPosition);
            foreach (ClientConnection Client in Clients)
                ConnectionManager.SendPacketTo(Client.ID, Writer.ToArray());
        }

        //Recieves a chat message from a player
        public static void HandlePlayerChatMessage(int ClientID, byte[] PacketData)
        {
            //Get the message contents from the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string MessageSender = Reader.ReadString();
            string MessageContent = Reader.ReadString();
            //Now send this to everyone else
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClientsExceptFor(ClientID);
            SendListPlayerChatMessage(OtherClients, MessageSender, MessageContent);
        }
        //Shares a players chat message to all the other clients in the game
        public static void SendListPlayerChatMessage(List<ClientConnection> Clients, string Sender, string Content)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerChatMessage);
            Writer.WriteString(Sender);
            Writer.WriteString(Content);
            foreach (ClientConnection Client in Clients)
                ConnectionManager.SendPacketTo(Client.ID, Writer.ToArray());
        }

        //Recieves a players updated position/rotation values
        public static void HandlePlayerUpdate(int ClientID, byte[] PacketData)
        {
            //Extract the updated player information from the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string CharacterName = Reader.ReadString();
            Vector3 CharacterPosition = Reader.ReadVector3();
            Quaternion CharacterRotation = Reader.ReadQuaternion();

            //Update this players position in the server
            ClientConnection Client = ConnectionManager.ClientConnections[ClientID];
            Client.ServerCollider.Position = CharacterPosition;
            Client.CharacterPosition = CharacterPosition;

            //Share this updated data out to all the other connected clients
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClientsExceptFor(ClientID);
            SendListPlayerUpdate(OtherClients, CharacterName, CharacterPosition, CharacterRotation);
        }
        //Shares a players updated position/rotation values to all the other players
        public static void SendListPlayerUpdate(List<ClientConnection> Clients, string CharacterName, Vector3 CharacterPosition, Quaternion CharacterRotation)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerUpdate);
            Writer.WriteString(CharacterName);
            Writer.WriteVector3(CharacterPosition);
            Writer.WriteQuaternion(CharacterRotation);
            foreach (ClientConnection Client in Clients)
                ConnectionManager.SendPacketTo(Client.ID, Writer.ToArray());
        }

        //Removes a player from the game once they have stopped playing
        public static void HandlePlayerDisconnect(int ClientID, byte[] PacketData)
        {
            //Find this clients information
            ClientConnection Client = ConnectionManager.ClientConnections[ClientID];
            string Account = Client.AccountName;
            string Character = Client.CharacterName;
            Vector3 Position = Client.CharacterPosition;

            //Remove them from the scene
            Physics.WorldSimulator.Space.Remove(Client.ServerCollider);
            Rendering.GameWindow.CurrentWindow.ModelDrawer.Remove(Client.ServerCollider);

            //Tell any enemies targetting the client to drop them as their target
            Entities.EntityManager.DropTarget(Client);

            //Backup their character data into the database
            Data.Database.SaveCharacterLocation(Character, Position);

            //Remove them from the clients list and tell all other players to remove them from their game worlds
            ConnectionManager.ClientConnections.Remove(ClientID);
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClients();
            SendListRemoveOther(OtherClients, Character);
            l.og(Account + " disconnected");
        }
        public static void SendListRemoveOther(List<ClientConnection> Clients, string CharacterName)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.RemovePlayer);
            Writer.WriteString(CharacterName);
            foreach (ClientConnection Client in Clients)
                ConnectionManager.SendPacketTo(Client.ID, Writer.ToArray());
        }

        //Takes note that a player has responded to the current connection timeout checker
        public static void HandleConnectionCheckReply(int ClientID, byte[] PacketData)
        {
            
        }
    }
}
