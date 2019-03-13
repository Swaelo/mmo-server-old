// ================================================================================================================================
// File:        Database.cs
// Description: Allows the server to interact with the local SQL database which is used to store all user account and character info
// ================================================================================================================================

using System;
using BEPUutilities;
using MySql.Data.MySqlClient;

namespace Server.Data
{
    public static class Database
    {
        public static MySqlConnection Connection;

        public static void InitializeDatabase(string IP, string Port)
        {
            //Open connection to the games sql database and tell it to use the gamedatabase
            Connection = new MySqlConnection(CreateConnectionString(IP, Port));
            Connection.Open();
            MySqlCommand Command = new MySqlCommand("USE gamedatabase", Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Close();
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

        //Saves a brand new player character and all of its information into the database
        public static void SaveNewCharacter(string AccountName, string CharacterName, bool IsMale)
        {
            //First create a new row in the characters table and save all this new characters information there
            string Query = "INSERT INTO characters(OwnerAccountName,XPosition,YPosition,ZPosition,CharacterName,ExperiencePoints,ExperienceToLevel,Level,IsMale) " +
                "VALUES('" + AccountName + "','" + 0f + "','" + 0f + "','" + 0f + "','" + CharacterName + "','" + 0 + "','" + 100 + "','" + 1 + "','" + IsMale + "')";
            MySqlCommand Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
            //update this users database account info, so that this new characters name is stored in the correct character slot 
            int CharacterCount = GetCharacterCount(AccountName) + 1;
            Query = "USE GameServerDatabase UPDATE accounts SET " +
                (CharacterCount == 1 ? "FirstCharacterName" : CharacterCount == 2 ? "SecondCharacterName" : "ThirdCharacterName") +
                "='" + CharacterName + "' WHERE Username='" + AccountName + "'";
            Command = new MySqlCommand(Query, Connection);
            Command.ExecuteNonQuery();
            //now update the users account info with the number of characters they have created so far
            Query = "USE GameServerDatabase UPDATE accounts SET CharactersCreated='" + CharacterCount + "' WHERE Username='" + AccountName + "'";
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
            CharacterData Data = new CharacterData();
            Data.Account = Reader["OwnerAccountName"].ToString();
            Data.Position = new Vector3(Convert.ToInt64(Reader["XPosition"]), Convert.ToInt64(Reader["YPosition"]), Convert.ToInt64(Reader["ZPosition"]));
            Data.Name = CharacterName;
            Data.Experience = Convert.ToInt32(Reader["ExperiencePoints"]);
            Data.ExperienceToLevel = Convert.ToInt32(Reader["ExperienceTOLevel"]);
            Data.Level = Convert.ToInt32(Reader["Level"]);
            Data.IsMale = Convert.ToBoolean(Reader["IsMale"]);
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
