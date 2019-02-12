using System;
using System.Data.SqlClient;
//using MySql.Data.MySqlClient;

/*
namespace Swaelo_Server
{
    public class RobertSQL
    {
        public static MySQLSettings mySQLSettings;
        public static void ConnectToMySQL()
        {
            mySQLSettings.connection = new MySqlConnection(CreateConnectionString());
            ConnectToMySQLServer();
        }
        public static void ConnectToMySQLServer()
        {
            try
            {
                mySQLSettings.connection.Open();
                Console.WriteLine("Succesfully connected to MySQL Server.");
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); throw; }
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
*/