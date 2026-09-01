using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lea.Image
{
    // Mg applies decoder-safe policies before the helper accepts image bytes.
    internal static class Mg
    {
        public const int Required = 3;
        public const int All = 15;

        public static int Apply()
        {
            if (!Set(3, 3)) throw Error("Lea could not enable permanent strict-handle checks.");
            if (!Set(6, 1)) throw Error("Lea could not disable legacy extension points.");
            var result = Required;
            if (Set(10, 7)) result |= 4; // no remote/low images; prefer System32
            if (Set(13, 1)) result |= 8; // no child-process creation
            return result;
        }

        private static Win32Exception Error(string message)
        {
            return new Win32Exception(Marshal.GetLastWin32Error(), message);
        }

        private static bool Set(int kind, uint flags)
        {
            var policy = new Policy { Flags = flags };
            try { return SetProcessMitigationPolicy(kind, ref policy, new UIntPtr(4)); }
            catch (EntryPointNotFoundException) { return false; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Policy { public uint Flags; }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessMitigationPolicy(int kind, ref Policy policy, UIntPtr length);
    }
}
