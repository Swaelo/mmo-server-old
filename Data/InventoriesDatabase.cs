// ================================================================================================================================
// File:        InventoriesDatabase.cs
// Description: Allows the server to interact with the local SQL database inventories tables
// ================================================================================================================================

using System;
using System.Collections.Generic;
using BEPUutilities;
using MySql.Data.MySqlClient;
using Server.Items;

namespace Server.Data
{
    class InventoriesDatabase
    {
        //Returns an InventoryItem object detailing what is currently being stored in a characters specific inventory slot
        public static InventoryItem GetCharactersInventoryItem(string CharacterName, int InventorySlot)
        {
            //Create the new InventoryItem object to store the data about what the character has stored in the given inventory slot
            InventoryItem InventoryItem = new InventoryItem();

            //Define the SQL queries to extract the relevant information from the database
            string ItemNumberQuery = "SELECT ItemSlot" + InventorySlot + "ItemNumber FROM inventories WHERE CharacterName='" + CharacterName + "'";
            string ItemIDQuery = "SELECT ItemSlot" + InventorySlot + "ItemID FROM inventories WHERE CharacterName='" + CharacterName + "'";

            //Use these queries to extract the information from the database and store it in the InventoryItem object
            //First get the ItemNumber value
            MySqlCommand Command = new MySqlCommand(ItemNumberQuery, Database.Connection);
            InventoryItem.ItemNumber = Convert.ToInt32(Command.ExecuteScalar());
            //Second get the ItemID value
            Command = new MySqlCommand(ItemIDQuery, Database.Connection);
            InventoryItem.ItemID = Convert.ToInt32(Command.ExecuteScalar());

            //Return the final InventoryItem object
            return InventoryItem;
        }

        //Returns a list of InventoryItem objects representing every item that is currently being stored in a characters inventory
        //NOTE: Assumes a character with this name already exsits
        public static List<InventoryItem> GetCharactersInventory(string CharacterName)
        {
            //Define the list of InventoryItem objects to list everything the character currently has in their bags
            List<InventoryItem> InventoryItems = new List<InventoryItem>();

            //Loop through each slot of the players inventory and grab a new InventoryItem object detailing what is stored in each inventory slot
            for (int i = 0; i < 9; i++)
                InventoryItems.Add(GetCharactersInventoryItem(CharacterName, i + 1));

            //Return the final list of items being held by the character
            return InventoryItems;
        }

        //Checks if the given characters inventory is currently full or not
        public static bool IsCharactersInventoryFull(string CharacterName)
        {
            //Get the characters current inventory state
            List<InventoryItem> CharactersInventory = GetCharactersInventory(CharacterName);

            //Loop through each of these objects checking the contents of each slot in the characters inventory
            foreach(InventoryItem ItemSlot in CharactersInventory)
            {
                //If we find any ItemSlot with a ItemNumber value of 0, this means that bag slot is currently empty
                //Find just a single inventory slot which is empty means the inventory is not full so we return true
                if (ItemSlot.ItemNumber == 0)
                    return false;
            }

            //If we finished looping through the players inventory and couldnt find any empty slots, then we return false
            return true;
        }

        //Returns the bag slot number which is the first free inventory slot in a characters inventory
        //NOTE: Assumes there is atleast 1 free slot in that characters inventory
        public static int GetFirstFreeInventorySlot(string CharacterName)
        {
            //Fetch the current state of the characters inventory
            List<InventoryItem> CharactersInventory = GetCharactersInventory(CharacterName);

            //Loop through each inventory slot, looking for one which is empty
            for(int i = 0; i < 9; i++)
            {
                //Check if this inventory slot is empty, return this slots index number if it is
                if (CharactersInventory[i].ItemNumber == 0)
                    return i + 1;
            }

            //Return -1 error value if no empty inventory slot could be found
            return -1;
        }

        //Places an item into the first available slot in a characters inventory
        //NOTE: Assumes the character has atleast 1 free slot in their inventory
        public static void GivePlayerItem(string CharacterName, InventoryItem Item)
        {
            //Find the first available slot in the characters inventory
            int AvailableItemSlot = GetFirstFreeInventorySlot(CharacterName);

            //Define the query to update the characters inventory table to store this new item within
            string InventoryUpdateQuery = "UPDATE inventories SET ItemSlot" + AvailableItemSlot + "ItemNumber='" + Item.ItemNumber + "', ItemSlot" + AvailableItemSlot + "ItemID='" + Item.ItemID + "' WHERE CharacterName='" + CharacterName + "'";

            //Execute the command to finish updating the players current inventory state
            MySqlCommand InventoryUpdateCommand = new MySqlCommand(InventoryUpdateQuery, Database.Connection);
            InventoryUpdateCommand.ExecuteNonQuery();
        }

        //Removes whatever item is being stored in a specific slot of a characters inventory
        public static void RemovePlayerItem(string CharacterName, int InventorySlot)
        {
            //Define the query to update the players inventory table
            string InventoryUpdateQuery = "UPDATE inventories SET ItemSlot" + InventorySlot + "ItemNumber='0' WHERE CharacterName='" + CharacterName + "'";
            //Execute the command to complete updating the characters inventory table
            MySqlCommand InventoryUpdateCommand = new MySqlCommand(InventoryUpdateQuery, Database.Connection);
            InventoryUpdateCommand.ExecuteNonQuery();
        }
    }
}
