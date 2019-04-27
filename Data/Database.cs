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

        //Helps define the connection string used to establish connection between the game server and sql database
        private static string CreateConnectionString(string IP, string Port)
        {
            return "Server=" + IP + ";" +
                    "Port=" + Port + ";" +
                    "Database;User=;";
        }

        //Initializes the connection between the server and the database
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
    }
}
