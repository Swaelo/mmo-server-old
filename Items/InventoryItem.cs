using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Items
{
    class InventoryItem
    {
        public int ItemNumber = 0;
        public int ItemID = 0;
        public EquipmentSlot ItemSlot = EquipmentSlot.NULL;

        //Tells you if this item can be equipped or not
        public bool CanEquip()
        {
            if (ItemSlot == EquipmentSlot.NULL)
                return false;
            return true;
        }
    }
}
