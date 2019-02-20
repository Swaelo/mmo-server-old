using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    //The game server keeps track of a list of all items which are currently on the ground and where they are
    //Clients are told where all the items are when they connect, and cross communication ensures world synchronization between all clients
    class WorldItems
    {
        public static List<GameItem> GroundItems = new List<GameItem>();

        public void LoadGroundItems()
        {
            //Load all the ground items from the database
            double ItemCount;
            try
            {
                string ItemCountQuery = "SELECT count(ItemNumber) FROM item_counts";
                MySqlCommand cmd = new MySqlCommand(ItemCountQuery, MySQL.mySQLSettings.connection);
                ItemCount = Convert.ToDouble(cmd.ExecuteScalar());
                Console.WriteLine("There are " + ItemCount + " items on the ground");

                for (int ItemIterator = 0; ItemIterator < ItemCount; ItemIterator++)
                {
                    //Look up all the information about each object from the database, and add them to the active list
                    string ItemDataQuery = "SELECT * FROM ground_items WHERE ItemNumber='" + (ItemIterator + 1) + "'";
                    MySqlCommand ItemDataQueryy = new MySqlCommand(ItemDataQuery, MySQL.mySQLSettings.connection);
                    MySqlDataReader reader = ItemDataQueryy.ExecuteReader();
                    if (reader.Read() == true)
                    {
                        double ItemID = Convert.ToDouble(reader["ItemID"]);
                        int ItemNumber = Convert.ToInt32(reader["ItemNumber"]);
                        string ItemName = reader["ItemName"].ToString();
                        Vector3 ItemPosition = new Vector3(Convert.ToInt64(reader["XPosition"]), Convert.ToInt64(reader["YPosition"]), Convert.ToInt64(reader["ZPosition"]));
                        Vector4 ItemRotation = new Vector4(Convert.ToInt64(reader["XRotation"]), Convert.ToInt64(reader["YRotation"]), Convert.ToInt64(reader["ZRotation"]), Convert.ToInt64(reader["WRotation"]));
                        //store all the info in a new object and add it to the list
                        GameItem NewItem = new GameItem((int)ItemID, ItemNumber, ItemName, ItemPosition, ItemRotation);
                        GroundItems.Add(NewItem);
                        Console.WriteLine(ItemName + " is on the ground at " + ItemPosition.X + ", " + ItemPosition.Y + ", " + ItemPosition.Z);
                        reader.Close();
                    }
                    else
                    {

                    }


                }

            }
            catch (Exception)
            {
                throw;
            }
            //load the information about each item into the active list of ground items
        }

        public List<GameItem> GetGroundItems() { return GroundItems; }
        public int GetItemCount() { return GroundItems.Count; }

        public static bool ItemOnGround(int ItemNumber)
        {
            for (int i = 0; i < GroundItems.Count; i++)
            {
                if (GroundItems[i].ItemNumber == ItemNumber)
                    return true;
            }
            return false;
        }

        public static GameItem GetItemByNumber(int ItemNumber)
        {
            for (int i = 0; i < GroundItems.Count; i++)
            {
                if (GroundItems[i].ItemNumber == ItemNumber)
                    return GroundItems[i];
            }
            return null;
        }
    }
}
