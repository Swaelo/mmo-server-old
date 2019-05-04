// ================================================================================================================================
// File:        Item.cs
// Description: Stores information for an ingame item
// ================================================================================================================================

using BEPUphysics.Entities.Prefabs;
using BEPUutilities;

namespace Server.Items
{
    public class Item
    {
        public string Name; //The name of this item
        public string Type; //The type of this item
        public int Number; //Unique Item Number Identifier
        public int ID;  //Unique Item Network ID
        public Box Collider;    //Physics collider so this item can be dropped item the game world

        public Item(string ItemName, string ItemType, int ItemNumber, int ItemID, Vector3 ItemLocation)
        {
            //Store all the item values in the class variables
            Name = ItemName;
            Type = ItemType;
            Number = ItemNumber;
            ID = ItemID;

            //Instantiate a new box collider into the server game world and start rendering it so we can see where the item is
            Collider = new Box(ItemLocation, .25f, .25f, .25f);
            Physics.WorldSimulator.Space.Add(Collider);
            Rendering.Window.Instance.ModelDrawer.Add(Collider);
        }
    }
}