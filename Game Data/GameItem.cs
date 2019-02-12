using System;
using System.Collections.Generic;
using System.Text;

namespace Swaelo_Server
{
    //Defines everything about a single item in the game
    class GameItem
    {
        public int ItemID;
        public int ItemNumber;
        public string ItemName;
        public SwaeloMath.Vector3 ItemPosition;
        public SwaeloMath.Vector4 ItemRotation;

        public GameItem(int ID, int Number, string Name, SwaeloMath.Vector3 Position, SwaeloMath.Vector4 Rotation)
        {
            ItemID = ID;
            ItemNumber = Number;
            ItemName = Name;
            ItemPosition = Position;
            ItemRotation = Rotation;
        }
    }
}
