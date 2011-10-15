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
        public int EffectID { get; set; }
        public Actor Actor { get; set; } // initial target for effect + attachment
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
            if (this.Actor.World.GetActor(this.Actor.DynamicID) == null)
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
                        this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                        {
                            Id = 0x7a,
                            ActorID = this.Actor.DynamicID,
                            Field1 = 32,
                            Field2 = this.EffectID,
                        }, this.Actor);
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
                        _started = true;
                    }
                }
            }
            else
            {
                // check if effect should end
                if ((this.NeedsActor) && (this.ProxyActor.World.GetActor(this.ProxyActor.DynamicID) == null))
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
            // temporary HACK: TODO: move to subclasses
            if (EffectID == 99694)
            {
                int PowerSNO = Skills.Skills.Monk.Mantras.MantraOfEvasion;
                GameAttributeMap map = new GameAttributeMap();
                map[GameAttribute.Dodge_Chance_Bonus] += 0.3f;
                map.SendMessage((Actor as Player.Player).InGameClient, Actor.DynamicID);
                // icon + cooldown
                GameAttributeMap atm = new GameAttributeMap();
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
                this.Actor.World.BroadcastIfRevealed(new PlayEffectMessage()
                    {
                        ActorID = this.ProxyActor.DynamicID,
                        Field1 = 44,
                    }, this.ProxyActor);
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
            Vector3D targetPosition = message.Field2.Position;
            if (message.TargetID != 0xFFFFFFFF)
            {
                Actor npc = actor.World.GetActor(message.TargetID);
                if (npc != null)
                {
                    targetPosition = npc.Position;
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
                        break;
                    case 1:
                        effectID = 143569; // cast
                        effectID2 = 96177;
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
                switch (message.Field5)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        effectID = 142473;// WRONG - must be on target
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, });
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritGenerator.DeadlyReach)
            {
                int effectID = 71921;
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        startingTick += actor.World.Game.Tick + (6 * 1);
                        break;
                    case 1:
                        effectID = 72134;
                        break;
                    case 2:
                        effectID = 72331;
                        startingTick += actor.World.Game.Tick + (6 * 3);
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
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
                int startingTick = 0;
                switch (message.Field5)
                {
                    case 0:
                        break;
                    case 1:
                        effectID = 98412;
                        break;
                    case 2:
                        startingTick = actor.World.Game.Tick + (6 * 1);
                        effectID = 98416;
                        break;
                }
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.DashingStrike)
            {
                //                id = actor.World.SpawnTempObject(actor, 192095, actor.Position, actor.RotationAmount, actor.DynamicID);
                //                System.Threading.Thread.Sleep(500);
                //                actor.World.BroadcastIfRevealed(new ANNDataMessage(Opcodes.ANNDataMessage6) { ActorID = id, }, actor);
            }
            else if (message.PowerSNO == Skills.Skills.Monk.SpiritSpenders.SevenSidedStrike)
            {
                /* // find targets for effects
                int effectID = 98826;
                int startingTick = actor.World.Game.Tick + 20;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
                effectID = 98831;
                startingTick += 60;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
                effectID = 98842;
                startingTick += 80;
                actor.World.AddEffect(new ActorEffect { Actor = actor, EffectID = effectID, StartingTick = startingTick});
                 */
            }
        }
    }
}
