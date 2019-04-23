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
        Health_Potion,
        Mana_Potion,
        Energy_Potion,
        Damage_Potion
    };

    public class EnumToItemName
    {
        public static string GetItemName(Potions Type)
        {
            switch(Type)
            {
                case (Potions.Health_Potion):
                    return "Health Potion";
                case (Potions.Mana_Potion):
                    return "Mana Potion";
                case (Potions.Energy_Potion):
                    return "Energy Potion";
                case (Potions.Damage_Potion):
                    return "Damage Potion";
            }
            return "";
        }
    }
}