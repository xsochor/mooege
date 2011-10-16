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

namespace Mooege.Core.GS.Effect
{
    public class ActorEffect
    {
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
                return new Effect(Actor.World, EffectID, Actor.Position);
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
            }
            else if (EffectID == 92225)
            {
                // doesn't work (exploding palm 2)
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Buff_Active, 92225] = false;
                map[GameAttribute.Bleeding] = false;
                map[GameAttribute.Bleeding] = false;
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
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0) {
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
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0) {
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
                if (map[GameAttribute.Buff_Icon_Count0, PowerSNO] == 0) {
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
            }
        }
    }

    public class Effect : Actor
    {
        public const int GenericPowerProxyID = 4176;

        public override ActorType ActorType { get { return ActorType.Effect; } }

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
            
            this.World.Enter(this); // Enter only once all fields have been initialized to prevent a run condition
        }

        public override bool Reveal(Mooege.Core.GS.Player.Player player)
        {
            base.Reveal(player);
            this.Attributes.SendMessage(player.InGameClient, this.DynamicID);

            player.InGameClient.SendMessage(new AffixMessage()
            {
                ActorID = this.DynamicID,
                Field1 = 0x1,
                aAffixGBIDs = new int[0]
            });
            player.InGameClient.SendMessage(new AffixMessage()
            {
                ActorID = this.DynamicID,
                Field1 = 0x2,
                aAffixGBIDs = new int[0]
            });
            player.InGameClient.SendMessage(new ACDCollFlagsMessage
            {
                ActorID = this.DynamicID,
                CollFlags = 0x1
            });

            player.InGameClient.SendMessage(new ACDGroupMessage
            {
                ActorID = this.DynamicID,
                Field1 = unchecked((int)0xb59b8de4),
                Field2 = unchecked((int)0xffffffff)
            });

            player.InGameClient.SendMessage(new ANNDataMessage(Opcodes.ANNDataMessage24)
            {
                ActorID = this.DynamicID
            });

            player.InGameClient.SendMessage(new SetIdleAnimationMessage
            {
                ActorID = this.DynamicID,
                AnimationSNO = 0x11150
            });

            player.InGameClient.SendMessage(new SNONameDataMessage
            {
                Name = new SNOName
                {
                    Group = 0x1,
                    Handle = this.ActorSNO
                }
            });
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
                        effectID = 72331; // cast
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
                int effectID = 2612;
                int masterEffectID = 137345;
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        startingTick = actor.World.Game.Tick + (6 * 3);
                        break;
                    case 1:
                        effectID = 98412;
                        masterEffectID = 137346;
                        break;
                    case 2:
                        startingTick = actor.World.Game.Tick + (6 * 2);
                        masterEffectID = 137347;
                        effectID = 98416;
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = masterEffectID, StartingTick = startingTick });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.DashingStrike)
            {
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = 111132, DurationInTicks = 24, StartingTick = actor.World.Game.Tick + (6 * 1) });
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = 192085, DurationInTicks = 30 });
                
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
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 2588, });
                player.World.AddEffect(new ActorEffect { Actor = player, EffectID = 137107, });
            }
        }
    }
}
