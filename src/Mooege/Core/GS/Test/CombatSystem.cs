using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Actors;
using Mooege.Net.GS.Message.Fields;
using Mooege.Core.GS.Map;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.FXEffect;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.Actor;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Common;
using Mooege.Core.GS.Common.Types.Math;

namespace Mooege.Core.GS.Test
{
    public class CombatSystem
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        // get nearest target of targetType
        public static Actor GetNearestTarget(World world, Actor attacker, Vector3D centerPosition, float range, ActorType targetType = ActorType.Monster)
        {
            Actor result = null;
            List<Actor> actors = world.GetActorsInRange(centerPosition, range);
            if (actors.Count > 1)
            {
                float distanceNearest = range; // max. range
                float distance = 0f;
                foreach (var target in actors.Where(target => ((target.ActorType == targetType) && (target != attacker))))
                {
                    if ((target.World == null) || (world.GetActor(target.DynamicID) == null))
                    {
                        // leaving world
                        continue;
                    }
                    distance = ActorUtils.GetDistance(centerPosition, target.Position);
                    if ((result == null) || (distance < distanceNearest))
                    {
                        result = target;
                        distanceNearest = distance;
                    }
                }
            }
            return result;
        }

        // shhots projectile at 2D angle
        public static void ShootAtAngle(World world, Actor projectile, float angle, float speed)
        {
            float[] delta = ActorUtils.GetDistanceDelta(speed, angle);
            world.BroadcastInclusive(new ACDTranslateFixedMessage()
            {
                Id = 113, // needed
                ActorId = unchecked((int)projectile.DynamicID),
                Velocity = new Vector3D { X = delta[0], Y = delta[1], Z = 0 },
                Field2 = 1,
                AnimationTag = 1,//walkAnimationSNO
                Field4 = 1,
            }, projectile);
        }

        public static GameAttributeMap[] ResolveCombat(Actor attacker, Actor defender, bool damageTypeOverriden = false, int damageTypeOverride = 0)
        {
            GameAttributeMap[] maps = new GameAttributeMap[2];
            maps[0] = new GameAttributeMap();
            maps[1] = new GameAttributeMap();
            if (defender.Attributes[GameAttribute.No_Damage])
            {
                return maps;
            }
            bool hit = AttributeMath.IsHit(attacker, defender);
            bool critical = false;
            float damage = 0f;
            if (!hit)
            {
                return maps;
            }
            if (AttributeMath.IsDodge(defender))
            {
                attacker.World.AddEffect(new HitSpecialEffect
                {
                    Actor = attacker,
                    Target = defender,
                    Type = (defender is Player.Player) ? FloatingNumberMessage.FloatType.Dodge : FloatingNumberMessage.FloatType.Dodged
                });
            }
            if (AttributeMath.IsImmune(defender, damageTypeOverriden, damageTypeOverride))
            {
                attacker.World.AddEffect(new HitSpecialEffect
                {
                    Actor = attacker,
                    Target = defender,
                    Type = FloatingNumberMessage.FloatType.Immune
                });
                return maps;
            }
            bool blocked = AttributeMath.IsBlock(defender);
            if (blocked)
            {
                attacker.World.AddEffect(new HitSpecialEffect
                {
                    Actor = attacker,
                    Target = defender,
                    Type = FloatingNumberMessage.FloatType.Block
                });
                return maps;
            }
            damage = defender.Attributes[GameAttribute.Hitpoints_Cur];
            critical = AttributeMath.IsCriticalHit(attacker, defender);
            maps = AttributeMath.ComputeCombat(attacker, defender, critical, blocked, damageTypeOverriden, damageTypeOverride);
            damage -= defender.Attributes[GameAttribute.Hitpoints_Cur];
//            Logger.Info("damage: " + damage);
            if (damage == 0f)
            {
                attacker.World.AddEffect(new HitSpecialEffect
                {
                    Actor = attacker,
                    Target = defender,
                    Type = FloatingNumberMessage.FloatType.Absorbed
                });
            }
            else
            {
                attacker.World.AddEffect(new HitEffect
                {
                    Actor = attacker,
                    Target = defender,
                    Damage = damage,
                    Critical = critical,
                    Type = damageTypeOverriden ? damageTypeOverride : 0 // TODO: find damage type from combat
                });
                if (defender.Attributes[GameAttribute.Hitpoints_Cur] <= 0f)
                {
                    attacker.World.AddEffect(new DieEffect
                    {
                        Actor = defender,
                        Killer = attacker,
                        Type = damageTypeOverriden ? damageTypeOverride : 0, // TODO: find damage type from combat
                    });
                }
            }
            attacker.UpdateMap.CombineMap(maps[0]);
            defender.UpdateMap.CombineMap(maps[1]);
            return maps;
        }

        public static void MoveToBasic(Actor mover, Actor target, float speed, int? animationSNO)
        {
            if (target == null)
            {
                return;
            }

            float angle = ActorUtils.GetFacingAngle(mover.Position, target.Position);
            float[] delta = ActorUtils.GetDistanceDelta(speed, angle);
            mover.Position.X += delta[0];
            mover.Position.Y += delta[1];
            angle = ActorUtils.GetFacingAngle(mover.Position, target.Position);

            if (!animationSNO.HasValue)
            {
                mover.World.BroadcastInclusive(new NotifyActorMovementMessage()
                {
                    ActorId = (int)mover.DynamicID,
                    Position = mover.Position,
                    Angle = angle,
                    Id = 0x006E,
                }, mover);
            }
            else
            {
                mover.World.BroadcastInclusive(new NotifyActorMovementMessage()
                {
                    ActorId = (int)mover.DynamicID,
                    Position = mover.Position,
                    Angle = angle,
                    Field3 = false,
                    Speed = speed, // distance in Tick == speed
                    Field5 = 0,
                    Id = 0x006E,
                    AnimationTag = animationSNO.Value,

                }, mover);
            }
        }

        public static void MoveToBasic(Actor mover, Vector3D targetPosition, float speed, int? animationSNO)
        {
            if (targetPosition == null)
            {
                return;
            }

            float angle = ActorUtils.GetFacingAngle(mover.Position, targetPosition);
            float[] delta = ActorUtils.GetDistanceDelta(speed, angle);
            mover.Position.X += delta[0];
            mover.Position.Y += delta[1];
            angle = ActorUtils.GetFacingAngle(mover.Position, targetPosition);

            if (!animationSNO.HasValue)
            {
                mover.World.BroadcastInclusive(new NotifyActorMovementMessage()
                {
                    ActorId = (int)mover.DynamicID,
                    Position = mover.Position,
                    Angle = angle,
                    Id = 0x006E,
                }, mover);
            }
            else
            {
                mover.World.BroadcastInclusive(new NotifyActorMovementMessage()
                {
                    ActorId = (int)mover.DynamicID,
                    Position = mover.Position,
                    Angle = angle,
                    Field3 = false,
                    Speed = speed, // distance in Tick == speed
                    Field5 = 0,
                    Id = 0x006E,
                    AnimationTag = animationSNO.Value,

                }, mover);
            }
        }

        public static void Attack(Actor attacker, Actor target, int? animationSNO)
        {
            if (target == null)
            {
                return;
            }
            attacker.World.BroadcastInclusive(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorId = attacker.DynamicID,
                Angle = ActorUtils.GetFacingAngle(attacker.Position, target.Position),
                Immediately = false
            }, attacker);
            if (animationSNO.HasValue)
            {
                attacker.World.BroadcastInclusive(new PlayAnimationMessage()
                {
                    ActorID = attacker.DynamicID,
                    Field1 = 0x3,
                    Field2 = 0,
                    tAnim = new PlayAnimationMessageSpec[1]
                {
                    new PlayAnimationMessageSpec()
                    {
                        Field0 = 0x2,
                        Field1 = animationSNO.Value,
                        Field2 = 0x0,
                        Field3 = 1f
                    }
                }
                }, attacker);
            }
        }
    }
}
