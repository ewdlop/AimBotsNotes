using ClickableTransparentOverlay;
using ImGuiNET;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

//https://www.youtube.com/watch?v=hO1WmAO0rDA
//https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
const int HOTKEY = 0x06; // X2 mouse button

Swed swed = new Swed("cs2");


IntPtr cilent = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
renderer.Start().Wait();

List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

while(true)
{
    entities.Clear();

    IntPtr entityList = swed.ReadPointer(cilent, client_dll.dwEntityList);

    //first entry

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    localPlayer.PawnAddress = swed.ReadPointer(cilent, client_dll.dwLocalPlayerPawn);
    localPlayer.Team = swed.ReadInt(localPlayer.PawnAddress, 0x3BF); //C_BaseEntity.m_iTeamNum 
    localPlayer.Origin = swed.ReadVec(localPlayer.PawnAddress, 0x1244); //C_BasePlayerPawn.m_vOldOrigin
    localPlayer.View = swed.ReadVec(localPlayer.PawnAddress, 0xC48);//C_BaseModelEntity.m_vecViewOffset

    for(int i = 0; i < 64; i++) // 64 Controllers
    {
        if(listEntry == IntPtr.Zero)
        {
            continue;
        }

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); // 0x78 = 120
        
        if(currentController == IntPtr.Zero)
        {
            continue;
        }

        int pawnHandle = swed.ReadInt(currentController, 0x7EC); // CCSPlayerController.m_hPlayerPawn = 0x7EC

        if(pawnHandle == 0)
        {
            continue;
        }

        //apply bitmask 0x7FFF and shift bits by 9
        int pawnIndex = (pawnHandle & 0x7FFF) >> 9;
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * pawnIndex + 0x10);

        //get the pawn address from the entity list with the pawn index and the list entry
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if(currentPawn == IntPtr.Zero)
        {
            continue;
        }

        int health = swed.ReadInt(currentPawn, 0x32C); // C_BaseEntity.m_iHealth
        int team = swed.ReadInt(currentPawn, 0x3BF); // C_BaseEntity.m_iTeamNum
        uint lifeState = swed.ReadUInt(currentPawn, 0x330); // C_BaseEntity.m_lifeState

        if(lifeState != 256)
        {
            continue;
        }
        if( team == localPlayer.Team && !renderer.aimBotOnTeam)
        {
            continue;
        }

        Entity entity = new Entity();

        entity.PawnAddress = currentPawn;
        entity.ControllerAddress = currentController;
        entity.Health = health;
        entity.Team = team;
        entity.LifeState = lifeState;
        entity.Origin = swed.ReadVec(currentPawn, 0x1244); //C_BasePlayerPawn.m_vOldOrigin
        entity.View = swed.ReadVec(currentPawn, 0xC48);//C_BaseModelEntity.m_vecViewOffset
        entity.Distance = Vector3.Distance(entity.Origin, localPlayer.Origin);

        entities.Add(entity);
        Console.ForegroundColor = ConsoleColor.Green;

        if(team != localPlayer.Team)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"Entity: {i} Health: {health} Team: {team} LifeState: {lifeState} Distance: {(int)entity.Distance/100}m");
        Console.ResetColor();

        entities = entities.OrderBy(x => x.Distance).ToList();

        if(entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
        {
            Vector3 playerView = Vector3.Add(localPlayer.Origin, localPlayer.View);
            Vector3 entityView = Vector3.Add(entities[0].Origin, entities[0].View);

            Vector2 vector2 = Math2.CalculateAngles(playerView, entityView);
            Vector3 vector3 = new Vector3(vector2.X, vector2.Y, 0.0f);

            swed.WriteVec(cilent, 0x1880DC0, vector3); //client_dll.dwViewAngles
        }

        Thread.Sleep(20);

    }
}
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);
public class Math2
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

public class Renderer : Overlay
{
    public bool aimbot;
    public bool aimBotOnTeam;
    protected override void Render()
    {
        ImGui.BeginMenu("menu");
        ImGui.Checkbox("aimbot", ref aimbot);
        ImGui.Checkbox("aimbot on team", ref aimBotOnTeam);
    }
}

public class Entity
{
    public IntPtr PawnAddress { get; set; }
    public IntPtr ControllerAddress { get; set; }
    public Vector3 Origin { get; set; }
    public Vector3 View { get; set; }
    public int Health { get; set; }
    public int Team { get; set; }
    public uint LifeState { get; set; }
    public float Distance { get; set; }
}