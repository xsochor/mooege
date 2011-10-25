/*
 * Copyright (C) 2011 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using Mooege.Common.Helpers;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message;
using System;

namespace Mooege.Core.Common.Items.ItemCreation
{
    class ArmorAttributeCreator : IItemAttributeCreator
    {
        public void CreateAttributes(Item item)
        {
            /*
            item.Attributes[GameAttribute.Durability_Max] = 400;
            item.Attributes[GameAttribute.Durability_Cur] = 400;

            item.Attributes[GameAttribute.Armor_Item_Total, -1] = 400;
            item.Attributes[GameAttribute.Armor] = 1000;
            item.Attributes[GameAttribute.Armor_Item, -1] = 200;
            item.Attributes[GameAttribute.Armor_Item_SubTotal, -1] = 800;
            item.Attributes[GameAttribute.Armor_Bonus_Percent] = 0f;
            item.Attributes[GameAttribute.Armor_Item_Percent, -1] = 0.5f;
            item.Attributes[GameAttribute.Armor_Bonus_Item, -1] = 30;
            item.Attributes[GameAttribute.Armor_Item_Percent] = 0.3f;
            */
            int ArmorValue = 200;
                        int baseArmor = 100;
            int armorBonus = 0;
            float armorBonusPercentage = 0f;
            float itemPercent = 0f;
            int subTotal = 600;// (int)Math.Floor((double)(ArmorValue + armorBonus) * (itemPercent + 1));                                    
            double itemTotalArmor = (ArmorValue > 0) ? (Math.Max(subTotal, 1)) : subTotal;
            /*
            item.Attributes[GameAttribute.Armor, 0] = baseArmor;
            item.Attributes[GameAttribute.Armor, 0xFFFFF] = baseArmor;
            
            item.Attributes[GameAttribute.Armor_Bonus_Item, 0] = armorBonus;
            item.Attributes[GameAttribute.Armor_Bonus_Percent, 0] = (int)(armorBonusPercentage * 100);
            item.Attributes[GameAttribute.Armor_Item, 0] = ArmorValue;
            item.Attributes[GameAttribute.Armor_Item_Percent, 0] = (int)(itemPercent * 100);
            item.Attributes[GameAttribute.Armor_Item_SubTotal, 0] = subTotal;
            item.Attributes[GameAttribute.Armor_Item_Total, 0] = (int)itemTotalArmor;
            item.Attributes[GameAttribute.Armor_Total, 0] = (int)(Math.Floor((double)(baseArmor + itemTotalArmor)) * (armorBonusPercentage + 1));
            */
            for (int i =0; i<7;i++) {
            item.Attributes[GameAttribute.Armor_Bonus_Item, i] = armorBonus;
            item.Attributes[GameAttribute.Armor_Bonus_Percent, i] = (int)(armorBonusPercentage * 100);
            item.Attributes[GameAttribute.Armor_Item, i] = ArmorValue;
            item.Attributes[GameAttribute.Armor_Item_Percent, i] = (int)(itemPercent * 100);
            item.Attributes[GameAttribute.Armor_Item_SubTotal, i] = subTotal;
            item.Attributes[GameAttribute.Armor_Item_Total, i] = (int)itemTotalArmor;
            item.Attributes[GameAttribute.Armor_Total, i] = (int)(Math.Floor((double)(baseArmor + itemTotalArmor)) * (armorBonusPercentage + 1));
            }
            item.Attributes[GameAttribute.Armor_Bonus_Item, 0xfffffff] = armorBonus;
            item.Attributes[GameAttribute.Armor_Bonus_Percent, 0xfffffff] = (int)(armorBonusPercentage * 100);
            item.Attributes[GameAttribute.Armor_Item, 0xfffffff] = ArmorValue;
            item.Attributes[GameAttribute.Armor_Item_Percent, 0xfffffff] = (int)(itemPercent * 100);
            item.Attributes[GameAttribute.Armor_Item_SubTotal, 0xfffffff] = subTotal;
            item.Attributes[GameAttribute.Armor_Item_Total, 0xfffffff] = (int)itemTotalArmor;
            item.Attributes[GameAttribute.Armor_Total, 0xfffffff] = (int)(Math.Floor((double)(baseArmor + itemTotalArmor)) * (armorBonusPercentage + 1));

            item.Attributes[GameAttribute.Armor] = 1000;
            item.Attributes[GameAttribute.Armor_Bonus_Item] = armorBonus;
            //            item.Attributes[GameAttribute.Armor_Bonus_Percent, i] = (int)(armorBonusPercentage * 100);
            item.Attributes[GameAttribute.Armor_Item] = ArmorValue;
            //            item.Attributes[GameAttribute.Armor_Item_Percent, i] = (int)(itemPercent * 100);
            item.Attributes[GameAttribute.Armor_Item_SubTotal] = subTotal;
            item.Attributes[GameAttribute.Armor_Item_Total] = (int)itemTotalArmor;
            //            item.Attributes[GameAttribute.Armor_Total, i] = (int)(Math.Floor((double)(baseArmor + itemTotalArmor)) * (armorBonusPercentage + 1));

            item.Attributes[GameAttribute.Durability_Max] = 800;
            item.Attributes[GameAttribute.Durability_Cur] = 800;
            item.Attributes[GameAttribute.Damage_Shield] = true;
            if (item.ItemType == ItemType.Shield)
            {
                item.Attributes[GameAttribute.Block_Amount_Item_Delta, 0xfffff] = 50f; // OK -> Max = min + delta
                item.Attributes[GameAttribute.Block_Amount_Item_Min, 0xfffff] = 20f; // OK //
                item.Attributes[GameAttribute.Block_Chance_Item_Total, 0xfffff] = 0.3f; // OK // Block_Chance_Item + Block_Chance_Bonus_Item
            }
            /*
                        // shows 0 armor
                        item.Attributes[GameAttribute.Armor_Item_Total] = 450;
                        item.Attributes[GameAttribute.Armor] = 1000;
                        item.Attributes[GameAttribute.Armor_Item] = 200;
                        */

            /* // shows NOTHING
            item.Attributes[GameAttribute.Armor_Item_Total, 0] = 450;
            item.Attributes[GameAttribute.Armor, 0] = 1000;
            item.Attributes[GameAttribute.Armor_Item, 0] = 200;
            */

        }
    }
}
