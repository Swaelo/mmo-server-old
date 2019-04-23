// ================================================================================================================================
// File:        ItemManager.cs
// Description: Keeps track of what items are in the game world, allows functions to easily add more and provide all item info to players
// ================================================================================================================================

using System;
using System.Collections.Generic;
using Server.Networking;
using BEPUutilities;
using BEPUphysics.Entities.Prefabs;

namespace Server.Items
{
    public static class ItemManager
    {
        private static int NextItemID = -1;

        //Keep a track of all items that have been dropped in the game world and yet to be picked up by anyone
        public static List<Item> ActiveItems = new List<Item>();
        private static Random RNG = new Random();

        //Retrieve the next item ID to be assigned from the database backup
        public static void InitializeItemManager()
        {
            NextItemID = Data.Database.GetNextItemID();
        }

        //Save the next item ID to be assiged to the database
        public static void Backup()
        {
            Data.Database.SaveNextItemID(NextItemID);
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
                if (Item.ID == ItemID)
                    return true;

            return false;
        }

        //Removes the item with the given ID from the game world and instructs all game clients to remove it from their game worlds
        public static void RemoveItem(int ItemID)
        {
            //Get this correct item with the given ID number and remove it from the list of active items
            Item OldItem = GetItem(ItemID);
            ActiveItems.Remove(OldItem);
            //Tell all active players to remove this item from their game world
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListRemoveItem(ActivePlayers, OldItem);
        }

        //Gets the item from the active item list with the given ID number
        public static Item GetItem(int ItemID)
        {
            foreach (Item Item in ActiveItems)
                if (Item.ID == ItemID)
                    return Item;
            return null;
        }
        
        //Creates a random consumable of the given type
        public static Item GetRandomConsumable(ConsumableTypes Type)
        {
            //Create and seed a new instance of the Random class to use for random item selection
            Random Generator = new Random();

            //Generate a brand new item from the given type pool
            switch (Type)
            {
                //First available option is returning a random potion the player is able to drink
                case (ConsumableTypes.Potions):
                    //First figure out how many different types of potions there are available
                    var PotionTypes = Enum.GetNames(typeof(Potions)).Length;
                    //Select one of these types randomly
                    Potions RandomType = (Potions)Generator.Next(0, PotionTypes);
                    string TypeName = EnumToItemName.GetItemName(RandomType);
                    //Now create and return the potion of this type
                    Item NewItem = new Item(TypeName, NextID());
                    NewItem.Type = "Consumable";
                    ActiveItems.Add(NewItem);
                    return NewItem;
            }

            //Return the new item that was requested
            return null;
        }

        //Adds a new consumable item pickup into the game world
        public static void AddRandomConsumablePickup(ConsumableTypes ConsumableType, Vector3 SpawnLocation)
        {
            //Create the new random consumable object
            Item NewConsumable = GetRandomConsumable(ConsumableType);
            //Set up the items physics collider and add it into the scene
            NewConsumable.Collider = new Box(SpawnLocation, 0.25f, 0.25f, 0.25f);
            Physics.WorldSimulator.Space.Add(NewConsumable.Collider);
            //Set the consumable to be rendered inside the server window
            Rendering.Window.Instance.ModelDrawer.Add(NewConsumable.Collider);
            //Tell all active players to add this item into their game world
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListSpawnItem(ActivePlayers, NewConsumable);
        }

        //Adds a new random piece of equipment into the game world
        public static void AddRandomEquipmentPickup(Vector3 SpawnLocation)
        {
            var EquipmentTypes = Enum.GetNames(typeof(Equipments)).Length;
            Equipments NewEquipmentType = (Equipments)RNG.Next(0, EquipmentTypes);
            string EquipmentName = Enum.GetName(typeof(Equipments), NewEquipmentType);
            Item NewEquipment = new Item(EquipmentName, NextID());
            NewEquipment.Type = "Equipment";
            ActiveItems.Add(NewEquipment);
            NewEquipment.Collider = new Box(SpawnLocation, 0.25f, 0.25f, 0.25f);
            Physics.WorldSimulator.Space.Add(NewEquipment.Collider);
            Rendering.Window.Instance.ModelDrawer.Add(NewEquipment.Collider);
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            PacketManager.SendListSpawnItem(ActivePlayers, NewEquipment);
            l.og("added a new " + EquipmentName + " equipment picked to the game world");
        }
    }
}
