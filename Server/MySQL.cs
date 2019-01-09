using System;
using ADODB;

//our SQL database saves the information about every item, every npc etc
namespace Server
{
    class MySQL
    {
        public Recordset DB_RS;
        public Connection DB_CONN;

        public void MySQLInit()
        {
            try
            {
                DB_RS = new Recordset();
                DB_CONN = new Connection();

                DB_CONN.ConnectionString = "Driver={MySQL ODBC 3.51 Driver};Server=localhost;Port=3306;Database=gamedatabase;User=root;Password=;Option=3;";
                DB_CONN.CursorLocation = CursorLocationEnum.adUseServer;
                DB_CONN.Open();
                Console.WriteLine("Connection to MYSQL Server was successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}
