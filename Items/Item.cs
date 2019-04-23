using BEPUphysics.Entities.Prefabs;

namespace Server.Items
{
    public class Item
    {
        public string Name; //The name of this item
        public string Type; //The type of this item
        public int ID;  //The unique ID number assigned to this item
        public Box Collider;    //Physics collider so this item can be dropped item the game world

        public Item(string Name, int ID)
        {
            this.Name = Name;
            this.ID = ID;
        }
    }
}