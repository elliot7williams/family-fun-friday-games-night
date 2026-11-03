using System;
using System.Runtime.InteropServices;
using System.Windows.Automation.Provider;

// Minimal-provider ABI experiment only. Ownership transfers on S_OK.
internal static class NativeProviderArray
{
    [StructLayout(LayoutKind.Sequential)] private struct Bound { internal uint Count; internal int Lower; }
    [DllImport("oleaut32.dll")] private static extern IntPtr SafeArrayCreateEx(ushort type, uint dimensions, ref Bound bound, ref Guid iid);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayPutElement(IntPtr array, ref int index, IntPtr value);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayDestroy(IntPtr array);

    internal static int Create(ITextRangeProvider[] values, out IntPtr result)
    {
        result = IntPtr.Zero;
        IntPtr array = IntPtr.Zero;
        try
        {
            Guid iid = typeof(ITextRangeProvider).GUID;
            var bound = new Bound { Count = checked((uint)values.Length), Lower = 0 };
            array = SafeArrayCreateEx(13, 1, ref bound, ref iid);
            if (array == IntPtr.Zero) throw new OutOfMemoryException();
            for (int i = 0; i < values.Length; i++)
            {
                IntPtr item = Marshal.GetComInterfaceForObject(values[i], typeof(ITextRangeProvider));
                try { Marshal.ThrowExceptionForHR(SafeArrayPutElement(array, ref i, item)); }
                finally { Marshal.Release(item); }
            }
            result = array; array = IntPtr.Zero;
            return 0;
        }
        catch (Exception error) { return Marshal.GetHRForException(error); }
        finally { if (array != IntPtr.Zero) SafeArrayDestroy(array); GC.KeepAlive(values); }
    }
}
