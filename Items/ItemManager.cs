// ================================================================================================================================
// File:        ItemManager.cs
// Description: Keeps track of what items are in the game world, allows functions to easily add more and provide all item info to players
// ================================================================================================================================

using System;
using System.Collections.Generic;
using Server.Networking;
using BEPUutilities;

namespace Server.Items
{
    public static class ItemManager
    {
        //Helper Function to find what type of item the given id belongs to
        

        private static int NextItemID = -1;

        //Keep a track of all items that have been dropped in the game world and yet to be picked up by anyone
        public static List<Item> ActiveItems = new List<Item>();
        private static Random RNG = new Random();

        //Retrieve the next item ID to be assigned from the database backup
        public static void InitializeItemManager()
        {
            NextItemID = Data.GlobalsDatabase.GetNextItemID();
        }

        //Save the next item ID to be assiged to the database
        public static void Backup()
        {
            Data.GlobalsDatabase.SaveNextItemID(NextItemID);
        }

        //Returns the next available item ID to be used for a newly created item
        private static int NextID()
        {
            NextItemID++;
            return NextItemID;
        }

        //Checks if there is an item able to be picked up with the given ID number
        public static bool CanTakeItem(int ItemID)
        {
            foreach (Item Item in ActiveItems)
                if (Item.ItemID == ItemID)
                    return true;

            return false;
        }

        //Removes the item with the given ID from the game world and instructs all game clients to remove it from their game worlds
        public static void RemoveItem(int ItemID)
        {
            //Get this correct item with the given ID number and remove it from the list of active items
            Item OldItem = GetItem(ItemID);
            ActiveItems.Remove(OldItem);

            //Remove the item from the physics / rendering scene
            Physics.WorldSimulator.Space.Remove(OldItem.Collider);
            Rendering.Window.Instance.ModelDrawer.Remove(OldItem.Collider);

            //Tell all active players to remove this item from their game world
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListRemoveItem(ActivePlayers, OldItem);
        }

        //Gets the item from the active item list with the given ID number
        public static Item GetItem(int ItemID)
        {
            foreach (Item Item in ActiveItems)
                if (Item.ItemID == ItemID)
                    return Item;
            return null;
        }

        //Adds a pickup item into the game world that players can loot and use
        public static void AddEquipmentPickup(ItemList NewItemType, Vector3 ItemPosition)
        {
            //Create the new equipment item and assign its values to it
            string NewItemName = Enum.GetName(typeof(ItemList), NewItemType);
            int NewItemNumber = (int)NewItemType;

            Item NewItem = new Item(NewItemName, "Equipment", ItemPosition, NewItemNumber, NextID());
            ActiveItems.Add(NewItem);

            //Tell any active players to spawn this new item in their game world
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListSpawnItem(ActivePlayers, NewItem);
        }

        //Adds a consumable potion item into the game world that players can loot
        public static void AddPotionPickup(Potions NewPotionType, Vector3 PotionLocation)
        {
            string NewPotionName = Enum.GetName(typeof(Potions), NewPotionType);
            int NewPotionNumber = (int)NewPotionType;

            Item NewPotion = new Item(NewPotionName, "Consumable", PotionLocation, NewPotionNumber, NextID());
            ActiveItems.Add(NewPotion);

            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListSpawnItem(ActivePlayers, NewPotion);
        }

        //Adds a random consumable potion item into the game world
        public static void AddRandomPotion(Vector3 Position)
        {
            Potions PotionType = (Potions)RNG.Next(1, 2);
            AddPotionPickup(PotionType, Position);
        }
    }
}
