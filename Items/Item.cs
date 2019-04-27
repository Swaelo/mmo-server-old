using BEPUphysics.Entities.Prefabs;
using BEPUutilities;

namespace Server.Items
{
    public class Item
    {
        public string Name; //The name of this item
        public string Type; //The type of this item
        public int ItemNumber; //Unique Item Number Identifier
        public int ItemID;  //Unique Item Network ID
        public Box Collider;    //Physics collider so this item can be dropped item the game world

        public Item(string Name, int ID)
        {
            this.Name = Name;
            this.ItemID = ID;
        }
        
        public Item(string ItemName, string ItemType, Vector3 ItemPosition, int ItemNumber, int ItemID)
        {
            //Store values
            Name = ItemName;
            Type = ItemType;
            this.ItemNumber = ItemNumber;
            this.ItemID = ItemID;

            //Create physics shape and render it
            Collider = new Box(ItemPosition, .25f, .25f, .25f);
            Physics.WorldSimulator.Space.Add(Collider);
            Rendering.Window.Instance.ModelDrawer.Add(Collider);
        }
    }
}