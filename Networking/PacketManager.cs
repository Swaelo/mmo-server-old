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
    PlayerActionBarRequest = 16,
    PlayerTakeItemRequest = 17,
    RemoveInventoryItem = 18,
    EquipInventoryItem = 19,
    UnequipItem = 20,

    PlayerMoveInventoryItem = 21,
    PlayerSwapInventoryItems = 22,
    PlayerSwapEquipmentItem = 23,
    PlayerDropItem = 24,
    PlayerEquipAbility = 25,
    PlayerSwapEquipAbility = 26,
    PlayerUnequipAbility = 27,
    PlayerSwapAbilities = 28,
    PlayerMoveAbility = 29,
    PlayerDropAbility = 30,

    PlayerPurgeItems = 31
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
    PlayerActionBarAbilities = 18,
    PlayerTotalItemUpdate = 19
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
            Packets.Add((int)ClientPacketType.PlayerActionBarRequest, HandlePlayerActionBarRequest);
            Packets.Add((int)ClientPacketType.PlayerTakeItemRequest, HandlePlayerTakeItem);
            Packets.Add((int)ClientPacketType.RemoveInventoryItem, HandleRemoveInventoryItem);
            Packets.Add((int)ClientPacketType.EquipInventoryItem, HandleEquipInventoryItem);
            Packets.Add((int)ClientPacketType.UnequipItem, HandleUnequipItem);
            Packets.Add((int)ClientPacketType.PlayerMoveInventoryItem, HandleMoveInventoryItem);
            Packets.Add((int)ClientPacketType.PlayerSwapInventoryItems, HandleSwapInventoryItems);
            Packets.Add((int)ClientPacketType.PlayerSwapEquipmentItem, HandleSwapEquipmentItem);
            Packets.Add((int)ClientPacketType.PlayerDropItem, HandlePlayerDropItem);
            Packets.Add((int)ClientPacketType.PlayerEquipAbility, HandlePlayerEquipAbility);
            Packets.Add((int)ClientPacketType.PlayerSwapEquipAbility, HandlePlayerSwapEquipAbility);
            Packets.Add((int)ClientPacketType.PlayerUnequipAbility, HandlePlayerUnequipAbility);
            Packets.Add((int)ClientPacketType.PlayerSwapAbilities, HandlePlayerSwapAbilities);
            Packets.Add((int)ClientPacketType.PlayerMoveAbility, HandlePlayerMoveAbility);
            Packets.Add((int)ClientPacketType.PlayerPurgeItems, HandlePlayerPurgeItems);
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

        //Removes an item from a players posession and drops it into the game world for anyone to take
        public static void HandlePlayerDropItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Handle the item drop diferent depending on where its coming from (inventory, equipment or ability bar)
            int DropSource = Reader.ReadInt();
            switch(DropSource)
            {
                //DropSource 1 = Characters Inventory
                case (1):
                    {
                        //Find out who dropped the item, what item was dropped, and where they dropped it
                        string CharacterName = Reader.ReadString();
                        int BagSlot = Reader.ReadInt();
                        Vector3 DropLocation = Maths.VectorTranslate.ConvertVector(Reader.ReadVector3());
                        //Find the items information and remove it from the characters possession
                        ItemData InventoryItem = InventoriesDatabase.GetInventorySlot(CharacterName, BagSlot);
                        InventoriesDatabase.RemoveCharacterItem(CharacterName, BagSlot);
                        SendPlayerTotalItemUpdate(ClientID, CharacterName);
                        //FInally, add the item into the game world as a new pickup object
                        ItemManager.AddItemPickup(InventoryItem.ItemNumber, DropLocation);
                    }
                    break;

                //DropSource 2 = Characters Equipment
                case (2):
                    {
                        //Find out who dropped the item, what slot its equipped in, and where they dropped it
                        string CharacterName = Reader.ReadString();
                        EquipmentSlot GearSlot = (EquipmentSlot)Reader.ReadInt();
                        Vector3 DropLocation = Maths.VectorTranslate.ConvertVector(Reader.ReadVector3());
                        //Find the items information and remove it from the characters possession
                        ItemData EquipmentItem = EquipmentsDatabase.GetEquipmentSlot(CharacterName, GearSlot);
                        EquipmentsDatabase.CharacterRemoveItem(CharacterName, GearSlot);
                        SendPlayerTotalItemUpdate(ClientID, CharacterName);
                        //Finally add the item into the game world as a new pickup object
                        ItemManager.AddItemPickup(EquipmentItem.ItemNumber, DropLocation);
                    }
                    break;

                //DropSource 3 = Characters Action Bar
                case (3):
                    {
                        //Find out who dropped the item, which action bar slot it came from, and where they dropped it
                        string CharacterName = Reader.ReadString();
                        int ActionBarSlot = Reader.ReadInt();
                        Vector3 DropLocation = Maths.VectorTranslate.ConvertVector(Reader.ReadVector3());
                        //Get the items information and remove it from the characters possession
                        ItemData AbilityItem = ActionBarsDatabase.GetActionBarItem(CharacterName, ActionBarSlot);
                        ActionBarsDatabase.TakeCharacterAbility(CharacterName, ActionBarSlot);
                        SendPlayerTotalItemUpdate(ClientID, CharacterName);
                        //Add the item into the game world as a new pickup object
                        ItemManager.AddItemPickup(AbilityItem.ItemNumber, DropLocation);
                    }
                    break;
            }
        }

        //Moves an ability gem from a characters inventory onto their action bar
        public static void HandlePlayerEquipAbility(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Get the characters name, what bag slot has the ability gem inside, and what action bar slot its going to be equipped to
            string CharacterName = Reader.ReadString();
            int GemBagSlot = Reader.ReadInt();
            int ActionBarSlot = Reader.ReadInt();

            //Read the item data from the players inventory
            ItemData AbilityGem = InventoriesDatabase.GetInventorySlot(CharacterName, GemBagSlot);
            //Place it onto their action bar
            ActionBarsDatabase.GiveCharacterAbility(CharacterName, AbilityGem, ActionBarSlot);
            //Remove it from their inventory
            InventoriesDatabase.RemoveCharacterItem(CharacterName, GemBagSlot);
            //Give player total item update
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Swaps an ability gem from a characters inventory with one that is already equipped
        public static void HandlePlayerSwapEquipAbility(int ClientID, byte[] PacketData)
        {
            //Open up the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Extract the required information out of the packet data
            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();
            int ActionBarSlot = Reader.ReadInt();

            //Read the information for both the item in the players inventory and the item currently on their action bar
            ItemData InventoryItem = InventoriesDatabase.GetInventorySlot(CharacterName, BagSlot);
            ItemData ActionBarItem = ActionBarsDatabase.GetActionBarItem(CharacterName, ActionBarSlot);

            //Remove the current ability gem from the players action bar and place it into the players inventory
            ActionBarsDatabase.TakeCharacterAbility(CharacterName, ActionBarSlot);
            InventoriesDatabase.GiveCharacterItem(CharacterName, ActionBarItem, BagSlot);
            //Add the newly equipped ability gem onto the players action bar and send them their updated item data
            ActionBarsDatabase.GiveCharacterAbility(CharacterName, InventoryItem, ActionBarSlot);
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Unequips an ability gem from a character and places it into their inventory
        public static void HandlePlayerUnequipAbility(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();
            int ActionBarSlot = Reader.ReadInt();

            //Look up the item, place it into the players inventory
            ItemData EquippedAbility = ActionBarsDatabase.GetActionBarItem(CharacterName, ActionBarSlot);
            InventoriesDatabase.GiveCharacterItem(CharacterName, EquippedAbility, BagSlot);

            //Remove it from their ability bar
            ActionBarsDatabase.TakeCharacterAbility(CharacterName, ActionBarSlot);

            //Update players items status
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Swaps the positions of the gems currently on the action bar
        public static void HandlePlayerSwapAbilities(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the required information from the packet data
            string CharacterName = Reader.ReadString();
            int FirstActionBarSlot = Reader.ReadInt();
            int SecondActionBarSlot = Reader.ReadInt();

            //Look up the items in each slot of the action bar
            ItemData FirstAbility = ActionBarsDatabase.GetActionBarItem(CharacterName, FirstActionBarSlot);
            ItemData SecondAbility = ActionBarsDatabase.GetActionBarItem(CharacterName, SecondActionBarSlot);

            //Place each item in the other items position
            ActionBarsDatabase.GiveCharacterAbility(CharacterName, FirstAbility, SecondActionBarSlot);
            ActionBarsDatabase.GiveCharacterAbility(CharacterName, SecondAbility, FirstActionBarSlot);

            //Send the player a full update now
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Moves the position of one of the gems on the characters action bar
        public static void HandlePlayerMoveAbility(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the data needed from the packet
            string CharacterName = Reader.ReadString();
            int AbilityBarSlot = Reader.ReadInt();
            int DestinationBarSlot = Reader.ReadInt();

            //Get the data regarding the item that needs to be moved
            ItemData AbilityGem = ActionBarsDatabase.GetActionBarItem(CharacterName, AbilityBarSlot);
            //Remove this gem from the characters ability bar, then re add it at the new location
            ActionBarsDatabase.TakeCharacterAbility(CharacterName, AbilityBarSlot);
            ActionBarsDatabase.GiveCharacterAbility(CharacterName, AbilityGem, DestinationBarSlot);

            //Send updated UI info to the player now that the ability gem has been moved to where they wanted it
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Removes every single item, equipment and ability gem from a characters position, then sends them a UI update
        public static void HandlePlayerPurgeItems(int ClientID, byte[] PacketData)
        {
            //Open the network packet and find out which character requested the item purge
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string CharacterName = Reader.ReadString();

            //Update all the databases, removing everything from the characters possesion
            InventoriesDatabase.PurgeCharactersItems(CharacterName);
            EquipmentsDatabase.PurgeCharactersEquipment(CharacterName);
            ActionBarsDatabase.PurgeCharactersActionBar(CharacterName);

            //Finally send the player a UI update to show that everythings been complete
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Changes the position of one of the items in a players inventory
        public static void HandleMoveInventoryItem(int ClientID, byte[] PacketData)
        {
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();
            string PlayerName = Reader.ReadString();
            int OriginalBagSlot = Reader.ReadInt();
            int DestinationBagSlot = Reader.ReadInt();
            InventoriesDatabase.MoveInventoryItem(PlayerName, OriginalBagSlot, DestinationBagSlot);
            SendPlayerTotalItemUpdate(ClientID, PlayerName);
        }

        //Swaps the positions of two items in a players inventory
        public static void HandleSwapInventoryItems(int ClientID, byte[] PacketData)
        {
            //Extract all the info we need from the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            string PlayerName = Reader.ReadString();
            int FirstBagSlot = Reader.ReadInt();
            int SecondBagSlot = Reader.ReadInt();

            //Swap the positions of these two items in the players inventory
            InventoriesDatabase.SwapInventoryItem(PlayerName, FirstBagSlot, SecondBagSlot);

            //Send back to the player up to date information on the inventory and equipment state
            SendPlayerTotalItemUpdate(ClientID, PlayerName);
        }

        //Swaps the position of an equipped item and an item in the players inventory
        public static void HandleSwapEquipmentItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Extract the nessacery information
            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();
            EquipmentSlot EquipSlot = (EquipmentSlot)Reader.ReadInt();

            //Get each items information that is going to be swapped around
            ItemData InventoryItemData = InventoriesDatabase.GetInventorySlot(CharacterName, BagSlot);
            InventoryItemData.ItemEquipmentSlot = EquipSlot;
            ItemData EquippedItemData = EquipmentsDatabase.GetEquipmentSlot(CharacterName, EquipSlot);

            //Update the characters inventory and equipment, overwriting each item with the other one
            InventoriesDatabase.GiveCharacterItem(CharacterName, EquippedItemData, BagSlot);
            EquipmentsDatabase.CharacterEquipItem(CharacterName, InventoryItemData);

            //Send the player a UI update now
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Moves an item from the players equipment to their inventory
        public static void HandleUnequipItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Find the characters name, which slot they want to remove the item from, and which bag slot they want to move it to
            string CharacterName = Reader.ReadString();
            EquipmentSlot EquipmentSlot = (EquipmentSlot)Reader.ReadInt();
            int BagSlot = Reader.ReadInt();

            //Find the items data, move it from the characters equipment to their inventory, send them UI update
            ItemData EquippedItem = EquipmentsDatabase.GetEquipmentSlot(CharacterName, EquipmentSlot);
            EquipmentsDatabase.CharacterRemoveItem(CharacterName, EquipmentSlot);
            InventoriesDatabase.GiveCharacterItem(CharacterName, EquippedItem, BagSlot);
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }

        //Moves an item from the players inventory to their equipment screen
        public static void HandleEquipInventoryItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Find the characters name, which bag slot holds the item, and which gear slot its being equipped to
            string CharacterName = Reader.ReadString();
            int BagSlot = Reader.ReadInt();
            EquipmentSlot GearSlot = (EquipmentSlot)Reader.ReadInt();

            //Get the information about the item in the players bag, remove it from their possession then equip it
            ItemData InventoryItem = InventoriesDatabase.GetInventorySlot(CharacterName, BagSlot);
            InventoryItem.ItemEquipmentSlot = GearSlot;
            InventoriesDatabase.RemoveCharacterItem(CharacterName, BagSlot);
            EquipmentsDatabase.CharacterEquipItem(CharacterName, InventoryItem);
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
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
            InventoriesDatabase.RemoveCharacterItem(CharacterName, BagSlot);

            //Update the player on their new inventory contents
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
        }
        
        //Sends a player up to date information regarding what they have in their inventory, what gear they are wearing and what abilities they have socketed
        private static void SendPlayerTotalItemUpdate(int ClientID, string CharacterName)
        {
            //Create a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerTotalItemUpdate);

            //Get the contents of the characters inventory and write all that into the packet
            List<ItemData> InventoryItems = InventoriesDatabase.GetAllInventorySlots(CharacterName);
            foreach(ItemData InventoryItem in InventoryItems)
            {
                Writer.WriteInt(InventoryItem.ItemNumber);
                Writer.WriteInt(InventoryItem.ItemID);
            }

            //Next, get the contents of the characters equipment and write all that into the packet too
            List<ItemData> EquipmentItems = EquipmentsDatabase.GetAllEquipmentSlots(CharacterName);
            foreach(ItemData EquipmentItem in EquipmentItems)
            {
                Writer.WriteInt((int)EquipmentItem.ItemEquipmentSlot);
                Writer.WriteInt(EquipmentItem.ItemNumber);
                Writer.WriteInt(EquipmentItem.ItemID);
            }

            //Finally, get the contents of the characters action bar slots and write all them into the packet
            List<ItemData> AbilityItems = ActionBarsDatabase.GetEveryActionBarItem(CharacterName);
            foreach(ItemData AbilityItem in AbilityItems)
            {
                Writer.WriteInt(AbilityItem.ItemNumber);
                Writer.WriteInt(AbilityItem.ItemID);
            }

            //Send the packet to the client who requested it
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //User is trying to pick up an item from the groundf
        public static void HandlePlayerTakeItem(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Find the information about the item the character is trying to pick up
            string CharacterName = Reader.ReadString();
            int ItemNumber = Reader.ReadInt();
            int ItemID = Reader.ReadInt();

            //Ignore the request entirely if the character has no free space in their inventory
            if (InventoriesDatabase.IsInventoryFull(CharacterName))
                return;

            //Get the information about the item the character is picking up, place it in their inventory
            ItemData GroundItem = ItemList.MasterItemList[ItemNumber];
            InventoriesDatabase.GiveCharacterItem(CharacterName, GroundItem);
            //Send them a UI update and sleep for a moment to allow that to go through before removing the pickup from the game world
            SendPlayerTotalItemUpdate(ClientID, CharacterName);
            Thread.Sleep(250);

            //Remove the item pickup from the game world, automatically telling all active clients to do the same on their worlds
            ItemManager.RemoveItemPickup(ItemID);
        }

        //User is requesting for a list of items they have equipped on their character
        public static void HandlePlayerEquipmentRequest(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Read the information from the packet
            string CharacterName = Reader.ReadString();

            SendPlayerEquipmentItems(ClientID, CharacterName);
        }

        //Provide a user with a list of all the abilities currently equipped on their characters action bar
        public static void HandlePlayerActionBarRequest(int ClientID, byte[] PacketData)
        {
            //Open the network packet
            PacketReader Reader = new PacketReader(PacketData);
            int PacketType = Reader.ReadInt();

            //Find the characters action bar and send it back to the user who requested it
            string CharacterName = Reader.ReadString();
            SendPlayerActionBarAbilities(ClientID, CharacterName);
        }

        //Sends a user a complete list of every item currently stored in a characters inventory
        private static void SendPlayerInventoryItems(int ClientID, string CharacterName)
        {
            //Create a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerInventoryItems);

            //Read from the database the state of every one of the characters inventory slots
            List<ItemData> InventoryItems = InventoriesDatabase.GetAllInventorySlots(CharacterName);
            
            //Loop through and write in the data for each item in the characters inventory
            foreach(ItemData InventoryItem in InventoryItems)
            {
                //Simple write in each items ItemNumber and ItemID values
                Writer.WriteInt(InventoryItem.ItemNumber);
                Writer.WriteInt(InventoryItem.ItemID);
            }

            //Send the finished packet out to the client who requested it
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Sends a user a complete list of every item currently equipped / stored in the characters equipment screen
        private static void SendPlayerEquipmentItems(int ClientID, string CharacterName)
        {
            //Create a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerEquipmentItems);

            //Read from the database the state of every one of the characters equipment slots
            List<ItemData> EquipmentItems = EquipmentsDatabase.GetAllEquipmentSlots(CharacterName);

            //Loop through and write in each items values into the packet data
            foreach(ItemData EquipmentItem in EquipmentItems)
            {
                Writer.WriteInt(EquipmentItem.ItemNumber);
                Writer.WriteInt(EquipmentItem.ItemID);
            }

            //Send all the data to the client who requested it
            ConnectionManager.SendPacketTo(ClientID, Writer.ToArray());
        }

        //Sends a user a complete list of every ability currently stored on a characters action bar
        private static void SendPlayerActionBarAbilities(int ClientID, string CharacterName)
        {
            //Create a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.PlayerActionBarAbilities);

            //Read from the database the current state of every one of the characters action bar slots
            List<ItemData> ActionBarItems = ActionBarsDatabase.GetEveryActionBarItem(CharacterName);

            //Loop through and write each items data into the packet
            foreach(ItemData ActionBarItem in ActionBarItems)
            {
                Writer.WriteInt(ActionBarItem.ItemNumber);
                Writer.WriteInt(ActionBarItem.ItemID);
            }

            //Send the final packet to the client who requested it
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
            List<ItemData> InventoryContents = Data.InventoriesDatabase.GetAllInventorySlots(CharacterName);

            SendPlayerInventoryItems(ClientID, CharacterName);
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
            //Open a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.ActiveItemList);

            //Grab the list of current item pickups, write the amount, then loop and write each items data into the packet
            List<Item> ItemPickups = ItemManager.GetActiveItemList();
            Writer.WriteInt(ItemPickups.Count);
            for(int i = 0; i < ItemPickups.Count; i++)
            {
                Writer.WriteInt(ItemPickups[i].Number);
                Writer.WriteInt(ItemPickups[i].ID);
                Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(ItemPickups[i].Collider.Position));

            }
            //Send the final packet to the client who requested it
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
            //Write a new network packet
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.SpawnItem);

            //Fill in the items Number, ID and location values
            Writer.WriteInt(NewItem.Number);
            Writer.WriteInt(NewItem.ID);
            Writer.WriteVector3(Maths.VectorTranslate.ConvertVector(NewItem.Collider.Position));

            //Send this packet to all of the active client connections
            ConnectionManager.SendPacketToActiveClients(Writer.ToArray());
    }
        
        public static void SendListRemoveItem(List<ClientConnection> Clients, int ItemID)
        {
            //Define a new network packet storing the items universal ID number
            PacketWriter Writer = new PacketWriter();
            Writer.WriteInt((int)ServerPacketType.RemoveItem);
            Writer.WriteInt(ItemID);

            //Send this packet to every active game client
            ConnectionManager.SendPacketToActiveClients(Writer.ToArray());
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
            if(Client.ServerCollider != null)
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
