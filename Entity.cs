using System.Numerics;

namespace AimBots;

public class Entity
{
    public nint PawnAddress { get; set; }
    public nint ControllerAddress { get; set; }
    public Vector3 Origin { get; set; }
    public Vector3 View { get; set; }
    public Vector3 Head { get; set; }
    public int Health { get; set; }
    public int Team { get; set; }
    public uint LifeState { get; set; }
    public float Distance { get; set; }
}