using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Net.GS.Message.Fields;

namespace Mooege.Core.GS.Actors
{
    public class ActorUtils
    {
        //  Checks if in 2D range
        public static bool CheckRange(Actor actor, Actor target, float range)
        {
            if ((actor == null) || (target == null)) return false;
            return (Math.Sqrt(Math.Pow(actor.Position.X - target.Position.X, 2) + Math.Pow(actor.Position.Y - target.Position.Y, 2)) < range);
        }

        //  returns 2D distance
        public static float GetDistance(Vector3D startPosition, Vector3D targetPosition)
        {
            if ((startPosition == null) || (targetPosition == null)) return 0;
            return (float)Math.Sqrt(Math.Pow(startPosition.X - targetPosition.X, 2) + Math.Pow(startPosition.Y - targetPosition.Y, 2));
        }

        //  returns 2D position increments
        public static float[] GetDistanceDelta(float speed, float facingAngle, int ticks = 6)
        {
            float[] res = new float[2];
            res[0] = (speed * ticks) * (float)Math.Cos(facingAngle);
            res[1] = (speed * ticks) * (float)Math.Sin(facingAngle);
            // omitting Z axis
            return res;
        }

        //  returns 2D angle to face targetPosition
        public static float GetFacingAngle(Vector3D lookerPosition, Vector3D targetPosition)
        {
            if ((lookerPosition == null) || (targetPosition == null))
            {
                return 0f;
            }
            return (float)Math.Atan2((targetPosition.Y - lookerPosition.Y), (targetPosition.X - lookerPosition.X));
        }
    }
}
