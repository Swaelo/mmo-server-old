using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    class Database
    {
        //a function to store new data into the database
        public void AddAccount(string Username, string Password)
        {
            //form a connection to the sql database already being run on this computer
            var db = Globals.mysql.DB_RS;
            {
                db.Open("SELECT * FROM accounts WHERE 0=1", Globals.mysql.DB_CONN, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic);
                db.AddNew();
                db.Fields["Username"].Value = Username;
                db.Fields["Password"].Value = Password;
                db.Update();
                db.Close();
            }
        }

        //Checks if a user account exists inside our database
        public bool DoesAccountExist(int ClientIndex, string AccountName)
        {
            Console.WriteLine("Checking if " + AccountName + " account exists");
            //Fetch our connection to our database
            var Database = Globals.mysql.DB_RS;
            //Query the database for this user account
            Database.Open("SELECT * FROM accounts WHERE Username='" + AccountName + "'", Globals.mysql.DB_CONN, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic);
            //If the entire database was searched without finding the account then it does not exist
            if(Database.EOF)
            {
                Console.WriteLine(AccountName + " account does not exist");
                Database.Close();
                Globals.networkSendData.SendAlertMessage(ClientIndex, "account does not exist");
                return false;
            }
            //The account was found
            Console.WriteLine(AccountName + " account exists");
            Database.Close();
            Globals.networkSendData.SendAlertMessage(ClientIndex, "account exists");
            return true;
        }

        //Checks if the password is correct for the account the user is trying to log into
        //This function assumes you have already check to make sure the account exists
        public bool DoesPasswordMatch(int ClientIndex, string AccountName, string AccountPassword)
        {
            //Fetch our connection to the database
            var Database = Globals.mysql.DB_RS;
            //Send our query to see if the username and password given are a match
            Database.Open("SELECT * FROM accounts WHERE Username='" + AccountName + "' AND Password='" + AccountPassword + "'", Globals.mysql.DB_CONN, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic);
            //Check if the login information was correct, if the entire database was searched without finding what we need then it failed
            bool LoginSuccess = !Database.EOF;
            //Send the player an alert message if their login attempt was rejected
            if(!LoginSuccess)
            {
                //The login attempt was a failure
                Console.WriteLine("Password check failed for account " + AccountName);
                Database.Close();
                Globals.networkSendData.SendAlertMessage(ClientIndex, "password incorrect");
                return false;
            }
            //The login attempt was a success, tell the player they can log into the game now
            Globals.networkSendData.SendAlertMessage(ClientIndex, "login success");
            Database.Close();
            return true;
        }

        //Loads all the player character info from the users account within the database
        //This function assumed you have already checked to make sure the account exists
        public void LoadPlayerData(int ClientIndex, string AccountName)
        {
            Console.WriteLine("Loading " + AccountName + "s player data from the database");
            //fetch our connection to the database
            var Database = Globals.mysql.DB_RS;
            //fetch the info from the database
            Database.Open("Select * FROM accounts WHERE Username='" + AccountName + "'", Globals.mysql.DB_CONN, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic);
            //grab all the info we need from the table
            Vector3 PlayerPosition = new Vector3(Database.Fields["XPosition"].Value, Database.Fields["YPosition"].Value, Database.Fields["ZPosition"].Value);
            Vector4 PlayerRotation = new Vector4(Database.Fields["XRotation"].Value, Database.Fields["YRotation"].Value, Database.Fields["ZRotation"].Value, Database.Fields["WRotation"].Value);
            //Close the database connection
            Database.Close();
            //update the player class with the info from the database
            Globals.LivePlayers[ClientIndex].AccountName = AccountName;
            Globals.LivePlayers[ClientIndex].PlayerPosition = PlayerPosition;
            Globals.LivePlayers[ClientIndex].PlayerRotation = PlayerRotation;
        }

        //Saves all the character info into the users account within the database
        public void SavePlayerData(int ClientIndex)
        {
            Console.WriteLine("Backing up player data for client index: " + ClientIndex);
            //Grab the players character data which we are going to save into the database
            LivePlayer Player = Globals.LivePlayers[ClientIndex];
            Vector3 Position = Player.PlayerPosition;
            Vector4 Rotation = Player.PlayerRotation;
            //fetch our database connection
            var Database = Globals.mysql.DB_RS;
            //query the database to find the table for this user account
            Database.Open("SELECT * FROM accounts WHERE Username='" + Player.AccountName + "'", Globals.mysql.DB_CONN, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic);
            //save all the players position info into the database
            Database.Fields["XPosition"].Value = Position.x;
            Database.Fields["YPosition"].Value = Position.y;
            Database.Fields["ZPosition"].Value = Position.z;
            //save all the players rotation info into the database
            Database.Fields["XRotation"].Value = Rotation.x;
            Database.Fields["YRotation"].Value = Rotation.y;
            Database.Fields["ZRotation"].Value = Rotation.z;
            Database.Fields["WRotation"].Value = Rotation.w;
            //Update the close the database no we have saved all the information into it
            Database.Update();
            Database.Close();
        }
    }
}
