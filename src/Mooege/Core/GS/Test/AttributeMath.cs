using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.FXEffect;
using Mooege.Common.Helpers;
using Mooege.Common;

namespace Mooege.Core.GS.Test
{
    public class AttributeMath
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        /*
         * #NONE means 0xFFFFF
         */
        /* // dmg attributes
            attacker.Attributes[GameAttribute.Attacks_Per_Second] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Bonus] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Bonus] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_CurrentHand] = 0; // (DualWield_Hand ? Attacks_Per_Second_Item_OffHand : Attacks_Per_Second_Item_MainHand)
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_MainHand] = 0; // (Held_In_OffHand ? 0 : Attacks_Per_Second_Item_Subtotal )
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_OffHand] = 0; // (Held_In_OffHand ? Attacks_Per_Second_Item_Subtotal : 0)
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Percent] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Subtotal] = 0; // Attacks_Per_Second_Item * (1 + Attacks_Per_Second_Item_Percent)
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Total] = 0; // (Attacks_Per_Second_Item_Subtotal + Attacks_Per_Second_Item_Bonus)
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Total_MainHand] = 0; // Attacks_Per_Second_Item_MainHand + Attacks_Per_Second_Item_Bonus
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Item_Total_OffHand] = 0; // Attacks_Per_Second_Item_OffHand + Attacks_Per_Second_Item_Bonus
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Percent] = 0;
            attacker.Attributes[GameAttribute.Attacks_Per_Second_Total] = 0; // (((Attacks_Per_Second_Item_CurrentHand > 0.0) ? Attacks_Per_Second_Item_CurrentHand : Attacks_Per_Second) + Attacks_Per_Second_Bonus + Attacks_Per_Second_Item_Bonus) * Max(0.1, (1 + Attacks_Per_Second_Percent))


        defender.Attributes[Thorns_Percent] = 0f;
        defender.Attributes[Thorns_Percent_All] = 0f; // elemental
        defender.Attributes[Thorns_Percent_Total] = 0f; // Thorns_Percent + Thorns_Percent_All#NONE
        defender.Attributes[Thorns_Fixed] = 0f; // elemental
        attacker.Attributes[GameAttribute.Damage_Absorb_Percent] = 0f;
        attacker.Attributes[GameAttribute.Damage_Absorb_Percent_All] = 0;
        attacker.Attributes[GameAttribute.Damage_Done_Reduction_Percent] = 0;
        // reduction
        attacker.Attributes[GameAttribute.Damage_Reduction_Last_Tick] = 0; // looks like reduction can occur only once per xxx ticks
        attacker.Attributes[GameAttribute.Damage_Reduction_Current] = 0;
        attacker.Attributes[GameAttribute.Damage_Reduction_Total] = 0;
        // override
        attacker.Attributes[GameAttribute.Damage_Type_Override] = 0; // why float?

        attacker.Attributes[GameAttribute.Damage_Type_Percent_Bonus] = 0; // elemental
        attacker.Attributes[GameAttribute.DamageCap_Percent] = 0; // maximal percent to take from defender?
        attacker.Attributes[GameAttribute.Damage_Bonus_Min] = 0;
        attacker.Attributes[GameAttribute.Damage_Delta] = 0;
        attacker.Attributes[GameAttribute.Damage_Delta_Total] = 0; // Max(Damage_Delta - Damage_Bonus_Min + Damage_Weapon_Delta_Total_CurrentHand, 0)
        attacker.Attributes[GameAttribute.Damage_Done_Reduction_Percent] = 0; // attacker's decrease
        attacker.Attributes[GameAttribute.Damage_Min] = 0;
        attacker.Attributes[GameAttribute.Damage_Min_Subtotal] = 0; // Damage_Min + Damage_Bonus_Min + Damage_Weapon_Min_Total_CurrentHand
        attacker.Attributes[GameAttribute.Damage_Min_Total] = 0; // Damage_Min_Subtotal + Damage_Type_Percent_Bonus * Damage_Min_Subtotal#Physical
        attacker.Attributes[GameAttribute.Damage_Percent_All_From_Skills] = 0;
        attacker.Attributes[GameAttribute.Damage_Percent_Bonus_Witchdoctor] = 0; // item mod
        attacker.Attributes[GameAttribute.Damage_Percent_Bonus_Wizard] = 0; // item mod
        attacker.Attributes[GameAttribute.Damage_Power_Delta] = 0;
        attacker.Attributes[GameAttribute.Damage_Power_Min] = 0;
        attacker.Attributes[GameAttribute.Damage_Shield] = false; // flag if destroying shield (aka bonus hitpoints)?
        attacker.Attributes[GameAttribute.Damage_State_Current] = 0; // ???
        attacker.Attributes[GameAttribute.Damage_State_Max] = 0; // ???
        attacker.Attributes[GameAttribute.Damage_To_Mana] = 0; // percent to damage mana instead of health
        // TODO: should be computed in inventory manipulation
        // TODO: dual wielding
        attacker.Attributes[GameAttribute.Damage_Weapon_Bonus_Delta] = 0; // elemental
        attacker.Attributes[GameAttribute.Damage_Weapon_Bonus_Min] = 0; // elemental
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta] = 0; // elemental
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_SubTotal] = 0; // (Damage_Weapon_Delta > 0.0) ? (Max(1, Damage_Weapon_Delta - Damage_Weapon_Bonus_Min)) : Damage_Weapon_Delta
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total] = 0; // elemental Max((Damage_Weapon_Delta_SubTotal + Damage_Weapon_Bonus_Delta) * (1 + Damage_Weapon_Percent_Total), 0)
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = 0; // sum of elemental totals
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand] = 0; // (DualWield_Hand#NONE ? Damage_Weapon_Delta_Total_OffHand : Damage_Weapon_Delta_Total_MainHand)
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total_MainHand] = 0; // (Held_In_OffHand#NONE ? 0 : Damage_Weapon_Delta_Total )
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total_OffHand] = 0; // (Held_In_OffHand#NONE ? Damage_Weapon_Delta_Total : 0)
        attacker.Attributes[GameAttribute.Damage_Weapon_Max] = 0; // (Damage_Weapon_Min_Total + Damage_Weapon_Delta_Total)
        attacker.Attributes[GameAttribute.Damage_Weapon_Max_Total] = 0; // elemental (Damage_Weapon_Min_Total + Damage_Weapon_Delta_Total)
        attacker.Attributes[GameAttribute.Damage_Weapon_Min] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Min_Total] = 0; // elemental (Damage_Weapon_Min + Damage_Weapon_Bonus_Min) * (1 + Damage_Weapon_Percent_Total)
        attacker.Attributes[GameAttribute.Damage_Weapon_Min_Total_All] = 0; // sum of elemental totals
        attacker.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand] = 0; // (DualWield_Hand#NONE ? Damage_Weapon_Min_Total_OffHand : Damage_Weapon_Min_Total_MainHand)
        attacker.Attributes[GameAttribute.Damage_Weapon_Min_Total_MainHand] = 0; // (Held_In_OffHand#NONE ? 0 : Damage_Weapon_Min_Total )
        attacker.Attributes[GameAttribute.Damage_Weapon_Min_Total_OffHand] = 0; // (Held_In_OffHand#NONE ? Damage_Weapon_Min_Total : 0)
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_All] = 0; // LOOKS like elemental
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_Bonus] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_Total] = 0; // Damage_Weapon_Percent_Bonus + Damage_Weapon_Percent_All#NONE
                    attribs[GameAttribute.Attack_Cooldown_Delta] = 0; // monster's?
            attribs[GameAttribute.Attack_Cooldown_Delta_Total] = 0; // monster's?
            attribs[GameAttribute.Attack_Cooldown_Min] = 0; // monster's?
            attribs[GameAttribute.Attack_Cooldown_Min_Total] = 0; // monster's?
            attribs[GameAttribute.Attack_Slow] = false; // ???
            attribs[GameAttribute.Attack_Fear_Chance] = 0;
            attribs[GameAttribute.Attack_Fear_Time_Delta] = 0;
            attribs[GameAttribute.Attack_Fear_Time_Min] = 0;

        */

        /*
         // armor
            attribs[GameAttribute.Armor] = 10;
            attribs[GameAttribute.Armor_Bonus_Item] = 0;
            attribs[GameAttribute.Armor_Bonus_Percent] = 0;
            attribs[GameAttribute.Armor_Item] = 10;
            attribs[GameAttribute.Armor_Item_Percent] = 0;
            attribs[GameAttribute.Armor_Item_SubTotal] = 10; // FLOOR((Armor_Item + Armor_Bonus_Item) * (Armor_Item_Percent + 1))
            attribs[GameAttribute.Armor_Item_Total] = 10; // (Armor_Item > 0)?(Max(Armor_Item_SubTotal, 1)):Armor_Item_SubTotal
            attribs[GameAttribute.Armor_Total] = 20; // FLOOR((Armor + Armor_Item_Total) * (Armor_Bonus_Percent + 1))
        */
        /* // all attributes on resource
           map[GameAttribute.Resource_Cur, resourceID] = 0f;
           map[GameAttribute.Resource_Capacity_Used] = 0f; // lowers max by this amount
           map[GameAttribute.Resource_Cost_Reduction_Amount] = 0;
           map[GameAttribute.Resource_Cost_Reduction_Percent] = 0f;
           map[GameAttribute.Resource_Cost_Reduction_Total] = 0f;
           map[GameAttribute.Resource_Degeneration_Prevented] = false;
           map[GameAttribute.Resource_Degeneration_Stop_Point] = 0f;
           map[GameAttribute.Resource_Effective_Max] = 0f; // total_max - capacity used
           map[GameAttribute.Spending_Resource_Heals_Percent] = 0f;
           map[GameAttribute.Resource_Factor_Level] = 0f;
           map[GameAttribute.Resource_Gain_Bonus_Percent] = 0f;
           map[GameAttribute.Resource_Granted] = 0f;
           map[GameAttribute.Resource_Granted_Duration] = 0;
           map[GameAttribute.Resource_Max] = 0f;
           map[GameAttribute.Resource_Max_Bonus] = 0f;
           map[GameAttribute.Resource_Max_Percent_Bonus] = 0f;
           map[GameAttribute.Resource_Max_Total] = 0f;
           map[GameAttribute.Resource_On_Crit] = 0f;
           map[GameAttribute.Resource_On_Hit] = 0f;
           map[GameAttribute.Resource_On_Kill] = 0f;
           map[GameAttribute.Resource_Percent] = 0f;
           map[GameAttribute.Resource_Regen_Bonus_Percent] = 0f;
           map[GameAttribute.Resource_Regen_Per_Second] = 0f;
           map[GameAttribute.Resource_Regen_Percent_Per_Second] = 0f;
           map[GameAttribute.Resource_Regen_Total] = 0f;
           map[GameAttribute.Resource_Set_Point_Bonus] = 0f;
           map[GameAttribute.Resource_Type_Primary] = 0;
           map[GameAttribute.Resource_Type_Secondary] = 0;
           */
        public static GameAttributeMap ModifyResource(Player.Player player, int resourceID, float amount)
        {
            GameAttributeMap map = new GameAttributeMap();
            if (amount < 0f)
            {
                // spending resource
                amount += player.Attributes[GameAttribute.Resource_Cost_Reduction_Total, resourceID]; // cost reduction
            }
            float temp = player.Attributes[GameAttribute.Resource_Cur, resourceID] + amount;
            // TODO: add regen - needs accurate ticking
            if (temp > player.Attributes[GameAttribute.Resource_Max_Total, resourceID])
            {
                temp = player.Attributes[GameAttribute.Resource_Max_Total, resourceID];
            }
            else if (temp <= 0f)
            {//TODO: set 0
                temp = player.Attributes[GameAttribute.Resource_Max_Total, resourceID];//0;
            }
            if (temp != player.Attributes[GameAttribute.Resource_Cur, resourceID])
            {
                map[GameAttribute.Resource_Cur, resourceID] = temp;
                player.Attributes[GameAttribute.Resource_Cur, resourceID] = temp;
            }
            map[GameAttribute.Resource_Set_Point_Bonus, resourceID] = amount;
            return map;
        }

        private static void ComputeResourceSpent(Player.Player player, float amount)
        {
        }

        public static GameAttributeMap ComputeResourceRegen(Player.Player player, int resourceID)
        {
            float total = player.Attributes[GameAttribute.Resource_Regen_Per_Second, resourceID] * (1 + player.Attributes[GameAttribute.Resource_Regen_Bonus_Percent, resourceID]) + 
                (player.Attributes[GameAttribute.Resource_Regen_Percent_Per_Second, resourceID] * player.Attributes[GameAttribute.Resource_Max_Total, resourceID]);
            GameAttributeMap map = new GameAttributeMap();
            int index = -1;
            if (resourceID == 6) // Discipline
            {
                index = 6; // NOT working
            }
            // without value or -1 - regens primary
            if (total != player.Attributes[GameAttribute.Resource_Regen_Total, index])
            {
                map[GameAttribute.Resource_Regen_Total, index] = total;
                player.Attributes[GameAttribute.Resource_Regen_Total, index] = total;
            }
            return map;
        }

        public static GameAttributeMap CooldownStart(Player.Player player, int PowerSNO, int startTick, int seconds)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Power_Cooldown_Start, PowerSNO] = startTick;
            map[GameAttribute.Power_Cooldown, PowerSNO] = startTick + (60 * seconds);
            player.World.AddEffect(new CooldownStopEffect { Actor = player, EffectID = PowerSNO, StartingTick = startTick + (60 * seconds) });
            return map;
        }

        public static GameAttributeMap CooldownStop(Player.Player player, int PowerSNO)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Power_Cooldown, PowerSNO] = 0;
            return map;
        }

        public static GameAttributeMap BuffIconStart(Player.Player player, int PowerSNO, int startTick, int seconds)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Buff_Icon_Count0, PowerSNO] = player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] + 1;
            map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = startTick;
            map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = startTick + (60 * seconds);
            if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
            {
                map.CombineMap(BuffStart(player, PowerSNO));
            }
            player.Attributes.CombineMap(map);
            return map;
        }

        public static GameAttributeMap BuffStart(Actor actor, int PowerSNO)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Buff_Active, PowerSNO] = true;
            map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // TODO: expand to deal with runed powers
            actor.Attributes.CombineMap(map);
            return map;
        }

        public static GameAttributeMap BuffIconStop(Player.Player player, int PowerSNO)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Buff_Icon_Count0, PowerSNO] = player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] - 1;
            if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0)
            {
                map.CombineMap(BuffStop(player, PowerSNO));
            } else if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] < 0) {
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = 0;
            }
            player.Attributes.CombineMap(map);
            return map;
        }

        public static GameAttributeMap BuffStop(Actor actor, int PowerSNO)
        {
            GameAttributeMap map = new GameAttributeMap();
            map[GameAttribute.Buff_Active, PowerSNO] = false;
            map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false;
            actor.Attributes.CombineMap(map);
            return map;
        }

        public static GameAttributeMap ComputeStats(Player.Player player, GameAttributeMap equippedMap)
        {
            GameAttributeMap attribs = new GameAttributeMap();
            // basic stats
            //            attribs[GameAttribute.Attack_Bonus] = 0;
            //            attribs[GameAttribute.Attack_Bonus_Percent] = 0;
            //            attribs[GameAttribute.Attack_Reduction_Percent] = 0;
            //            attribs[GameAttribute.Stats_All_Bonus] = 0f;
            attribs[GameAttribute.Attack] = (float)Math.Floor(((player.InitialAttack + equippedMap[GameAttribute.Stats_All_Bonus] + player.Attributes[GameAttribute.Attack_Bonus] + equippedMap[GameAttribute.Attack_Bonus]) *
                (1 + player.Attributes[GameAttribute.Attack_Bonus_Percent] + equippedMap[GameAttribute.Attack_Bonus_Percent])) * 
                (1 - player.Attributes[GameAttribute.Attack_Reduction_Percent] - equippedMap[GameAttribute.Attack_Reduction_Percent]));
                // ((Attack.Agg + Stats_All_Bonus + Attack_Bonus) * (1 + Attack_Bonus_Percent)) * (1 - Attack_Reduction_Percent)
                // bonus + reduction can come from outside too (skills/monsters)
            attribs[GameAttribute.Defense] = (float)Math.Floor(((player.InitialDefense + equippedMap[GameAttribute.Stats_All_Bonus] + player.Attributes[GameAttribute.Defense_Bonus] + equippedMap[GameAttribute.Defense_Bonus]) *
                (1 + player.Attributes[GameAttribute.Defense_Bonus_Percent] + equippedMap[GameAttribute.Defense_Bonus_Percent])) *
                (1 - player.Attributes[GameAttribute.Defense_Reduction_Percent] - equippedMap[GameAttribute.Defense_Reduction_Percent]));
            attribs[GameAttribute.Precision] = (float)Math.Floor(((player.InitialPrecision + equippedMap[GameAttribute.Stats_All_Bonus] + player.Attributes[GameAttribute.Precision_Bonus] + equippedMap[GameAttribute.Precision_Bonus]) *
                (1 + player.Attributes[GameAttribute.Precision_Bonus_Percent] + equippedMap[GameAttribute.Precision_Bonus_Percent])) *
                (1 - player.Attributes[GameAttribute.Precision_Reduction_Percent] - equippedMap[GameAttribute.Precision_Reduction_Percent]));
            attribs[GameAttribute.Vitality] = (float)Math.Floor(((player.InitialVitality + equippedMap[GameAttribute.Stats_All_Bonus] + player.Attributes[GameAttribute.Vitality_Bonus] + equippedMap[GameAttribute.Vitality_Bonus]) *
                (1 + player.Attributes[GameAttribute.Vitality_Bonus_Percent] + equippedMap[GameAttribute.Vitality_Bonus_Percent])) *
                (1 - player.Attributes[GameAttribute.Vitality_Reduction_Percent] - equippedMap[GameAttribute.Vitality_Reduction_Percent]));

            // armor
            attribs[GameAttribute.Armor] = 0f; // class's basic armor w/o ANY items TODO: find out
//            attribs[GameAttribute.Armor_Bonus_Percent] = 0; // from skills
            attribs[GameAttribute.Armor_Total] = (float)Math.Floor((player.Attributes[GameAttribute.Armor] + equippedMap[GameAttribute.Armor_Item_Total] *
                (equippedMap[GameAttribute.Armor_Bonus_Percent] + 1)));// FLOOR((Armor + Armor_Item_Total) * (Armor_Bonus_Percent + 1))

            // hitpoints
            attribs[GameAttribute.Hitpoints_Total_From_Level] = (player.Attributes[GameAttribute.Level] - 1) * player.Attributes[GameAttribute.Hitpoints_Factor_Level];
            attribs[GameAttribute.Hitpoints_Total_From_Vitality] = attribs[GameAttribute.Vitality] * player.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
            attribs[GameAttribute.Hitpoints_Max_Total] = (player.Attributes[GameAttribute.Hitpoints_Max] + attribs[GameAttribute.Hitpoints_Total_From_Level] +
                attribs[GameAttribute.Hitpoints_Total_From_Vitality] + player.Attributes[GameAttribute.Hitpoints_Max_Bonus] + equippedMap[GameAttribute.Hitpoints_Max_Bonus]) * 
                (1 + player.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus] + player.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus_Item]);
            attribs[GameAttribute.Hitpoints_Cur] = attribs[GameAttribute.Hitpoints_Max_Total];
            // block
            attribs[GameAttribute.Block_Amount_Bonus_Percent] = player.Attributes[GameAttribute.Block_Amount_Bonus_Percent] + equippedMap[GameAttribute.Block_Amount_Bonus_Percent]; // equipment OR player
            attribs[GameAttribute.Block_Amount] = player.Attributes[GameAttribute.Block_Amount]; // TODO: can it be on equipment too?
            attribs[GameAttribute.Block_Amount_Total_Min] = (player.Attributes[GameAttribute.Block_Amount] + equippedMap[GameAttribute.Block_Amount_Item_Min]) * (1 + attribs[GameAttribute.Block_Amount_Bonus_Percent]) ; 
            // (Block_Amount + Block_Amount_Item_Min + Block_Amount_Item_Bonus) * (1 + Block_Amount_Bonus_Percent)
            attribs[GameAttribute.Block_Amount_Total_Max] = (player.Attributes[GameAttribute.Block_Amount] + equippedMap[GameAttribute.Block_Amount_Item_Min] + equippedMap[GameAttribute.Block_Amount_Item_Delta]
                 + equippedMap[GameAttribute.Block_Amount_Item_Bonus]) * (1 + attribs[GameAttribute.Block_Amount_Bonus_Percent]);
            // (Block_Amount + Block_Amount_Item_Min + Block_Amount_Item_Delta + Block_Amount_Item_Bonus) * (1 + Block_Amount_Bonus_Percent)
            attribs[GameAttribute.Block_Chance] = player.Attributes[GameAttribute.Block_Chance]; // TODO: player, can it be on equipment too?
            attribs[GameAttribute.Block_Chance_Total] = attribs[GameAttribute.Block_Chance] + equippedMap[GameAttribute.Block_Chance_Item_Total];//Block_Chance + Block_Chance_Item_Total

            // compute resource
            ComputeResourceRegen(player, player.ResourceID);
            // compute damage, TODO: elemental based attributes (now only physical), dual wield
            attribs[GameAttribute.Damage_Min, 0] = player.Attributes[GameAttribute.Damage_Min, 0]; // from player
            attribs[GameAttribute.Damage_Type_Percent_Bonus,0] = 0;// elemental
            attribs[GameAttribute.Damage_Bonus_Min, 0] = player.Attributes[GameAttribute.Damage_Bonus_Min,0] + equippedMap[GameAttribute.Damage_Bonus_Min,0]; // from player and equipment
            attribs[GameAttribute.Damage_Weapon_Min_Total_CurrentHand,0] = equippedMap[GameAttribute.Damage_Weapon_Min_Total,0];
            attribs[GameAttribute.Damage_Min_Subtotal,0] = attribs[GameAttribute.Damage_Min,0] + attribs[GameAttribute.Damage_Bonus_Min,0] + attribs[GameAttribute.Damage_Weapon_Min_Total_CurrentHand,0];
            //elemental Damage_Min + Damage_Bonus_Min + Damage_Weapon_Min_Total_CurrentHand
            attribs[GameAttribute.Damage_Min_Total,0] = attribs[GameAttribute.Damage_Min_Subtotal,0] + attribs[GameAttribute.Damage_Type_Percent_Bonus,0] * attribs[GameAttribute.Damage_Min_Subtotal, 0];
            //elemental Damage_Min_Subtotal + Damage_Type_Percent_Bonus * Damage_Min_Subtotal#Physical
            attribs[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand,0] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total,0]; // elemental
            attribs[GameAttribute.Damage_Delta, 0] = equippedMap[GameAttribute.Damage_Delta, 0]; // elemental , from e.g. rings 2-4 dmg?
            attribs[GameAttribute.Damage_Delta_Total,0] = (float)Math.Max(attribs[GameAttribute.Damage_Delta,0] - attribs[GameAttribute.Damage_Bonus_Min,0] + attribs[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand,0], 0);
            // elemental Max(Damage_Delta - Damage_Bonus_Min + Damage_Weapon_Delta_Total_CurrentHand, 0)
            attribs[GameAttribute.Damage_Weapon_Delta_Total_MainHand, 0] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total_MainHand]; // elemental
            attribs[GameAttribute.Damage_Weapon_Delta_Total_OffHand, 0] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total_OffHand]; // elemental
            attribs[GameAttribute.Damage_Weapon_Min_Total_MainHand, 0] = equippedMap[GameAttribute.Damage_Weapon_Min_Total_MainHand];
            attribs[GameAttribute.Damage_Weapon_Min_Total_OffHand, 0] = equippedMap[GameAttribute.Damage_Weapon_Min_Total_OffHand];

            // TEMP - disable all criticals
            attribs[GameAttribute.Crit_Percent_Base] = 0f;
            attribs[GameAttribute.Crit_Percent_Cap] = 0f;
            attribs[GameAttribute.Crit_Percent_Bonus_Capped] = 0f;
            attribs[GameAttribute.Crit_Percent_Bonus_Uncapped] = 0f;

            // w/ criticals one item 43.y DPS, second weapon sets to 2.x DPS, offhand needs to calculate ALL, not reduction? or _OffHand/_MainHand needs to be computed?
            // w/o criticals 33.6 DPS, 1.81 DPS

            // compute attacks per second
            attribs[GameAttribute.Attacks_Per_Second_Percent] = player.Attributes[GameAttribute.Attacks_Per_Second_Percent]; // player
            attribs[GameAttribute.Attacks_Per_Second_Bonus] = player.Attributes[GameAttribute.Attacks_Per_Second_Bonus]; // player
            attribs[GameAttribute.Attacks_Per_Second] = player.Attributes[GameAttribute.Attacks_Per_Second]; // ??? basic attack w/o items?
            // TODO: dual wield, (critical dmg with chance should compute client, current effects should be percent + bonus from player)
            attribs[GameAttribute.Attacks_Per_Second_Item_CurrentHand] = equippedMap[GameAttribute.Attacks_Per_Second_Item_Total];
            attribs[GameAttribute.Attacks_Per_Second_Total] = (((attribs[GameAttribute.Attacks_Per_Second_Item_CurrentHand] > 0) ? attribs[GameAttribute.Attacks_Per_Second_Item_CurrentHand] : attribs[GameAttribute.Attacks_Per_Second]) +
                attribs[GameAttribute.Attacks_Per_Second_Bonus] + equippedMap[GameAttribute.Attacks_Per_Second_Item_Bonus]) * (float)Math.Max(0.1, (1 + attribs[GameAttribute.Attacks_Per_Second_Percent]));
            // (((Attacks_Per_Second_Item_CurrentHand > 0.0) ? Attacks_Per_Second_Item_CurrentHand : Attacks_Per_Second) + Attacks_Per_Second_Bonus + Attacks_Per_Second_Item_Bonus) * Max(0.1, (1 + Attacks_Per_Second_Percent))
            
            // TODO: optimize sending (not sending attribute when it didn't change)
            // store to attributes
            attribs = attribs.CombineToMapAndRemoveIdentities(player.Attributes);
//            player.Attributes.CombineMap(attribs);
            // send update to player
            player.UpdateMap.CombineMap(attribs);
            return attribs;
        }

        public static GameAttributeMap[] ComputeCombat(Actor attacker, Actor defender, bool critical, Boolean blocked, bool damageTypeOverriden = false, int damageTypeOverride = 0)
        {
            // TODO: absorbing elemental dmg ([GameAttribute.Damage_Absorb_Percent, type] + [GameAttribute.Damage_Absorb_Percent_All]
            // TODO: add thorn dmg back to attacker
            // blocked -> substract from dmg blocked amount
            GameAttributeMap[] attribs = new GameAttributeMap[2];
            attribs[0] = new GameAttributeMap();
            attribs[1] = new GameAttributeMap();
            // Temp - do 1 dmg
            attribs[1][GameAttribute.Hitpoints_Cur] = defender.Attributes[GameAttribute.Hitpoints_Cur] - 1f;
            if ((defender is Player.Player) && (attribs[1][GameAttribute.Hitpoints_Cur] < 10f))
            {
                // temp, not die as player
                attribs[1][GameAttribute.Hitpoints_Cur] = 50f;
            }
            attacker.Attributes.CombineMap(attribs[0]);
            defender.Attributes.CombineMap(attribs[1]);
            return attribs;
        }

        public static bool IsCriticalHit(Actor attacker, Actor defender)
        {
            if (defender.Attributes[GameAttribute.Ignores_Critical_Hits]) {
                return false;
            }
            float chance = (attacker.Attributes[GameAttribute.Crit_Percent_Base] + attacker.Attributes[GameAttribute.Crit_Percent_Bonus_Capped]) / 100;// TODO: level adjustments, are those really ints?
            if (RandomHelper.NextDouble() < chance)
            {
                return true;
            }
            return false;
        }

        public static bool IsHit(Actor attacker, Actor defender)
        {
            if (attacker.Attributes[GameAttribute.Always_Hits])
            {
                return true;
            }
            float chance = attacker.Attributes[GameAttribute.Hit_Chance]; // TODO: level adjustments
            if (RandomHelper.NextDouble() < chance)
            {
                return true;
            }
            return false;
        }

        public static bool IsDodge(Actor defender)
        {
            float chance = defender.Attributes[GameAttribute.Dodge_Rating_Total] + defender.Attributes[GameAttribute.Dodge_Chance_Bonus]; // TODO: level adjustments
//            Logger.Info(string.Format("chance = {0}, ratingTotal = {1}, dodgeBonus = {2}", chance, defender.Attributes[GameAttribute.Dodge_Rating_Total], defender.Attributes[GameAttribute.Dodge_Chance_Bonus]));
            if (RandomHelper.NextDouble() < chance)
            {
                return true;
            }
            return false;
        }

        public static bool IsBlock(Actor defender)
        {
            float chance = defender.Attributes[GameAttribute.Block_Chance_Total]; // TODO: level adjustments, (Block_Chance + Block_Chance_Item_Total computed when doing inventory)
            if (RandomHelper.NextDouble() < chance)
            {
                return true;
            }
            return false;
        }

        public static bool IsImmune(Actor defender, bool damageTypeOverriden, int damageTypeOverride)
        {
            if (defender.Attributes[GameAttribute.Invulnerable])
            {
                return true;
            }
            // TODO: add elemental immunities
            return false;
        }

        public static GameAttributeMap ComputeEquipment(Player.Player player, List<GameAttributeMap> equipped)
        {
            // TODO: add non-basic stats of items, use AddMap method, figure out exception for offHand weapon
            GameAttributeMap map = new GameAttributeMap();
            if (equipped.Count != 0)
            {
                float weaponDmgMultiplicator = 1f;
                foreach (GameAttributeMap m in equipped)
                {
                    // compute weapon to temp map 
                    // TODO: process only when m[Weapon_xxx] to decide dual wielding and setting mainHand attributes
                    /* // looks like client is reducing off hand dmg
                    if (map[GameAttribute.Held_In_OffHand])
                    {
                        weaponDmgMultiplicator = 0.8f;
                    }
                    else
                    {
                        weaponDmgMultiplicator = 1f;
                    }
                     */
                    for (int damageType = 0; damageType < 7; damageType++)
                    {
                        map[GameAttribute.Damage_Weapon_Min_Total, damageType] += m[GameAttribute.Damage_Weapon_Min_Total, damageType] * weaponDmgMultiplicator;
                        map[GameAttribute.Damage_Weapon_Max_Total, damageType] += m[GameAttribute.Damage_Weapon_Max_Total, damageType] * weaponDmgMultiplicator;
                        map[GameAttribute.Damage_Weapon_Delta_Total, damageType] += (m[GameAttribute.Damage_Weapon_Max_Total, damageType] - m[GameAttribute.Damage_Weapon_Min_Total, damageType]);
                    }
                    map[GameAttribute.Damage_Weapon_Min_Total_All] += m[GameAttribute.Damage_Weapon_Min_Total_All] * weaponDmgMultiplicator;
                    map[GameAttribute.Damage_Weapon_Delta_Total_All] += m[GameAttribute.Damage_Weapon_Delta_Total_All] * weaponDmgMultiplicator;
                    map[GameAttribute.Attacks_Per_Second_Item_Total] += m[GameAttribute.Attacks_Per_Second_Item_Total];
                    // dual wield
                    map[GameAttribute.Damage_Weapon_Delta_Total_MainHand] += m[GameAttribute.Damage_Weapon_Delta_Total_MainHand];
                    map[GameAttribute.Damage_Weapon_Min_Total_MainHand] += m[GameAttribute.Damage_Weapon_Min_Total_MainHand];
                    map[GameAttribute.Damage_Weapon_Delta_Total_OffHand] += m[GameAttribute.Damage_Weapon_Delta_Total_OffHand];
                    map[GameAttribute.Damage_Weapon_Min_Total_OffHand] += m[GameAttribute.Damage_Weapon_Min_Total_OffHand];
                    
                    // compute armor to temp map
                    map[GameAttribute.Armor_Item_Total] += m[GameAttribute.Armor_Item_Total];
                    // compute block to temp map
                    map[GameAttribute.Block_Amount_Item_Delta] += m[GameAttribute.Block_Amount_Item_Delta];
                    map[GameAttribute.Block_Amount_Item_Min] += m[GameAttribute.Block_Amount_Item_Min];
                    map[GameAttribute.Block_Chance_Item_Total] += m[GameAttribute.Block_Chance_Item_Total];
                }
            }
            return map;
        }

        public static void UnlockSkills(Player.Player player)
        {
            // TODO: WRONG, should unlock skill when selected in skills panel (added to slot)
            // TODO: disable skills when unselected in skills panel
            int level = player.Attributes[GameAttribute.Level];
            if (level > 30)
            {
                // no skills after lvl 30
                return;
            }
            // hardcoded, needs to take values from MPQ
            List<int> PowerSNOs = new List<int>();
            switch (player.Properties.Class)
            {
                /* set for skillSNO
                    player.Attributes[GameAttribute.Skill, ] = 1;
                    player.Attributes[GameAttribute.Skill_Total, ] = 1;
                 */
                case Common.Toons.ToonClass.Barbarian:
                    break;
                case Common.Toons.ToonClass.DemonHunter:
                    break;
                case Common.Toons.ToonClass.Monk:
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritGenerator.FistsOfThunder);
                    if (level < 2)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritSpenders.BlindingFlash);
                    PowerSNOs.Add(Skills.Skills.Monk.Mantras.MantraOfEvasion);
                    if (level < 3)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritSpenders.LashingTailKick);
                    if (level < 4)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritGenerator.DeadlyReach);
                    if (level < 5)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritSpenders.BreathOfHeaven);
                    if (level < 6)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritSpenders.DashingStrike);
                    if (level < 7)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritSpenders.LethalDecoy);
                    if (level < 8)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.SpiritGenerator.CripplingWave);
                    if (level < 9)
                    {
                        break;
                    }
                    PowerSNOs.Add(Skills.Skills.Monk.Mantras.MantraOfRetribution);
                    break;
                case Common.Toons.ToonClass.WitchDoctor:
                    break;
                case Common.Toons.ToonClass.Wizard:
                    break;
            }
            GameAttributeMap map = new GameAttributeMap();
            UnlockSkills(map, PowerSNOs);
            player.Attributes.CombineMap(map);
            // my addition
            player.UpdateMap.CombineMap(map);
        }

        private static void UnlockSkills(GameAttributeMap map, List<int> PowerSNOs)
        {
            if (PowerSNOs == null) {
                return;
            }
            foreach (int PowerSNO in PowerSNOs) {
                UnlockSkill(map, PowerSNO);
            }
        }

        private static void UnlockSkill(GameAttributeMap map, int PowerSNO)
        {
            map[GameAttribute.Skill, PowerSNO] = 1;
            map[GameAttribute.Skill_Total, PowerSNO] = 1;
        }
    }
}
