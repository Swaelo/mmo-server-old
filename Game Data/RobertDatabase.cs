using ADODB;
//using MySql.Data.MySqlClient;
using System;

/*
namespace Swaelo_Server
{
    class Database
    {
        public int CharacterCount;
        public void Connect()
        {

        }
        public static void InitializeMySQLServer()
        {
            MySQL.mySQLSettings.user = "root";
            MySQL.mySQLSettings.password = "12345678";
            MySQL.mySQLSettings.server = "localhost";
            MySQL.mySQLSettings.database = "gamedatabase";

            MySQL.ConnectToMySQL();
        }
        public bool DoesCharacterExist(int ClientID, string CharacterName)
        {
            Console.WriteLine("checking if " + CharacterName + " character already exists");
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }
        public bool DoesAccountExist(int ClientID, string AccountName)
        {
            string query = "SELECT Username FROM accounts WHERE username='" + AccountName + "'";
            MySqlCommand cmd = new MySqlCommand(query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }
        public bool DoesPasswordMatch(int ClientID, string Username, string Password)
        {
            string Query = "SELECT * FROM accounts WHERE Username='" + Username + "' AND Password='" + Password + "'";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            string tempPass = string.Empty;

            while (reader.Read())
            {
                tempPass = reader["password"] + "";

            }
            reader.Close();

            if (Password == tempPass) return true;
            else return false;
        }
        public int GetCharacterCount(int ClientID, string AccountName)
        {
            string Query = "SELECT CharactersCreated FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            CharacterCount = Convert.ToInt32(cmd.ExecuteScalar());
            return CharacterCount;
        }
        public CharacterData GetCharacterData(string CharacterName)
        {
            CharacterData Data = new CharacterData();
            string Query = "SELECT * FROM characters WHERE CharacterName='" + CharacterName + "'";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                if (reader.Read() == true)
                {
                    Data.Account = reader["OwnerAccountName"].ToString();
                    Data.Position = new Vector3(Convert.ToInt64(reader["XPosition"]), Convert.ToInt64(reader["YPosition"]), Convert.ToInt64(reader["ZPosition"]));
                    Data.Name = CharacterName;
                    Data.Experience = Convert.ToInt32(reader["ExperiencePoints"]);
                    Data.ExperienceToLevel = Convert.ToInt32(reader["ExperienceTOLevel"]);
                    Data.Level = Convert.ToInt32(reader["Level"]);
                    Data.IsMale = Convert.ToBoolean(reader["IsMale"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            reader.Close();
            return Data;
        }
        public string GetCharacterName(string AccountName, int CharacterIndex)
        {
            string Query = "select * FROM accounts WHERE Username='" + AccountName + "'";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            string CharacterName = "";
            try
            {
                if (reader.Read() == true)
                {

                    switch (CharacterIndex)
                    {
                        case (1):
                            CharacterName = reader["FirstCharacterName"].ToString();
                            break;
                        case (2):
                            CharacterName = reader["SecondCharacterName"].ToString();
                            break;
                        case (3):
                            CharacterName = reader["ThirdCharacterName"].ToString();
                            break;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            reader.Close();
            return CharacterName;
        }
        public void RegisterNewCharacter(string AccountName, string CharacterName, bool IsMale)
        {
            string Query = "INSERT INTO characters(OwnerAccountName,XPosition, YPosition,ZPosition,CharacterName,ExperiencePoints,ExperienceToLevel,Level,IsMale) VALUES('" + AccountName + "','" + 0f + "','" + 0f + "','" + 0f + "','" + CharacterName + "','" + 0 + "','" + 100 + "','" + 1 + "','" + IsMale + "')";
            MySqlCommand cmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Account '{0}' was successfully created! ", AccountName);

            int CharacterCount = GetCharacterCount(0, AccountName);
            switch (CharacterCount)
            {
                case (0):
                    CharacterCount++;
                    string FirstCharacterName = "UPDATE accounts SET FirstCharacterName='" + CharacterName + "' WHERE Username='" + AccountName + "'";
                    MySqlCommand co = new MySqlCommand(FirstCharacterName, MySQL.mySQLSettings.connection);
                    co.ExecuteNonQuery();
                    break;
                case (1):
                    CharacterCount++;
                    string SecondCharacterName = "UPDATE accounts SET SecondCharacterName='" + CharacterName + "' WHERE Username='" + AccountName + "'";
                    MySqlCommand Second = new MySqlCommand(SecondCharacterName, MySQL.mySQLSettings.connection);
                    Second.ExecuteNonQuery();
                    break;
                case (2):
                    CharacterCount++;
                    string ThirdCharacterName = "UPDATE accounts SET ThirdCharacterName='" + CharacterName + "' WHERE Username='" + AccountName + "'";
                    MySqlCommand Third = new MySqlCommand(ThirdCharacterName, MySQL.mySQLSettings.connection);
                    Third.ExecuteNonQuery();
                    break;
            }
            string Coun = "UPDATE accounts SET CharactersCreated='" + CharacterCount + "' WHERE Username='" + AccountName + "'";
            MySqlCommand Cou = new MySqlCommand(Coun, MySQL.mySQLSettings.connection);
            Cou.ExecuteNonQuery();
        }
        public Vector3 GetPlayerLocation(int ClientID, string AccountName)
        {
            //Find the clients row in the account table
            string Query = "SELECT * FROM characters WHERE OwnerAccountName='" + AccountName + "'";
            MySqlCommand ccmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = ccmd.ExecuteReader();
            //Extract the position information from it
            float XPos = Convert.ToInt64(reader["XPosition"]);
            float YPos = Convert.ToInt64(reader["YPosition"]);
            float ZPos = Convert.ToInt64(reader["ZPosition"]);
            //close the database
            reader.Close();
            //return the position values
            return new Vector3(XPos, YPos, ZPos);
        }
        public Vector4 GetPlayerRotation(int ClientID, string AccountName)
        {
            //Find the clients row in the accounts table
            string Query = "SELECT * FROM characters WHERE OwnerAccountName='" + AccountName + "'";
            MySqlCommand ccmd = new MySqlCommand(Query, MySQL.mySQLSettings.connection);
            MySqlDataReader reader = ccmd.ExecuteReader();
            //extract the rotation values from it
            float XRot = Convert.ToInt64(reader["XRotation"]);
            float YRot = Convert.ToInt64(reader["YRotation"]);
            float ZRot = Convert.ToInt64(reader["ZRotation"]);
            float WRot = Convert.ToInt64(reader["WRotation"]);
            //close the database                            )
            reader.Close();
            //return the rotation values
            return new Vector4(XRot, YRot, ZRot, WRot);
        }
    }
}
*/