using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lea.Build
{
    // Read-only diagnostics from the retained job handle, never a PID-open/kill API.
    public static class OwnedJobMembers
    {
        public sealed class Row
        { public uint Id, Parent; public string Name; }
        public static Row[] Read(SafeHandle job)
        {
            const int bytes = 65536;
            IntPtr data = Marshal.AllocHGlobal(bytes);
            try
            {
                if (!QueryInformationJobObject(job, 3, data, bytes, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
                int count = Marshal.ReadInt32(data, 4);
                if (count < 0 || count > (bytes - 8) / IntPtr.Size) throw new InvalidOperationException("Job member bounds");
                var ids = new HashSet<uint>();
                for (int i = 0; i < count; i++) ids.Add(checked((uint)Marshal.ReadIntPtr(data, 8 + i * IntPtr.Size).ToInt64()));
                var rows = new List<Row>();
                IntPtr snapshot = CreateToolhelp32Snapshot(2, 0);
                if (snapshot == new IntPtr(-1)) throw new Win32Exception(Marshal.GetLastWin32Error());
                try
                {
                    var entry = new Entry { Size = (uint)Marshal.SizeOf(typeof(Entry)) };
                    bool more = Process32First(snapshot, ref entry);
                    while (more)
                    {
                        if (ids.Remove(entry.Id)) rows.Add(new Row { Id = entry.Id, Parent = entry.Parent, Name = entry.Name });
                        more = Process32Next(snapshot, ref entry);
                    }
                    foreach (uint id in ids) rows.Add(new Row { Id = id, Name = "<metadata no longer present>" });
                }
                finally { CloseHandle(snapshot); }
                return rows.ToArray();
            }
            finally { Marshal.FreeHGlobal(data); }
        }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct Entry
        {
            public uint Size, Usage, Id;
            public UIntPtr Heap;
            public uint Module, Threads, Parent;
            public int Priority;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)] public string Name;
        }
        [DllImport("kernel32.dll", SetLastError=true)] private static extern bool QueryInformationJobObject(SafeHandle job, int kind, IntPtr data, int bytes, IntPtr returned);
        [DllImport("kernel32.dll", SetLastError=true)] private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint process);
        [DllImport("kernel32.dll", EntryPoint="Process32FirstW", CharSet=CharSet.Unicode)] private static extern bool Process32First(IntPtr snapshot, ref Entry entry);
        [DllImport("kernel32.dll", EntryPoint="Process32NextW", CharSet=CharSet.Unicode)] private static extern bool Process32Next(IntPtr snapshot, ref Entry entry);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
    }
}
