// ================================================================================================================================
// File:        Database.cs
// Description: Along with the server, the game is also supported by an SQL Database which is used for all long term data storage
//              Whenever information needs to be save into the database, or retrieving from within, it will be done in this file
// Author:      Robert        
// Notes:       What is Roberts Surname?
// ================================================================================================================================

using System;
using MySql.Data.MySqlClient;

namespace Swaelo_Server
{
    public struct DatabaseConnectionSettings
    {
        public MySqlConnection Connection;
        public string ServerIP;
        public string ServerPort;
        public string ConnectionString;
        public string Username;
        public string Password;
    }

    public class Database
    {
        public static DatabaseConnectionSettings ConnectionSettings;    //configuration settings for connecting to the sql database

        //Default Constructor
        public Database(string user, string password, string IP, string Port)
        {
            ConnectionSettings.ServerIP = IP;
            ConnectionSettings.ServerPort = Port;
            ConnectionSettings.Username = user;
            ConnectionSettings.Password = password;
            ConnectionSettings.Connection = new MySqlConnection(CreateConnectionString());
            ConnectionSettings.Connection.Open();
            string Query = "USE gamedatabase";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Close();
        }

        //Formats a connection string used to connect to the sql database
        private static string CreateConnectionString()
        {
            var Settings = ConnectionSettings;
            string ConnectionString =
                "Server=" + Settings.ServerIP + ";" +
                "Port=" + Settings.ServerPort + ";" +
                "Database" + Settings.ConnectionString + ";" +
                "User=" + Settings.Password + ";";
            l.o(ConnectionString);
            return ConnectionString;
        }

        //Checks if a player character name has already been taken or not
        public bool IsCharacterNameAvailable(string CharacterName)
        {
            l.o("Checking if the character name " + CharacterName + " is still available");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool NameAvailable = !Reader.HasRows;
            Reader.Close();
            return NameAvailable;
        }

        //Checks if a player account name has already been taken or not
        public bool IsAccountNameAvailable(string AccountName)
        {
            l.o("Checking if the account name " + AccountName + " is still available");
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            //string Query = "USE GameServerDatabase SELECT Username FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool NameAvailable = !Reader.HasRows;
            Reader.Close();
            return NameAvailable;
        }

        //Checks if the given password matches correctly for the given account name
        public bool IsPasswordCorrect(string AccountName, string Password)
        {
            l.o("Checking if " + Password + " is the correct password for the " + AccountName + " account");
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "' AND Password='" + Password + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            bool PasswordMatches = Reader.HasRows;
            Reader.Close();
            return PasswordMatches;
        }

        //Returns the number of characters that exist in a given users account
        public int GetCharacterCount(string AccountName)
        {
            l.o("Checking how many characters " + AccountName + " has created so far");
            string Query = "SELECT CharactersCreated FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        //Loads all of a player characters information from the database and returns it all, stored in a CharacterData structure object
        public CharacterData GetCharacterData(string CharacterName)
        {
            l.o("Retrieving all of " + CharacterName + " character data from the database");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
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

        //Returns the name of a users player character being stored in the given character slot
        public string GetCharacterName(string AccountName, int CharacterSlot)
        {
            l.o("Retrieving the name of " + AccountName + "'s character in slot #" + CharacterSlot);
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            string SlotKey = CharacterSlot == 1 ? "FirstCharacterName" :
                CharacterSlot == 2 ? "SecondCharacterName" : "ThirdCharacterName";
            string CharacterName = Reader[SlotKey].ToString();
            Reader.Close();
            return CharacterName;
        }

        //Saves a new user account info into the database
        public void SaveNewAccount(string AccountName, string Password)
        {
            l.o("Saving the new user account " + AccountName + " into the database");
            string Query = "INSERT INTO accounts(Username,Password) " +
                "VALUES('" + AccountName + "','" + Password + "')";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
        }

        //Saves a brand new player character and all of its information into the database
        public void SaveNewCharacter(string AccountName, string CharacterName, bool IsMale)
        {
            l.o("Saving the new character " + CharacterName + " into the database under " + AccountName + "'s account");
            //First create a new row in the characters table and save all this new characters information there
            string Query = "INSERT INTO characters(OwnerAccountName,XPosition,YPosition,ZPosition,CharacterName,ExperiencePoints,ExperienceToLevel,Level,IsMale) " +
                "VALUES('" + AccountName + "','" + 0f + "','" + 0f + "','" + 0f + "','" + CharacterName + "','" + 0 + "','" + 100 + "','" + 1 + "','" + IsMale + "')";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
            //update this users database account info, so that this new characters name is stored in the correct character slot 
            int CharacterCount = GetCharacterCount(AccountName) + 1;
            Query = "USE GameServerDatabase UPDATE accounts SET " +
                (CharacterCount == 1 ? "FirstCharacterName" : CharacterCount == 2 ? "SecondCharacterName" : "ThirdCharacterName") +
                "='" + CharacterName + "' WHERE Username='" + AccountName + "'";
            Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
            //now update the users account info with the number of characters they have created so far
            Query = "USE GameServerDatabase UPDATE accounts SET CharactersCreated='" + CharacterCount + "' WHERE Username='" + AccountName + "'";
            Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
        }

        //Gets the location of a player character from the database
        public Vector3 GetCharacterLocation(string CharacterName)
        {
            l.o("Retreiving the character " + CharacterName + "'s world location value");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            Vector3 Position = new Vector3(Convert.ToInt64(Reader["XPosition"]), Convert.ToInt64(Reader["YPosition"]), Convert.ToInt64(Reader["ZPosition"]));
            Reader.Close();
            return Position;
        }

        //Updates the location of a player character in the database
        public void SaveCharacterLocation(string CharacterName, Vector3 CharacterLocation)
        {
            l.o("Backing up " + CharacterName + "'s location in the database");
            string Query = "UPDATE characters SET XPosition='" + CharacterLocation.X + "', YPosition='" + CharacterLocation.Y + "', ZPosition='" + CharacterLocation.Z + "' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
        }

        //Gets the rotation of a player character from the database
        public Quaternion GetCharacterRotation(string CharacterName)
        {
            l.o("Retreiving the character " + CharacterName + "'s rotation values");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            MySqlDataReader Reader = Command.ExecuteReader();
            Reader.Read();
            Quaternion Rotation = new Quaternion(Convert.ToInt64(Reader["XRotation"]), Convert.ToInt64(Reader["YRotation"]), Convert.ToInt64(Reader["ZRotation"]), Convert.ToInt64(Reader["WRotation"]));
            Reader.Close();
            return Rotation;
        }

        //Updates the rotation of a player character in the database
        public void SaveCharacterRotation(string CharacterName, Quaternion CharacterRotation)
        {
            l.o("Backing up " + CharacterName + "'s rotation in the database");
            string Query = "UPDATE characters SET XRotation='" + CharacterRotation.X + "', YRotation='" + CharacterRotation.Y + "', ZRotation='" + CharacterRotation.Z + "', WRotation='" + CharacterRotation.W + "' WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand Command = new MySqlCommand(Query, ConnectionSettings.Connection);
            Command.ExecuteNonQuery();
        }
    }
}