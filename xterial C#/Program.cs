using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;



class Program
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    const int F1 = 0x70;

    static void Main()
    {
        Memory memory = new Memory("cs2");
        if (!memory.IsValid())
        {
            Console.WriteLine("CS2 process not found.");
            return;
        }

        IntPtr client = memory.GetModuleBase("client.dll");
        Console.Title = "xterial C#";
        Console.WriteLine("            $$\\                         $$\\           $$\\                 $$\\                         \r\n            $$ |                        \\__|          $$ |                $$ |                        \r\n$$\\   $$\\ $$$$$$\\    $$$$$$\\   $$$$$$\\  $$\\  $$$$$$\\  $$ |       $$$$$$\\  $$ | $$$$$$\\  $$\\  $$\\  $$\\ \r\n\\$$\\ $$  |\\_$$  _|  $$  __$$\\ $$  __$$\\ $$ | \\____$$\\ $$ |      $$  __$$\\ $$ |$$  __$$\\ $$ | $$ | $$ |\r\n \\$$$$  /   $$ |    $$$$$$$$ |$$ |  \\__|$$ | $$$$$$$ |$$ |      $$ /  $$ |$$ |$$ /  $$ |$$ | $$ | $$ |\r\n $$  $$<    $$ |$$\\ $$   ____|$$ |      $$ |$$  __$$ |$$ |      $$ |  $$ |$$ |$$ |  $$ |$$ | $$ | $$ |\r\n$$  /\\$$\\   \\$$$$  |\\$$$$$$$\\ $$ |      $$ |\\$$$$$$$ |$$ |      \\$$$$$$$ |$$ |\\$$$$$$  |\\$$$$$\\$$$$  |\r\n\\__/  \\__|   \\____/  \\_______|\\__|      \\__| \\_______|\\__|       \\____$$ |\\__| \\______/  \\_____\\____/ \r\n                                                                $$\\   $$ |                            \r\n                                                                \\$$$$$$  |                            \r\n                                                                 \\______/                             ");
        Console.WriteLine("Enable glow ESP - F1");

        while (true)
        {
            if ((GetAsyncKeyState(F1) & 0x1) != 0)
            {
                Config.EnableGlowESP = !Config.EnableGlowESP;
            }

            if (Config.EnableGlowESP)
                ApplyGlowESP(memory, client);

            Thread.Sleep(10);
        }
    }

    static void ApplyGlowESP(Memory memory, IntPtr client)
    {
        IntPtr localPawn = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)client + Offsets.dwLocalPlayerPawn));
        if (localPawn == IntPtr.Zero) return;

        IntPtr entityList = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)client + Offsets.dwEntityList));
        if (entityList == IntPtr.Zero) return;

        for (int i = 0; i < 65; i++)
        {
            IntPtr entityPtr = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)entityList + (8 * (i & 0x7FFF) >> 9) + 16));
            if (entityPtr == IntPtr.Zero) continue;

            IntPtr controllerPtr = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)entityPtr + 120 * (i & 0x1FF)));
            if (controllerPtr == IntPtr.Zero) continue;

            uint m_hPlayerPawn = memory.Read<uint>((IntPtr)((long)controllerPtr + Offsets.m_hPlayerPawn));
            if (m_hPlayerPawn == 0) continue;

            IntPtr listEntityPtr = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)entityList + 0x8 * ((m_hPlayerPawn & 0x7FFF) >> 9) + 16));
            if (listEntityPtr == IntPtr.Zero) continue;

            IntPtr entityPawn = (IntPtr)memory.Read<UIntPtr>((IntPtr)((long)listEntityPtr + 120 * (m_hPlayerPawn & 0x1FF)));
            if (entityPawn == IntPtr.Zero || entityPawn == localPawn) continue;

            uint health = memory.Read<uint>(entityPawn + Offsets.m_iHealth);
            if (health == 0) continue;

            memory.Write<uint>(entityPawn + Offsets.m_Glow + Offsets.m_iGlowType, 0);
            memory.Write<uint>(entityPawn + Offsets.m_Glow + Offsets.m_glowColorOverride, Config.GlowColor);
            memory.Write<uint>(entityPawn + Offsets.m_Glow + Offsets.m_bGlowing, 1);
        }
    }
}
