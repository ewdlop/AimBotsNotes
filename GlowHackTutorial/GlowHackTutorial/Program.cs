using Swed64;

// init swed 
Swed swed = new Swed("cs2");

// get client.dll
IntPtr client = swed.GetModuleBase("client.dll");

// time for some offsets 

// offsets.cs, these update regularly
int dwEntityList = 0x17C1950;

// client.dll.cs, these doesn't update that often
int m_hPlayerPawn = 0x7EC;
int m_flDetectedByEnemySensorTime = 0x13E4;

// glow loop 

while (true)
{
    // get entity list 
    IntPtr entityList = swed.ReadPointer(client,dwEntityList);

    // first entry 
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    for (int i = 0; i < 64;i++) // 64 controllers 
    {
        if (listEntry == IntPtr.Zero)
            continue;

        // get current controller 
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero)
            continue;

        // get current pawn
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
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
    Thread.Sleep(50);
    Console.Clear();
}