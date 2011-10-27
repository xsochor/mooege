﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.Common.Items;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Test;

namespace Mooege.Core.GS.Player
{

    // these ids are transmitted by the client when equipping an item         
    public enum EquipmentSlotId
    {
        Helm = 1, Chest = 2, Off_Hand = 3, Main_Hand = 4, Hands = 5, Belt = 6, Feet = 7,
        Shoulders = 8, Legs = 9, Bracers = 10, Ring_right = 11, Ring_left = 12, Amulett = 13
    }

    class Equipment
    {
        public int EquipmentSlots { get { return _equipment.GetLength(0); } }
        
        private readonly Mooege.Core.GS.Player.Player _owner; // Used, because most information is not in the item class but Actors managed by the world
        private Item _inventoryGold;

        private uint[] _equipment;      // array of equiped items_id  (not item)

        public Equipment(Player owner){
            this._equipment = new uint[16];
            this._inventoryGold = null;           
            this._owner = owner;
        }
       
        /// <summary>
        /// Equips an item in an equipment slot
        /// </summary>
        public void EquipItem(Item item, int slot)
        {
            _equipment[slot] = item.DynamicID;
            item.Owner = _owner;
            item.SetInventoryLocation(slot, 0, 0);
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttributeB.Item_Equipped] = true;
            if (slot == (int)EquipmentSlotId.Off_Hand)
            {
                if (_equipment[(int)EquipmentSlotId.Main_Hand] != 0)
                {
                    map[GameAttribute.Held_In_OffHand] = true;
                    Item mainWeapon = GetEquipment(EquipmentSlotId.Main_Hand);
                    GameAttributeMap other = new GameAttributeMap();
                    other[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = mainWeapon.Attributes[GameAttribute.Damage_Weapon_Delta_Total];
                    other[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = 0;
                    map[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = item.Attributes[GameAttribute.Damage_Weapon_Delta_Total];
                    map[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = 0;
                    other[GameAttribute.Damage_Weapon_Min_Total_MainHand] = mainWeapon.Attributes[GameAttribute.Damage_Weapon_Min_Total];
                    other[GameAttribute.Damage_Weapon_Min_Total_OffHand] = 0;
                    map[GameAttribute.Damage_Weapon_Min_Total_OffHand] = item.Attributes[GameAttribute.Damage_Weapon_Min_Total];
                    map[GameAttribute.Damage_Weapon_Min_Total_MainHand] = 0;
                    mainWeapon.Attributes.CombineMap(other);
                    other.SendMessage(mainWeapon.Owner.InGameClient, mainWeapon.DynamicID);
                }
            }
            else if (slot == (int)EquipmentSlotId.Main_Hand)
            {
                if (_equipment[(int)EquipmentSlotId.Off_Hand] != 0)
                {
                    Item offHandWeapon = GetEquipment(EquipmentSlotId.Off_Hand);
                    GameAttributeMap other = new GameAttributeMap();
                    map[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = item.Attributes[GameAttribute.Damage_Weapon_Delta_Total];
                    other[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = offHandWeapon.Attributes[GameAttribute.Damage_Weapon_Delta_Total];
                    map[GameAttribute.Damage_Weapon_Min_Total_MainHand] = map[GameAttribute.Damage_Weapon_Min_Total];
                    other[GameAttribute.Damage_Weapon_Min_Total_OffHand] = offHandWeapon.Attributes[GameAttribute.Damage_Weapon_Min_Total];
                    other[GameAttribute.Held_In_OffHand] = true;
                    map[GameAttribute.Damage_Weapon_Min_Total_OffHand] = 0;
                    map[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = 0;
                    other[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = 0;
                    other[GameAttribute.Damage_Weapon_Min_Total_MainHand] = 0;
                    offHandWeapon.Attributes.CombineMap(other);
                    other.SendMessage(offHandWeapon.Owner.InGameClient, offHandWeapon.DynamicID);
                }
            }
            item.Attributes.CombineMap(map);
            map.SendMessage(item.Owner.InGameClient, item.DynamicID); // flag item as equipped, so as not to shown in red color
            _equippedMap = null;
            // compute stats (depends on items)
            AttributeMath.ComputeStats(_owner, GetEquippedMap());
        }

        public void EquipItem(uint itemID, int slot)
        {
            EquipItem(_owner.World.GetItem(itemID), slot);
        }

        /// <summary>
        /// Removes an item from the equipment slot it uses
        /// returns the used equipmentSlot
        /// </summary>
        public int UnequipItem(Item item)
        {
            for (int i = 0; i < EquipmentSlots; i++)
            {
                if (_equipment[i] == item.DynamicID)
                {
                    _equipment[i] = 0;
                    item.SetInventoryLocation(-1, -1, -1);
                    GameAttributeMap map = new GameAttributeMap();
                    map[GameAttributeB.Item_Equipped] = false;
                    map.SendMessage(item.Owner.InGameClient, item.DynamicID); // unflag item
                    if (item.Attributes[GameAttribute.Held_In_OffHand])
                    {
                        map[GameAttribute.Held_In_OffHand] = false;
                        if (_equipment[(int)EquipmentSlotId.Main_Hand] != 0)
                        {
                            Item mainWeapon = GetEquipment(EquipmentSlotId.Main_Hand);
                            GameAttributeMap other = new GameAttributeMap();
                            other[GameAttribute.Damage_Weapon_Min_Total_MainHand] = 0;
                            other[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = 0;
                            mainWeapon.Attributes.CombineMap(other);
                            other.SendMessage(mainWeapon.Owner.InGameClient, mainWeapon.DynamicID);
                        }
                    }
                    else if (i == (int)EquipmentSlotId.Main_Hand)
                    {
                        if (_equipment[(int)EquipmentSlotId.Off_Hand] != 0)
                        {
                            Item offHandWeapon = GetEquipment(EquipmentSlotId.Off_Hand);
                            GameAttributeMap other = new GameAttributeMap();
                            other[GameAttribute.Damage_Weapon_Min_Total_OffHand] = 0;
                            other[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = 0;
                            other[GameAttribute.Held_In_OffHand] = false;
                            offHandWeapon.Attributes.CombineMap(other);
                            other.SendMessage(offHandWeapon.Owner.InGameClient, offHandWeapon.DynamicID);
                        }
                    }
                    item.Attributes.CombineMap(map);
                    item.Owner = null;
                    _equippedMap = null;
                    // compute stats (depends on items)
                    AttributeMath.ComputeStats(_owner, GetEquippedMap());
                    return i;
                }
            }

            return 0;
        }     

        /// <summary>
        /// Returns whether an item is equipped
        /// </summary>
        public bool IsItemEquipped(uint itemID)
        {
            for (int i = 0; i < EquipmentSlots; i++)
                if (_equipment[i] == itemID)
                    return true;
            return false;
        }

        public bool IsItemEquipped(Item item)
        {
            return IsItemEquipped(item.DynamicID);
        }

        private VisualItem GetEquipmentItem(EquipmentSlotId equipSlot)
        {
            if (_equipment[(int)equipSlot] == 0)
            {
                return new VisualItem()
                {
                    GbId = -1, // 0 causes error logs on the client  - angerwin
                    Field1 = 0,
                    Field2 = 0,
                    Field3 = 0,
                };
            }
            else
            {
                return _owner.World.GetItem(_equipment[(int)equipSlot]).CreateVisualItem();
            }
        }

        public VisualItem[] GetVisualEquipment(){
            return new VisualItem[8]
                    {
                        GetEquipmentItem(EquipmentSlotId.Helm),
                        GetEquipmentItem(EquipmentSlotId.Chest),
                        GetEquipmentItem(EquipmentSlotId.Feet),
                        GetEquipmentItem(EquipmentSlotId.Hands),
                        GetEquipmentItem(EquipmentSlotId.Main_Hand),
                        GetEquipmentItem(EquipmentSlotId.Off_Hand),
                        GetEquipmentItem(EquipmentSlotId.Shoulders),
                        GetEquipmentItem(EquipmentSlotId.Legs),
                    };
        }

        internal Item AddGoldItem(Item collectedItem)
        {
            if (_inventoryGold == null)
            {
                _inventoryGold = ItemGenerator.CreateGold(_owner, collectedItem.Attributes[GameAttribute.Gold]);
                _inventoryGold.Attributes[GameAttribute.ItemStackQuantityLo] = collectedItem.Attributes[GameAttribute.Gold];
                _inventoryGold.Owner = _owner;
                _inventoryGold.SetInventoryLocation(18, 0, 0); // Equipment slot 18 ==> Gold
                _inventoryGold.Reveal(_owner);
            }
            else
            {
                _inventoryGold.Attributes[GameAttribute.ItemStackQuantityLo] += collectedItem.Attributes[GameAttribute.Gold];
            }

            return _inventoryGold;
        }

        private List<GameAttributeMap> GetEquippedItemsAttributes()
        {
            List<GameAttributeMap> result = new List<GameAttributeMap>();
            for (int i = 0; i < EquipmentSlots; i++)
            {
                if (_equipment[i] == 0)
                {
                    continue;
                }
                result.Add(GetEquipment(i).Attributes);
            }
            return result;
        }

        private GameAttributeMap _equippedMap;

        internal GameAttributeMap GetEquippedMap()
        {
            if (_equippedMap == null)
            {
                _equippedMap = AttributeMath.ComputeEquipment(_owner, GetEquippedItemsAttributes());
            }
            return _equippedMap;
        }

        internal Item GetEquipment(int targetEquipSlot)
        {
            return _owner.World.GetItem(this._equipment[targetEquipSlot]);
        }

        internal Item GetEquipment(EquipmentSlotId targetEquipSlot)
        {
            return GetEquipment((int)targetEquipSlot);
        }
    }
}
