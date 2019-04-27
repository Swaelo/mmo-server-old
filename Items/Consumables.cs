using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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