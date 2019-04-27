// ================================================================================================================================
// File:        EquipmentsDatabase.cs
// Description: Allows the server to interact with the local SQL database equipments tables
// ================================================================================================================================

using System;
using System.Collections.Generic;
using BEPUutilities;
using MySql.Data.MySqlClient;
using Server.Items;

namespace Server.Data
{
    class EquipmentsDatabase
    {
        //Adds an item into the specified slot of a characters equipment table
        public static void EquipCharacterItem(string CharacterName, InventoryItem NewItem)
        {
            //Convert the name of the equipment slot to a string so it can be used in the sql queries
            string SlotName = NewItem.ItemSlot.ToString();

            //Define the query to update the characters equipment table
            string EquipItemQuery = "UPDATE equipments SET " + SlotName + "ItemNumber='" + NewItem.ItemNumber + "', " + SlotName + "ItemID='" + NewItem.ItemID + "' WHERE CharacterName='" + CharacterName + "'";

            //Execute the command to finish updating the characters equipped equipment state
            MySqlCommand EquipItemCommand = new MySqlCommand(EquipItemQuery, Database.Connection);
            EquipItemCommand.ExecuteNonQuery();
        }

        //Removes whatever item in being held in the characters equipment slot
        public static void UnequipCharacterItem(string CharacterName, EquipmentSlot EquipSlot)
        {
            //Convert the name of the equipment slot to string to be used in the sql queries
            string SlotName = EquipSlot.ToString();

            //Define the query to update the characters equipment table
            string UnequipItemQuery = "UPDATE equipments SET " + SlotName + "ItemNumber='0' WHERE CharacterName='" + CharacterName + "'";

            //Execute the command to finish updating the characters equipment table state
            MySqlCommand UnequipItemCommand = new MySqlCommand(UnequipItemQuery, Database.Connection);
            UnequipItemCommand.ExecuteNonQuery();
        }

        //Checks if the character currently has an item equipped in the given equipmentslot
        public static bool IsItemEquipped(string CharacterName, EquipmentSlot TargetSlot)
        {
            //Define the query to check this characters equipment slot
            string SlotName = TargetSlot.ToString();
            string EquipmentSlotQuery = "SELECT " + SlotName + "ItemNumber FROM equipments WHERE CharacterName='" + CharacterName + "'";

            //Execute the command to retrieve the ItemNumber value for what item is currently equipped in this characters EquipmentSlot
            MySqlCommand EquipmentSlotCommand = new MySqlCommand(EquipmentSlotQuery, Database.Connection);
            int EquipmentItemNumber = Convert.ToInt32(EquipmentSlotCommand.ExecuteScalar());

            //If the EquipmentItemNumber is 0, this means there is no item currently equipped in that item slot, any other value means there IS an item equipped in that slot
            return EquipmentItemNumber != 0;
        }

        //Returns an InventoryItem object detailing what is currently equipped in a characters given equipment slot
        public static InventoryItem GetEquippedItem(string CharacterName, EquipmentSlot EquipmentSlot)
        {
            //Create the new InventoryItem object to detail what is currently equipped in this inventory slot
            InventoryItem EquippedItem = new InventoryItem();
            EquippedItem.ItemSlot = EquipmentSlot;

            //Define the queries used to fetch the relevant item from the players equipment table
            string SlotName = EquipmentSlot.ToString();
            string ItemNumberQuery = "SELECT " + SlotName + "ItemNumber FROM equipments WHERE CharacterName='" + CharacterName + "'";
            string ItemIDQuery = "SELECT " + SlotName + "ItemID FROM equipments WHERE CharacterName='" + CharacterName + "'";

            //Execute these commands and store the values into the EquippedItem object
            MySqlCommand Command = new MySqlCommand(ItemNumberQuery, Database.Connection);
            EquippedItem.ItemNumber = Convert.ToInt32(Command.ExecuteScalar());
            Command = new MySqlCommand(ItemIDQuery, Database.Connection);
            EquippedItem.ItemID = Convert.ToInt32(Command.ExecuteScalar());

            //Return the final InventoryItem object
            return EquippedItem;
        }

        //Returns a list of all the items currently equipped to a specific character
        public static List<InventoryItem> GetCharactersEquipment(string CharacterName)
        {
            //Create the new list of InventoryItems which list everything currently equipped to the character
            List<InventoryItem> EquippedItems = new List<InventoryItem>();

            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.Head));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.Back));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.Neck));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.LeftShoulder));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.RightShoulder));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.Chest));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.LeftGlove));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.RightGlove));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.Legs));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.LeftHand));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.RightHand));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.LeftFoot));
            EquippedItems.Add(GetEquippedItem(CharacterName, EquipmentSlot.RightFoot));

            //return the final list of items equipped to the character
            return EquippedItems;
        }
    }
}
