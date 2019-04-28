// ================================================================================================================================
// File:        ItemList.cs
// Description: Lists every item available in the game
// ================================================================================================================================


//The complete list of items available in the game
public enum ItemList
{
    //Potions
    HealingPotion = 1,
    ManaPotion = 2,

    //Weapons
    AstorasStraightSword = 3,
    Kusabimaru = 4,

    //Shields
    CrusadersShield = 5,

    //Armour Pieces
    //Helmets
    BattleHelm = 6,
    //Pauldrons
    LeftBonemouldPauldron = 7,
    RightBonemouldPauldron = 8,
    //Gloves
    LeftClothGlove = 9,
    RightClothGlove = 10,
    //Amulets
    MysteriousAmulet = 11,
    //Cloaks
    LeatherCloak = 12,
    //Chest Pieces
    EpicPurpleShirt = 13,
    MannaAbs = 14,
    //Leggings
    OldPants = 15,
    MannaLeggings = 16,
    //Boots
    LeftNormalBoot = 17,
    LeftMannaSandal = 18,
    RightNormalBoot = 19,
    RightMannaSandal = 20
}

//Takes an item number and tells you what slot that item can be equipped to
public static class BelongingItemSlot
{
    public static EquipmentSlot FindSlot(int ItemNumber)
    {
        //1-2 are potions
        if (ItemNumber > 0 && ItemNumber < 3)
            return EquipmentSlot.NULL;
        //3-4 are weapons, right hand
        else if (ItemNumber > 2 && ItemNumber < 5)
            return EquipmentSlot.RightHand;
        //5 is shields, left hand
        else if (ItemNumber == 5)
            return EquipmentSlot.LeftHand;
        //6 is helmets
        else if (ItemNumber == 6)
            return EquipmentSlot.Head;
        //7-8 is left/right shoulders
        else if (ItemNumber == 7)
            return EquipmentSlot.LeftShoulder;
        else if (ItemNumber == 8)
            return EquipmentSlot.RightShoulder;
        //9-10 is left/right gloves
        else if (ItemNumber == 9)
            return EquipmentSlot.LeftGlove;
        else if (ItemNumber == 10)
            return EquipmentSlot.RightGlove;
        //11 is amulets
        else if (ItemNumber == 11)
            return EquipmentSlot.Neck;
        //12 is cloaks
        else if (ItemNumber == 12)
            return EquipmentSlot.Back;
        //13-14 is shirts/chest armour
        else if (ItemNumber > 12 && ItemNumber < 15)
            return EquipmentSlot.Chest;
        //15-16 is pants/leg armour
        else if (ItemNumber > 14 && ItemNumber < 17)
            return EquipmentSlot.Legs;
        //17-18 left boots, 19-20 right boots
        else if (ItemNumber > 16 && ItemNumber < 19)
            return EquipmentSlot.LeftFoot;
        else if (ItemNumber > 18 && ItemNumber < 21)
            return EquipmentSlot.RightFoot;
        else
            return EquipmentSlot.NULL;
    }
}