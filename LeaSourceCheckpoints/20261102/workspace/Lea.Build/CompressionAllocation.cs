using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public static class CompressionAllocation
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetCompressedFileSizeW(string path, out uint high);

    public static long Bytes(string path)
    {
        uint high;
        var low = GetCompressedFileSizeW(path, out high);
        if (low == UInt32.MaxValue && Marshal.GetLastWin32Error() != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return checked((long)(((ulong)high << 32) | low));
    }
}
