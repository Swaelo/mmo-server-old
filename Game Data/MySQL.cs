using System;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;


namespace Swaelo_Server
{
    public class MySQL
    {
        public static MySQLSettings mySQLSettings;
        public static bool ConnectToMySQL()
        {
            bool IsTrue = false;
            mySQLSettings.connection = new MySqlConnection(CreateConnectionString());
            try
            {
                mySQLSettings.connection.Open();
                IsTrue = true;
            }
            catch (Exception)
            {

                IsTrue = false;
            }

            return IsTrue;
        }
        public static void CloseConnection() { mySQLSettings.connection.Close(); }
        private static string CreateConnectionString()
        {
            var db = mySQLSettings;
            string connectionString = "SERVER=" + db.server + ";" +
                "DATABASE=" + db.database + ";" +
                "User=" + db.user + ";" +
                "PASSWORD=" + db.password + ";";
            return connectionString;
        }
    }
    public struct MySQLSettings
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string user;
        public string password;
    }
}

