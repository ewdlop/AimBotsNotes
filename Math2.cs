using System.Numerics;

namespace AimBots;

public static class Math2
{
    public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
    {
        float yaw;
        float pitch;

        float deltaX = to.X - from.X;
        float deltaY = to.Y - from.Y;
        yaw = (float)Math.Atan2(deltaY, deltaX) * (180f / (float)Math.PI);

        float deltaZ = to.Z - from.Z;
        float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        pitch = -(float)Math.Atan2(deltaZ, distance) * (180f / (float)Math.PI);

        return new Vector2(yaw, pitch);
    }
}