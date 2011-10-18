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

namespace Mooege.Core.GS.Effect
{
    public class ActorEffect
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        // TODO: deal with repeated casting of the same overlapping effect with actor (e. g. lethal decoy)
        // TODO: after ComplexEffectAddMessage is decyphered switch from sending multiple effect to sending one complex

        public int EffectID { get; set; }
        public Actor Actor { get; set; } // initial actor for effect + attachment
        public Actor Target { get; set; } // target actor, used when effect is Actor->Target
        public Actor ProxyActor { get; protected set; } // newly created proxy actor if DurationInTicks present
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
                                ActorID = this.Actor.DynamicID,
                                Field1 = 32,
                                Field2 = this.EffectID,
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
                        if (this.NeedsActor && ProxyActor.DynamicID == Effect.GenericPowerProxyID) // generic power proxy
                        {
                            // not sure if needed
                            this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                            {
                                Id = 0x7a,
                                ActorID = this.ProxyActor.DynamicID,
                                Field1 = 32,
                                Field2 = this.EffectID,
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
        protected virtual Actor CreateProxyActor()
        {
            
            if ((EffectID == 99241) || (EffectID == 208435))
            {
                return new Effect(Actor.World, EffectID, Actor.Position);
            }
            else if ((EffectID == 169904) || (EffectID == 123885))
            {
                return new MysticAllyEffect(Actor.World, EffectID, Actor.Position, Actor);
            }
            else if (EffectID == 98557)
            {
                return new Effect(Actor.World, EffectID, Actor.Position);
            }
            else
            {
                return new Effect(Actor.World, Effect.GenericPowerProxyID, Actor.Position);
            }
        }

        // destroy proxy actor for this effect
        protected virtual void DestroyProxyActor()
        {
            ProxyActor.Destroy();
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
                map[GameAttribute.Buff_Active, 92225] = true;
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
                    Field4 = Effect.GetDistance(Actor.Position, Position) / DurationInTicks, // speed, distance per tick
                    //Field5 = 0x00220008, // ???
                    Field6 = (Actor as Player.Player).Properties.Gender == 0 ? 69840 : 90432// animation TAG, 0xFFFFFFFF - actor stopped (use idle?)
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
                // blinding flash
                List<Actor> actors = this.Actor.World.GetActorsInRange(Actor.Position, 20f);
                for (int i = 0; i < actors.Count; i++)
                {
                    if ((actors[i].World != null) && (actors[i].ActorType == ActorType.Monster))
                    {
                        this.Actor.World.AddEffect(new ActorEffect { Actor = actors[i], EffectID = 137107, DurationInTicks = (60 * 5) });
                    }
                }
            }
            else if (EffectID == 137107)
            {
                // blind flash contact
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                {
                    ActorID = Actor.DynamicID,
                    Field1 = 32,
                    Field2 = 137107,
                }, Actor);
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
                        ActorID = this.Actor.DynamicID,
                        Field1 = 32,
                        Field2 = 199677,
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
                        ActorID = this.ProxyActor.DynamicID,
                        Field1 = 32,
                        Field2 = 99504
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
                map[GameAttribute.Buff_Active, 92225] = false;
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
                    ActorID = this.Actor.DynamicID,
                    Field1 = 32,
                    Field2 = 143230,
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
                        ActorID = this.Actor.DynamicID,
                        Field1 = 32,
                        Field2 = 199677,
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
                        ActorID = this.Actor.DynamicID,
                        Field1 = 32,
                        Field2 = 199677,
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
                        ActorID = this.Actor.DynamicID,
                        Field1 = 32,
                        Field2 = 199677,
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
                        ActorID = this.ProxyActor.DynamicID,
                        Field1 = 32,
                        Field2 = 98556
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
                    ActorID = this.Actor.DynamicID,
                    Field1 = 32,
                    Field2 = 113720
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

    public class Effect : Actor
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();

        public const int GenericPowerProxyID = 4176;

        public override ActorType ActorType { get { return ActorType.Effect; } }

        protected int? idleAnimationSNO;

        protected int ticksBetweenActions = 6;

        protected float walkDistance = 0.1f; // disatnce per 1 Tick

        public Effect(World world, int actorSNO, Vector3D position)
            : base(world, world.NewActorID)
        {
            this.ActorSNO = actorSNO;
            // FIXME: This is hardcoded crap
            this.Field2 = 0x8;
            this.Field3 = 0x0;
            this.Scale = 1.35f;
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
            this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
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

        public void Die(Mooege.Core.GS.Player.Player player)
        {
            this.World.BroadcastIfRevealed(new ANNDataMessage(Opcodes.ANNDataMessage24)
            {
                ActorID = this.DynamicID,
            }, this);
            this.Destroy();
        }

        public static bool CheckRange(Actor actor, Actor target, float range)
        {
            if (target == null) return false;
            return (Math.Sqrt(Math.Pow(actor.Position.X - target.Position.X, 2) + Math.Pow(actor.Position.Y - target.Position.Y, 2)) < range);
        }

        public static float GetDistance(Vector3D startPosition, Vector3D targetPosition)
        {
            if (targetPosition == null) return 0;
            return (float)Math.Sqrt(Math.Pow(startPosition.X - targetPosition.X, 2) + Math.Pow(startPosition.Y - targetPosition.Y, 2));
        }

        protected float[] GetDistanceDelta(float facingAngle)
        {
            float[] res = new float[2];
            res[0] = (walkDistance * 6) * (float)Math.Cos(facingAngle); // sending this in 100ms (6 Ticks) -> walkDistance * 6 Ticks
            res[1] = (walkDistance * 6) * (float)Math.Sin(facingAngle);
            return res;
        }

        public static float GetFacingAngle(Vector3D lookerPosition, Vector3D targetPosition)
        {
            return (float)Math.Atan2((targetPosition.Y - lookerPosition.Y), (targetPosition.X - lookerPosition.X));
        }
    }

    public class MysticAllyEffect : Effect
    {
        public override ActorType ActorType { get { return ActorType.NPC; } }

        private int walkAnimationSNO;
        private int attackAnimationSNO;

        public MysticAllyEffect(World world, int actorSNO, Vector3D position, Actor owner) :
            base(world, actorSNO, position)
        {
            this.Attributes[GameAttribute.Summoned_By_ACDID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Summoned_By_SNO] = Skills.Skills.Monk.SpiritSpenders.MysticAlly;
            this.Attributes[GameAttribute.Summoner_ID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Follow_Target_ACDID] = unchecked((int)owner.DynamicID);
            this.Attributes[GameAttribute.Last_ACD_Attacked] = 0;
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
            this.attackAnimationSNO = (this.ActorSNO == 169904) ? 69776 : 69776; // TODO: find tags
            this.Scale = 1.22f;
            this.ticksBetweenActions = 6 * 9; // 900 ms
            this.walkDistance = 0.23f;

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
                target = GetTarget(50f);
                if (target != null)
                {
                    this.Attributes[GameAttribute.Last_ACD_Attacked] = unchecked((int)target.DynamicID);
                }
            }
            if (!CheckRange(this, target != null ? target : owner, 8f))
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

        private Actor GetTarget(float range)
        {
            Actor result = null;
            List<Actor> actors = this.World.GetActorsInRange(this.Position, range);
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
                        distance = GetDistance(this.Position, actors[i].Position);
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

        private void Attack(Actor target)
        {
            if (target == null)
            {
                return;
            }

            this.World.BroadcastIfRevealed(new PlayAnimationMessage()
            {
                ActorID = this.DynamicID,
                Field1 = 0x3,
                Field2 = 0,
                tAnim = new PlayAnimationMessageSpec[1]
                {
                    new PlayAnimationMessageSpec()
                    {
                        Field0 = 0x26,
                        Field1 = attackAnimationSNO,
                        Field2 = 0x0,
                        Field3 = 1.235224f
                    }
                }
            }, this);
        }

        private bool _cLientKnowsWalkAnimation = false;
        private void MoveTo(Actor target)
        {
            if (target == null)
            {
                return;
            }

            float angle = GetFacingAngle(this.Position, target.Position);
            float[] delta = GetDistanceDelta(angle);
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
                    Field4 = walkDistance, // distance in Tick == speed
                    Field5 = 0,
                    Id = 0x006E,
                    Field6 = this.walkAnimationSNO,

                }, this);
                _cLientKnowsWalkAnimation = true;
            }
        }
    }

    public class ClientEffect
    {
        public static void CreateVisualSkill(Actor actor, TargetMessage message)
        {
            // TODO: refactor to switches toonclass/power (or subclases)
            Vector3D targetPosition = message.Field2.Position;
            Actor target = null;
            if (message.TargetID != 0xFFFFFFFF)
            {
                target = actor.World.GetActor(message.TargetID);
                if (target != null)
                {
                    targetPosition = target.Position;
                }
            }
            if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.FistsOfThunder)
            { // + effect on target
                int effectID = 143570; // cast
                int effectID2 = 96176; // projectile
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        startingTick += actor.World.Game.Tick + (6 * 1);
                        break;
                    case 1:
                        effectID =  143561;//143569; // cast
                        effectID2 = 96176;//96177;
                        break;
                    case 2:
                        effectID = 143566; // cast
                        effectID2 = 96178;
                        startingTick += actor.World.Game.Tick + (6 * 3);
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID2, StartingTick = startingTick });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.ExplodingPalm)
            { 
                int effectID = 142471;
                int masterEffectID = 143841;
                switch (message.Field5)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        effectID = 142473;
                        masterEffectID = 143473;
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = masterEffectID, });
                if ((target != null) & (message.Field5 == 2))
                {
                    actor.World.AddEffect(new ActorEffect { Actor = target, EffectID = 92225, DurationInTicks = (60 * 3) });
                }
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.DeadlyReach)
            {
                int masterEffectID = 140870;
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        startingTick += actor.World.Game.Tick + (6 * 1);
                        break;
                    case 1:
                        masterEffectID = 140871;
                        break;
                    case 2:
                        masterEffectID = 140872;
                        startingTick += actor.World.Game.Tick + (6 * 3);
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = masterEffectID, StartingTick = startingTick });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.CripplingWave)
            {
                int effectID = 2603;
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
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.SweepingWind)
            {
                int effectID = 196981;
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
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.WayOfTheHundredFists)
            {
                int effectID = 2612;//((actor as Player.Player).Properties.Gender == 0) ? 2612 : ???;
                int masterEffectID = 137345;//((actor as Player.Player).Properties.Gender == 0) ? 137345 ; ???;
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        startingTick = actor.World.Game.Tick + (6 * 3);
                        break;
                    case 1:
                        effectID = 98412;//((actor as Player.Player).Properties.Gender == 0) ? 98412 : ???;
                        masterEffectID = 137346;//((actor as Player.Player).Properties.Gender == 0) ? 137346 : ???;
                        break;
                    case 2:
                        startingTick = actor.World.Game.Tick + (6 * 2);
                        masterEffectID = 137347;//((actor as Player.Player).Properties.Gender == 0) ? 137347 : ???;
                        effectID = 98416;//((actor as Player.Player).Properties.Gender == 0) ? 98416 : ???;
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = masterEffectID, StartingTick = startingTick });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.DashingStrike)
            {
                int duration = 6;
                actor.World.AddEffect(new ActorEffect
                {
                    Actor = actor,
                    EffectID = 192085,
                    DurationInTicks = duration,
                });
                actor.World.AddEffect(new ActorEffect
                {
                    Actor = actor,
                    EffectID = 111132,
                    DurationInTicks = duration,
                    Position = targetPosition,
                    Angle = Effect.GetFacingAngle(actor.Position, targetPosition)
                });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.LashingTailKick)
            {
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = 143782 });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.WaveOfLight)
            {
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = 145011, });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = 144079, });
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.SevenSidedStrike)
            {
                // TODO: find targets for effects, now targetting self
                int effectID = 98826;
                int startingTick = actor.World.Game.Tick + 12;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
                effectID = 98831;
                startingTick += 12;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
                effectID = 98842;
                startingTick += 12;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
            }
        }

        public static void CreateVisualSkill(Player.Player player, SecondaryAnimationPowerMessage message) {
            // TODO: refactor to switches  toonclass/power (or subclases)
            if (message.PowerSNO == Skills.Skills.Monk.Mantras.MantraOfEvasion)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 143964, });
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 99694, Attached = true }); // 60 ticks/s * 120 = 120s
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.Mantras.MantraOfHealing)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 99948, });
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 140190, Attached = true }); // 60 ticks/s * 120 = 120s
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.Mantras.MantraOfConviction)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 95955, });
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 146990, Attached = true }); // 60 ticks/s * 120 = 120s
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.Mantras.MantraOfRetribution)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 142974, });
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 120), EffectID = 142987, Attached = true }); // 60 ticks/s * 120 = 120s
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.LethalDecoy)
            {
                int effectID = (player.Properties.Gender == 0) ? 99241 : 208435;
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 5), EffectID = effectID, NeedsActor = true }); // 60 ticks/s * 5 = 5s
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.BreathOfHeaven)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 101174, });
                return;
                /*
                 * move to effect
                Actor.Attributes[GameAttribute.Resource_Cur, player.ResourceID] -= 75f;
                GameAttributeMap atm = new GameAttributeMap();
                atm[GameAttribute.Resource_Cur, player.ResourceID] = Actor.Attributes[GameAttribute.Resource_Cur, player.ResourceID];
                atm.SendMessage(client, player.DynamicID);
                 * */
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.InnerSanctuary)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = (60 * 8), EffectID = 98557, NeedsActor = true });
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.Serenity)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 123156, });
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 142890, });
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 143230, DurationInTicks = (60 * 3) });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.MysticAlly)
            {
                int effectID = (player.Properties.Gender == 0) ? 169904 : 123885;
                player.World.AddEffect(new ActorEffect { Actor = player, DurationInTicks = -1, EffectID = effectID, NeedsActor = true }); // until is destroyed
                return;
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.BlindingFlash)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 2588});
            }
            else if (message.PowerSNO == Skills.Skills.Wizard.Utility.Archon)
            {
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 162301, DurationInTicks = (60 * 15) });
            }
        }
    }
}
