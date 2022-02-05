using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Bartz24.Data;


namespace memory
{
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
        public abstract bool writeBytes(Process process, ulong address, String pattern);

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
        private Process lprocess;
        private String lpattern;
        private IntPtr handleProcess;
        private ProcessModule localModule;
        private byte[] backupBytes = null;

        public Scanner(Process process, IntPtr handle = default(IntPtr), String pattern = "" )
        {
            lprocess = process;
            lpattern = pattern;
            handleProcess = handle;
        }

        public void setPattern(String pattern) { lpattern = pattern;  }

        public void setHandle(IntPtr handle) { handleProcess = handle;  }

        public void setModule(ProcessModule module) { localModule = module; }

        private byte[] ConvertPattern(String pattern)
        {
            List<byte> convertertedArray = new List<byte>();
            foreach (String each in pattern.Split(' '))
            {
                if (each == "??") { convertertedArray.Add(Convert.ToByte("0", 16)); }
                else{ convertertedArray.Add(Convert.ToByte(each,16)); }
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
                    continue;
                for (var MatchedIndex = 0; MatchedIndex < convertedByteArray.Length && indexAfterBase + MatchedIndex < localModulebytes.Length; MatchedIndex++)
                {
                    if (convertedByteArray[MatchedIndex] == 0x0)
                        continue;
                    if (convertedByteArray[MatchedIndex] != localModulebytes[indexAfterBase + MatchedIndex])
                    {
                        noMatch = true;
                        break;
                    }
                }
                if (!noMatch)
                    return (ulong)localModule.BaseAddress + (ulong)indexAfterBase;
            }
            return address;
        }

        public override ulong FindPattern()
        {
            IntPtr bytesRead;
            byte[] localModulebytes = new byte[localModule.ModuleMemorySize];
            byte[] convertedByteArray = ConvertPattern(lpattern);
            Memory.ReadProcessMemory(handleProcess,localModule.BaseAddress,localModulebytes,localModule.ModuleMemorySize,out bytesRead);
            return scanLogic(localModulebytes,convertedByteArray);

        }

        public override bool writeBytes(Process process, ulong address, byte[] bytesToWrite)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            setBackupBytes(process,address,bytesToWrite);
            Memory.WriteProcessMemory(process.Handle, (IntPtr)address, bytesToWrite, bytesToWrite.Length, out bytesWritten);
            if (bytesWritten == IntPtr.Zero) { return false; }
            else { return true; }
        }
        public override bool writeBytes(Process process, ulong address, String pattern)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            byte[] bytesToWrite = ConvertPattern(pattern);
            return writeBytes(process, address, bytesToWrite);

        }

        public override void setBackupBytes(Process process, ulong address, byte[] bytesToWrite)
        {
            IntPtr bytesRead = IntPtr.Zero;
            backupBytes = new byte[bytesToWrite.Length];
            Memory.ReadProcessMemory(process.Handle, (IntPtr)address, backupBytes, bytesToWrite.Length, out bytesRead);
        }

        public override byte[] getBackupBytes()
        {
            return backupBytes;
        }
    }

    
}
