using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Lea.Build
{
    public static class NtfsStorageProbe
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Info
        {
            public uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
            public uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
        }
        public sealed class State
        {
            public uint Attributes, Volume, Links;
            public ulong FileId, LogicalBytes, StorageBytes;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle file, out Info info);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetCompressedFileSizeW(string path, out uint high);

        public static State Read(string path)
        {
            Info info;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                if (!GetFileInformationByHandle(stream.SafeFileHandle, out info)) throw new Win32Exception(Marshal.GetLastWin32Error());
            uint high;
            uint low = GetCompressedFileSizeW(path, out high);
            if (low == uint.MaxValue && Marshal.GetLastWin32Error() != 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            return new State { Attributes = info.Attributes, Volume = info.Volume, Links = info.Links,
                FileId = ((ulong)info.IndexHigh << 32) | info.IndexLow,
                LogicalBytes = ((ulong)info.SizeHigh << 32) | info.SizeLow,
                StorageBytes = ((ulong)high << 32) | low };
        }
    }
}
