using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Globalization;

namespace ExternalSkins
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int access, bool inherit, int pid);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProc, IntPtr baseAddr, byte[] buffer, int size, out int read);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProc, IntPtr baseAddr, byte[] buffer, int size, out int written);

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProc, IntPtr addr, out MEM_INFO info, int len);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEM_INFO
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        const int ALL_ACCESS = 0x1F0FFF;
        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_RW = 0x04;

        // Signature suffix for skin data in memory
        static string skinMask = " 01 00 00 00 ?? 00 00 ??";

        static void Main(string[] args)
        {
            Console.Title = "SO2 Skin Tool v1.0 - Private Build";
            Console.WriteLine(">>> Searching for emulator process...");

            Process proc = GetTarget();
            if (proc == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] FATAL: Emulator process not found. Please start the game first.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            IntPtr handle = OpenProcess(ALL_ACCESS, false, proc.Id);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] Attached to: {proc.ProcessName} [PID: {proc.Id}]");
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("\n===========================================");
                Console.Write("Target Skin ID: ");
                string rawOld = Console.ReadLine();
                Console.Write("New Skin ID: ");
                string rawNew = Console.ReadLine();

                if (!int.TryParse(rawOld, out int oldId) || !int.TryParse(rawNew, out int newId))
                {
                    Console.WriteLine("[!] Input error: Use numeric IDs only.");
                    continue;
                }

                byte[] idBytes = BitConverter.GetBytes(oldId);
                string hexPattern = BitConverter.ToString(idBytes).Replace("-", " ") + skinMask;

                Console.WriteLine($"[*] Scanning for: {hexPattern}...");

                var hits = RunScan(handle, hexPattern);

                if (hits.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[-] No matches found. Try to equip/unequip the item.");
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[!] Memory hits: {hits.Count}");
                for (int i = 0; i < hits.Count; i++)
                {
                    Console.WriteLine($" [{i}] @ 0x{hits[i].ToString("X")}");
                }
                Console.ResetColor();

                Console.WriteLine("\nAction Menu:");
                Console.WriteLine(" 1 - Patch ALL addresses");
                Console.WriteLine(" 2 - Patch specific index");
                Console.WriteLine(" 0 - Cancel");
                Console.Write("> ");

                string cmd = Console.ReadLine();

                if (cmd == "1")
                {
                    byte[] patch = BitConverter.GetBytes(newId);
                    foreach (var addr in hits)
                    {
                        WriteRaw(handle, addr, patch);
                    }
                    Console.WriteLine("[++] Success: All addresses patched.");
                }
                else if (int.TryParse(cmd, out int idx) && idx >= 0 && idx < hits.Count)
                {
                    byte[] patch = BitConverter.GetBytes(newId);
                    WriteRaw(handle, hits[idx], patch);
                    Console.WriteLine($"[+] Target [0x{hits[idx].ToString("X")}] updated.");
                }
                else
                {
                    Console.WriteLine("[?] Operation aborted.");
                }
            }
        }

        static Process GetTarget()
        {
            string[] emuNames = { "HD-Player", "LdVBoxHeadless", "Ld9BoxHeadless" };
            foreach (var name in emuNames)
            {
                var p = Process.GetProcessesByName(name).FirstOrDefault();
                if (p != null) return p;
            }
            return null;
        }

        static List<IntPtr> RunScan(IntPtr hProc, string pattern)
        {
            var results = new List<IntPtr>();
            var split = pattern.Split(' ');
            byte?[] pBytes = new byte?[split.Length];

            for (int i = 0; i < split.Length; i++)
            {
                pBytes[i] = (split[i] == "??") ? (byte?)null : byte.Parse(split[i], NumberStyles.HexNumber);
            }

            long current = 0;
            MEM_INFO info;

            while (VirtualQueryEx(hProc, (IntPtr)current, out info, Marshal.SizeOf(typeof(MEM_INFO))) != 0)
            {
                if (info.State == MEM_COMMIT && (info.Protect & PAGE_RW) != 0)
                {
                    byte[] buf = new byte[(int)info.RegionSize];
                    if (ReadProcessMemory(hProc, info.BaseAddress, buf, buf.Length, out _))
                    {
                        for (int i = 0; i < buf.Length - pBytes.Length; i++)
                        {
                            if (Check(buf, i, pBytes))
                            {
                                results.Add(IntPtr.Add(info.BaseAddress, i));
                            }
                        }
                    }
                }
                current = info.BaseAddress.ToInt64() + info.RegionSize.ToInt64();
            }
            return results;
        }

        static bool Check(byte[] data, int offset, byte?[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i].HasValue && data[offset + i] != pattern[i].Value)
                    return false;
            }
            return true;
        }

        static void WriteRaw(IntPtr hProc, IntPtr addr, byte[] data)
        {
            WriteProcessMemory(hProc, addr, data, data.Length, out _);
        }
    }

}
