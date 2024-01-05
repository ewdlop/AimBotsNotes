using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2HeadAimbotHeadTutorial
{
    public static class Calculate
    {
        public static Vector2 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float yaw;
            float pitch;

            // calculate the yaw

            float deltaX = destination.X - from.X;
            float deltaY = destination.Y - from.Y;
            yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

            // calculate the pitch

            float deltaZ = destination.Z - from.Z;
            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            // return calculated angles
            return new Vector2(yaw, pitch);
        }

    }
}
