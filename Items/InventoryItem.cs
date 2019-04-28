// ================================================================================================================================
// File:        InventoryItem.cs
// Description: Keeps information regarding a single game item currently in the players inventory or equipment
// ================================================================================================================================


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
