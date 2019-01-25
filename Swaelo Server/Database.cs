using System;
using ADODB;

namespace Swaelo_Server
{
    class Database
    {
        public Recordset recorder;
        public Connection connection;
        public CursorTypeEnum cursorType = ADODB.CursorTypeEnum.adOpenStatic;
        public LockTypeEnum lockType = ADODB.LockTypeEnum.adLockOptimistic;

        public void Connect()
        {
            recorder = new Recordset();
            connection = new Connection();
            connection.ConnectionString = "Driver={MySQL ODBC 3.51 Driver};Server=localhost;Port=3306;Database=gamedatabase;User=root;Password=;Option=3;";
            connection.CursorLocation = CursorLocationEnum.adUseServer;
            connection.Open();
        }

        //Checks if an account exists within the database
        public bool DoesAccountExist(int ClientID, string AccountName)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            if (connection.State == 0)
            {
                Console.WriteLine("database connection is not open, reopening");
                recorder = new Recordset();
                connection = new Connection();
                connection.ConnectionString = "Driver={MySQL ODBC 3.51 Driver};Server=localhost;Port=3306;Database=gamedatabase;User=root;Password=;Option=3;";
                connection.CursorLocation = CursorLocationEnum.adUseServer;
                connection.Open();
            }
            recorder.Open(Query, connection, cursorType, lockType);
            //If we searched the entire database then the account doesnt exist
            bool AccountExists = !recorder.EOF;
            recorder.Close();
            return AccountExists;
        }

        //Checks if the password for a given username is a match
        public bool DoesPasswordMatch(int ClientID, string Username, string Password)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + Username + "' AND Password='" + Password + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            bool LoginSuccess = !recorder.EOF;
            recorder.Close();
            return LoginSuccess;
        }

        //Gets the character position data from a users account in the database
        public Vector3 GetPlayerLocation(int ClientID, string AccountName)
        {
            //Find the clients row in the account table
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            //Extract the position information from it
            float XPos = recorder.Fields["XPosition"].Value;
            float YPos = recorder.Fields["YPosition"].Value;
            float ZPos = recorder.Fields["ZPosition"].Value;
            //close the database
            recorder.Close();
            //return the position values
            return new Vector3(XPos, YPos, ZPos);
        }

        //Get the character rotation data from a users account in the database
        public Vector4 GetPlayerRotation(int ClientID, string AccountName)
        {
            //Find the clients row in the accounts table
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            //extract the rotation values from it
            float XRot = recorder.Fields["XRotation"].Value;
            float YRot = recorder.Fields["YRotation"].Value;
            float ZRot = recorder.Fields["ZRotation"].Value;
            float WRot = recorder.Fields["WRotation"].Value;
            //close the database
            recorder.Close();
            //return the rotation values
            return new Vector4(XRot, YRot, ZRot, WRot);
        }
    }
}