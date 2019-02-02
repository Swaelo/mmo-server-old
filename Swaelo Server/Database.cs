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

        //Checks if a character exists within the database
        public bool DoesCharacterExist(int ClientID, string CharacterName)
        {
            Console.WriteLine("checking if " + CharacterName + " character already exists");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            bool CharacterExists = !recorder.EOF;
            recorder.Close();
            return CharacterExists;
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

        //Gets the number of characters this user has created so far
        public int GetCharacterCount(int ClientID, string AccountName)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            int CharacterCount = recorder.Fields["CharactersCreated"].Value;
            recorder.Close();
            return CharacterCount;
        }

        public CharacterData GetCharacterData(string CharacterName)
        {
            CharacterData Data = new CharacterData();
            //Extract all of this characters data from the database and save it into this structure object
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            Data.Account = recorder.Fields["OwnerAccountName"].Value;
            Data.Position = new Vector3(recorder.Fields["XPosition"].Value, recorder.Fields["YPosition"].Value, recorder.Fields["ZPosition"].Value);
            Data.Name = CharacterName;
            Data.Experience = recorder.Fields["ExperiencePoints"].Value;
            Data.ExperienceToLevel = recorder.Fields["ExperienceTOLevel"].Value;
            Data.Level = recorder.Fields["Level"].Value;
            Data.IsMale = recorder.Fields["IsMale"].Value == 1;
            recorder.Close();
            return Data;
        }

        public string GetCharacterName(string AccountName, int CharacterIndex)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            string CharacterName = "";
            switch(CharacterIndex)
            {
                case (1):
                    CharacterName = recorder.Fields["FirstCharacterName"].Value;
                    break;
                case (2):
                    CharacterName = recorder.Fields["SecondCharacterName"].Value;
                    break;
                case (3):
                    CharacterName = recorder.Fields["ThirdCharacterName"].Value;
                    break;
            }
            recorder.Close();
            return CharacterName;
        }

        //Registers a new character into the database under a username
        public void RegisterNewCharacter(string AccountName, string CharacterName, bool IsMale)
        {
            //Register this character into the database
            string Query = "SELECT * FROM characters WHERE 0=1";
            recorder.Open(Query, connection, cursorType, lockType);
            recorder.AddNew();
            recorder.Fields["OwnerAccountName"].Value = AccountName;
            recorder.Fields["XPosition"].Value = 0f;
            recorder.Fields["YPosition"].Value = 0f;
            recorder.Fields["ZPosition"].Value = 0f;
            recorder.Fields["CharacterName"].Value = CharacterName;
            recorder.Fields["ExperiencePoints"].Value = 0;
            recorder.Fields["ExperienceToLevel"].Value = 100;
            recorder.Fields["Level"].Value = 1;
            recorder.Fields["IsMale"].Value = (IsMale ? 1 : 0);
            recorder.Update();
            recorder.Close();

            //Update the users account to note that this character belongs to them, and they have used up on of their character slots
            Query = "SELECT * FROM accounts WHERE Username='" + AccountName + "'";
            recorder.Open(Query, connection, cursorType, lockType);
            int CharacterCount = recorder.Fields["CharactersCreated"].Value;
            switch(CharacterCount)
            {
                case (0):
                    recorder.Fields["FirstCharacterName"].Value = CharacterName;
                    break;
                case (1):
                    recorder.Fields["SecondCharacterName"].Value = CharacterName;
                    break;
                case (2):
                    recorder.Fields["ThirdCharacterName"].Value = CharacterName;
                    break;
            }
            CharacterCount++;
            recorder.Fields["CharactersCreated"].Value = CharacterCount;
            recorder.Update();
            recorder.Close();
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