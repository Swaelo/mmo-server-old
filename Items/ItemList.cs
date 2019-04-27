using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
    BattleHelm = 6,
    LeftBonemouldPauldron = 7,
    RightBonemouldPauldron = 8,
    LeftClothGlove = 9,
    RightClothGlove = 10,
    MysteriousAmulet = 11,
    LeatherCloak = 12,
    EpicPurpleShirt = 13,
    OldPants = 14,
    LeftNormalBoot = 15,
    RightNormalBoot = 16
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
        //13 is shirts/chest armour
        else if (ItemNumber == 13)
            return EquipmentSlot.Chest;
        //14 is pants/leg armour
        else if (ItemNumber == 14)
            return EquipmentSlot.Legs;
        //15-16 is left/right boots
        else if (ItemNumber == 15)
            return EquipmentSlot.LeftFoot;
        else if (ItemNumber == 16)
            return EquipmentSlot.RightFoot;
        else
            return EquipmentSlot.NULL;
    }
}