using CS2HeadAimbotHeadTutorial;
using CS2HeadAimbotTutorial;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

// init swed library
Swed swed = new Swed("cs2");

// get client.dll
IntPtr client = swed.GetModuleBase("client.dll");

// time for some offsets 


int m_flDetectedByEnemySensorTime = 0x13E4;

Renderer renderer = new Renderer();
renderer.Start().Wait();


// program variables
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

const int HOTKEY = 0x06;

while (true) // aimbot loop
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
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

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


        if (currentPawn == localPlayer.pawnAddress)
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

        if (team == localPlayer.team && renderer.aimOnTeam == false)
            continue;

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.health = health;
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
        entity.head = swed.ReadVec(boneMatrix, 6 * 32); // 6 = bone id, 32 = step between bone coordinates.

        entities.Add(entity);

        Console.ForegroundColor = ConsoleColor.Green; // Set text color to green

        if (team != localPlayer.team)
        {
            Console.ForegroundColor = ConsoleColor.Red; // Set text color to red
        }
        Console.WriteLine($"{entity.health}hp, head coordinate {entity.head}");

        // Reset text color to the default after printing
        Console.ResetColor();
    }

    // Aimbot stuff 

    entities = entities.OrderBy(o => o.distance).ToList();


    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
    {
        // get view values
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);


        // get angles 
        Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);

        // set angles 
        swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);

    }

    Thread.Sleep(14); // change depending on your need.
}

// loop through entity list

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);