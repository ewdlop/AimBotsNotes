using AimBots;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

//https://www.youtube.com/watch?v=hO1WmAO0rDA
//https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes

const int HOTKEY = 0x06; // X2 mouse button

const string processName = "cs2";

Swed swed = new Swed(processName);


IntPtr client = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
renderer.Start().Wait();

List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

while(true)
{
    entities.Clear();

    IntPtr entityList = swed.ReadPointer(client, client_dll.dwEntityList);

    //first entry

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    localPlayer.PawnAddress = swed.ReadPointer(client, client_dll.dwLocalPlayerPawn);
    localPlayer.Team = swed.ReadInt(localPlayer.PawnAddress, 0x3BF); //C_BaseEntity.m_iTeamNum 
    localPlayer.Origin = swed.ReadVec(localPlayer.PawnAddress, 0x1244); //C_BasePlayerPawn.m_vOldOrigin
    localPlayer.View = swed.ReadVec(localPlayer.PawnAddress, 0xC48);//C_BaseModelEntity.m_vecViewOffset

    for (int i = 0; i < 64; i++) // 64 Controllers
    {
        if (listEntry == IntPtr.Zero)
        {
            continue;
        }

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); // 0x78 = 120

        if (currentController == IntPtr.Zero)
        {
            continue;
        }

        int pawnHandle = swed.ReadInt(currentController, 0x7EC); // CCSPlayerController.m_hPlayerPawn = 0x7EC

        if (pawnHandle == 0)
        {
            continue;
        }

        //apply bitmask 0x7FFF and shift bits by 9
        int pawnIndex = (pawnHandle & 0x7FFF) >> 9;
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * pawnIndex + 0x10);

        //get the pawn address from the entity list with the pawn index and the list entry
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if (currentPawn == IntPtr.Zero)
        {
            continue;
        }

        int health = swed.ReadInt(currentPawn, 0x32C); // C_BaseEntity.m_iHealth
        int team = swed.ReadInt(currentPawn, 0x3BF); // C_BaseEntity.m_iTeamNum
        uint lifeState = swed.ReadUInt(currentPawn, 0x330); // C_BaseEntity.m_lifeState

        if (lifeState != 256)
        {
            continue;
        }
        if (team == localPlayer.Team && !renderer.aimBotOnTeam)
        {
            continue;
        }

        Entity entity = new Entity()
        {
            PawnAddress = currentPawn,
            ControllerAddress = currentController,
            Health = health,
            Team = team,
            LifeState = lifeState,
            Origin = swed.ReadVec(currentPawn, 0x1244), //C_BasePlayerPawn.m_vOldOrigin
            View = swed.ReadVec(currentPawn, 0xC48),//C_BaseModelEntity.m_vecViewOffset
            Distance = Vector3.Distance(swed.ReadVec(currentPawn, 0x1244), localPlayer.Origin)
        };

        entities.Add(entity);
        Console.ForegroundColor = ConsoleColor.Green;

        if(team != localPlayer.Team)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"Entity: {i} Health: {health} Team: {team} LifeState: {lifeState} Distance: {(int)entity.Distance/100}m");
        Console.ResetColor();

        entities = [.. entities.OrderBy(x => x.Distance)];

        if(entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
        {
            Vector3 playerView = Vector3.Add(localPlayer.Origin, localPlayer.View);
            Vector3 entityView = Vector3.Add(entities[0].Origin, entities[0].View);

            Vector2 vector2 = Math2.CalculateAngles(playerView, entityView);
            Vector3 vector3 = new Vector3(vector2.X, vector2.Y, 0.0f);

            swed.WriteVec(client, 0x1880DC0, vector3); //client_dll.dwViewAngles
        }

        Thread.Sleep(20);

    }
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);