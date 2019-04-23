// ================================================================================================================================
// File:        Database.cs
// Description: Allows the server to interact with the local SQL database which is used to store all user account and character info
// ================================================================================================================================

using System;
using System.Collections.Generic;
using BEPUutilities;
using MySql.Data.MySqlClient;
using Server.Items;

namespace Server.Data
{
    public static class Database
    {
        public static MySqlConnection Connection;

        public static bool InitializeDatabase(string IP, string Port)
        {
            //Open connection to the games sql database and tell it to use the gamedatabase
            Connection = new MySqlConnection(CreateConnectionString(IP, Port));
            try
            {
                Connection.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                l.og("failed to connect to the sql database");
                return false;
            }

            MySqlCommand Command = new MySqlCommand("USE gamedatabase", Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Close();
            return true;
        }

        private static string CreateConnectionString(string IP, string Port)
        {
            return  "Server=" + IP + ";" +
                    "Port=" + Port + ";" +
                    "Database;User=;";
        }

        //Checks if any existing user exists with the given account name
        public static bool IsAccountNameAvailable(string AccountName)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool NameAvailable = !Reader.HasRows;
            Reader.Close();
            return NameAvailable;
        }

        //Registers a brand new user account into the database
        public static void RegisterNewAccount(string AccountName, string AccountPassword)
        {
            string Query = "INSERT INTO accounts(Username,Password) VALUES('" + AccountName + "','" + AccountPassword + "')";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Checks if the password matches for the given account
        public static bool IsPasswordCorrect(string AccountName, string AccountPassword)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "' AND Password='" + AccountPassword + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool PasswordMatches = Reader.HasRows;
            Reader.Close();
            return PasswordMatches;
        }

        //Adds an item into the first available slot in the players inventory
        public static void GivePlayerItem(string CharacterName, int ItemNumber)
        {
            //Find which slot of the players inventory we are going to store this item in
            int ItemSlot = GetFirstFreeBagSlot(CharacterName);

            //Update the players inventory to contain this new item
            string Query = "UPDATE inventories SET ItemSlot" + ItemSlot + "='" + ItemNumber + "' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Removes whatever item is in the specified slot of the players inventory
        public static void RemovePlayerItem(string CharacterName, int BagSlot)
        {
            string Query = "UPDATE inventories SET ItemSlot" + BagSlot + "='0' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Adds an item into the specified slot in the players equipment screen
        public static void EquipPlayerItem(string CharacterName, int ItemNumber, EquipmentSlot EquipSlot)
        {
            //Convert the name of the equipment slot to a string to be used in the sql query
            string SlotName = EquipSlot.ToString();
            //Define and use the query to update the players equipment screen
            string Query = "UPDATE equipments SET " + SlotName + "='" + ItemNumber + "' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Finds which is the first free slot in the players inventory
        public static int GetFirstFreeBagSlot(string CharacterName)
        {
            List<int> InventoryContents = GetPlayersInventory(CharacterName);
            for(int i = 0; i < InventoryContents.Count; i++)
            {
                if (InventoryContents[i] == 0)
                    return i + 1;
            }

            return -1;
        }

        //Checks if the target characters inventory is full or not
        public static bool IsBagFull(string CharacterName)
        {
            //Get the characters current inventory contents
            List<int> InventoryContents = GetPlayersInventory(CharacterName);

            //Loop through checking for any empty space
            foreach (int Slot in InventoryContents)
                if (Slot == 0)
                    return false;

            //If no empty space was found the inventory is full
            return true;
        }

        //Checks if a given character name has already been taken or not
        public static bool IsCharacterNameAvailable(string CharacterName)
        {
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool NameAvailable = !Reader.HasRows;
            Reader.Close();
            return NameAvailable;
        }

        //Returns the number of characters that exist in a given users account
        public static int GetCharacterCount(string AccountName)
        {
            string Query = "SELECT CharactersCreated FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        //Returns a list of all the items stored in a players inventory
        public static List<int> GetPlayersInventory(string PlayerName)
        {
            List<int> BagItems = new List<int>();

            for(int i = 0; i < 9; i++)
                BagItems.Add(GetInventoryItem(PlayerName, i + 1));

            return BagItems;
        }

        //Returns the ID of whatever item is stored in a characters inventory
        private static int GetInventoryItem(string PlayerName, int BagSlot)
        {
            string Query = "SELECT ItemSlot" + BagSlot + " from inventories WHERE CharacterName='" + PlayerName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        //Queries the server to find out what the next item id will be for the ItemManager when the server is first starting up
        public static int GetNextItemID()
        {
            string Query = "SELECT NextItemID FROM globals";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        //Saves what the next item ID should be back into the database, usually when the server is shutting down, or during backup intervals etc
        public static void SaveNextItemID(int NewValue)
        {
            string Query = "UPDATE globals SET NextItemID='" + NewValue + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Saves a brand new player character and all of its information into the database
        public static void SaveNewCharacter(string AccountName, string CharacterName, bool IsMale)
        {
            //Store this new characters info in the player character database
            string Query = "INSERT INTO characters(OwnerAccountName,CharacterName,IsMale) VALUES('" + AccountName + "','" + CharacterName + "','" + (IsMale ? 1 : 0) + "')";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();

            //Note how many characters this user has created so far
            int CharacterCount = GetCharacterCount(AccountName) + 1;
            Query = "UPDATE accounts SET CharactersCreated='" + CharacterCount + "' WHERE Username='" + AccountName + "'";
            Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();

            //Reference the players new character in their account info database
            Query = "UPDATE accounts SET " +
                (CharacterCount == 1 ? "FirstCharacterName" : CharacterCount == 2 ? "SecondCharacterName" : "ThirdCharacterName") +
                "='" + CharacterName + "' WHERE Username='" + AccountName + "'";
            Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();

            //Create a new entry into the inventory database to keep track of what items this character has collected
            Query = "INSERT INTO inventories(CharacterName) VALUES('" + CharacterName + "')";
            Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }

        //Returns the name of a users player character being stored in the given character slot
        public static string GetCharacterName(string AccountName, int CharacterSlot)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            string SlotKey = CharacterSlot == 1 ? "FirstCharacterName" :
                CharacterSlot == 2 ? "SecondCharacterName" : "ThirdCharacterName";
            string CharacterName = Reader[SlotKey].ToString();
            Reader.Close();
            return CharacterName;
        }

        //Loads all of a player characters information from the database and returns it all, stored in a CharacterData structure object
        public static CharacterData GetCharacterData(string CharacterName)
        {
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();

            //Create a new character data object with all the information stored inside
            CharacterData Data = new CharacterData
            {
                Account = Reader["OwnerAccountName"].ToString(),
                Position = new Vector3(Convert.ToInt64(Reader["XPosition"]), Convert.ToInt64(Reader["YPosition"]), Convert.ToInt64(Reader["ZPosition"])),
                Name = CharacterName,
                Experience = Convert.ToInt32(Reader["ExperiencePoints"]),
                ExperienceToLevel = Convert.ToInt32(Reader["ExperienceTOLevel"]),
                Level = Convert.ToInt32(Reader["Level"]),
                IsMale = Convert.ToBoolean(Reader["IsMale"])
            };

            Reader.Close();
            return Data;
        }

        //Updates the location of a player character in the database
        public static void SaveCharacterLocation(string CharacterName, Vector3 CharacterLocation)
        {
            string Query = "UPDATE characters SET XPosition='" + CharacterLocation.X + "', YPosition='" + CharacterLocation.Y + "', ZPosition='" + CharacterLocation.Z + "' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
        }
    }
}
