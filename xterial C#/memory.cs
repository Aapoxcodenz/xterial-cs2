using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Memory
{
    private IntPtr processHandle;
    private int processId;

    public Memory(string processName)
    {
        foreach (Process proc in Process.GetProcessesByName(processName))
        {
            processId = proc.Id;
            processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite | ProcessAccessFlags.VirtualMemoryOperation, false, processId);
            break;
        }
    }

    public bool IsValid() => processHandle != IntPtr.Zero;

    public IntPtr GetModuleBase(string moduleName)
    {
        foreach (ProcessModule module in Process.GetProcessById(processId).Modules)
        {
            if (module.ModuleName == moduleName)
                return module.BaseAddress;
        }
        return IntPtr.Zero;
    }

    public T Read<T>(IntPtr address) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];
        ReadProcessMemory(processHandle, address, buffer, size, out _);
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        T result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();
        return result;
    }

    public void Write<T>(IntPtr address, T value) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(value, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        WriteProcessMemory(processHandle, address, buffer, size, out _);
        Marshal.FreeHGlobal(ptr);
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

    [Flags]
    private enum ProcessAccessFlags : int
    {
        VirtualMemoryRead = 0x0010,
        VirtualMemoryWrite = 0x0020,
        VirtualMemoryOperation = 0x0008
    }
}
