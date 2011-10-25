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
            // TODO: randomize this
            item.Attributes[GameAttribute.Durability_Max] = 800; // shows floor(value/10)
            item.Attributes[GameAttribute.Durability_Cur] = 800; // shows floor(value/10)

            item.Attributes[GameAttribute.Armor_Item_Total] = 45;

            if (item.ItemType == ItemType.Shield)
            {
                item.Attributes[GameAttribute.Block_Amount_Item_Delta] = 15f; // OK -> Max = min + delta
                item.Attributes[GameAttribute.Block_Amount_Item_Min] = 5f; // OK //
                item.Attributes[GameAttribute.Block_Chance_Item_Total] = 0.2f; // OK // Block_Chance_Item + Block_Chance_Bonus_Item
            }
        }
    }
}
