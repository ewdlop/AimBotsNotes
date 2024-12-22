using AimBots;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

//https://www.youtube.com/watch?v=hO1WmAO0rDA
//https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes

const int HOTKEY = 0x06; // X2 mouse button
const int m_flDetectedByEnemySensorTime = 0x13E4; // time for some offsets

const string processName = "cs2";

Swed swed = new Swed(processName);


IntPtr client = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
renderer.Start().Wait();

List<Entity> entities = [];
Entity localPlayer = new();

while(true)
{
    loop();
}

void loop()
{
    entities.Clear(); // clean list of old ents 

    // get entity list 
    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    // first entry into entity list
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10); // we don't have an ID yet.

    if (renderer.wallHack)
    {
        for (int i = 0; i < 64; i++) // 64 controllers 
        {
            if (listEntry == IntPtr.Zero)
                continue;

            // get current controller 
            IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

            if (currentController == IntPtr.Zero)
                continue;

            // get current pawn
            int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
            if (pawnHandle == 0)
                continue;

            // second entry 
            IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

            IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

            // now that we have the pawn we can force the glow 

            swed.WriteFloat(currentPawn, m_flDetectedByEnemySensorTime, 86400); // for some odd reason this is the value for glow.

            // write pawn so that we can see that they're there.
            Console.WriteLine($"{i}: {currentPawn}");

        }
    }

    // get our player 
    localPlayer.PawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.Team = swed.ReadInt(localPlayer.PawnAddress, Offsets.m_iTeamNum);
    localPlayer.Origin = swed.ReadVec(localPlayer.PawnAddress, Offsets.m_vOldOrigin);
    localPlayer.View = swed.ReadVec(localPlayer.PawnAddress, Offsets.m_vecViewOffset);

    Console.Clear();
    entities.Clear();

    for (int i = 0; i < 64; i++) // 64 controllers
    {

        if (listEntry == IntPtr.Zero) // skip if entry invalid 
            continue;

        // get current controller 
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); // you might want to add a bitmask if you use higher a higher i.

        if (currentController == IntPtr.Zero) // skip if controller invalid 
            continue;

        // get pawn handle 
        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);

        if (pawnHandle == 0) // you get it by this point, the same.
            continue;

        // second entry, now we find the pawn! 
        // apply bitmask 0x7FFF and shift bits by 9.
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        // read current pawn, apply bitmask to stay inside range 
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));


        if (currentPawn == localPlayer.PawnAddress)
            continue;

        // get scene node
        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);

        // get bone array / bone matrix 
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80); // 0x80 is the dwBoneMatrix offset

        // get pawn attributes 
        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        if (lifeState != 256)
            continue;

        if (team == localPlayer.Team && renderer.aimBotOnTeam == false)
            continue;

        Entity entity = new Entity()
        {
            PawnAddress = currentPawn,
            ControllerAddress = currentController,
            Health = health,
            LifeState = lifeState,
            Origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin),
            View = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset),
            Distance = Vector3.Distance(swed.ReadVec(currentPawn, Offsets.m_vOldOrigin), localPlayer.Origin),
            Head = swed.ReadVec(boneMatrix, 6 * 32) // 6 = bone id, 32 = step between bone coordinates.
        };
        
        entities.Add(entity);

        Console.ForegroundColor = ConsoleColor.Green; // Set text color to green

        if (team != localPlayer.Team)
        {
            Console.ForegroundColor = ConsoleColor.Red; // Set text color to red
        }
        Console.WriteLine($"{entity.Health}hp, head coordinate {entity.Head}");

        // Reset text color to the default after printing
        Console.ResetColor();
    }

    // Aimbot stuff 

    entities = [.. entities.OrderBy(o => o.Distance)];


    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
    {
        // get view values
        Vector3 playerView = Vector3.Add(localPlayer.Origin, localPlayer.View);
        Vector3 entityView = Vector3.Add(entities[0].Origin, entities[0].View);


        // get angles 
        Vector2 newAngles = Math2.CalculateAngles(playerView, entities[0].Head);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);

        // set angles 
        swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);

    }

    Thread.Sleep(14); // change depending on your need.
}
void loop1()
{
    while (true)
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

            if (team != localPlayer.Team)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"Entity: {i} Health: {health} Team: {team} LifeState: {lifeState} Distance: {(int)entity.Distance / 100}m");
            Console.ResetColor();

            entities = [.. entities.OrderBy(x => x.Distance)];

            if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
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
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);