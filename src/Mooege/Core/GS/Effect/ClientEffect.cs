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
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Actors;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Effect;
using Mooege.Net.GS.Message.Definitions.Tick;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Map;
using Mooege.Common.Helpers;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Net.GS.Message.Definitions.Actor;
using Mooege.Common;

namespace Mooege.Core.GS.FXEffect
{
    public class FXEffect
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        // TODO: deal with repeated casting of the same overlapping effect with actor (e. g. lethal decoy)
        // TODO: after ComplexEffectAddMessage is decyphered switch from sending multiple effect to sending one complex

        public int EffectID { get; set; }
        public Actor Actor { get; set; } // initial actor for effect + attachment
        public Actor Target { get; set; } // target actor, used when effect is Actor->Target
        public EffectActor ProxyActor { get; protected set; } // newly created proxy actor if DurationInTicks present
        public int? StartingTick { get; set; } // don't spawn until Game.Tick >= StartingTick
        public int? DurationInTicks { get; set; } // longetivity of effect 
        public bool NeedsActor { get; set; } // proxy actor - some effects (mainly those lingering in world for time) need actor
        public Vector3D Position { get; set; } // some effects are cast on Position
        public float Angle { get; set; } // some effects need angle
        public bool Attached { get; set; } // some lingering effects are attached to other actors

        private Boolean _started = false;

        /*
         * Returns true when effect should be removed from list
         */
        public bool Process(int tick)
        {
            if ((this.Actor == null) || (this.Actor.World == null) || (this.Actor.World.GetActor(this.Actor.DynamicID) == null))
            {
                // actor already left world, remove effect
                return true;
            }
            if (!_started)
            {
                // check if effect should start
                if (!this.StartingTick.HasValue || (tick >= this.StartingTick))
                {
                    // immediate or effect should start
                    if (!DurationInTicks.HasValue)
                    {
                        // one-shot effect
                        if (this.Target == null)
                        {
                            // effect on Actor
                            this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                            {
                                Id = 0x7a,
                                ActorId = this.Actor.DynamicID,
                                Effect = Effect.PlayEffectGroup,
                                OptionalParameter = this.EffectID,
                            }, this.Actor);
                        }
                        else
                        {
                            // effect Actor->Target
                            this.Actor.World.BroadcastIfRevealed(new EffectGroupACDToACDMessage()
                            {
                                Id = 0x7a,
                                Field0 = unchecked((int)this.Actor.DynamicID),
                                Field1 = unchecked((int)this.Target.DynamicID),
                                Field2 = this.EffectID,
                            }, this.Actor);
                        }
                        EffectStartingAction();
                        return true;
                    }
                    else
                    {
                        if (!this.StartingTick.HasValue)
                        {
                            // lingering effect without set start, set actual tick
                            this.StartingTick = Actor.World.Game.Tick;
                        }
                        // lingering effect
                        if (this.NeedsActor)
                        {
                            // create proxy actor
                            this.ProxyActor = CreateProxyActor();
                            if (ProxyActor == null)
                            {
                                return true;
                            }
                            // attach if needed
                            if (this.Attached)
                            {
                                GameAttributeMap map = new GameAttributeMap();
                                map[GameAttribute.Attached_To_ACD] = unchecked((int)this.Actor.DynamicID);
                                map[GameAttribute.Attachment_Handled_By_Client] = true;
                                map[GameAttribute.Actor_Updates_Attributes_From_Owner] = true;
                                Actor a = (this.NeedsActor ? ProxyActor : Actor);
                                foreach (var msg in map.GetMessageList(a.DynamicID))
                                    this.ProxyActor.World.BroadcastIfRevealed(msg, a);
                            }
                        }
                        if (this.NeedsActor && ProxyActor.DynamicID == EffectActor.GenericPowerProxyID) // generic power proxy
                        {
                            // not sure if needed
                            this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                            {
                                Id = 0x7a,
                                ActorId = this.ProxyActor.DynamicID,
                                Effect = Effect.PlayEffectGroup,
                                OptionalParameter = this.EffectID,
                            }, this.ProxyActor);
                        }
                        EffectStartingAction();
                        if (DurationInTicks == -1)
                        {
                            // infinite duration ("until is destroyed")
                            return true;
                        }
                        _started = true;
                    }
                }
            }
            else
            {
                // check if effect should end
                if ((this.NeedsActor) && ((this.ProxyActor == null) || (this.ProxyActor.World == null) || (this.ProxyActor.World.GetActor(this.ProxyActor.DynamicID) == null)))
                {
                    // proxy actor already left world, remove effect
                    EffectEndingAction();
                    return true;
                }
                if (tick > this.StartingTick + DurationInTicks)
                {
                    EffectEndingAction();
                    if (this.NeedsActor)
                    {
                        // destroy proxy actor for this effect
                        DestroyProxyActor();
                    }
                    return true;
                }
            }
            return false;
        }

        // create proxy actor for this effect
        protected virtual EffectActor CreateProxyActor()
        {
            
            if ((EffectID == 99241) || (EffectID == 208435))
            {
                return new EffectActor(Actor.World, EffectID, Actor.Position);
            }
            else if ((EffectID == 169904) || (EffectID == 123885))
            {
                return new MysticAllyEffectActor(Actor.World, EffectID, Actor.Position, Actor);
            }
            else if (EffectID == 98557)
            {
                return new EffectActor(Actor.World, EffectID, Actor.Position);
            }
            else if (EffectID == 81103)
            {
                // hydra - fire pool
//                return new EffectActor(Actor.World, EffectID, Position); // needs scaling down A LOT
                return new EffectActor(Actor.World, 81239, Position); // now returning arcane
            }
            else
            {
                return new EffectActor(Actor.World, EffectActor.GenericPowerProxyID, Actor.Position);
            }
        }

        // destroy proxy actor for this effect
        protected virtual void DestroyProxyActor()
        {
            ProxyActor.Die();
        }

        // effect start actions
        protected virtual void EffectStartingAction()
        {
            /* streaming skills (which drain resource until dismissed:
             * starts using with targetMessage
             * ends with DWordDataMessage5
            */
            // temporary HACK: TODO: move to subclasses + add sending visuals to other players
            if (EffectID == 99694)
            {
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfEvasion;
                GameAttributeMap map = new GameAttributeMap();
                // skill effect
                map[GameAttribute.Dodge_Chance_Bonus] += 0.3f;
                // icon + cooldown
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] += 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = Actor.World.Game.Tick;//Actor.World.Game.Tick;
                map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = Actor.World.Game.Tick + (60 * 120); // 60 ticks per second 
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 30);
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
                {
                    // first mantra casted
                    map[GameAttribute.Buff_Active, PowerSNO] = true;
                    // visual effect
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 140190) {
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfHealing;
                GameAttributeMap map = new GameAttributeMap();
                // skill effect
                map[GameAttribute.Dodge_Chance_Bonus] += 0.3f;
                // icon + cooldown
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] += 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = Actor.World.Game.Tick;//Actor.World.Game.Tick;
                map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = Actor.World.Game.Tick + (60 * 120); // 60 ticks per second 
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 30);
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
                {
                    // first mantra casted
                    map[GameAttribute.Buff_Active, PowerSNO] = true;
                    // visual effect
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 146990) {
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfConviction;
                GameAttributeMap map = new GameAttributeMap();
                // skill effect
                map[GameAttribute.Dodge_Chance_Bonus] += 0.3f;
                // icon + cooldown
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] += 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = Actor.World.Game.Tick;//Actor.World.Game.Tick;
                map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = Actor.World.Game.Tick + (60 * 120); // 60 ticks per second 
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 30);
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
                {
                    // first mantra casted
                    map[GameAttribute.Buff_Active, PowerSNO] = true;
                    // visual effect
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 142987)
            {
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfRetribution;
                GameAttributeMap map = new GameAttributeMap();
                // skill effect
                map[GameAttribute.Dodge_Chance_Bonus] += 0.3f;
                // icon + cooldown
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] += 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = Actor.World.Game.Tick;//Actor.World.Game.Tick;
                map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = Actor.World.Game.Tick + (60 * 120); // 60 ticks per second 
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 30);
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 1)
                {
                    // first mantra casted
                    map[GameAttribute.Buff_Active, PowerSNO] = true;
                    // visual effect
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if ((EffectID == 99241) || (EffectID == 208435))
            {
                // LethalDecoy
                //                Actor.Attributes[GameAttribute.Custom_Target_Weight] = 100f; // on decoy 
                //                 map[GameAttribute.Forced_Enemy_ACDID]
                //                map[GameAttribute.Is_Player_Decoy] = true; // on decoy
                List<Actor> actors = this.Actor.World.GetActorsInRange(Actor.Position, 8f);
                Monster monster = null;
                for (int i = 0; i < actors.Count; i++)
                {
                    if ((actors[i].World != null) && (actors[i].ActorType == ActorType.Monster))
                    {
                        monster = (actors[i] as Monster);
                        monster.Attributes[GameAttribute.Forced_Enemy_ACDID] = unchecked((int)ProxyActor.DynamicID); // redying monsters
                        monster.Attributes[GameAttribute.Last_ACD_Attacked] = unchecked((int)ProxyActor.DynamicID);
                    }
                }

            }
            else if (EffectID == 143230)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.Serenity;
                GameAttributeMap map = new GameAttributeMap();
                // skill effect
                map[GameAttribute.Invulnerable] = true;
                map[GameAttribute.Immune_To_Knockback] = true;
                map[GameAttribute.Immune_To_Charm] = true;
                map[GameAttribute.Immune_To_Blind] = true;
                map[GameAttribute.Freeze_Immune] = true;
                map[GameAttribute.Fear_Immune] = true;
                map[GameAttribute.Stun_Immune] = true;
                map[GameAttribute.Slowdown_Immune] = true;
                map[GameAttribute.Root_Immune] = true;
                // icon + cooldown
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] = 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                map[GameAttribute.Buff_Icon_Start_Tick0, PowerSNO] = Actor.World.Game.Tick;//Actor.World.Game.Tick;
                map[GameAttribute.Buff_Icon_End_Tick0, PowerSNO] = Actor.World.Game.Tick + (60 * 3); // 60 ticks per second 
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 60);
                map[GameAttribute.Buff_Active, PowerSNO] = true;
                    // visual effect
                map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 92225)
            {
                // doesn't work (exploding palm 2)
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Bleeding] = true;
                map[GameAttribute.Bleed_Duration] = DurationInTicks.Value;
                map[GameAttribute.Buff_Visual_Effect, 92225] = true;
                foreach (var msg in map.GetMessageList(Actor.DynamicID))
                    this.Actor.World.BroadcastIfRevealed(msg, Actor);
            }
            else if (EffectID == 111132)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.DashingStrike;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = true; // switch on effect
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
                Actor.World.BroadcastIfRevealed(new NotifyActorMovementMessage
                {
                    ActorId = unchecked((int)Actor.DynamicID),
                    Position = Position,
                    Angle = Angle,
                    Field3 = false,
                    Speed = EffectActor.GetDistance(Actor.Position, Position) / DurationInTicks, // speed, distance per tick
                    //Field5 = 0x00220008, // ???
                    AnimationTag = (Actor as Player.Player).Properties.Gender == 0 ? 69840 : 90432// animation TAG, 0xFFFFFFFF - actor stopped (use idle?)
                }, Actor);
                Actor.Position = Position; // update position on server
            }
            else if (EffectID == 143782)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.LashingTailKick;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 3);
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 98826)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.SevenSidedStrike;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 30);
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 145011)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.WaveOfLight;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Power_Cooldown_Start, PowerSNO] = Actor.World.Game.Tick;
                map[GameAttribute.Power_Cooldown, PowerSNO] = Actor.World.Game.Tick + (60 * 15);
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 162301)
            {
                // Archon TEST
                /* // not working
                Actor.World.BroadcastIfRevealed(new ACDChangeActorMessage
                {
                    Field0 = unchecked((int)Actor.DynamicID),
                    Field1 = EffectID
                }, Actor);
                 */
            }
            else if (EffectID == 2588)
            {
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                {
                    ActorId = Actor.DynamicID,
                    Effect = Effect.PlayEffectGroup,
                    OptionalParameter = 137107,
                }, Actor);
                // blinding flash
                List<Actor> actors = this.Actor.World.GetActorsInRange(Actor.Position, 20f);
                for (int i = 0; i < actors.Count; i++)
                {
                    if ((actors[i].World != null) && (actors[i].ActorType == ActorType.Monster))
                    {
                        this.Actor.World.AddEffect(new FXEffect { Actor = actors[i], EffectID = 137107, DurationInTicks = (60 * 5) });
                    }
                }
            }
            else if (EffectID == 137107)
            {
                // blind flash contact
                Actor.Attributes[GameAttribute.Hit_Chance] -= 0.4f;
                if (Actor.Attributes[GameAttribute.Hit_Chance] < 0.05)
                {
                    Actor.Attributes[GameAttribute.Hit_Chance] = 0.05f;
                }
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Blind] = true;
                map[GameAttribute.Buff_Visual_Effect, 2032] = true;
                map[GameAttribute.Hit_Chance] = Actor.Attributes[GameAttribute.Hit_Chance]; 
                foreach (var msg in map.GetMessageList(Actor.DynamicID))
                    this.Actor.World.BroadcastIfRevealed(msg, Actor);
            }
            // Tempest rush               map[GameAttribute.Skill_Toggled_State]
            //map[GameAttribute.Spawned_by_ACDID]
            //map[GameAttribute.Spawner_Concurrent_Count_ID]
            //map[GameAttribute.Summon_Count]
            //map[GameAttribute.Summon_Expiration_Tick]
            //map[GameAttribute.Summoned_By_ACDID]
            //map[GameAttribute.Summoned_By_SNO]
            //map[GameAttribute.Summoner_ID]
        }

        // effect end actions
        protected virtual void EffectEndingAction()
        {
            if (EffectID == 99694)
            {
                // temporary HACK: TODO: move to subclasses
                GameAttributeMap map = new GameAttributeMap();
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfEvasion;
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] -= 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0) {
                    // last mantra casted expired
                    this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        Id = 0x7a,
                        ActorId = this.Actor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 199677,
                    }, this.Actor);
                    map[GameAttribute.Dodge_Chance_Bonus] -= 0.3f;
                    map[GameAttribute.Buff_Active, PowerSNO] = false;
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if ((EffectID == 99241) || (EffectID == 208435))
            {
                // LethalDecoy
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        ActorId = this.ProxyActor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 99504
                    }, this.ProxyActor);
                List<Actor> actors = this.Actor.World.GetActorsInRange(ProxyActor.Position, 20f);
                for (int i = 0; i < actors.Count; i++)
                {
                    if ((actors[i].World != null) && (actors[i].ActorType == ActorType.Monster))
                    {
                        (actors[i] as Monster).Die((Actor as Player.Player));
                    }
                }
            }
            else if (EffectID == 92225)
            {
                // doesn't work (exploding palm 2)
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Bleeding] = false;
                map[GameAttribute.Bleed_Duration] = 0;
                map[GameAttribute.Buff_Visual_Effect, 92225] = false;
                foreach (var msg in map.GetMessageList(Actor.DynamicID))
                    this.Actor.World.BroadcastIfRevealed(msg, Actor);
            }
            else if (EffectID == 143230)
            {
                GameAttributeMap map = new GameAttributeMap();
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.Serenity;
                // skill effect
                map[GameAttribute.Invulnerable] = false;
                map[GameAttribute.Immune_To_Knockback] = false;
                map[GameAttribute.Immune_To_Charm] = false;
                map[GameAttribute.Immune_To_Blind] = false;
                map[GameAttribute.Freeze_Immune] = false;
                map[GameAttribute.Fear_Immune] = false;
                map[GameAttribute.Stun_Immune] = false;
                map[GameAttribute.Slowdown_Immune] = false;
                map[GameAttribute.Root_Immune] = false;
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] = 0; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                {
                    Id = 0x7a,
                    ActorId = this.Actor.DynamicID,
                    Effect = Effect.PlayEffectGroup,
                    OptionalParameter = 143230,
                }, this.Actor);
                map[GameAttribute.Buff_Active, PowerSNO] = false;
                map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 140190)
            {
                GameAttributeMap map = new GameAttributeMap();
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfHealing;
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] -= 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0)
                {
                    // last mantra casted expired
                    this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        Id = 0x7a,
                        ActorId = this.Actor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 199677,
                    }, this.Actor);
                    map[GameAttribute.Buff_Active, PowerSNO] = false;
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 146990)
            {
                GameAttributeMap map = new GameAttributeMap();
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfConviction;
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] -= 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0)
                {
                    // last mantra casted expired
                    this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        Id = 0x7a,
                        ActorId = this.Actor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 199677,
                    }, this.Actor);
                    map[GameAttribute.Buff_Active, PowerSNO] = false;
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 142987)
            {
                GameAttributeMap map = new GameAttributeMap();
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfRetribution;
                Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO] -= 1; // update attributes on server too
                map[GameAttribute.Buff_Icon_Count0, PowerSNO] = Actor.Attributes[GameAttribute.Buff_Icon_Count0, PowerSNO];
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0)
                {
                    // last mantra casted expired
                    this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        Id = 0x7a,
                        ActorId = this.Actor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 199677,
                    }, this.Actor);
                    map[GameAttribute.Buff_Active, PowerSNO] = false;
                    map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                }
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
            }
            else if (EffectID == 98557)
            {
                // inner sanct
                this.ProxyActor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        ActorId = this.ProxyActor.DynamicID,
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 98556
                    }, this.ProxyActor);
            }
            else if (EffectID == 111132)
            {
                int PowerSNO = Skills.Skills.Monk.SpiritSpenders.DashingStrike;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Power_Buff_0_Visual_Effect_None, PowerSNO] = false; // switch off effect
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                {
                    ActorId = this.Actor.DynamicID,
                    Effect = Effect.PlayEffectGroup,
                    OptionalParameter = 113720
                }, this.Actor);
                List<Actor> actors = this.Actor.World.GetActorsInRange(Actor.Position, 8f);
                for (int i = 0; i < actors.Count; i++)
                {
                    if ((actors[i].World != null) && (actors[i].ActorType == ActorType.Monster))
                    {
                        (actors[i] as Monster).Die((Actor as Player.Player));
                    }
                }
            }
            else if (EffectID == 162301)
            {
                int sno = ((Actor as Player.Player).Properties.Gender == 0) ? 6544 : 6526;
                Actor.World.BroadcastIfRevealed(new ACDChangeActorMessage
                {
                    Field0 = unchecked((int)Actor.DynamicID),
                    Field1 = sno
                }, Actor);
            }
            else if (EffectID == 137107)
            {
                // stop blind
                Actor.Attributes[GameAttribute.Hit_Chance] += 0.4f;
                if (Actor.Attributes[GameAttribute.Hit_Chance] > 0.95)
                {
                    Actor.Attributes[GameAttribute.Hit_Chance] = 0.95f;
                }
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Blind] = false;
                map[GameAttribute.Buff_Visual_Effect, 2032] = false;
                foreach (var msg in map.GetMessageList(Actor.DynamicID))
                    this.Actor.World.BroadcastIfRevealed(msg, Actor);
            }
        }
    }

    public class ProjectileFXEffect : FXEffect
    {
        public float Speed = 0.1f;

        public ProjectileFXEffect()
            : base()
        {
            NeedsActor = true;

        }

        protected override EffectActor CreateProxyActor()
        {
//            return new PowerProjectile(this.Actor.World, EffectID, this.Position, Target.Position, 1f, 3000);
            return new Projectile(this.Actor.World, this.Actor, EffectID, this.Position, Angle, Speed, (DurationInTicks.HasValue ? (StartingTick + DurationInTicks) : null));
        }

    }

    public class EffectActor : Actor
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();

        public const int GenericPowerProxyID = 4176;

        public override ActorType ActorType { get { return ActorType.Effect; } }

        protected int? idleAnimationSNO;

        protected int ticksBetweenActions = 30; // 500 ms

        public EffectActor(World world, int actorSNO, Vector3D position)
            : base(world, world.NewActorID)
        {
            this.ActorSNO = actorSNO;
            // FIXME: This is hardcoded crap
            this.Field2 = 0x8;
            this.Field3 = 0x0;
            this.Scale = 1f;
            this.Position.Set(position);
            this.RotationAmount = (float)(RandomHelper.NextDouble() * 2.0f * Math.PI);
            this.RotationAxis.X = 0f; this.RotationAxis.Y = 0f; this.RotationAxis.Z = 1f;
            this.GBHandle.Type = (int)GBHandleType.ClientEffect; this.GBHandle.GBID = actorSNO;
            this.Field7 = 0x00000001;
            this.Field8 = this.ActorSNO;
            this.Field10 = 0x0;
            this.Field11 = 0x0;
            this.Field12 = 0x0;
            this.Field13 = 0x0;
            this.Attributes[GameAttribute.Cannot_Be_Added_To_AI_Target_List] = true;
            this.Attributes[GameAttribute.Is_Power_Proxy] = true;
            this.Attributes[GameAttribute.Untargetable] = true;
            this.Attributes[GameAttribute.UntargetableByPets] = true;
            this.Attributes[GameAttribute.Invulnerable] = true;
            setAdditionalAttributes();
            if (this.GetType().Equals(typeof(EffectActor)))
            {
                this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
            }
        }

        public virtual void setAdditionalAttributes()
        {
        }

        public override bool Reveal(Mooege.Core.GS.Player.Player player)
        {
            if (!base.Reveal(player))
            {
                return false;
            }
            if (this.idleAnimationSNO.HasValue)
            {
                player.InGameClient.SendMessage(new SetIdleAnimationMessage
                {
                    ActorID = this.DynamicID,
                    AnimationSNO = this.idleAnimationSNO.Value
                });
            }
            return true;
        }

        public virtual void Die()
        {
            this.World.BroadcastIfRevealed(new ANNDataMessage(Opcodes.ANNDataMessage24)
            {
                ActorID = this.DynamicID,
            }, this);
            this.Destroy();
        }

        //  should go to some Utils
        public static bool CheckRange(Actor actor, Actor target, float range)
        {
            if (target == null) return false;
            return (Math.Sqrt(Math.Pow(actor.Position.X - target.Position.X, 2) + Math.Pow(actor.Position.Y - target.Position.Y, 2)) < range);
        }

        //  should go to some Utils
        public static float GetDistance(Vector3D startPosition, Vector3D targetPosition)
        {
            if (targetPosition == null) return 0;
            return (float)Math.Sqrt(Math.Pow(startPosition.X - targetPosition.X, 2) + Math.Pow(startPosition.Y - targetPosition.Y, 2));
        }

        //  should go to some Utils
        public static float[] GetDistanceDelta(float speed, float facingAngle)
        {
            float[] res = new float[2];
            res[0] = (speed * 6) * (float)Math.Cos(facingAngle); // sending this in 100ms (6 Ticks) -> walkDistance * 6 Ticks
            res[1] = (speed * 6) * (float)Math.Sin(facingAngle);
            // omitting Z axis
            return res;
        }

        //  should go to some Utils
        public static float GetFacingAngle(Vector3D lookerPosition, Vector3D targetPosition)
        {
            return (float)Math.Atan2((targetPosition.Y - lookerPosition.Y), (targetPosition.X - lookerPosition.X));
        }
    }

    public class MysticAllyEffectActor : MovableEffectActor
    {
        public override ActorType ActorType { get { return ActorType.NPC; } }


        public MysticAllyEffectActor(World world, int actorSNO, Vector3D position, Actor owner) :
            base(world, actorSNO, position)
        {
            this.Attributes[GameAttribute.Summoned_By_ACDID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Summoned_By_SNO] = Skills.Skills.Monk.SpiritSpenders.MysticAlly;
            this.Attributes[GameAttribute.Summoner_ID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Follow_Target_ACDID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
            this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
         }

        public override void setAdditionalAttributes()
        {
            this.Attributes[GameAttribute.Cannot_Be_Added_To_AI_Target_List] = false;
            this.Attributes[GameAttribute.Is_Power_Proxy] = false;
            this.Attributes[GameAttribute.Untargetable] = true;
            this.Attributes[GameAttribute.UntargetableByPets] = true;
            this.Attributes[GameAttribute.Invulnerable] = false;

            this.Attributes[GameAttribute.Uninterruptible] = true;

            this.Attributes[GameAttribute.Hitpoints_Max_Total] = 100f;
            this.Attributes[GameAttribute.Hitpoints_Max] = 100f;
            this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 0f;
            this.Attributes[GameAttribute.Hitpoints_Cur] = 100f;
            this.Attributes[GameAttribute.TeamID] = 1;
            this.Attributes[GameAttribute.Level] = 1;

            this.GBHandle.Type = (int)GBHandleType.CustomBrain;
            this.idleAnimationSNO = (this.ActorSNO == 169904) ? 69968 : 69632;
            this.walkAnimationSNO = (this.ActorSNO == 169904) ? 69728 : 69728; // TODO: find tags
            this.attackAnimationSNO = (this.ActorSNO == 169904) ? 69776 : 130509; // TODO: find tags
            this.speed = 0.23f;
            this.Scale = 1.22f;
            this.ticksBetweenActions = 6 * 9; // 900 ms
        }

        public override void Update()
        {
            Player.Player owner = this.World.GetPlayer((uint)this.Attributes[GameAttribute.Summoner_ID]);
            if (GetDistance(this.Position, owner.Position) > 200)
            {
                // player is too far away, warp to his position
                this.Position = owner.Position;
                this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
                this.World.BroadcastIfRevealed(ACDWorldPositionMessage, this);
                return;
            }
            Actor target = null;
            if (this.Attributes[GameAttribute.Last_ACD_Attacked] != 0)
            {
                target = this.World.GetActor((uint)this.Attributes[GameAttribute.Last_ACD_Attacked]);
            }
            if ((target == null) || (target.World == null))
            {
                target = GetTarget(this.Position, 50f);
                if (target != null)
                {
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = unchecked((int)target.DynamicID);
                }
            }
            if (!CheckRange(this, target != null ? target : owner, 12f))
            {
                MoveTo(target != null ? target : owner);
            }
            else if (target != null)
            {
                if (this.World.Game.Tick < this.Attributes[GameAttribute.Last_Action_Timestamp] + this.ticksBetweenActions)
                {
                    return;
                }
                this.Attributes[GameAttribute.Last_Action_Timestamp] = this.World.Game.Tick;
                if (target.World != null)
                {
                    Attack(target);
                    (target as Monster).Die((owner as Player.Player));
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
                }
                else
                {
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
                }
            }
        }

    }

    /*
     * Actor attacking
     */
    public class AttackingEffectActor : EffectActor
    {
        protected int attackAnimationSNO;

        public AttackingEffectActor(World world, int actorSNO, Vector3D position) : base(world, actorSNO, position) {
            if (this.GetType().Equals(typeof(AttackingEffectActor)))
            {
                this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
            }

        }

        protected virtual void Attack(Actor target)
        {
            if (target == null)
            {
                return;
            }
            if (attackAnimationSNO != 0)
            {
                this.World.BroadcastIfRevealed(new PlayAnimationMessage()
                {
                    ActorID = this.DynamicID,
                    Field1 = 0x3,
                    Field2 = 0,
                    tAnim = new PlayAnimationMessageSpec[1]
                {
                    new PlayAnimationMessageSpec()
                    {
                        Field0 = 0x2,
                        Field1 = attackAnimationSNO,
                        Field2 = 0x0,
                        Field3 = 1f
                    }
                }
                }, this);
            }
        }

        protected virtual Actor GetTarget(Vector3D centerPosition, float range)
        {
            Actor result = null;
            List<Actor> actors = this.World.GetActorsInRange(centerPosition, range);
            if (actors.Count > 1)
            {
                float distanceNearest = range; // max. range
                float distance = 0f;
                // TODO: get nearest monster
                for (int i = 0; i < actors.Count; i++)
                {
                    if (actors[i] == this)
                    {
                        // don't target self
                        continue;
                    }
                    if (actors[i].ActorType == Actors.ActorType.Monster)
                    {
                        if ((actors[i].World == null) || (this.World.GetActor(actors[i].DynamicID) == null))
                        {
                            // leaving world
                            continue;
                        }
                        distance = GetDistance(centerPosition, actors[i].Position);
                        if ((result == null) || (distance < distanceNearest))
                        {
                            result = actors[i];
                            distanceNearest = distance;
                        }
                    }
                }
            }
            return result;
        }

    }

    /*
     * Actor attacking and moving
     */
    public class MovableEffectActor : AttackingEffectActor
    {
        protected int walkAnimationSNO;

        private bool _cLientKnowsWalkAnimation = false;

        protected float speed = 0.1f; // distance per 1 Tick

        protected Vector3D velocity;

        public MovableEffectActor(World world, int actorSNO, Vector3D position)
            : base(world, actorSNO, position)
        {
            if (this.GetType().Equals(typeof(MovableEffectActor)))
            {
                this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
            }
        }

        protected virtual void MoveTo(Actor target)
        {
            if (target == null)
            {
                return;
            }

            float angle = EffectActor.GetFacingAngle(this.Position, target.Position);
            float[] delta = EffectActor.GetDistanceDelta(this.speed, angle);
            Position.X += delta[0];
            Position.Y += delta[1];
            angle = GetFacingAngle(this.Position, target.Position);

            if (_cLientKnowsWalkAnimation)
            {
                this.World.BroadcastIfRevealed(new NotifyActorMovementMessage()
                {
                    ActorId = (int)this.DynamicID,
                    Position = this.Position,
                    Angle = angle,
                    Id = 0x006E,
                }, this);
            }
            else
            {
                this.World.BroadcastIfRevealed(new NotifyActorMovementMessage()
                {
                    ActorId = (int)this.DynamicID,
                    Position = this.Position,
                    Angle = angle,
                    Field3 = false,
                    Speed = this.speed, // distance in Tick == speed
                    Field5 = 0,
                    Id = 0x006E,
                    AnimationTag = this.walkAnimationSNO,

                }, this);
                _cLientKnowsWalkAnimation = true;
            }
        }

        protected virtual void MoveTo(Vector3D targetPosition)
        {
            if (targetPosition == null)
            {
                return;
            }

            float angle = EffectActor.GetFacingAngle(this.Position, targetPosition);
            float[] delta = EffectActor.GetDistanceDelta(this.speed, angle);
            Position.X += delta[0];
            Position.Y += delta[1];
            angle = GetFacingAngle(this.Position, targetPosition);

            if (_cLientKnowsWalkAnimation)
            {
                this.World.BroadcastIfRevealed(new NotifyActorMovementMessage()
                {
                    ActorId = (int)this.DynamicID,
                    Position = this.Position,
                    Angle = angle,
                    Id = 0x006E,
                }, this);
            }
            else
            {
                this.World.BroadcastIfRevealed(new NotifyActorMovementMessage()
                {
                    ActorId = (int)this.DynamicID,
                    Position = this.Position,
                    Angle = angle,
                    Field3 = false,
                    Speed = this.speed, // distance in Tick == speed
                    Field5 = 0,
                    Id = 0x006E,
                    AnimationTag = this.walkAnimationSNO,

                }, this);
                _cLientKnowsWalkAnimation = true;
            }
        }

        protected virtual void ShootAtAngle(float angle, float speed)
        {
            this.World.BroadcastIfRevealed(new ACDTranslateFixedMessage()
            {
                Id = 113, // needed
                ActorId = unchecked((int)this.DynamicID),
                Velocity = this.velocity,
                Field2 = 1,
                AnimationTag = 1,//this.walkAnimationSNO,
                Field4 = 1,
            }, this);
        }

    }

    public class Projectile : MovableEffectActor
    {
        public override ActorType ActorType { get { return ActorType.Projectile; } }
        protected int? expiresInTick;
        protected Actor shooter;
        protected bool destroyWhenBlocked;

        public Projectile(World world, Actor shooter, int actorSNO, Vector3D position, float angle, float speed, int? expiresInTick, bool destroyWhenBlocked = false)
            : base(world, actorSNO, position) 
        {
            this.destroyWhenBlocked = destroyWhenBlocked;
            this.shooter = shooter;
            Scale = 1f;
            this.speed = speed;
            this.expiresInTick = expiresInTick;
            this.Field7 = 0x00000001;
            this.Field8 = this.ActorSNO;
            this.Field10 = 0x1;
            this.Field11 = 0x1;
            this.Field12 = 0x1;
            this.Field13 = 0x1;
            this.CollFlags = 0x4;
            this.RotationAmount = (float)Math.Cos(angle / 2);
            this.RotationAxis.X = 0f;
            this.RotationAxis.Y = 0f;
            this.RotationAxis.Z = (float)Math.Sin(angle / 2);
            this.GBHandle.Type = (int)GBHandleType.Projectile; this.GBHandle.GBID = 1;
            float[] delta = GetDistanceDelta(speed, angle);
            this.velocity = new Vector3D { X = delta[0], Y = delta[1], Z = 0 };

            this.Attributes[GameAttribute.Projectile_Speed] = speed;
            this.Attributes[GameAttribute.Destroy_When_Path_Blocked] = destroyWhenBlocked;
            if (this.GetType().Equals(typeof(Projectile)))
            {
                this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
            }
            ShootAtAngle(angle, speed);
        }

        public override void setAdditionalAttributes()
        {
            //this.GBHandle.Type = (int)GBHandleType.Projectile; this.GBHandle.GBID = 1;
        }

        public override void Update()
        {
            if (expiresInTick.HasValue && (this.World.Game.Tick >= expiresInTick.Value))
            {
                this.Die();
                return;
            }
            // TODO: fix targetting info
            this.Position.X += velocity.X * 6;
            this.Position.Y += velocity.Y * 6;
            Actor target = GetTarget(this.Position, 5f); // TODO: expand targetting (line, arc, target types)
            if (target != null)
            {
                (target as Monster).Die((shooter as Player.Player));
            }
            // if hydra, spawn effect 81874 on target
        }

        public override void Die()
        {
            base.Die();
        }

        public override bool Reveal(Mooege.Core.GS.Player.Player player)
        {
            if (!base.Reveal(player))
                return false;

            player.InGameClient.SendMessage(new SetIdleAnimationMessage
            {
                ActorID = this.DynamicID,
                AnimationSNO = 0x0
            });

            player.InGameClient.SendMessage(new EndOfTickMessage()
            {
                Field0 = player.InGameClient.Game.Tick,
                Field1 = player.InGameClient.Game.Tick + 20
            });

            return true;
        }

        
    }

    public class HydraEffectActor : AttackingEffectActor
    {

        public HydraEffectActor(World world, int actorSNO, Vector3D position, int attackOffsetTick, Actor owner)
            : base(world, actorSNO, position)
        {
            this.Attributes[GameAttribute.Last_Action_Timestamp] = world.Game.Tick - attackOffsetTick;
            this.Attributes[GameAttribute.Spawned_by_ACDID] = unchecked((int)owner.DynamicID);
            this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
        }

        public override void setAdditionalAttributes()
        {
            this.GBHandle.Type = (int)GBHandleType.CustomBrain;
//            this.idleAnimationSNO = (this.ActorSNO == 80745) ? 80658 : (this.ActorSNO == 80757) ? 80773 : 80800; // crashes!
            this.attackAnimationSNO = (this.ActorSNO == 80745) ? 80659 : (this.ActorSNO == 80757) ? 80771 : 80797;
            this.Scale = 1f;
            this.ticksBetweenActions = 6 * 12; // 1200 ms
        }

        public override void Update()
        {
            if (this.World.Game.Tick >= this.Attributes[GameAttribute.Last_Action_Timestamp] + this.ticksBetweenActions) {
                Actor target = GetTarget(this.Position, 30f);
                if (target != null)
                {
                    this.Attributes[GameAttribute.Last_Action_Timestamp] = this.World.Game.Tick;
                    this.World.BroadcastIfRevealed(new ACDTranslateFacingMessage()
                    {
                        Id = 0x0070,
                        ActorId = this.DynamicID,
                        Angle = GetFacingAngle(this.Position, target.Position),
                        Immediately = false
                    }, this);
                    Attack(target);
                    Vector3D pos = new Vector3D
                    {
                        X = Position.X,
                        Y = Position.Y,
                        Z = Position.Z + 6f,
                    };

                    this.World.AddEffect(new ProjectileFXEffect
                    {
                        Actor = this.World.GetActor((uint)this.Attributes[GameAttribute.Spawned_by_ACDID]),
                        EffectID = 77116,
                        Target = target,
                        NeedsActor = true,
                        DurationInTicks = (60 * 5),
                        Position = pos,
                        Speed = 0.3f,
                        StartingTick = this.World.Game.Tick,
                        Angle = GetFacingAngle(pos, target.Position),
                    });
                }
            }
        }

        public override void Die()
        {
            this.World.BroadcastIfRevealed(new PlayAnimationMessage
            {
                ActorID = this.DynamicID,
                Field1 = 0xb,
                Field2 = 0,
                tAnim = new PlayAnimationMessageSpec[1]
                {
                    new PlayAnimationMessageSpec()
                    {
                        Field0 = 0x2,
                        Field1 = (this.ActorSNO == 80745) ? 80660 : (this.ActorSNO == 80757) ? 80772 : 80799,
                        Field2 = 0x0,
                        Field3 = 1f
                    }
                }
            }, this);
            base.Die();
        }
    }

    public class HydraFXEffect : FXEffect
    {
        public int AttackOffset = 0;

        public HydraFXEffect()
            : base()
        {
            NeedsActor = true;
        }

        protected override EffectActor CreateProxyActor()
        {
            return new HydraEffectActor(this.Actor.World, EffectID, Position, AttackOffset, this.Actor);
        }
    }

    public class ClientEffect
    {
        public static void ProcessSkill(Actor actor, TargetMessage message)
        {
            switch (actor.ActorType)
            {
                case ActorType.Player:
                    ProcessSkillPlayer((actor as Player.Player), message);
                    break;
                case ActorType.Monster:
                    break;
                case ActorType.NPC:
                    break;
            }
        }

        private static void ProcessSkillPlayer(Player.Player player, TargetMessage message) {
            switch (player.Properties.Class)
            {
                case Common.Toons.ToonClass.Barbarian:
                    ProcessSkillTEST(player, player.World, message);
                    break;
                case Common.Toons.ToonClass.DemonHunter:
                    ProcessSkillTEST(player, player.World, message);
                    break;
                case Common.Toons.ToonClass.Monk:
                    ProcessSkillMonk(player, player.World, message);
                    break;
                case Common.Toons.ToonClass.WitchDoctor:
                    ProcessSkillTEST(player, player.World, message);
                    break;
                case Common.Toons.ToonClass.Wizard:
                    ProcessSkillTEST(player, player.World, message);
                    break;
            }
        }

        private static void ProcessSkillMonk(Player.Player player, World world, TargetMessage message) {
            Vector3D targetPosition = message.Field2.Position;
            Actor target = null;
            if (message.TargetID != 0xFFFFFFFF)
            {
                target = world.GetActor(message.TargetID);
                if (target != null)
                {
                    targetPosition = target.Position;
                }
            }
            int startingTick = world.Game.Tick;
            int effectID = 0;
            int masterEffectID = 0;
            switch (message.PowerSNO)
            {
                case Skills.Skills.Monk.SpiritGenerator.FistsOfThunder:
                    masterEffectID = 143570; // cast
                    effectID = 96176; // projectile
                    switch (message.Field5)
                    {
                        case 0:
                            startingTick += (6 * 1);
                            break;
                        case 1:
                            masterEffectID = 143561;//143569; // cast
                            effectID = 96176;//96177;
                            break;
                        case 2:
                            masterEffectID = 143566; // cast
                            effectID = 96178;
                            startingTick += (6 * 3);
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = masterEffectID, StartingTick = startingTick });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    break;
                case Skills.Skills.Monk.SpiritGenerator.ExplodingPalm:
                    effectID = 142471;
                    masterEffectID = 143841;
                    switch (message.Field5)
                    {
                        case 0:
                            break;
                        case 1:
                            break;
                        case 2:
                            effectID = 142473;
                            masterEffectID = 143473;
                            startingTick += (6 * 2);
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = masterEffectID, StartingTick = startingTick });
                    /* // to effect
                    if ((target != null) & (message.Field5 == 2))
                    {
                        world.AddEffect(new Effect { Actor = target, EffectID = 92225, DurationInTicks = (60 * 3) });
                    }
                     */
                    break;
                case Skills.Skills.Monk.SpiritGenerator.DeadlyReach:
                    masterEffectID = 140870;
                    switch (message.Field5)
                    {
                        case 0:
                            startingTick += (6 * 1);
                            break;
                        case 1:
                            masterEffectID = 140871;
                            startingTick += (6 * 1);
                            break;
                        case 2:
                            masterEffectID = 140872;
                            startingTick += (6 * 3);
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = masterEffectID, StartingTick = startingTick });
                    break;
                case Skills.Skills.Monk.SpiritGenerator.CripplingWave:
                    effectID = 2603;
                    switch (message.Field5)
                    {
                        case 0:
                            break;
                        case 1:
                            effectID = 147912;
                            break;
                        case 2:
                            effectID = 147929;
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, });
                    break;
                case Skills.Skills.Monk.SpiritGenerator.SweepingWind:
                    effectID = 196981;
                    switch (message.Field5)
                    {
                        case 0:
                            break;
                        case 1:
                            effectID = 196983;
                            break;
                        case 2:
                            effectID = 196984;
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, });
                    break;
                case Skills.Skills.Monk.SpiritGenerator.WayOfTheHundredFists:
                    effectID = 2612;//(player.Properties.Gender == 0) ? 2612 : ???;
                    masterEffectID = 137345;//(player.Properties.Gender == 0) ? 137345 ; ???;
                    switch (message.Field5)
                    {
                        case 0:
                            startingTick += (6 * 3);
                            break;
                        case 1:
                            effectID = 98412;//(player.Properties.Gender == 0) ? 98412 : ???;
                            masterEffectID = 137346;//(player.Properties.Gender == 0) ? 137346 : ???;
                            break;
                        case 2:
                            startingTick += (6 * 2);
                            masterEffectID = 137347;//(player.Properties.Gender == 0) ? 137347 : ???;
                            effectID = 98416;//(player.Properties.Gender == 0) ? 98416 : ???;
                            break;
                    }
                    world.AddEffect(new FXEffect { Actor = player, EffectID = masterEffectID, StartingTick = startingTick });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.DashingStrike:
                    world.AddEffect(new FXEffect
                    {
                        Actor = player,
                        EffectID = 192085,
                        DurationInTicks = 6,
                    });
                    world.AddEffect(new FXEffect
                    {
                        Actor = player,
                        EffectID = 111132,
                        DurationInTicks = 6,
                        Position = targetPosition,
                        Angle = EffectActor.GetFacingAngle(player.Position, targetPosition)
                    });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.LashingTailKick:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 143782 });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.WaveOfLight:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 145011, });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 144079, });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.SevenSidedStrike:
                    // TODO: find targets for effects, now targetting self
                    effectID = 98826;
                    startingTick += 12;
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    effectID = 98831;
                    startingTick += 12;
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    effectID = 98842;
                    startingTick += 12;
                    world.AddEffect(new FXEffect { Actor = player, EffectID = effectID, StartingTick = startingTick });
                    break;
            }
        }

        private static void ProcessSkillTEST(Player.Player player, World world, TargetMessage message)
        {
            Vector3D targetPosition = message.Field2.Position;
            Actor target = null;
            if (message.TargetID != 0xFFFFFFFF)
            {
                target = world.GetActor(message.TargetID);
                if (target != null)
                {
                    targetPosition = target.Position;
                }
            } switch (message.PowerSNO)
            {
                
                case Skills.Skills.Wizard.Offensive.Hydra:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 81103, DurationInTicks = (60 * 9), Position = targetPosition, NeedsActor = true });
                    world.AddEffect(new HydraFXEffect { Actor = player, EffectID = 80745, DurationInTicks = (60 * 9), Position = targetPosition, AttackOffset = 0 });
                    world.AddEffect(new HydraFXEffect { Actor = player, EffectID = 80757, DurationInTicks = (60 * 9), Position = targetPosition, AttackOffset = (6 * 4) });
                    world.AddEffect(new HydraFXEffect { Actor = player, EffectID = 80758, DurationInTicks = (60 * 9), Position = targetPosition, AttackOffset = (6 * 8) });
                    break;
            }
        }

        public static void ProcessSkillPlayer(Player.Player player, SecondaryAnimationPowerMessage message)
        {
            switch (player.Properties.Class)
            {
                case Common.Toons.ToonClass.Barbarian:
                    break;
                case Common.Toons.ToonClass.DemonHunter:
                    break;
                case Common.Toons.ToonClass.Monk:
                    ProcessSkillMonk(player, player.World, message);
                    break;
                case Common.Toons.ToonClass.WitchDoctor:
                    break;
                case Common.Toons.ToonClass.Wizard:
                    ProcessSkillTEST(player, player.World, message);
                    break;
            }
        }

        private static void ProcessSkillMonk(Player.Player player, World world, SecondaryAnimationPowerMessage message)
        {
            int effectID = 0;
            switch (message.PowerSNO)
            {
                case Skills.Skills.Monk.Mantras.MantraOfEvasion:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 143964, });
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 99694, Attached = true }); // 60 ticks/s * 120 = 120s
                    break;
                case Skills.Skills.Monk.Mantras.MantraOfHealing:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 99948, });
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 140190, Attached = true }); // 60 ticks/s * 120 = 120s
                    break;
                case Skills.Skills.Monk.Mantras.MantraOfConviction:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 95955, });
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 146990, Attached = true }); // 60 ticks/s * 120 = 120s
                    break;
                case Skills.Skills.Monk.Mantras.MantraOfRetribution:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 142974, });
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 142987, Attached = true }); // 60 ticks/s * 120 = 120s
                    break;
                case Skills.Skills.Monk.SpiritSpenders.LethalDecoy:
                    effectID = (player.Properties.Gender == 0) ? 99241 : 208435;
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 5), EffectID = effectID, NeedsActor = true }); // 60 ticks/s * 5 = 5s
                    break;
                case Skills.Skills.Monk.SpiritSpenders.BreathOfHeaven:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 101174, });
                    /*
                     * move to effect
                    Actor.Attributes[GameAttribute.Resource_Cur, player.ResourceID] -= 75f;
                    GameAttributeMap atm = new GameAttributeMap();
                    atm[GameAttribute.Resource_Cur, player.ResourceID] = Actor.Attributes[GameAttribute.Resource_Cur, player.ResourceID];
                    atm.SendMessage(client, player.DynamicID);
                     * */
                    break;
                case Skills.Skills.Monk.SpiritSpenders.InnerSanctuary:
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = (60 * 8), EffectID = 98557, NeedsActor = true });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.Serenity:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 123156, });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 142890, });
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 143230, DurationInTicks = (60 * 3) });
                    break;
                case Skills.Skills.Monk.SpiritSpenders.MysticAlly:
                    effectID = (player.Properties.Gender == 0) ? 169904 : 123885;
                    world.AddEffect(new FXEffect { Actor = player, DurationInTicks = -1, EffectID = effectID, NeedsActor = true }); // until is destroyed
                    break;
                case Skills.Skills.Monk.SpiritSpenders.BlindingFlash:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 2588 });
                    break;
            }
        }

        private static void ProcessSkillTEST(Player.Player player, World world, SecondaryAnimationPowerMessage message)
        {
            switch (message.PowerSNO)
            {
                case Skills.Skills.Wizard.Utility.Archon:
                    world.AddEffect(new FXEffect { Actor = player, EffectID = 162301, DurationInTicks = (60 * 15) });
                    break;
            }
        }
    }

}
