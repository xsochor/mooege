﻿/*
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

using System;
using Mooege.Common.Helpers;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Map;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Tick;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Net.GS.Message.Definitions.Effect;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Core.GS.Test;

namespace Mooege.Core.GS.Actors
{
    public class Monster : Living
    {
        public override ActorType ActorType { get { return ActorType.Monster; } }

        public Monster(World world, int actorSNO, Vector3D position)
            : base(world, actorSNO, position)
        {
            this.Field2 = 0x8;
            this.GBHandle.Type = (int)GBHandleType.Monster; this.GBHandle.GBID = 1;
            this.Attributes[GameAttribute.TeamID] = 10;
            this.Attributes[GameAttribute.Experience_Granted] = 125;
           
        }

        public override void Update()
        {
            this.Brain(); // let him think. /raist
            base.Update();
        }

        private bool sentWalkAnimation = false;

        public virtual void Brain()
        {
            // intellectual activities goes here ;) /raist
            if (this.Attributes[GameAttribute.Queue_Death])
            {
                // will die
                return;
            }
            if (true)
            {
                return; // weird side-effect on mpq based spawning - all npcs are monsters
            }
            Actor target = null;
            if (this.Attributes[GameAttribute.Last_ACD_Attacked] != 0)
            {
                target = this.World.GetActor((uint)this.Attributes[GameAttribute.Last_ACD_Attacked]);
            }
            if (this.Attributes[GameAttribute.Forced_Enemy_ACDID] != 0)
            {
                target = this.World.GetActor((uint)this.Attributes[GameAttribute.Forced_Enemy_ACDID]);
            }
            if ((target == null) || (target.World == null))
            {
                this.Attributes[GameAttribute.Forced_Enemy_ACDID] = 0;
                target = CombatSystem.GetNearestTarget(this.World, this, this.Position, 50f, Actors.ActorType.Player);
                if (target != null)
                {
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = unchecked((int)target.DynamicID);
                }
            }
            if (target == null)
            {
                return;
            }
            if (!ActorUtils.CheckRange(this, target, 8f))
            {
                if (!sentWalkAnimation)
                {
                    CombatSystem.MoveToBasic(this, target, 0.1f, 69728);
                    sentWalkAnimation = true;
                }
                else
                {
                    CombatSystem.MoveToBasic(this, target, 0.1f, null);
                }
            }
            else if (target != null)
            {
                if (this.World.Game.Tick < this.Attributes[GameAttribute.Last_Action_Timestamp] + (6 * 12))
                {
                    return;
                }
                this.Attributes[GameAttribute.Last_Action_Timestamp] = this.World.Game.Tick;
                if (target.World != null)
                {
                    CombatSystem.Attack(this, target, 11465);
                    CombatSystem.ResolveCombat(this, target);
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
                }
                else
                {
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
                }
            }
        }

        public override void OnTargeted(Mooege.Core.GS.Player.Player player, TargetMessage message)
        {
//            this.Die(player);
        }

        // FIXME: Hardcoded hell. /komiga
        public void Die(Mooege.Core.GS.Player.Player player)
        {
            var killAni = new int[]{
                    0x2cd7,
                    0x2cd4,
                    0x01b378,
                    0x2cdc,
                    0x02f2,
                    0x2ccf,
                    0x2cd0,
                    0x2cd1,
                    0x2cd2,
                    0x2cd3,
                    0x2cd5,
                    0x01b144,
                    0x2cd6,
                    0x2cd8,
                    0x2cda,
                    0x2cd9
            };

            player.UpdateExp(this.Attributes[GameAttribute.Experience_Granted]);
            player.UpdateExpBonusData(player.GBHandle.Type, this.GBHandle.Type);

            this.World.BroadcastIfRevealed(new PlayEffectMessage()
            {
                ActorId = this.DynamicID,
                Effect = Effect.Hit,
                OptionalParameter = 0x2,
            }, this);

            this.World.BroadcastIfRevealed(new PlayEffectMessage()
            {
                ActorId = this.DynamicID,
                Effect = Effect.Unknown12,
            }, this);

            this.World.BroadcastIfRevealed(new PlayHitEffectMessage()
            {
                ActorID = this.DynamicID,
                HitDealer = player.DynamicID,
                Field2 = 0x2,
                Field3 = false,
            }, this);

            this.World.BroadcastIfRevealed(new FloatingNumberMessage()
            {
                ActorID = this.DynamicID,
                Number = 9001.0f,
                Type = FloatingNumberMessage.FloatType.White,
            }, this);

            this.World.BroadcastIfRevealed(new ANNDataMessage(Opcodes.ANNDataMessage13)
            {
                ActorID = this.DynamicID
            }, this);

            this.World.BroadcastIfRevealed(new PlayAnimationMessage()
            {
                ActorID = this.DynamicID,
                Field1 = 0xb,
                Field2 = 0,
                tAnim = new PlayAnimationMessageSpec[1]
                {
                    new PlayAnimationMessageSpec()
                    {
                        Field0 = 0x2,
                        Field1 = killAni[RandomHelper.Next(killAni.Length)],
                        Field2 = 0x0,
                        Field3 = 1f
                    }
                }
            }, this);

            this.World.BroadcastIfRevealed(new ANNDataMessage(Opcodes.ANNDataMessage24)
            {
                ActorID = this.DynamicID,
            }, this);

            GameAttributeMap attribs = new GameAttributeMap();
            attribs[GameAttribute.Hitpoints_Cur] = 0f;
            attribs[GameAttribute.Could_Have_Ragdolled] = true;
            attribs[GameAttribute.Deleted_On_Server] = true;

            foreach (var msg in attribs.GetMessageList(this.DynamicID))
                this.World.BroadcastIfRevealed(msg, this);

            this.World.BroadcastIfRevealed(new PlayEffectMessage()
            {
                ActorId = this.DynamicID,
                Effect = Effect.Unknown12
            }, this);

            this.World.BroadcastIfRevealed(new PlayEffectMessage()
            {
                ActorId = this.DynamicID,
                Effect = Effect.Burned2
            }, this);
            /*
            this.World.BroadcastIfRevealed(new PlayHitEffectMessage()
            {
                ActorID = this.DynamicID,
                HitDealer = player.DynamicID,
                Field2 = 0x2,
                Field3 = false,
            }, this);
            */
            this.World.SpawnRandomDrop(player, this.Position);
            this.World.SpawnGold(player, this.Position);
            int rGlobes = RandomHelper.Next(1, 100);
            if (rGlobes < 20)
                this.World.SpawnGlobe(player, this.Position);
            this.Destroy();
        }

        public void Die()
        {
            var players = this.World.GetPlayersInRange(this.Position, 480.0f);
            foreach (var player in players)
            {
                player.UpdateExp(this.Attributes[GameAttribute.Experience_Granted]);
                player.UpdateExpBonusData(player.GBHandle.Type, this.GBHandle.Type);
                this.World.SpawnRandomDrop(player, this.Position);
                this.World.SpawnGold(player, this.Position);
                int rGlobes = RandomHelper.Next(1, 100);
                if (rGlobes < 20)
                    this.World.SpawnGlobe(player, this.Position); // should be shared globe
            }
            this.Destroy();
        }
    }
}
