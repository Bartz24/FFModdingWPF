using Swed64;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bartz24.Memory;

public class MemoryReaderWriter : IDisposable
{
    private Swed? Swed { get; set; }
    public IntPtr ModuleBase { get; set; }
    private IntPtr ProcessHandle { get; set; }

    private Dictionary<string, IntPtr> CodeCaves { get; set; } = new();

    private Dictionary<IntPtr, byte[]> OriginalDetourBytes { get; set; } = new();

    private Dictionary<string, (IntPtr address, Type type, bool cleanup)> CustomVariables { get; set; } = new();

    [DllImport("kernel32.dll")]

    static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]

    static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]

    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddres, int dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

    public MemoryReaderWriter(string processName)
    {
        try
        {
            Swed = new(processName);
            ModuleBase = Swed.GetModuleBase(".exe");
            ProcessHandle = OpenProcess(0x0008 | 0x0020, 1, Swed.GetProcess().Id);
        }
        catch (Exception e)
        {
            Swed = null;
            throw new Exception("Could not open process " + processName, e);
        }
    }

    public void Dispose()
    {
        for (int i = CodeCaves.Count - 1; i >= 0; i--)
        {
            FreeCodeCave(CodeCaves.ElementAt(i).Value);
        }

        for (int i = CustomVariables.Count - 1; i >= 0; i--)
        {            
            FreeCustomVariable(CustomVariables.ElementAt(i).Key);
        }

        for (int i = OriginalDetourBytes.Count - 1; i >= 0; i--)
        {
            Swed.WriteBytes(OriginalDetourBytes.ElementAt(i).Key, OriginalDetourBytes.ElementAt(i).Value);
        }

        CloseHandle(ProcessHandle);
    }

    public void AddCustomVariable<T>(string name, IntPtr? preferredAddress = null)
    {
        IntPtr address = VirtualAllocEx(ProcessHandle, (nint)preferredAddress, Marshal.SizeOf(typeof(T)), 0x1000 | 0x2000, 0x40);
        if (address != IntPtr.Zero)
        {
            if (preferredAddress != address && preferredAddress != null)
            {
                throw new Exception("Could not allocate memory at preferred address");
            }

            CustomVariables.Add(name, (address, typeof(T), true));
        }
        else
        {
            throw new Exception("Could not allocate memory for custom variable");
        }
    }

    public void RegisterExistingCustomVariable<T>(string name, IntPtr address)
    {
        CustomVariables.Add(name, (address, typeof(T), false));
    }

    public T GetCustomVariable<T>(string name)
    {
        if (CustomVariables.ContainsKey(name))
        {
            var (address, type, cleanup) = CustomVariables[name];
            if (type != typeof(T))
            {
                throw new Exception($"Type mismatch for custom variable '{name}': expected {type}, got {typeof(T)}");
            }
            if (typeof(T) == typeof(int))
            {
                return (T)(object)Swed.ReadInt(address);
            }
            else if (typeof(T) == typeof(short))
            {
                return (T)(object)Swed.ReadShort(address);
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)Swed.ReadLong(address);
            }
            else if (typeof(T) == typeof(byte))
            {
                return (T)(object)Swed.ReadBytes(address, 1)[0];
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)Swed.ReadFloat(address);
            }
            else if (typeof(T) == typeof(double))
            {
                return (T)(object)Swed.ReadDouble(address);
            }
            else
            {
                throw new Exception($"Unsupported type for custom variable '{name}': {type}");
            }
        }
        else
        {
            throw new Exception($"Custom variable '{name}' not found");
        }
    }

    public void SetCustomVariable<T>(string name, T value)
    {
        if (CustomVariables.ContainsKey(name))
        {
            var (address, type, cleanup) = CustomVariables[name];
            if (type != typeof(T))
            {
                throw new Exception($"Type mismatch for custom variable '{name}': expected {type}, got {typeof(T)}");
            }
            if (typeof(T) == typeof(int))
            {
                Swed.WriteInt(address, (int)(object)value);
            }
            else if (typeof(T) == typeof(short))
            {
                Swed.WriteShort(address, (short)(object)value);
            }
            else if (typeof(T) == typeof(long))
            {
                Swed.WriteLong(address, (long)(object)value);
            }
            else if (typeof(T) == typeof(byte))
            {
                Swed.WriteBytes(address, [(byte)(object)value]);
            }
            else if (typeof(T) == typeof(float))
            {
                Swed.WriteFloat(address, (float)(object)value);
            }
            else if (typeof(T) == typeof(double))
            {
                Swed.WriteDouble(address, (double)(object)value);
            }
            else
            {
                throw new Exception($"Unsupported type for custom variable '{name}': {type}");
            }
        }
        else
        {
            throw new Exception($"Custom variable '{name}' not found");
        }
    }

    public IntPtr GetCodeCave(string name)
    {
        return CodeCaves.ContainsKey(name) ? CodeCaves[name] : IntPtr.Zero;
    }

    public bool CreateCodeCave(IntPtr addressFrom, string caveName, int caveSize, string bytes, int detourSize)
    {
        // If there's already a jump here, assume it's our own and skip
        if (Swed.ReadBytes(addressFrom, 1)[0] == 0xE9)
        {
            Trace.WriteLine($"Code cave '{caveName}' at address 0x{addressFrom.ToString("X")} already has a jump, skipping creation");
            CodeCaves.Add(caveName, addressFrom);
            return false;
        }

        int numBytes = bytes.Split(' ').Length;
        if (numBytes + 5 > caveSize)
        {
            // Round up to nearest multiple of 1000
            numBytes = ((numBytes + 5 + 999) / 1000) * 1000;
        }

        IntPtr caveAddress = VirtualAllocEx(ProcessHandle, (nint)null, caveSize, 0x1000 | 0x2000, 0x40);
        if (caveAddress != IntPtr.Zero)
        {
            CodeCaves.Add(caveName, caveAddress);
        }
        else
        {
            throw new Exception("Could not allocate memory for code cave");
        }

        // Keep original bytes for later restoration
        OriginalDetourBytes.Add(addressFrom, Swed.ReadBytes(addressFrom, detourSize));

        // Create the detour and write the bytes to jump to the cave
        MakeDetour(addressFrom, caveAddress, detourSize);

        // Write the provided bytes into the code cave
        Swed.WriteBytes(caveAddress, bytes);

        // Write the return jump back to the original code
        MakeDetour(caveAddress + numBytes, addressFrom + detourSize, 5);

        Trace.WriteLine($"Created code cave '{caveName}' at address 0x{caveAddress.ToString("X")} to detour from 0x{addressFrom.ToString("X")}");
        return true;
    }

    public bool CreateCodeCave14Byte(IntPtr addressFrom, string caveName, int caveSize, string bytes, int detourSize)
    {
        // If there's already a jump here, assume it's our own and skip
        byte[] jumpBytes = Swed.ReadBytes(addressFrom, 2);
        if (jumpBytes[0] == 0xFF && jumpBytes[1] == 0x25)
        {
            Trace.WriteLine($"Code cave '{caveName}' at address 0x{addressFrom.ToString("X")} already has a jump, skipping creation");
            CodeCaves.Add(caveName, addressFrom);
            return false;
        }

        int numBytes = bytes.Split(' ').Length;
        if (numBytes + 14 > caveSize)
        {
            // Round up to nearest multiple of 1000
            numBytes = ((numBytes + 14 + 999) / 1000) * 1000;
        }

        IntPtr caveAddress = VirtualAllocEx(ProcessHandle, (nint)null, caveSize, 0x1000 | 0x2000, 0x40);
        if (caveAddress != IntPtr.Zero)
        {
            CodeCaves.Add(caveName, caveAddress);
        }
        else
        {
            throw new Exception("Could not allocate memory for code cave");
        }

        // Keep original bytes for later restoration
        OriginalDetourBytes.Add(addressFrom, Swed.ReadBytes(addressFrom, detourSize));

        // Create the detour and write the bytes to jump to the cave
        MakeDetour14Byte(addressFrom, caveAddress, detourSize);

        // Write the provided bytes into the code cave
        Swed.WriteBytes(caveAddress, bytes);

        // Write the return jump back to the original code
        MakeDetour14Byte(caveAddress + numBytes, addressFrom + detourSize, 14);

        Trace.WriteLine($"Created code cave '{caveName}' at address 0x{caveAddress.ToString("X")} to detour from 0x{addressFrom.ToString("X")}");
        return true;
    }

    public bool FreeCodeCave(IntPtr caveAddress)
    {
        bool result = VirtualFreeEx(ProcessHandle, caveAddress, 0, 0x8000);
        if (result)
        {
            CodeCaves.Remove($"Cave_{caveAddress.ToString("X")}");
        }

        return result;
    }

    public bool FreeCustomVariable(string name)
    {
        if (CustomVariables.ContainsKey(name))
        {
            var (address, type, cleanup) = CustomVariables[name];
            if (!cleanup)
            {
                return true;
            }

            bool result = VirtualFreeEx(ProcessHandle, address, 0, 0x8000);
            if (result)
            {
                CustomVariables.Remove(name);
            }
            return result;
        }
        else
        {
            throw new Exception($"Custom variable '{name}' not found");
        }
    }

    private void MakeDetour(IntPtr address, IntPtr destination, int bytesToPatch)
    {
        long offset = destination.ToInt64() - address.ToInt64() - 5;
        byte[] patch = new byte[bytesToPatch];

        // Replace with NOPs
        for (int i = 0; i < bytesToPatch; i++)
        {
            patch[i] = 0x90; // NOP instruction
        }

        patch[0] = 0xE9; // JMP instruction
        Array.Copy(BitConverter.GetBytes(offset), 0, patch, 1, 4);

        Swed.WriteBytes(address, patch);
    }

    private void MakeDetour14Byte(IntPtr address, IntPtr destination, int bytesToPatch)
    {
        byte[] patch = new byte[bytesToPatch];

        // Replace with NOPs
        for (int i = 0; i < bytesToPatch; i++)
        {
            patch[i] = 0x90; // NOP instruction
        }

        patch[0] = 0xFF; // JMP instruction
        patch[1] = 0x25; // JMP instruction
        patch[2] = 0x00; // RIP-relative addressing
        patch[3] = 0x00; // RIP-relative addressing
        patch[4] = 0x00; // RIP-relative addressing
        patch[5] = 0x00; // RIP-relative addressing
        Array.Copy(BitConverter.GetBytes((long)destination), 0, patch, 6, 8);

        Swed.WriteBytes(address, patch);
    }

    public bool IsProcessRunning()
    {
        return Swed != null && !Swed.GetProcess().HasExited;
    }

    public long ReadLongFromBase(IntPtr offset)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        return Swed.ReadLong(ModuleBase + offset);
    }

    public void WriteLongToBase(IntPtr offset, long value)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        Swed.WriteLong(ModuleBase + offset, value);
    }

    public int ReadInt(IntPtr address)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }
        return Swed.ReadInt(address);
    }

    public int ReadIntFromBase(IntPtr offset)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        return Swed.ReadInt(ModuleBase + offset);
    }

    public void WriteIntToBase(IntPtr offset, int value)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        Swed.WriteInt(ModuleBase + offset, value);
    }

    public void WriteInt(IntPtr address, int value)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }
        Swed.WriteInt(address, value);
    }   

    public byte ReadByteFromBase(IntPtr offset)
    {
        return ReadBytesFromBase(offset, 1)[0];
    }

    public void WriteByteToBase(IntPtr offset, byte value)
    {
        WriteBytesToBase(offset, [value]);
    }

    public byte[] ReadBytesFromBase(IntPtr offset, int count)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        return Swed.ReadBytes(ModuleBase + offset, count);
    }

    public void WriteBytesToBase(IntPtr offset, byte[] bytes)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        Swed.WriteBytes(ModuleBase + offset, bytes);
    }

    public short ReadShort(IntPtr address)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        return Swed.ReadShort(address);
    }

    public short ReadShortFromBase(IntPtr offset)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }
        return Swed.ReadShort(ModuleBase + offset);
    }

    public void WriteShortToBase(IntPtr offset, short value)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }
        Swed.WriteShort(ModuleBase + offset, value);
    }

    public IntPtr GetPointerChainFromBase(params IntPtr[] offsets)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }

        IntPtr address = ModuleBase + offsets[0];
        for (int i = 1; i < offsets.Length; i++)
        {
            address = (IntPtr)Swed.ReadLong(address);
            address += offsets[i];
        }

        return address;
    }

    public bool VerifyAOBPattern(IntPtr address, string pattern)
    {
        if (Swed == null)
        {
            throw new Exception("Process not opened");
        }
        byte[] bytes = Swed.ReadBytes(address, pattern.Split(' ').Length);
        string[] patternParts = pattern.Split(' ');
        for (int i = 0; i < patternParts.Length; i++)
        {
            if (patternParts[i] != "??")
            {
                byte expectedByte = Convert.ToByte(patternParts[i], 16);
                if (bytes[i] != expectedByte)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
