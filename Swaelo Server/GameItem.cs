using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    //Defines everything about a single item in the game
    class GameItem
    {
        public int ItemID;
        public int ItemNumber;
        public string ItemName;
        public Vector3 ItemPosition;
        public Vector4 ItemRotation;

        public GameItem(int ID, int Number, string Name, Vector3 Position, Vector4 Rotation)
        {
            ItemID = ID;
            ItemNumber = Number;
            ItemName = Name;
            ItemPosition = Position;
            ItemRotation = Rotation;
        }
    }
}
