using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Actors;

namespace Mooege.Core.GS.Test
{
    public class AttributeMath
    {
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

        public static GameAttributeMap[] ComputeCombat(Actor attacker, Actor defender)
        {
            GameAttributeMap[] attribs = new GameAttributeMap[2];
//            attacker.Attributes[GameAttribute.]
//            attacker.Attributes[GameAttribute.]
                /*
                 Queue_Death
                 
                 Damage_Power_Min
                 Damage_Weapon_Min
                 Damage_Weapon_Min_Total
                 Damage_Weapon_Min_Total_All
                 */
            return attribs;
        }
    }
}
