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
    PlayerInventoryGearUpdate = 18
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
            //Read in the packet type
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read in the characters name and the slot they want to remove the item from
            string CharacterName = Reader.ReadString();
            EquipmentSlot EquipmentSlot = (EquipmentSlot)Reader.ReadInt();

            //Make sure the character actually has an item equipped in this gear slot
            bool ItemEquipped = EquipmentsDatabase.IsItemEquipped(CharacterName, EquipmentSlot);
            //If no item is equipped in this slot then we just ignore the request all together
            if (!ItemEquipped)
            {
                l.og(CharacterName + "s unequip item request denied: there is no item in this bag slot.");
                return;
            }

            //Make sure the character has space available in their inventory to store this item once it has been unequipped
            bool CharactersBagsFull = InventoriesDatabase.IsCharactersInventoryFull(CharacterName);
            //If they have no space available in their inventory just ignore the request
            if (CharactersBagsFull)
            {
                l.og(CharacterName + "s unequip item request denied: there is no space in the characters inventory to store the item.");
                return;
            }

            //Retrieve from the database the information about what item the character currently has equipped in this gear slot
            InventoryItem EquippedItem = EquipmentsDatabase.GetEquippedItem(CharacterName, EquipmentSlot);

            //Remove this item from the players equipment
            EquipmentsDatabase.UnequipCharacterItem(CharacterName, EquipmentSlot);
            //Add this item to the first available slot in the players inventory
            InventoriesDatabase.GivePlayerItem(CharacterName, EquippedItem);

            //Send back to the player their updated inventory and equipment state
            SendPlayerInventoryEquipmentUpdate(ClientID, CharacterName);
        }

        //Moves an item from the players inventory to their equipment screen
        public static void HandleEquipInventoryItem(int ClientID, byte[] PacketData)
        {
            //Read in the packet type
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read in the characters name and the bag slot number with the item they want to equip
            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();

            //Find out what item the character currently has stored in this bag slot
            InventoryItem Item = InventoriesDatabase.GetCharactersInventoryItem(CharacterName, BagSlot);
            //Find what slot this item can be equipped to
            Item.ItemSlot = BelongingItemSlot.FindSlot(Item.ItemNumber);

            //If this isnt an item that can be equipped to the player then just ignore this request
            if (!Item.CanEquip())
            {
                l.og(CharacterName + "s equip item request denied: This is not an item that can be equipped.");
                return;
            }

            //Make sure there isnt already an item equipped in the characters gear slot that this item uses
            bool SlotTaken = EquipmentsDatabase.IsItemEquipped(CharacterName, Item.ItemSlot);
            //If there is already an item equipped in this gear slot then ignore this request
            if(SlotTaken)
            {
                l.og(CharacterName + "s equip item request denied: There is already an item equipped in this gear slot.");
                return;
            }

            //Remove this item from the players inventory
            InventoriesDatabase.RemovePlayerItem(CharacterName, BagSlot);
            //Add this item to the players equipment
            EquipmentsDatabase.EquipCharacterItem(CharacterName, Item);

            //Give the player updated information on their current equipped items
            SendPlayerInventoryEquipmentUpdate(ClientID, CharacterName);
        }
        
        //Removes an item from a players inventory
        public static void HandleRemoveInventoryItem(int ClientID, byte[] PacketData)
        {
            //Read the information from the packet data
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();

            //Remove the item from the players inventory
            InventoriesDatabase.RemovePlayerItem(CharacterName, BagSlot);

            //Update the player on their new inventory contents
            SendPlayerInventoryEquipmentUpdate(ClientID, CharacterName);
        }
        
        //Sends a player up to date information regarding what they have in their inventory and what they have equipped
        private static void SendPlayerInventoryEquipmentUpdate(int ClientID, string CharacterName)
        {
            //Write a new packet to store all the information
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerInventoryGearUpdate);

            //Get the current contents of the players inventory and write all that data into the network packet
            List<InventoryItem> InventoryContents = InventoriesDatabase.GetCharactersInventory(CharacterName);
            foreach(InventoryItem BagItem in InventoryContents)
            {
                Writer.WriteInt(BagItem.ItemNumber);
                Writer.WriteInt(BagItem.ItemID);
            }

            //Get the current contents of the players equipment and write all that data into the network packet too
            List<InventoryItem> EquipmentContents = EquipmentsDatabase.GetCharactersEquipment(CharacterName);
            foreach(InventoryItem EquippedItem in EquipmentContents)
            {
                Writer.WriteInt((int)EquippedItem.ItemSlot);
                Writer.WriteInt(EquippedItem.ItemNumber);
                Writer.WriteInt(EquippedItem.ItemID);
            }

            //Send this packet to the client
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }
        
        //User is trying to pick up an item from the groundf
        public static void HandlePlayerTakeItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the characters name and the data of the item they want to take into a new InventoryItem object
            string CharacterName = Reader.ReadString();
            InventoryItem Item = new InventoryItem();
            Item.ItemNumber = Reader.ReadInt();
            Item.ItemID = Reader.ReadInt();

            //Ignore this request if the character has no space in their bags to take the item
            if(InventoriesDatabase.IsCharactersInventoryFull(CharacterName))
            {
                l.og(CharacterName + "s take item request denied: characters inventory is full.");
                return;
            }

            //Update the players inventory to contain the new item
            InventoriesDatabase.GivePlayerItem(CharacterName, Item);
            //Send the player their updated inventory state
            SendPlayerInventoryEquipmentUpdate(ClientID, CharacterName);

            //Sleep for a moment, then have the item manager remove this object from the game world, while telling all active clients to do the same thing too
            Thread.Sleep(250);
            ItemManager.RemoveItem(Item.ItemID);
        }

        //User is requesting for a list of items they have equipped on their character
        public static void HandlePlayerEquipmentRequest(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the information from the packet
            string CharacterName = Reader.ReadString();

            //Load from the database all the items this character currently has equipped
            List<InventoryItem> CharactersEquipment = Data.EquipmentsDatabase.GetCharactersEquipment(CharacterName);

            //Write a new network packet for the equipped items
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerEquipmentItems);

            //Write into the packet the details of each item currently equipped to the player
            Writer.WriteInt(CharactersEquipment.Count);
            foreach(InventoryItem EquippedItem in CharactersEquipment)
            {
                Writer.WriteInt((int)EquippedItem.ItemSlot);
                Writer.WriteInt(EquippedItem.ItemNumber);
                Writer.WriteInt(EquippedItem.ItemID);
            }

            //Send this packet to the client
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //User is requesting for a list of all items in their inventory
        public static void HandlePlayerInventoryRequest(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the characters name whos inventory is being requested
            string CharacterName = Reader.ReadString();

            //Retrieve the current list of items being stored in this players inventory
            List<InventoryItem> InventoryContents = Data.InventoriesDatabase.GetCharactersInventory(CharacterName);

            //Write a new packet to store all this characters inventory contents
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerInventoryItems);

            //Write into the packet each items ItemNumber and ItemID
            foreach (InventoryItem Item in InventoryContents)
            {
                Writer.WriteInt(Item.ItemNumber);
                Writer.WriteInt(Item.ItemID);
            }

            //Send the packet to the client who requested it
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
            if(!AccountsDatabase.IsAccountNameAvailable(AccountName))
            {
                SendAccountRegistrationReply(ClientID, false, "username is already taken");
                return;
            }

            //Account credentials are valid and the username is available, register this into the database
            AccountsDatabase.RegisterNewAccount(AccountName, AccountPass);
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
            if(AccountsDatabase.IsAccountNameAvailable(AccountName))
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
            if(!AccountsDatabase.IsPasswordCorrect(AccountName, AccountPass))
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
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Create a new CharacterData object and store all the new characters data extracted from the network packet into it
            CharacterData NewCharacterData = new CharacterData();
            NewCharacterData.Account = Reader.ReadString();
            NewCharacterData.Name = Reader.ReadString();
            NewCharacterData.IsMale = Reader.ReadInt() == 1;

            //Make sure this character name hasnt already been taken by someone else
            if(!CharactersDatabase.IsCharacterNameAvailable(NewCharacterData.Name))
            {
                SendCharacterCreationReply(ClientID, false, "character name already taken.");
                return;
            }

            //Otherwise, save the new character into the database
            CharactersDatabase.SaveNewCharacter(NewCharacterData);
            SendCharacterCreationReply(ClientID, true, "character created.");
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

        //Sends a client all the data regarding every character existing under this account name
        public static void SendCharacterData(int ClientID, string AccountName)
        {
            //Create a new network packet to store all the information
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.CharacterDataReply);

            //Write in the number of characters existing in this account, then loop through and write each characters details into the packet
            int CharacterCount = CharactersDatabase.GetCharacterCount(AccountName);
            Writer.WriteInt(CharacterCount);
            for(int i = 0; i < CharacterCount; i++)
            {
                //Grab the next characters name from the database
                string CharacterName = CharactersDatabase.GetCharacterName(AccountName, i + 1);
                //Use the characters name to get the rest of its information from the database
                CharacterData Data = CharactersDatabase.GetCharacterData(CharacterName);

                //Write all of this characters details into the network packet
                Writer.WriteString(Data.Account);
                Writer.WriteString(Data.Name);
                Writer.WriteVector3(Data.Position);
                Writer.WriteInt(Data.Level);
                Writer.WriteInt(Data.IsMale ? 1 : 0);
            }

            //Send this packet to the client who requested it
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
            //Define new packet with instructions to spawn a list of item picks into the clients game world
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.ActiveItemList);

            //Grab the list of current item picks active in the world right now
            List<Item> ItemList = ItemManager.ActiveItems;
            //Write into the packet the total number of items that are currently active
            Writer.WriteInt(ItemList.Count);

            foreach (Item Item in ItemList)
            {
                //For each item, write in its Name, Type, Number, ID and Position
                Writer.WriteString(Item.Name);
                Writer.WriteString(Item.Type);
                Writer.WriteInt(Item.ItemNumber);
                Writer.WriteInt(Item.ItemID);
                Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(Item.Collider.Position));
            }

            //Send the packet off to the client
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
            //Define new packet with SpawnItem instruction
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.SpawnItem);

            //Write in the items Type, Name, Number, ID and Position
            Writer.WriteString(NewItem.Type);
            Writer.WriteString(NewItem.Name);
            Writer.WriteInt(NewItem.ItemNumber);
            Writer.WriteInt(NewItem.ItemID);
            Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(NewItem.Collider.Position));

            //Now grab the current list of active client connections and tell them all to spawn this new item into their game worlds
            List<ClientConnection> ActiveClients = ConnectionManager.GetActiveClients();
            ConnectionManager.SendPacketTo(ActiveClients, Writer.ToArray());
        }
        
        public static void SendListRemoveItem(List<ClientConnection> Clients, Item OldItem)
        {
            //Define new packet with RemoveItem instruction
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.RemoveItem);

            //All we need to write is the items Network ID which is enough for the game clients to know which item to remove
            Writer.WriteInt(OldItem.ItemID);

            //Now send this packet to all active client connections
            List<ClientConnection> ActiveClients = ConnectionManager.GetActiveClients();
            ConnectionManager.SendPacketTo(ActiveClients, Writer.ToArray());
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
