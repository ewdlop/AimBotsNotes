//https://www.youtube.com/watch?v=y3byXt_ZHFc

using Swed32;

Swed swed = new Swed("hl2");

nint moduleBase = swed.GetModuleBase(".exe");

//nint ptr = swed.ReadPointer(moduleBase, 0xB81E288);
//byte[]? ammo = swed.ReadBytes(ptr, 0x144, 4);

//Console.WriteLine(BitConverter.ToInt32(ammo, 0));

//swed.WriteBytes(ptr, 0x144, BitConverter.GetBytes(99999));
swed.WriteBytes(0xB81E288, BitConverter.GetBytes(99999));