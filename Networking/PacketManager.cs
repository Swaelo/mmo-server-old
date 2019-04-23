// ================================================================================================================================
// File:        PacketManager.cs
// Description: Defines all the functions used for sending and recieving data through the network to player clients during runtime
// ================================================================================================================================

using System.Collections.Generic;
using Server.Entities;
using Server.Data;
using Server.Items;
using System.Threading;
using BEPUutilities;

public enum ClientPacketType
{
    AccountRegistrationRequest = 1,
    AccountLoginRequest = 2,
    CharacterCreationRequest = 3,
    CharacterDataRequest = 4,
    EnterWorldRequest = 5,
    ActiveEntityRequest = 6,
    ActiveItemRequest = 7,
    NewPlayerReady = 8,
    PlayerChatMessage = 9,
    PlayerUpdate = 10,
    PlayerAttack = 11,
    DisconnectionNotice = 12,
    ConnectionCheckReply = 13,

    PlayerInventoryRequest = 14,
    PlayerEquipmentRequest = 15,
    PlayerTakeItemRequest = 16,
    RemoveInventoryItem = 17,
    EquipInventoryItem = 18,
    UnequipItem = 19
}

public enum ServerPacketType
{
    AccountRegistrationReply = 1,
    AccountLoginReply = 2,
    CharacterCreationReply = 3,
    CharacterDataReply = 4,

    ActivePlayerList = 5,
    ActiveEntityList = 6,
    ActiveItemList = 7,
    SpawnItem = 8,
    RemoveItem = 9,

    EntityUpdates = 10,
    RemoveEntities = 11,

    PlayerChatMessage = 12,
    PlayerUpdate = 13,
    SpawnPlayer = 14,
    RemovePlayer = 15,

    PlayerInventoryItems = 16,
    PlayerEquipmentItems = 17,
    PlayerInventoryUpdate = 18
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
            Packets.Add((int)ClientPacketType.ActiveItemRequest, HandleActiveItemRequest);
            Packets.Add((int)ClientPacketType.NewPlayerReady, HandleNewPlayerReady);
            Packets.Add((int)ClientPacketType.PlayerChatMessage, HandlePlayerChatMessage);
            Packets.Add((int)ClientPacketType.PlayerUpdate, HandlePlayerUpdate);
            Packets.Add((int)ClientPacketType.PlayerAttack, HandlePlayerAttack);
            Packets.Add((int)ClientPacketType.DisconnectionNotice, HandlePlayerDisconnect);
            Packets.Add((int)ClientPacketType.ConnectionCheckReply, HandleConnectionCheckReply);
            Packets.Add((int)ClientPacketType.PlayerInventoryRequest, HandlePlayerInventoryRequest);
            Packets.Add((int)ClientPacketType.PlayerEquipmentRequest, HandlePlayerEquipmentRequest);
            Packets.Add((int)ClientPacketType.PlayerTakeItemRequest, HandlePlayerTakeItem);
            Packets.Add((int)ClientPacketType.RemoveInventoryItem, HandleRemoveInventoryItem);
            Packets.Add((int)ClientPacketType.EquipInventoryItem, HandleEquipInventoryItem);
            Packets.Add((int)ClientPacketType.UnequipItem, HandleUnequipItem);
        }

        public static void ReadClientPacket(int ClientID, byte[] PacketBuffer)
        {
            //Find out what type of packet has been sent to us from the user
            PacketReader Reader = new PacketReader(PacketBuffer);
            int PacketType = Reader.ReadInt();
            //Invoke whatever function is registered to handle this type of packet
            if (Packets.TryGetValue(PacketType, out Packet Packet))
                Packet.Invoke(ClientID, PacketBuffer);
        }

        //Moves an item from the players equipment to their inventory
        public static void HandleUnequipItem(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            int ItemID = Reader.ReadInt();
            EquipmentSlot ItemSlot = (EquipmentSlot)Reader.ReadInt();

            //Remove the item from the players equipment
            Database.UnequipPlayerItem(PlayerName, ItemSlot);
            //Add the item to the players inventory 
            Database.GivePlayerItem(PlayerName, ItemID);

            //Send the players updated inventory state to them
            SendPlayerInventoryUpdate(ClientID, PlayerName);
        }

        //Moves an item from the players inventory to their equipment screen
        public static void HandleEquipInventoryItem(int ClientID, byte[] PacketData)
        {
            //Read the information from the packet data
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();
            int ItemNumber = Reader.ReadInt();
            EquipmentSlot EquipSlot = (EquipmentSlot)Reader.ReadInt();

            //Remove the item from the players inventory
            Database.RemovePlayerItem(PlayerName, BagSlot);
            //Add the item into the players equipment screen
            Database.EquipPlayerItem(PlayerName, ItemNumber, EquipSlot);
        }

        //Removes an item from a players inventory
        public static void HandleRemoveInventoryItem(int ClientID, byte[] PacketData)
        {
            //Read the information from the packet data
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();

            //Remove the item from the players inventory
            Database.RemovePlayerItem(PlayerName, BagSlot);

            //Update the player on their new inventory contents
            SendPlayerInventoryUpdate(ClientID, PlayerName);
        }
        
        //Sends a player up to date information regarding what they have in their inventory
        private static void SendPlayerInventoryUpdate(int ClientID, string PlayerName)
        {
            //Get the current contents of the players inventory
            List<int> InventoryItems = Database.GetPlayersInventory(PlayerName);

            //Store all the item data in a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerInventoryUpdate);
            foreach (int ID in InventoryItems)
                Writer.WriteInt(ID);

            //Send the packet
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //User is trying to pick up an item from the groundf
        public static void HandlePlayerTakeItem(int ClientID, byte[] PacketData)
        {
            //Read the information from the packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            int ItemNumber = Reader.ReadInt();
            int ItemID = Reader.ReadInt();
            
            //If the item is still available for pick and the player has item in their bags then we let them take it
            if(ItemManager.CanTakeItem(ItemID) && !Database.IsBagFull(PlayerName))
            {
                //Add this item into the players inventory
                Database.GivePlayerItem(PlayerName, ItemNumber);
                //Remove this item from the game world
                ItemManager.RemoveItem(ItemID);
                Thread.Sleep(250);

                SendPlayerInventoryUpdate(ClientID, PlayerName);
            }
        }

        //User is requesting for a list of items they have equipped on their character
        public static void HandlePlayerEquipmentRequest(int ClientID, byte[] PacketData)
        {
            //Read from the packet data which characters equipment we are looking for
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            //Load all of that players equipment from the database
            List<int> EquipmentItems = Database.GetPlayersEquipment(PlayerName);
            //Write each equipment item into a new packet and send that back to the client who requested it
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerEquipmentItems);
            foreach (int ItemNumber in EquipmentItems)
                Writer.WriteInt(ItemNumber);
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //User is requesting for a list of all items in their inventory
        public static void HandlePlayerInventoryRequest(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string CharacterName = Reader.ReadString();

            //Check what items are in this players inventory
            List<int> InventoryItems = Database.GetPlayersInventory(CharacterName);

            //Write the ID of each item in a new packet and return it to the user who requested it
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerInventoryItems);
            foreach (int ItemID in InventoryItems)
                Writer.WriteInt(ItemID);
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
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
            int CharacterCount = Database.GetCharacterCount(AccountName);
            Writer.WriteInt(CharacterCount);

            //Loop through for each character registered under this users account
            for(int i = 0; i < CharacterCount; i++)
            {
                //Get each characters name from the database
                string CharacterName = Database.GetCharacterName(AccountName, i + 1);
                //Then load that characters data
                CharacterData Data = Database.GetCharacterData(CharacterName);

                //Write all the characters data into the network packet
                Writer.WriteString(Data.Account);   //Which account this character belongs to
                Writer.WriteString(Data.Name);  //This characters ingame name
                Writer.WriteVector3(Data.Position); //This characters position in the game world
                Writer.WriteInt(Data.Level);    //This characters current level
                Writer.WriteInt(Data.IsMale ? 1 : 0);  //The characters gender
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
            Client.CharacterPosition = Maths.VectorTranslate.ConvertVector(Reader.ReadVector3());
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
        }//After recieving the active player list, the new client will then request info about all the active entities in the game
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
                Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(Entity.Position));
                Writer.WriteInt(Entity.HealthPoints);
            }
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }
        //After recieving the active entity list, the new client will then request to be sent the list of items active in the game world
        public static void HandleActiveItemRequest(int ClientID, byte[] PacketData)
        {
            SendActiveItems(ClientID);
        }
        //Tells a client where all the active items are in the world to have them spawned in before they can start playing
        public static void SendActiveItems(int ClientID)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.ActiveItemList);
            List<Item> ItemList = ItemManager.ActiveItems;
            Writer.WriteInt(ItemList.Count);
            foreach (Item Item in ItemList)
            {
                Writer.WriteString(Item.Name);
                Writer.WriteString(Item.Type);
                Writer.WriteInt(Item.ID);
                Writer.WriteVector3(Item.Collider.Position);
            }
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //After recieving the active entity list, the new client will tell us they are ready to enter into the game world
        public static void HandleNewPlayerReady(int ClientID, byte[] PacketData)
        {
            ClientConnection NewClient = ConnectionManager.ClientConnections[ClientID];
            NewClient.InGame = true;
            NewClient.ServerCollider = new BEPUphysics.Entities.Prefabs.Sphere(NewClient.CharacterPosition, 1);
            Physics.WorldSimulator.Space.Add(NewClient.ServerCollider);
            Rendering.Window.Instance.ModelDrawer.Add(NewClient.ServerCollider);
            //All the other players need to be told about the new player
            List<ClientConnection> OtherClients = ConnectionManager.GetActiveClientsExceptFor(ClientID);
            l.og(ClientID + " has entered the game");
        }

        //Tells a list of players to spawn a new item onto the ground in their game client
        public static void SendListSpawnItem(List<ClientConnection> Clients, Item NewItem)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.SpawnItem);
            Writer.WriteString(NewItem.Type);
            Writer.WriteString(NewItem.Name);
            Writer.WriteInt(NewItem.ID);
            Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(NewItem.Collider.Position));
            foreach (ClientConnection Player in Clients)
            {
                ConnectionManager.SendPacketTo(Player.ID, Writer.ToArray());
            }
        }
        
        public static void SendListRemoveItem(List<ClientConnection> Clients, Item OldItem)
        {
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.RemoveItem);
            Writer.WriteInt(OldItem.ID);
            foreach (ClientConnection Player in Clients)
                ConnectionManager.SendPacketTo(Player.ID, Writer.ToArray());
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
                Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(Entity.Position));
                //Rotation
                Writer.WriteQuaternion(Entity.Rotation);
                //Health Points Remaining
                Writer.WriteInt(Entity.HealthPoints);
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

        //Tells a list of players that an enemy is no longer active in the game world
        public static void SendListRemoveEntities(List<ClientConnection> Players, List<BaseEntity> DeadEntities)
        {
            //Create a new packet to store the information about all entities that need to be removed from the game
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.RemoveEntities);
            //Write the number of entities to be removed
            Writer.WriteInt(DeadEntities.Count);
            //Write the ID of each entity to be removed
            foreach (BaseEntity DeadEntity in DeadEntities)
                Writer.WriteString(DeadEntity.ID);
            //Send this information to all players in the given list
            foreach (ClientConnection Player in Players)
                ConnectionManager.SendPacketTo(Player.ID, Writer.ToArray());
        }

        //Recieves a players updated position/rotation values
        public static void HandlePlayerUpdate(int ClientID, byte[] PacketData)
        {
            //Extract the updated player information from the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string CharacterName = Reader.ReadString();
            Vector3 CharacterPosition = Maths.VectorTranslate.ConvertVector(Reader.ReadVector3());
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

        //Receives the location where a players attack landed in the world
        public static void HandlePlayerAttack(int ClientID, byte[] PacketData)
        {
            //Figure out where the players attack landed
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            Vector3 AttackPosition = Reader.ReadVector3();
            //Any positions read in from unity need to be converted as the axis directions are different in the BEPU physics engine
            AttackPosition = Maths.VectorTranslate.ConvertVector(AttackPosition);
            Vector3 AttackScale = Reader.ReadVector3();
            Quaternion AttackRotation = Reader.ReadQuaternion();
            //Pass the information about this attack on to the entity manager so it can process which enemies the attack hit
            Entities.EntityManager.HandlePlayerAttack(AttackPosition, AttackScale, AttackRotation);
        }

        //Removes a player from the game once they have stopped playing
        public static void HandlePlayerDisconnect(int ClientID, byte[] PacketData)
        {
            ClientConnection Client = ConnectionManager.ClientConnections[ClientID];
            Entities.EntityManager.HandleClientDisconnect(Client);
            Networking.ConnectionManager.HandleClientDisconnect(Client);
        }
        public static void SendListRemoveOther(List<ClientConnection> Clients, string CharacterName)
        {
            //Dont do anything if an empty list of clients has been sent to the function
            if (Clients.Count == 0)
                return;

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
