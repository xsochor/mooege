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

namespace Mooege.Core.Common.Items.ItemCreation
{
    class WeaponAttributeCreator : IItemAttributeCreator
    {
        public void CreateAttributes(Item item)
        {
            // TODO: randomize this


            // DPS: (([GameAttribute.Damage_Weapon_Delta_Total_All] / 2) + [GameAttribute.Damage_Weapon_Min_Total_All]) * [GameAttribute.Attacks_Per_Second_Item_Total]
            
            item.Attributes[GameAttribute.Skill, 0x7780] = 1; // basic attack
            item.Attributes[GameAttribute.IdentifyCost] = 1;

            item.Attributes[GameAttribute.Durability_Max] = 400; // shows Floor(value/10)
            item.Attributes[GameAttribute.Durability_Cur] = 400; // shows Floor(value/10)

            item.Attributes[GameAttribute.Damage_Weapon_Min_Total, 0] = 20; // physical, shows in view
            item.Attributes[GameAttribute.Damage_Weapon_Min_Total, 1] = 10; // fire, DOESN'T show in view
            item.Attributes[GameAttribute.Damage_Weapon_Min_Total_All] = 30; // used in DPS, sum of elemental
            item.Attributes[GameAttribute.Damage_Weapon_Max_Total, 0] = 40; // physical, shows in view
            item.Attributes[GameAttribute.Damage_Weapon_Max_Total, 1] = 10; // fire, DOESN'T show in view

            float speed = 1f;
            double r = RandomHelper.NextDouble();
            if (r > 0.75)
            {
                speed = 2f;
            }
            else if (r > 0.5)
            {
                speed = 1.5f;
            }
            else if (r > 0.25)
            {
                speed = 1f;
            }
            else
            {
                speed = 0.5f;
            }
            item.Attributes[GameAttribute.Attacks_Per_Second_Item_Total] = speed; // shows in view, used in DPS
            item.Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = 20f; // used in DPS, sum of elemental 

            item.Attributes[GameAttribute.Item_Equipped] = false;

        }
    }
}
