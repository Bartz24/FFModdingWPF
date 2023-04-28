using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Multiworld.MemoryScanner.memory;

public abstract class Memory
{
    [DllImport("kernel32.dll")]
    protected static extern bool ReadProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    int dwSize,
    out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    protected static extern bool WriteProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    int dwSize,
    out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    public abstract ulong FindPattern();

    public abstract bool writeBytes(Process process, ulong address, byte[] bytesToWrite);
    public abstract bool writeBytes(Process process, ulong address, string pattern);

    public abstract void setBackupBytes(Process process, ulong address, byte[] bytesToWrite);
    public abstract byte[] getBackupBytes();

    public static uint ReadUInt(IntPtr hProcess, long address)
    {
        byte[] bytes = new byte[4];
        ReadProcessMemory(hProcess, (IntPtr)address, bytes, bytes.Length, out _);
        return bytes.ReadUInt(0);
    }
    public static byte ReadByte(IntPtr hProcess, long address)
    {
        byte[] bytes = new byte[1];
        ReadProcessMemory(hProcess, (IntPtr)address, bytes, bytes.Length, out _);
        return bytes[0];
    }
    public static string ReadString(IntPtr hProcess, long address, int size)
    {
        byte[] bytes = new byte[size];
        ReadProcessMemory(hProcess, (IntPtr)address, bytes, bytes.Length, out _);
        return System.Text.Encoding.Default.GetString(bytes).Trim('\0');
    }
}

public class Scanner : Memory
{
    private readonly Process lprocess;
    private string lpattern;
    private IntPtr handleProcess;
    private ProcessModule localModule;
    private byte[] backupBytes = null;

    public Scanner(Process process, IntPtr handle = default, string pattern = "")
    {
        lprocess = process;
        lpattern = pattern;
        handleProcess = handle;
    }

    public void setPattern(string pattern) { lpattern = pattern; }

    public void setHandle(IntPtr handle) { handleProcess = handle; }

    public void setModule(ProcessModule module) { localModule = module; }

    private byte[] ConvertPattern(string pattern)
    {
        List<byte> convertertedArray = new();
        foreach (string each in pattern.Split(' '))
        {
            if (each == "??")
            {
                convertertedArray.Add(Convert.ToByte("0", 16));
            }
            else
            {
                convertertedArray.Add(Convert.ToByte(each, 16));
            }
        }

        return convertertedArray.ToArray();
    }

    private ulong scanLogic(byte[] localModulebytes, byte[] convertedByteArray)
    {
        ulong address = 0;
        for (int indexAfterBase = 0; indexAfterBase < localModulebytes.Length; indexAfterBase++)
        {
            bool noMatch = false;
            if (localModulebytes[indexAfterBase] != convertedByteArray[0])
            {
                continue;
            }

            for (int MatchedIndex = 0; MatchedIndex < convertedByteArray.Length && indexAfterBase + MatchedIndex < localModulebytes.Length; MatchedIndex++)
            {
                if (convertedByteArray[MatchedIndex] == 0x0)
                {
                    continue;
                }

                if (convertedByteArray[MatchedIndex] != localModulebytes[indexAfterBase + MatchedIndex])
                {
                    noMatch = true;
                    break;
                }
            }

            if (!noMatch)
            {
                return (ulong)localModule.BaseAddress + (ulong)indexAfterBase;
            }
        }

        return address;
    }

    public override ulong FindPattern()
    {
        byte[] localModulebytes = new byte[localModule.ModuleMemorySize];
        byte[] convertedByteArray = ConvertPattern(lpattern);
        ReadProcessMemory(handleProcess, localModule.BaseAddress, localModulebytes, localModule.ModuleMemorySize, out _);
        return scanLogic(localModulebytes, convertedByteArray);

    }

    public override bool writeBytes(Process process, ulong address, byte[] bytesToWrite)
    {
        setBackupBytes(process, address, bytesToWrite);
        WriteProcessMemory(process.Handle, (IntPtr)address, bytesToWrite, bytesToWrite.Length, out IntPtr bytesWritten);
        return bytesWritten != IntPtr.Zero;
    }
    public override bool writeBytes(Process process, ulong address, string pattern)
    {
        byte[] bytesToWrite = ConvertPattern(pattern);
        return writeBytes(process, address, bytesToWrite);

    }

    public override void setBackupBytes(Process process, ulong address, byte[] bytesToWrite)
    {
        backupBytes = new byte[bytesToWrite.Length];
        ReadProcessMemory(process.Handle, (IntPtr)address, backupBytes, bytesToWrite.Length, out _);
    }

    public override byte[] getBackupBytes()
    {
        return backupBytes;
    }
}

