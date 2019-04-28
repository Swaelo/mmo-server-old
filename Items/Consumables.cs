// ================================================================================================================================
// File:        Consumables.cs
// Description: Defines all the types of consumable items available in the game
// ================================================================================================================================

namespace Server.Items
{
    //Lists the different types of consumables that are available
    public enum ConsumableTypes
    {
        Potions
    };

    //Lists the different type of potions that are available
    public enum Potions
    {
        HealingPotion = 1,
        ManaPotion = 2
    };
}