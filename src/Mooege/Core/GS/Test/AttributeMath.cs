using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.FXEffect;
using Mooege.Common.Helpers;
using Mooege.Common;
using Mooege.Core.GS.Players;

namespace Mooege.Core.GS.Test
{
    public class AttributeMath
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        /*
         * Stone of Recall = Recipe 		SNOId	195660	int
        */


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
        public static void ModifyResource(Player player, int resourceID, float amount)
        {
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
                player.Attributes[GameAttribute.Resource_Cur, resourceID] = temp;
            }
        }

        private static void ComputeResourceSpent(Player player, float amount)
        {
        }

        public static void ComputeResourceRegen(Player player, int resourceID)
        {
            float total = player.Attributes[GameAttribute.Resource_Regen_Per_Second, resourceID] * (1 + player.Attributes[GameAttribute.Resource_Regen_Bonus_Percent, resourceID]) + 
                (player.Attributes[GameAttribute.Resource_Regen_Percent_Per_Second, resourceID] * player.Attributes[GameAttribute.Resource_Max_Total, resourceID]);
            int index = -1;
            if (resourceID == 6) // Discipline
            {
                index = 6; // NOT working
            }
            // without value or -1 - regens primary
            if (total != player.Attributes[GameAttribute.Resource_Regen_Total, index])
            {
                player.Attributes[GameAttribute.Resource_Regen_Total, index] = total;
            }
        }

        public static void CooldownStart(Player player, int PowerSNO, int startTick, int seconds)
        {
            player.Attributes[GameAttribute.Power_Cooldown_Start, PowerSNO] = startTick;
            player.Attributes[GameAttribute.Power_Cooldown, PowerSNO] = startTick + (60 * seconds);
            player.World.AddEffect(new CooldownStopEffect { Actor = player, EffectID = PowerSNO, StartingTick = startTick + (60 * seconds) });
            return;
        }

        public static void CooldownStop(Player player, int PowerSNO)
        {
            player.Attributes[GameAttribute.Power_Cooldown, PowerSNO] = 0;
        }

        public static void BuffIconStart(Player player, int PowerSNO, int startTick, int seconds)
        {
            player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] = player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] + 1;
            player.Attributes[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = startTick;
            player.Attributes[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = startTick + (60 * seconds);
            if (player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
            {
                BuffStart(player, PowerSNO);
            }
        }

        public static void BuffStart(Actor actor, int PowerSNO)
        {
            actor.Attributes[GameAttribute.Buff_Active, PowerSNO] = true;
            actor.Attributes[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // TODO: expand to deal with runed powers
        }

        public static void BuffIconStop(Player player, int PowerSNO)
        {
            player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] = player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] - 1;
            if (player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0)
            {
                BuffStop(player, PowerSNO);
            }
            else if (player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] < 0)
            {
                player.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] = 0;
            }
        }

        public static void BuffStop(Actor actor, int PowerSNO)
        {
            actor.Attributes[GameAttribute.Buff_Active, PowerSNO] = false;
            actor.Attributes[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false;
        }

        public static void ComputeStats(Player player, GameAttributeMap equippedMap, bool initialSetting = false)
        {
//            player.Attributes.CombineMap(equippedMap); // for all other attributes - not possible (doubling value of map in few instances)

            GameAttributeMap attribs = new GameAttributeMap();
            // basic stats
            //            attribs[GameAttribute.Attack_Bonus] = 0;
            //            attribs[GameAttribute.Attack_Bonus_Percent] = 0;
            //            attribs[GameAttribute.Attack_Reduction_Percent] = 0;
            //            attribs[GameAttribute.Stats_All_Bonus] = 0f;
            attribs[GameAttribute.Attack] = (float)Math.Floor(((player.InitialAttack + equippedMap[GameAttribute.Stats_All_Bonus] + equippedMap[GameAttribute.Attack] + player.Attributes[GameAttribute.Attack_Bonus] + equippedMap[GameAttribute.Attack_Bonus]) *
                (1 + player.Attributes[GameAttribute.Attack_Bonus_Percent] + equippedMap[GameAttribute.Attack_Bonus_Percent])) * 
                (1 - player.Attributes[GameAttribute.Attack_Reduction_Percent] - equippedMap[GameAttribute.Attack_Reduction_Percent]));
                // ((Attack.Agg + Stats_All_Bonus + Attack_Bonus) * (1 + Attack_Bonus_Percent)) * (1 - Attack_Reduction_Percent)
                // bonus + reduction can come from outside too (skills/monsters)
            attribs[GameAttribute.Defense] = (float)Math.Floor(((player.InitialDefense + equippedMap[GameAttribute.Stats_All_Bonus] + equippedMap[GameAttribute.Defense] + player.Attributes[GameAttribute.Defense_Bonus] + equippedMap[GameAttribute.Defense_Bonus]) *
                (1 + player.Attributes[GameAttribute.Defense_Bonus_Percent] + equippedMap[GameAttribute.Defense_Bonus_Percent])) *
                (1 - player.Attributes[GameAttribute.Defense_Reduction_Percent] - equippedMap[GameAttribute.Defense_Reduction_Percent]));
            attribs[GameAttribute.Precision] = (float)Math.Floor(((player.InitialPrecision + equippedMap[GameAttribute.Stats_All_Bonus] + equippedMap[GameAttribute.Precision] + player.Attributes[GameAttribute.Precision_Bonus] + equippedMap[GameAttribute.Precision_Bonus]) *
                (1 + player.Attributes[GameAttribute.Precision_Bonus_Percent] + equippedMap[GameAttribute.Precision_Bonus_Percent])) *
                (1 - player.Attributes[GameAttribute.Precision_Reduction_Percent] - equippedMap[GameAttribute.Precision_Reduction_Percent]));
            attribs[GameAttribute.Vitality] = (float)Math.Floor(((player.InitialVitality + equippedMap[GameAttribute.Stats_All_Bonus] + equippedMap[GameAttribute.Vitality] + player.Attributes[GameAttribute.Vitality_Bonus] + equippedMap[GameAttribute.Vitality_Bonus]) *
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
                (1 + player.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus] + equippedMap[GameAttribute.Hitpoints_Max_Percent_Bonus_Item]);
            if (initialSetting)
            {
                attribs[GameAttribute.Hitpoints_Cur] = attribs[GameAttribute.Hitpoints_Max_Total]; // heal to full
            }
            // block
            attribs[GameAttribute.Block_Amount_Bonus_Percent] = player.Attributes[GameAttribute.Block_Amount_Bonus_Percent] + equippedMap[GameAttribute.Block_Amount_Bonus_Percent]; // equipment OR player
            attribs[GameAttribute.Block_Amount] = player.Attributes[GameAttribute.Block_Amount]; // TODO: can it be on equipment too?
            attribs[GameAttribute.Block_Amount_Total_Min] = (player.Attributes[GameAttribute.Block_Amount] + equippedMap[GameAttribute.Block_Amount_Item_Min]) * (1 + attribs[GameAttribute.Block_Amount_Bonus_Percent]) ; 
            // (Block_Amount + Block_Amount_Item_Min + Block_Amount_Item_Bonus) * (1 + Block_Amount_Bonus_Percent)
            attribs[GameAttribute.Block_Amount_Total_Max] = (player.Attributes[GameAttribute.Block_Amount] + equippedMap[GameAttribute.Block_Amount_Item_Min] + equippedMap[GameAttribute.Block_Amount_Item_Delta]
                 + equippedMap[GameAttribute.Block_Amount_Item_Bonus]) * (1 + attribs[GameAttribute.Block_Amount_Bonus_Percent]);
            // (Block_Amount + Block_Amount_Item_Min + Block_Amount_Item_Delta + Block_Amount_Item_Bonus) * (1 + Block_Amount_Bonus_Percent)
            attribs[GameAttribute.Block_Chance] = player.Attributes[GameAttribute.Block_Chance]; // TODO: can it be on equipment too?
            attribs[GameAttribute.Block_Chance_Total] = attribs[GameAttribute.Block_Chance] + equippedMap[GameAttribute.Block_Chance_Item_Total];//Block_Chance + Block_Chance_Item_Total

            // compute resource
            ComputeResourceRegen(player, player.ResourceID);
            // compute damage, TODO: dual wield, total values
            for (int i = 0; i < 7; i++)
            {
                attribs[GameAttribute.Damage_Min, i] = player.Attributes[GameAttribute.Damage_Min, i]; // from player
                attribs[GameAttribute.Damage_Type_Percent_Bonus, i] = 0;// elemental
                attribs[GameAttribute.Damage_Bonus_Min, i] = player.Attributes[GameAttribute.Damage_Bonus_Min, i] + equippedMap[GameAttribute.Damage_Bonus_Min, i]; // from player and equipment
                attribs[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, i] = equippedMap[GameAttribute.Damage_Weapon_Min_Total, i];
                attribs[GameAttribute.Damage_Min_Subtotal, i] = attribs[GameAttribute.Damage_Min, i] + attribs[GameAttribute.Damage_Bonus_Min, i] + attribs[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, i];
                //elemental Damage_Min + Damage_Bonus_Min + Damage_Weapon_Min_Total_CurrentHand
                attribs[GameAttribute.Damage_Min_Total, i] = attribs[GameAttribute.Damage_Min_Subtotal, i] + attribs[GameAttribute.Damage_Type_Percent_Bonus, i] * attribs[GameAttribute.Damage_Min_Subtotal, i];
                //elemental Damage_Min_Subtotal + Damage_Type_Percent_Bonus * Damage_Min_Subtotal#Physical
                attribs[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, i] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total, i]; // elemental
                attribs[GameAttribute.Damage_Delta, i] = equippedMap[GameAttribute.Damage_Delta, i]; // elemental , from e.g. rings 2-4 dmg?
                attribs[GameAttribute.Damage_Delta_Total, i] = (float)Math.Max(attribs[GameAttribute.Damage_Delta, i] - attribs[GameAttribute.Damage_Bonus_Min, i] + attribs[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, i], 0);
                // elemental Max(Damage_Delta - Damage_Bonus_Min + Damage_Weapon_Delta_Total_CurrentHand, 0)
                /*
            attribs[GameAttribute.Damage_Weapon_Delta_Total_MainHand, i] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total_MainHand]; // elemental
            attribs[GameAttribute.Damage_Weapon_Delta_Total_OffHand, i] = equippedMap[GameAttribute.Damage_Weapon_Delta_Total_OffHand]; // elemental
            attribs[GameAttribute.Damage_Weapon_Min_Total_MainHand, i] = equippedMap[GameAttribute.Damage_Weapon_Min_Total_MainHand];
            attribs[GameAttribute.Damage_Weapon_Min_Total_OffHand, i] = equippedMap[GameAttribute.Damage_Weapon_Min_Total_OffHand];
                 */
            }
            // TODO: critical
            /*
            attribs[GameAttribute.Crit_Percent_Base] = 0f;
            attribs[GameAttribute.Crit_Percent_Cap] = 0f;
            attribs[GameAttribute.Crit_Percent_Bonus_Capped] = 0f;
            attribs[GameAttribute.Crit_Percent_Bonus_Uncapped] = 0f;
             */

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

            // store to attributes
            player.Attributes.CombineMap(attribs);
        }

        public static void ComputeCombat(Actor attacker, Actor defender, bool critical, Boolean blocked, bool damageTypeOverriden = false, int damageTypeOverride = 0)
        {
            // TODO: absorbing elemental dmg ([GameAttribute.Damage_Absorb_Percent, type] + [GameAttribute.Damage_Absorb_Percent_All]
            // TODO: add thorn dmg back to attacker
            // blocked -> substract from dmg blocked amount
            // Temp - do 1 dmg
            defender.Attributes[GameAttribute.Hitpoints_Cur] -= 1f;
            if ((defender is Player) && (defender.Attributes[GameAttribute.Hitpoints_Cur] < 10f))
            {
                // temp, not die as player
                defender.Attributes[GameAttribute.Hitpoints_Cur] = 50f;
            }
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

        public static GameAttributeMap ComputeEquipment(Player player, List<GameAttributeMap> equipped)
        {
            // TODO: add non-basic stats of items, use AddMap method, figure out exception for offHand weapon
            GameAttributeMap map = new GameAttributeMap();
            if (equipped.Count != 0)
            {
                foreach (GameAttributeMap m in equipped)
                {
                    map.AddMap(m);
                    /*
                    // compute weapon to temp map 
                    
                    for (int damageType = 0; damageType < 7; damageType++)
                    {
                        map[GameAttribute.Damage_Weapon_Min_Total, damageType] += m[GameAttribute.Damage_Weapon_Min_Total, damageType];
                        map[GameAttribute.Damage_Weapon_Max_Total, damageType] += m[GameAttribute.Damage_Weapon_Max_Total, damageType];
                        map[GameAttribute.Damage_Weapon_Delta_Total, damageType] += (m[GameAttribute.Damage_Weapon_Max_Total, damageType] - m[GameAttribute.Damage_Weapon_Min_Total, damageType]);
                    }
                    map[GameAttribute.Damage_Weapon_Min_Total_All] += m[GameAttribute.Damage_Weapon_Min_Total_All];
                    map[GameAttribute.Damage_Weapon_Delta_Total_All] += m[GameAttribute.Damage_Weapon_Delta_Total_All];
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
                     */
                }
            }
            return map;
        }

    }
}
/*
 /// World
 private readonly List<FXEffect.FXEffect> _effects = new List<FXEffect.FXEffect>();
        public override void Update()
        {
            // update players.
//            foreach (var pair in this._players) { pair.Value.Update(); }

            // update effects.
            int tick = this.Game.Tick;
            if (this._effects.Count != 0)
            {
                for (int index = 0; index < this._effects.Count;) {
                    if (this._effects[index].Process(tick))
                    {
                        this._effects.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            // update actors.
            foreach (var pair in this.Actors) { pair.Value.Update(); }

        }

public void AddEffect(FXEffect.FXEffect effect)
        {
            bool notAdding = false;
            effect.World = this;
            int tick = this.Game.Tick;
            if (!effect.StartingTick.HasValue || (effect.StartingTick <= tick))
            {
                notAdding = effect.Process(tick);
            }
            if (!notAdding) this._effects.Add(effect);
        }

 * /// player
   this.Attributes[GameAttribute.Hitpoints_Max] = 40f;
 // Class specific
            switch (this.Properties.Class)
            {
                case ToonClass.Barbarian:
                    this.Attributes[GameAttribute.Skill_Total, 30078] = 1;  //Fury Trait
                    this.Attributes[GameAttribute.Skill, 30078] = 1;
                    this.Attributes[GameAttribute.Trait, 30078] = 1;
                    break;
                case ToonClass.DemonHunter:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Hatred Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Discipline Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     * /
                    //Secondary Resource for the Demon Hunter
                    int Discipline = this.ResourceID + 1; //0x00000006
                    this.Attributes[GameAttribute.Resource_Cur, Discipline] = 15f;
                    this.Attributes[GameAttribute.Resource_Max, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Max_Total, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Effective_Max, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Type_Secondary] = Discipline;
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 30f;
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, Discipline] = 5f;
                    AttributeMath.ComputeResourceRegen(this, Discipline); // doesn't work
                    break;
                case ToonClass.Monk:
                    this.Attributes[GameAttribute.Skill_Total, 0x0000CE11] = 1;  //Spirit Trait
                    this.Attributes[GameAttribute.Skill, 0x0000CE11] = 1;
                    this.Attributes[GameAttribute.Trait, 0x0000CE11] = 1;
                    break;
                case ToonClass.WitchDoctor:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Mana Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     * /
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 50f; // needs proper number
                    break;
                case ToonClass.Wizard:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Arcane Power Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     * /
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 30f; // needs proper number
                    break;
            }

            AttributeMath.ComputeResourceRegen(this, this.ResourceID);

else if (message is SocketSpellMessage) OnSocketSpell(client, (SocketSpellMessage)message);
            else if (message is RequestAddSocketMessage) OnRequestAddSocket(client, (RequestAddSocketMessage)message);
            else return;
        }

        private void OnSocketSpell(GameClient client, SocketSpellMessage socketSpellMessage)
        {
            Item rune = this.World.GetItem(unchecked((uint)socketSpellMessage.Field0));
            int PowerSNO = socketSpellMessage.Field1;
            int skillIndex = -1; // find index of powerSNO.
            for (int i = 0; i < this.SkillSet.ActiveSkills.Length; i++)
            {
                if (this.SkillSet.ActiveSkills[i] == PowerSNO)
                {
                    skillIndex = i;
                    break;
                }
            }
            if (skillIndex == -1)
            {
                // validity of message in controlled on client side, this shouldn't happen
                return;
            }
            GameAttributeMap map = new GameAttributeMap();
            if (Attributes[GameAttribute.Rune_A, PowerSNO] != 0)
            {
                // TODO: remove old rune
                Attributes[GameAttribute.Rune_A, PowerSNO] = 0;
                map[GameAttribute.Rune_A, PowerSNO] = 0;
            }
            if (Attributes[GameAttribute.Rune_B, PowerSNO] != 0)
            {
                // TODO: remove old rune
                Attributes[GameAttribute.Rune_B, PowerSNO] = 0;
                map[GameAttribute.Rune_B, PowerSNO] = 0;
            }
            if (Attributes[GameAttribute.Rune_C, PowerSNO] != 0)
            {
                // TODO: remove old rune
                Attributes[GameAttribute.Rune_C, PowerSNO] = 0;
                map[GameAttribute.Rune_C, PowerSNO] = 0;
            }
            if (Attributes[GameAttribute.Rune_D, PowerSNO] != 0)
            {
                // TODO: remove old rune
                Attributes[GameAttribute.Rune_D, PowerSNO] = 0;
                map[GameAttribute.Rune_D, PowerSNO] = 0;
            }
            if (Attributes[GameAttribute.Rune_E, PowerSNO] != 0)
            {
                // TODO: remove old rune
                Attributes[GameAttribute.Rune_E, PowerSNO] = 0;
                map[GameAttribute.Rune_E, PowerSNO] = 0;
            }
            // type of rune is in AttributeSpec
            //            Attributes[GameAttribute.Rune_Rank] = <in spec>; // on rune, inititalized in creation
            //            Attributes[GameAttribute.Rune_Attuned_Power] = 0; // 0 on unattuned or  random value from all powers, inititalized in creation

            // if unattuned, pick random color and set attunement
            if (rune.Attributes[GameAttribute.Rune_Attuned_Power] == 0)
            {
                GameAttributeMap m = new GameAttributeMap();
                rune.Attributes[GameAttribute.Rune_Attuned_Power] = PowerSNO;
                m[GameAttribute.Rune_Attuned_Power] = PowerSNO;
                int colorIndex = RandomHelper.Next(0, 5);
                switch (colorIndex)
                {
                    case 0:
                        //rune.ActorSNO = <SNODataMessage for Rune_A>
                        break;
                    case 1:
                        //rune.ActorSNO = <SNODataMessage for Rune_A>
                        break;
                    case 2:
                        //rune.ActorSNO = <SNODataMessage for Rune_A>
                        break;
                    case 3:
                        //rune.ActorSNO = <SNODataMessage for Rune_A>
                        break;
                    case 4:
                        //rune.ActorSNO = <SNODataMessage for Rune_A>
                        break;
                }
                m.SendMessage(InGameClient, rune.DynamicID);
                // TODO: send change actor to rune
            }
            else
            {
                // TODO: switch between rune's color
                Attributes[GameAttribute.Rune_A, PowerSNO] = rune.Attributes[GameAttribute.Rune_Rank];
                map[GameAttribute.Rune_A, PowerSNO] = rune.Attributes[GameAttribute.Rune_Rank];
            }
            map.SendMessage(InGameClient, DynamicID);
            rune.SetInventoryLocation(16, skillIndex, 0); // skills (16), index, 0
             // need info from BETA if and how this changes 
//            this.SkillKeyMappings[0].Power = PowerSNO;
//            this.SkillKeyMappings[0].Field1 = unchecked((int)rune.DynamicID);
//            this.SkillKeyMappings[0].Field2 = rune.Attributes[GameAttribute.Rune_Rank];
            UpdateHeroState();
        }

        private void OnAssignActiveSkill(GameClient client, AssignActiveSkillMessage message)
        {
            var oldSNOSkill = this.SkillSet.ActiveSkills[message.SkillIndex]; // find replaced skills SNO.
            GameAttributeMap map = new GameAttributeMap();
            if (oldSNOSkill != -1)
            {
                // switch off old skill in hotbar
                map[GameAttribute.Skill, oldSNOSkill] = 0;
                map[GameAttribute.Skill_Total, oldSNOSkill] = 0;
            }
            // switch on new skill in hotbar
            map[GameAttribute.Skill, message.SNOSkill] = 1;
            map[GameAttribute.Skill_Total, message.SNOSkill] = 1;
            map.SendMessage(InGameClient, this.DynamicID);

            foreach (HotbarButtonData button in this.SkillSet.HotBarSkills.Where(button => button.SNOSkill == oldSNOSkill)) // loop through hotbar and replace the old skill with new one
            {
                button.SNOSkill = message.SNOSkill;
            }
            this.SkillSet.ActiveSkills[message.SkillIndex] = message.SNOSkill;
            this.UpdateHeroState();
        }

        private void OnAssignPassiveSkill(GameClient client, AssignPassiveSkillMessage message)
        {
            var oldSNOSkill = this.SkillSet.PassiveSkills[message.SkillIndex]; // find replaced skills SNO.
            GameAttributeMap map = new GameAttributeMap();
            if (oldSNOSkill != -1)
            {
                // switch off old passive skill
                map[GameAttribute.Trait, oldSNOSkill] = 0;
                map[GameAttribute.Skill, oldSNOSkill] = 0;
                map[GameAttribute.Skill_Total, oldSNOSkill] = 0;
            }
            // switch on new passive skill
            map[GameAttribute.Trait, message.SNOSkill] = 1;
            map[GameAttribute.Skill, message.SNOSkill] = 1;
            map[GameAttribute.Skill_Total, message.SNOSkill] = 1;
            map.SendMessage(InGameClient, this.DynamicID); this.SkillSet.PassiveSkills[message.SkillIndex] = message.SNOSkill;
            this.SkillSet.PassiveSkills[message.SkillIndex] = message.SNOSkill;
            this.UpdateHeroState();
        }

/// item
         public override void OnTargeted(Player player, TargetMessage message)
        {
            //Logger.Trace("OnTargeted");
            if (this.ItemType.Hash == 3646475)
            {
                // book with lore
                var y = MPQStorage.Data.Assets[SNOGroup.Actor].FirstOrDefault(x => x.Value.SNOId == this.SNOId);
                var e = (y.Value.Data as Mooege.Common.MPQ.FileFormats.Actor).TagMap.TagMapEntries.FirstOrDefault(z => z.Int1 == 67331);
                if (e != null)
                {
                    int loreSNO = e.Int2;
                    if ((loreSNO != -1) && !player.LearnedLore.m_snoLoreLearned.Contains(loreSNO))
                    {
                        // play lore to player
                        player.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.LoreMessage { Id = 213, snoLore = loreSNO }); // id 212 - new lore button, 213 - play immediatelly
                        // add lore to player's lores
                        int loreIndex = 0;
                        while ((loreIndex < player.LearnedLore.m_snoLoreLearned.Length) && (player.LearnedLore.m_snoLoreLearned[loreIndex] != 0))
                        {
                            loreIndex++;
                        }
                        if (loreIndex < player.LearnedLore.m_snoLoreLearned.Length)
                        {
                            player.LearnedLore.m_snoLoreLearned[loreIndex] = loreSNO;
                            player.LearnedLore.Field0++; // Count
                            player.UpdateHeroState();
                        }
                    }
                    if (player.GroundItems.ContainsKey(this.DynamicID))
                        player.GroundItems.Remove(this.DynamicID);
                    this.Destroy();
                }
                else
                {
                    // monster lore, shouldn't occure
                    player.Inventory.PickUp(this);
                }
            }
            else
            {
                // other items
                player.Inventory.PickUp(this);
            }
        }

*/