﻿// ================================================================================================================================
// File:        ItemData.cs
// Description: Contains information about a game item and everything about it
// ================================================================================================================================

namespace Server.Items
{
    public class ItemData
    {
        public string ItemName = "";
        public string ItemDisplayName = "";
        public ItemType ItemType = ItemType.NULL;
        public EquipmentSlot ItemEquipmentSlot = EquipmentSlot.NULL;
        public int ItemNumber = 0;
        public int ItemID = 0;

        public int ItemInventorySlot = 0;
        public int ItemActionBarSlot = 0;
    }
}