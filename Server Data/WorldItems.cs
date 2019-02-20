using System;
using System.Collections.Generic;
using System.Text;

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
            var db = Globals.database;
            var rec = db.recorder;
            //find out how many items the database says are on the ground always
            string ItemCountQuery = "SELECT * FROM item_counts WHERE ListCounter='GroundItems'";
            rec.Open(ItemCountQuery, db.connection, db.cursorType, db.lockType);
            double ItemCount = rec.Fields["ItemNumber"].Value;
            Console.WriteLine("There are " + ItemCount + " items on the ground");
            rec.Close();
            //load the information about each item into the active list of ground items
            for (int ItemIterator = 0; ItemIterator < ItemCount; ItemIterator++)
            {
                //Look up all the information about each object from the database, and add them to the active list
                string ItemDataQuery = "SELECT * FROM ground_items WHERE ItemNumber='" + (ItemIterator + 1) + "'";
                rec.Open(ItemDataQuery, db.connection, db.cursorType, db.lockType);
                double ItemID = rec.Fields["ItemID"].Value;
                int ItemNumber = (int)rec.Fields["ItemNumber"].Value;
                string ItemName = rec.Fields["ItemName"].Value;
                Vector3 ItemPosition = new Vector3(rec.Fields["XPosition"].Value, rec.Fields["YPosition"].Value, rec.Fields["ZPosition"].Value);
                Vector4 ItemRotation = new Vector4(rec.Fields["XRotation"].Value, rec.Fields["YRotation"].Value, rec.Fields["ZRotation"].Value, rec.Fields["WRotation"].Value);
                //store all the info in a new object and add it to the list
                GameItem NewItem = new GameItem((int)ItemID, ItemNumber, ItemName, ItemPosition, ItemRotation);
                GroundItems.Add(NewItem);
                Console.WriteLine(ItemName + " is on the ground at " + ItemPosition.X + ", " + ItemPosition.Y + ", " + ItemPosition.Z);
                rec.Close();
            }
        }

        public List<GameItem> GetGroundItems() { return GroundItems; }
        public int GetItemCount() { return GroundItems.Count; }

        public static bool ItemOnGround(int ItemNumber)
        {
            for(int i = 0; i < GroundItems.Count; i++)
            {
                if (GroundItems[i].ItemNumber == ItemNumber)
                    return true;
            }
            return false;
        }

        public static GameItem GetItemByNumber(int ItemNumber)
        {
            for(int i = 0; i < GroundItems.Count; i++)
            {
                if (GroundItems[i].ItemNumber == ItemNumber)
                    return GroundItems[i];
            }
            return null;
        }
    }
}
