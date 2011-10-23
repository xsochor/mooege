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
        /* // dmg attributes
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
        attacker.Attributes[GameAttribute.Damage_Weapon_Bonus_Delta] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Bonus_Min] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_SubTotal] = 0; // (Damage_Weapon_Delta > 0.0) ? (Max(1, Damage_Weapon_Delta - Damage_Weapon_Bonus_Min)) : Damage_Weapon_Delta
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total] = 0; // elemental Max((Damage_Weapon_Delta_SubTotal + Damage_Weapon_Bonus_Delta) * (1 + Damage_Weapon_Percent_Total), 0)
        attacker.Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = 0; // sum elemental
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
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_All] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_Bonus] = 0;
        attacker.Attributes[GameAttribute.Damage_Weapon_Percent_Total] = 0; // Damage_Weapon_Percent_Bonus + Damage_Weapon_Percent_All#NONE
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

        public static GameAttributeMap ComputeStats(Player.Player player)
        {
            GameAttributeMap attribs = new GameAttributeMap();
            player.Attributes[GameAttribute.Hitpoints_Total_From_Level] = player.Attributes[GameAttribute.Level] * player.Attributes[GameAttribute.Hitpoints_Factor_Level];
            player.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] = player.Attributes[GameAttribute.Vitality] * player.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
            player.Attributes[GameAttribute.Hitpoints_Max_Total] = (player.Attributes[GameAttribute.Hitpoints_Max] + player.Attributes[GameAttribute.Hitpoints_Total_From_Level] + player.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] + player.Attributes[GameAttribute.Hitpoints_Max_Bonus]) * (1 + player.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus] + player.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus_Item]);
            player.Attributes[GameAttribute.Hitpoints_Cur] = player.Attributes[GameAttribute.Hitpoints_Max_Total];
            /*
            DPS
             */
            attribs[GameAttribute.Hitpoints_Cur] = player.Attributes[GameAttribute.Hitpoints_Cur];
            attribs[GameAttribute.Hitpoints_Max_Total] = player.Attributes[GameAttribute.Hitpoints_Max_Total];
            return attribs;
        }

        public static GameAttributeMap[] ComputeCombat(Actor attacker, Actor defender, bool critical, Boolean blocked, bool damageTypeOverriden = false, int damageTypeOverride = 0)
        {
            // TODO: absorbing elemental dmg ([GameAttribute.Damage_Absorb_Percent, type] + [GameAttribute.Damage_Absorb_Percent_All]
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
            float chance = defender.Attributes[GameAttribute.Block_Chance_Total]; // TODO: level adjustments, Block_Chance + Block_Chance_Item_Total
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
    }
}
